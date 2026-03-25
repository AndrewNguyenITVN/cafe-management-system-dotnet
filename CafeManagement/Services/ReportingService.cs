using CafeManagement.Data;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

/// <summary>
/// TV5: Service tổng hợp số liệu báo cáo cho Dashboard.
/// Không xử lý nghiệp vụ phức tạp mà chỉ đọc dữ liệu, gom nhóm và trả DTO.
/// </summary>
public class ReportingService
{
    private readonly AppDbContext _db;

    public ReportingService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy số liệu tổng quan cho Dashboard trong ngày today (theo toàn hệ thống).
    /// </summary>
    public async Task<(decimal revenueToday, int ordersToday, int staffWorkingToday, int lowStockCount)>
        GetDashboardSummaryAsync(DateOnly today)
    {
        // Doanh thu hôm nay = SUM FinalAmount của tất cả đơn trong ngày (mọi chi nhánh)
        var revenueToday = await _db.Orders
            .Where(o => DateOnly.FromDateTime(o.OrderDate) == today)
            .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;

        // Số đơn hôm nay (mọi chi nhánh)
        var ordersToday = await _db.Orders
            .Where(o => DateOnly.FromDateTime(o.OrderDate) == today)
            .CountAsync();

        // Nhân viên đang làm việc: timekeeping có CheckOutTime = null trong ngày
        var staffWorkingToday = await _db.Schedules
            .Where(t => t.WorkDate == today)
            .Select(t => t.UserId)
            .Distinct()
            .CountAsync();

        // Cảnh báo tồn kho: số nguyên liệu có tồn kho < mức tối thiểu ở bất kỳ chi nhánh nào
        var lowStockCount = await _db.InventoryStocks
            .Include(s => s.Ingredient)
            .Where(s => s.Ingredient.MinStockLevel > 0 && s.CurrentQuantity < s.Ingredient.MinStockLevel)
            .Select(s => s.IngredientId)
            .Distinct()
            .CountAsync();

        return (revenueToday, ordersToday, staffWorkingToday, lowStockCount);
    }

    /// <summary>
    /// Lấy chuỗi doanh thu theo ngày cho biểu đồ (mặc định toàn hệ thống, chưa filter theo chi nhánh).
    /// </summary>
    public async Task<List<RevenuePointDto>> GetRevenueSeriesAsync(DateOnly fromDate, DateOnly toDate)
    {
        // Lấy dữ liệu raw: group theo ngày + SUM FinalAmount
        var raw = await _db.Orders
            .Where(o => DateOnly.FromDateTime(o.OrderDate) >= fromDate
                        && DateOnly.FromDateTime(o.OrderDate) <= toDate)
            .GroupBy(o => DateOnly.FromDateTime(o.OrderDate))
            .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.FinalAmount) })
            .ToListAsync();

        var map = raw.ToDictionary(x => x.Date, x => x.Amount);

        // Bảo đảm trả về đủ ngày, kể cả ngày không có đơn (Amount = 0)
        var result = new List<RevenuePointDto>();
        for (var d = fromDate; d <= toDate; d = d.AddDays(1))
        {
            result.Add(new RevenuePointDto
            {
                Date = d,
                Amount = map.TryGetValue(d, out var val) ? val : 0
            });
        }

        return result;
    }
}

