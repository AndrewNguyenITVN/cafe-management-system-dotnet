using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _db;

    // Ca làm cố định — ShiftId phải khớp với dữ liệu seed trong DB
    private static readonly List<ShiftInfo> _shifts = new()
    {
        new() { ShiftId = 1, ShiftName = "Ca 1 - Sáng",  TimeRange = "06:00 – 12:00", Hours = 6 },
        new() { ShiftId = 2, ShiftName = "Ca 2 - Chiều", TimeRange = "12:00 – 18:00", Hours = 6 },
        new() { ShiftId = 3, ShiftName = "Ca 3 - Tối",   TimeRange = "18:00 – 24:00", Hours = 6 },
    };

    // Giờ bắt đầu mỗi ca (dùng khi tạo CheckInTime cho Timekeeping)
    private static readonly Dictionary<int, TimeOnly> _shiftStartTimes = new()
    {
        { 1, new TimeOnly(6,  0) },
        { 2, new TimeOnly(12, 0) },
        { 3, new TimeOnly(18, 0) },
    };

    public ScheduleService(AppDbContext db) => _db = db;

    // ── Lấy lịch tuần ─────────────────────────────────────────────
    public async Task<ScheduleWeekViewModel> GetWeeklyScheduleAsync(int storeId, DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(6);

        // Lấy lịch phân công
        var schedules = await _db.Schedules
            .Include(s => s.User)
            .Where(s => s.StoreId == storeId
                     && s.WorkDate >= weekStart
                     && s.WorkDate <= weekEnd)
            .ToListAsync();

        // Lấy bản ghi chấm công trong tuần
        var timekeepings = await _db.Timekeepings
            .Where(t => t.StoreId == storeId
                     && t.Date >= weekStart
                     && t.Date <= weekEnd)
            .ToListAsync();

        var weekDays = Enumerable.Range(0, 7)
            .Select(i => weekStart.AddDays(i))
            .ToList();

        var cells = new Dictionary<string, ScheduleCellViewModel>();

        foreach (var day in weekDays)
        {
            foreach (var shift in _shifts)
            {
                var key = $"{day:yyyy-MM-dd}_{shift.ShiftId}";

                var dayShiftSchedules = schedules
                    .Where(s => s.WorkDate == day && s.ShiftId == shift.ShiftId)
                    .ToList();

                // CheckInTime của ca này trong ngày này
                var shiftStart = _shiftStartTimes[shift.ShiftId];
                var checkInDt = day.ToDateTime(shiftStart);

                // Nhân viên đã được chấm công = có bản ghi Timekeeping trùng UserId + Date + giờ vào ca
                var attendedIds = timekeepings
                    .Where(t => t.Date == day
                             && t.CheckInTime == checkInDt)
                    .Select(t => t.UserId)
                    .ToList();

                cells[key] = new ScheduleCellViewModel
                {
                    Date = day,
                    ShiftId = shift.ShiftId,
                    UserIds = dayShiftSchedules.Select(s => s.UserId).ToList(),
                    UserNames = dayShiftSchedules
                                        .Select(s => s.User?.FullName ?? s.User?.Email ?? "?")
                                        .ToList(),
                    AttendedUserIds = attendedIds
                };
            }
        }

        return new ScheduleWeekViewModel
        {
            SelectedStoreId = storeId,
            WeekStart = weekStart,
            WeekDays = weekDays,
            Shifts = _shifts,
            Cells = cells,
            Stores = await GetStoresAsync(),
            AllUsers = await GetUsersByStoreAsync(storeId)
        };
    }

    // ── Lên lịch: phân công nhân viên ─────────────────────────────
    public async Task<bool> AssignShiftAsync(
        int storeId, DateOnly date, int shiftId, List<string> userIds)
    {
        try
        {
            var existing = await _db.Schedules
                .Where(s => s.StoreId == storeId
                         && s.WorkDate == date
                         && s.ShiftId == shiftId)
                .ToListAsync();

            _db.Schedules.RemoveRange(existing);

            foreach (var userId in userIds.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                _db.Schedules.Add(new Schedule
                {
                    StoreId = storeId,
                    WorkDate = date,
                    ShiftId = shiftId,
                    UserId = userId
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    // ── Chấm công: ghi Timekeeping ────────────────────────────────
    public async Task<bool> AttendShiftAsync(
        int storeId, DateOnly date, int shiftId, List<string> attendedUserIds)
    {
        try
        {
            var shiftInfo = _shifts.First(s => s.ShiftId == shiftId);
            var shiftStart = _shiftStartTimes[shiftId];
            var checkInDt = date.ToDateTime(shiftStart);
            var checkOutDt = checkInDt.AddHours((double)shiftInfo.Hours);

            // Xóa bản ghi chấm công cũ của ca này (để ghi lại từ đầu)
            var existing = await _db.Timekeepings
                .Where(t => t.StoreId == storeId
                         && t.Date == date
                         && t.CheckInTime == checkInDt)
                .ToListAsync();

            _db.Timekeepings.RemoveRange(existing);

            // Ghi bản ghi mới cho từng nhân viên được tick
            foreach (var userId in attendedUserIds.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                // Lấy HourlyRate hiện tại của nhân viên để snapshot
                var user = await _db.Users
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                var hourlyRate = user?.Position?.HourlyRate ?? 0m;

                _db.Timekeepings.Add(new Timekeeping
                {
                    UserId = userId,
                    StoreId = storeId,
                    Date = date,
                    CheckInTime = checkInDt,
                    CheckOutTime = checkOutDt,
                    TotalHours = shiftInfo.Hours,
                    HourlyRateAtTime = hourlyRate,
                    IsLate = false
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    // ── Helpers ───────────────────────────────────────────────────
    public async Task<List<StoreSelectItem>> GetStoresAsync() =>
        await _db.Stores
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new StoreSelectItem { StoreId = s.Id, StoreName = s.Name })
            .ToListAsync();

    public async Task<List<UserSelectItem>> GetUsersByStoreAsync(int storeId) =>
        await _db.Users
            .Where(u => u.IsActive && u.StoreId == storeId)
            .OrderBy(u => u.FullName)
            .Select(u => new UserSelectItem { UserId = u.Id, FullName = u.FullName })
            .ToListAsync();
}