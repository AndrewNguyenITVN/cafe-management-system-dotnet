using CafeManagement.Data;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

/// <summary>
/// Service tổng hợp số liệu Dashboard: KPI trong ngày + chuỗi doanh thu theo ngày.
/// Chỉ đọc dữ liệu, không thay đổi trạng thái hệ thống.
/// </summary>
public class ReportingService
{
    private readonly AppDbContext _db;

    public ReportingService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lấy KPI dashboard theo ngày (toàn hệ thống).
    /// </summary>
    public async Task<(decimal revenueToday, int ordersToday, int staffWorkingToday, int lowStockCount)>
        GetDashboardSummaryAsync(DateOnly today)
    {
        // Doanh thu hôm nay: tổng FinalAmount của đơn trong ngày.
        var revenueToday = await _db.Orders
            .Where(o => DateOnly.FromDateTime(o.OrderDate) == today)
            .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;

        // Số lượng đơn hôm nay.
        var ordersToday = await _db.Orders
            .Where(o => DateOnly.FromDateTime(o.OrderDate) == today)
            .CountAsync();

        // Nhân viên đi làm hôm nay: đếm distinct UserId trong schedule theo WorkDate.
        var staffWorkingToday = await _db.Schedules
            .Where(t => t.WorkDate == today)
            .Select(t => t.UserId)
            .Distinct()
            .CountAsync();

        // Cảnh báo tồn kho: nguyên liệu có CurrentQuantity < MinStockLevel.
        var lowStockCount = await _db.InventoryStocks
            .Include(s => s.Ingredient)
            .Where(s => s.Ingredient.MinStockLevel > 0 && s.CurrentQuantity < s.Ingredient.MinStockLevel)
            .Select(s => s.IngredientId)
            .Distinct()
            .CountAsync();

        return (revenueToday, ordersToday, staffWorkingToday, lowStockCount);
    }

    /// <summary>
    /// Lấy chuỗi doanh thu theo ngày để vẽ chart.
    /// </summary>
    public async Task<List<RevenuePointDto>> GetRevenueSeriesAsync(DateOnly fromDate, DateOnly toDate)
    {
        // B1: Group đơn theo ngày và tính tổng tiền theo ngày.
        var raw = await _db.Orders
            .Where(o => DateOnly.FromDateTime(o.OrderDate) >= fromDate
                        && DateOnly.FromDateTime(o.OrderDate) <= toDate)
            .GroupBy(o => DateOnly.FromDateTime(o.OrderDate))
            .Select(g => new { Date = g.Key, Amount = g.Sum(x => x.FinalAmount) })
            .ToListAsync();

        var map = raw.ToDictionary(x => x.Date, x => x.Amount);

        // B2: Trả đủ dải ngày; ngày không có đơn thì amount = 0.
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

