using System;
using System.Collections.Generic;

namespace CafeManagement.Models.ViewModels;

/// <summary>
/// ViewModel của Dashboard: dữ liệu KPI và chuỗi doanh thu để render chart.
/// </summary>
public class DashboardViewModel
{
    // 4 card KPI chính ở đầu trang.
    public decimal RevenueToday { get; set; }
    public int OrdersToday { get; set; }
    public int StaffWorkingToday { get; set; }
    public int LowStockCount { get; set; }

    // Khoảng thời gian và điểm dữ liệu biểu đồ.
    public DateOnly ChartFromDate { get; set; }
    public DateOnly ChartToDate { get; set; }
    public List<RevenuePointDto> RevenueSeries { get; set; } = new();
}

/// <summary>
/// Một điểm dữ liệu doanh thu theo ngày dùng cho Chart trên Dashboard.
/// </summary>
public class RevenuePointDto
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
}

