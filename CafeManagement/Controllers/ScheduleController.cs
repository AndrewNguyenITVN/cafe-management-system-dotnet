using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

// Tất cả role đều xem được lịch
[Authorize]
public class ScheduleController : Controller
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
        => _scheduleService = scheduleService;

    // GET: /Schedule?storeId=1&weekStart=2026-03-16
    public async Task<IActionResult> Index(int? storeId, string? weekStart)
    {
        var stores = await _scheduleService.GetStoresAsync();
        if (!stores.Any())
        {
            TempData["Error"] = "Chưa có chi nhánh nào trong hệ thống.";
            return View(null);
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

    // POST: /Schedule/Assign — chỉ Admin/Manager
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Assign(
        int storeId, string date, int shiftId, List<string> userIds)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Ngày không hợp lệ.");

        await _scheduleService.AssignShiftAsync(storeId, parsedDate, shiftId, userIds);

        TempData["Success"] = "Đã cập nhật lịch làm việc thành công.";
        return RedirectToAction(nameof(Index),
            new { storeId, weekStart = GetMondayOf(parsedDate) });
    }

    // POST: /Schedule/Attend — chỉ Admin/Manager
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Attend(
        int storeId, string date, int shiftId, List<string> attendedUserIds)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return BadRequest("Ngày không hợp lệ.");

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