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
        // Query Timekeeping trong khoảng ngày, kèm User và Position
        // NOTE: DateOnly so sánh trực tiếp được với EF Core 6+
        var query = _db.Timekeepings
            .Include(t => t.User)
                .ThenInclude(u => u.Position)   // AppUser.Position -> JobPosition
            .Where(t => t.Date >= fromDate && t.Date <= toDate);

        if (storeId.HasValue)
            query = query.Where(t => t.StoreId == storeId.Value);

        var timekeepings = await query.ToListAsync();

        // Group theo UserId bằng LINQ
        var rows = timekeepings
            .GroupBy(t => t.UserId)
            .Select(g =>
            {
                var user = g.First().User;

                // TotalHours và HourlyRateAtTime đều là decimal? → dùng ?? 0
                var totalHours  = g.Sum(t => t.TotalHours ?? 0m);
                var totalSalary = g.Sum(t => (t.TotalHours ?? 0m) * (t.HourlyRateAtTime ?? 0m));
                var avgRate     = totalHours > 0 ? totalSalary / totalHours : 0m;

                return new PayrollRowViewModel
                {
                    UserId           = g.Key,
                    FullName         = user?.FullName ?? user?.Email ?? "?",
                    JobPosition      = user?.Position?.PositionName ?? "—",
                    TotalHours       = Math.Round(totalHours,  2),
                    AverageHourlyRate = Math.Round(avgRate,    0),
                    TotalSalary      = Math.Round(totalSalary, 0)
                };
            })
            .OrderBy(r => r.FullName)
            .ToList();

        // Đánh số thứ tự
        for (int i = 0; i < rows.Count; i++)
            rows[i].RowNumber = i + 1;

        return rows;
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
}
