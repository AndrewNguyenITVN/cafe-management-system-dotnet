using CafeManagement.Data;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _db;
    public PayrollService(AppDbContext db) => _db = db;

    public async Task<List<PayrollRowViewModel>> GetPayrollAsync(
        int? storeId, DateOnly fromDate, DateOnly toDate)
    {
        // ── 1. Timekeeping trong kỳ ─────────────────────────────────
        var tkQuery = _db.Timekeepings
            .Include(t => t.User)
                .ThenInclude(u => u.Position)
            .Where(t => t.Date >= fromDate && t.Date <= toDate
                     && t.User.StoreId != null);

        if (storeId.HasValue)
            tkQuery = tkQuery.Where(t => t.User.StoreId == storeId.Value);

        var timekeepings = await tkQuery.ToListAsync();

        // ── 2. Schedule (ca phân công) trong kỳ ────────────────────
        var schQuery = _db.Schedules
            .Where(s => s.WorkDate >= fromDate && s.WorkDate <= toDate);

        if (storeId.HasValue)
            schQuery = schQuery.Where(s => s.StoreId == storeId.Value);

        var schedules = await schQuery.ToListAsync();

        // ── 3. Gộp danh sách userId từ cả 2 nguồn ──────────────────
        var validUserIds = await _db.Users
            .Where(u => u.IsActive
                     && u.StoreId != null
                     && (!storeId.HasValue || u.StoreId == storeId.Value))
            .Select(u => u.Id)
            .ToListAsync();

        var allUserIds = timekeepings.Select(t => t.UserId)
            .Union(schedules.Select(s => s.UserId))
            .Distinct()
            .Where(uid => validUserIds.Contains(uid))
            .ToList();

        var adminIds = await _db.Users
            .Where(u => u.StoreId == null)
            .Select(u => u.Id)
            .ToListAsync();
        allUserIds = allUserIds.Where(uid => !adminIds.Contains(uid)).ToList();

        // ── 4. Tính lương từng người ────────────────────────────────
        var rows = new List<PayrollRowViewModel>();

        foreach (var userId in allUserIds)
        {
            var userTks = timekeepings.Where(t => t.UserId == userId).ToList();
            var user = userTks.FirstOrDefault()?.User;

            // User chỉ có trong Schedule chưa có timekeeping → lấy từ DB
            if (user == null)
            {
                user = await _db.Users
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(u => u.Id == userId);
            }

            var totalHours = userTks.Sum(t => t.TotalHours ?? 0m);
            var totalSalary = userTks.Sum(t => (t.TotalHours ?? 0m) * (t.HourlyRateAtTime ?? 0m));
            var avgRate = totalHours > 0 ? totalSalary / totalHours : 0m;

            // Số ca được phân công vs số ca đã chấm công
            int assignedShifts = schedules.Count(s => s.UserId == userId);
            int attendedShifts = userTks.Count; // mỗi Timekeeping = 1 ca

            rows.Add(new PayrollRowViewModel
            {
                UserId = userId,
                FullName = user?.FullName ?? user?.Email ?? "?",
                JobPosition = user?.Position?.PositionName ?? "—",
                TotalHours = Math.Round(totalHours, 2),
                AverageHourlyRate = Math.Round(avgRate, 0),
                TotalSalary = Math.Round(totalSalary, 0),
                AssignedShifts = assignedShifts,
                AttendedShifts = attendedShifts
            });
        }

        rows = rows.OrderBy(r => r.FullName).ToList();
        for (int i = 0; i < rows.Count; i++)
            rows[i].RowNumber = i + 1;

        return rows;
    }

    public async Task<List<StoreSelectItem>> GetStoresAsync() =>
        await _db.Stores
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new StoreSelectItem { StoreId = s.Id, StoreName = s.Name })
            .ToListAsync();
}