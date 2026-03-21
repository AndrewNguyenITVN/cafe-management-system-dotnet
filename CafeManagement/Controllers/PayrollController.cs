using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
        => _payrollService = payrollService;

    // GET: /Payroll?storeId=1&fromDate=2025-06-01&toDate=2025-06-30
    public async Task<IActionResult> Index(int? storeId, string? fromDate, string? toDate)
    {
        // Parse DateOnly từ query string (ASP.NET Core tự bind DateOnly được,
        // nhưng dùng string + TryParse an toàn hơn khi form date html)
        var today   = DateOnly.FromDateTime(DateTime.Today);
        var from    = DateOnly.TryParse(fromDate, out var fd)
                        ? fd
                        : new DateOnly(today.Year, today.Month, 1);
        var to      = DateOnly.TryParse(toDate, out var td) ? td : today;

        var vm = new PayrollViewModel
        {
            SelectedStoreId = storeId,
            FromDate        = from,
            ToDate          = to,
            Stores          = await _payrollService.GetStoresAsync(),
            Rows            = await _payrollService.GetPayrollAsync(storeId, from, to)
        };

        return View(vm);
    }
}
