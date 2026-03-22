using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

/// <summary>
/// Dashboard admin – hiển thị số liệu tổng quan và biểu đồ doanh thu.
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
        var today = DateOnly.FromDateTime(DateTime.Today);
        var fromDate = today.AddDays(-29); // 30 ngày gần nhất

        var (revenueToday, ordersToday, staffWorkingToday, lowStockCount)
            = await _reportingService.GetDashboardSummaryAsync(today);

        var series = await _reportingService.GetRevenueSeriesAsync(fromDate, today);

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
