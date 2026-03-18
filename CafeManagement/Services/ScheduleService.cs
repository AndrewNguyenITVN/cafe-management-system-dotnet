using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _db;

    // Ca làm cố định khớp với Shifts đã seed trong DB (ShiftId 1,2,3)
    private static readonly List<ShiftInfo> _shifts = new()
    {
        new() { ShiftId = 1, ShiftName = "Ca 1 - Sáng",  TimeRange = "06:00 – 12:00" },
        new() { ShiftId = 2, ShiftName = "Ca 2 - Chiều", TimeRange = "12:00 – 18:00" },
        new() { ShiftId = 3, ShiftName = "Ca 3 - Tối",   TimeRange = "18:00 – 24:00" },
    };

    public ScheduleService(AppDbContext db) => _db = db;

    public async Task<ScheduleWeekViewModel> GetWeeklyScheduleAsync(int storeId, DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(6);

        // Lấy toàn bộ lịch trong tuần của store, kèm thông tin user
        var schedules = await _db.Schedules
            .Include(s => s.User)
            .Where(s => s.StoreId == storeId
                     && s.WorkDate >= weekStart
                     && s.WorkDate <= weekEnd)
            .ToListAsync();

        // Tạo danh sách 7 ngày trong tuần
        var weekDays = Enumerable.Range(0, 7)
            .Select(i => weekStart.AddDays(i))
            .ToList();

        // Build dictionary cells
        var cells = new Dictionary<string, ScheduleCellViewModel>();
        foreach (var day in weekDays)
        {
            foreach (var shift in _shifts)
            {
                var key = $"{day:yyyy-MM-dd}_{shift.ShiftId}";
                var dayShiftSchedules = schedules
                    .Where(s => s.WorkDate == day && s.ShiftId == shift.ShiftId)
                    .ToList();

                cells[key] = new ScheduleCellViewModel
                {
                    Date    = day,
                    ShiftId = shift.ShiftId,
                    UserIds   = dayShiftSchedules.Select(s => s.UserId).ToList(),
                    UserNames = dayShiftSchedules
                        .Select(s => s.User?.FullName ?? s.User?.Email ?? "?")
                        .ToList()
                };
            }
        }

        return new ScheduleWeekViewModel
        {
            SelectedStoreId = storeId,
            WeekStart       = weekStart,
            WeekDays        = weekDays,
            Shifts          = _shifts,
            Cells           = cells,
            Stores          = await GetStoresAsync(),
            AllUsers        = await GetUsersByStoreAsync(storeId)
        };
    }

    public async Task<bool> AssignShiftAsync(
        int storeId, DateOnly date, int shiftId, List<string> userIds)
    {
        try
        {
            // Xóa phân công cũ của ca đó trong ngày đó (quản lý muốn chỉnh sửa)
            var existing = await _db.Schedules
                .Where(s => s.StoreId == storeId
                         && s.WorkDate == date
                         && s.ShiftId  == shiftId)
                .ToListAsync();

            _db.Schedules.RemoveRange(existing);

            // Thêm phân công mới
            foreach (var userId in userIds.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                _db.Schedules.Add(new Schedule
                {
                    StoreId  = storeId,
                    WorkDate = date,
                    ShiftId  = shiftId,
                    UserId   = userId
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<StoreSelectItem>> GetStoresAsync() =>
        await _db.Stores
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new StoreSelectItem
            {
                StoreId   = s.Id,
                StoreName = s.Name
            })
            .ToListAsync();

    public async Task<List<UserSelectItem>> GetUsersByStoreAsync(int storeId) =>
        await _db.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new UserSelectItem
            {
                UserId   = u.Id,
                FullName = u.FullName
            })
            .ToListAsync();
}
