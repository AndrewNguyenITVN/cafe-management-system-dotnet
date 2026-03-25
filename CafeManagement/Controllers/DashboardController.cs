using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

/// <summary>
/// Controller màn Dashboard: gom KPI + chuỗi chart và đẩy sang view.
/// </summary>
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ReportingService _reportingService;

    public DashboardController(ReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Mặc định hiển thị 30 ngày gần nhất tính đến hôm nay.
        var today = DateOnly.FromDateTime(DateTime.Today);
        var fromDate = today.AddDays(-29); // 30 ngày gần nhất

        // Lấy 4 chỉ số chính (revenue, orders, staff, low stock).
        var (revenueToday, ordersToday, staffWorkingToday, lowStockCount)
            = await _reportingService.GetDashboardSummaryAsync(today);

        // Lấy dữ liệu doanh thu theo từng ngày để vẽ biểu đồ.
        var series = await _reportingService.GetRevenueSeriesAsync(fromDate, today);

        // Đóng gói dữ liệu view.
        var vm = new DashboardViewModel
        {
            RevenueToday = revenueToday,
            OrdersToday = ordersToday,
            StaffWorkingToday = staffWorkingToday,
            LowStockCount = lowStockCount,
            ChartFromDate = fromDate,
            ChartToDate = today,
            RevenueSeries = series
        };

        return View(vm);
    }
}
