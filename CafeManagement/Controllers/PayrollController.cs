using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly UserManager<AppUser> _userManager;

    public PayrollController(IPayrollService payrollService, UserManager<AppUser> userManager)
    {
        _payrollService = payrollService;
        _userManager = userManager;
    }

    // GET: /Payroll?storeId=1&fromDate=2025-06-01&toDate=2025-06-30
    public async Task<IActionResult> Index(int? storeId, string? fromDate, string? toDate)
    {
        // Manager chỉ được xem bảng lương chi nhánh của mình
        if (User.IsInRole("Manager") && !User.IsInRole("Admin"))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StoreId == null)
            {
                TempData["Error"] = "Tài khoản quản lý chưa được gán chi nhánh.";
                return View(new PayrollViewModel());
            }
            storeId = currentUser.StoreId;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var from  = DateOnly.TryParse(fromDate, out var fd)
                        ? fd
                        : new DateOnly(today.Year, today.Month, 1);
        var to    = DateOnly.TryParse(toDate, out var td) ? td : today;

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
