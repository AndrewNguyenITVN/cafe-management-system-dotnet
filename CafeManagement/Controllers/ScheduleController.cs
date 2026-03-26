using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ScheduleController : Controller
{
    private readonly IScheduleService _scheduleService;
    private readonly UserManager<AppUser> _userManager;

    public ScheduleController(IScheduleService scheduleService, UserManager<AppUser> userManager)
    {
        _scheduleService = scheduleService;
        _userManager = userManager;
    }

    // GET: /Schedule?storeId=1&weekStart=2026-03-16
    public async Task<IActionResult> Index(int? storeId, string? weekStart)
    {
        var stores = await _scheduleService.GetStoresAsync();
        if (!stores.Any())
        {
            TempData["Error"] = "Chưa có chi nhánh nào trong hệ thống.";
            return View(null);
        }

        // Manager chỉ được xem chi nhánh của mình
        if (User.IsInRole("Manager") && !User.IsInRole("Admin"))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StoreId == null)
            {
                TempData["Error"] = "Tài khoản quản lý chưa được gán chi nhánh.";
                return View(null);
            }
            storeId = currentUser.StoreId;
        }

        var selectedStore = storeId ?? stores.First().StoreId;

        // Tính đầu tuần (Thứ 2)
        DateOnly monday;
        if (!string.IsNullOrEmpty(weekStart) && DateOnly.TryParse(weekStart, out var parsed))
        {
            monday = parsed;
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int diff = ((int)today.DayOfWeek + 6) % 7;
            monday = today.AddDays(-diff);
        }

        var vm = await _scheduleService.GetWeeklyScheduleAsync(selectedStore, monday);
        return View(vm);
    }

    // POST: /Schedule/Assign
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(
        int storeId, string date, int shiftId, List<string> userIds)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Ngày không hợp lệ.");

        // Manager chỉ được lên lịch cho chi nhánh của mình
        if (User.IsInRole("Manager") && !User.IsInRole("Admin"))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StoreId != storeId)
                return Forbid();
        }

        await _scheduleService.AssignShiftAsync(storeId, parsedDate, shiftId, userIds);

        TempData["Success"] = "Đã cập nhật lịch làm việc thành công.";
        return RedirectToAction(nameof(Index),
            new { storeId, weekStart = GetMondayOf(parsedDate) });
    }

    // POST: /Schedule/Attend
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Attend(
        int storeId, string date, int shiftId, List<string> attendedUserIds)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Ngày không hợp lệ.");

        // Manager chỉ được chấm công cho chi nhánh của mình
        if (User.IsInRole("Manager") && !User.IsInRole("Admin"))
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.StoreId != storeId)
                return Forbid();
        }

        await _scheduleService.AttendShiftAsync(storeId, parsedDate, shiftId, attendedUserIds);

        TempData["Success"] = "Đã cập nhật chấm công thành công.";
        return RedirectToAction(nameof(Index),
            new { storeId, weekStart = GetMondayOf(parsedDate) });
    }

    // Helper: lấy ngày thứ 2 của tuần chứa ngày đó
    private static string GetMondayOf(DateOnly date)
    {
        int diff = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-diff).ToString("yyyy-MM-dd");
    }
}