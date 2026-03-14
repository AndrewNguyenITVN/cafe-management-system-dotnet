using System;
using System.Collections.Generic;

namespace CafeManagement.Models.ViewModels;

/// <summary>
/// Dùng cho màn Dashboard admin: số liệu tổng quan + dữ liệu vẽ biểu đồ doanh thu.
/// </summary>
public class DashboardViewModel
{
    public decimal RevenueToday { get; set; }
    public int OrdersToday { get; set; }
    public int StaffWorkingToday { get; set; }
    public int LowStockCount { get; set; }

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

