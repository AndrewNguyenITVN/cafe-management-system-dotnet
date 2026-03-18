using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ScheduleController : Controller
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
        => _scheduleService = scheduleService;

    // GET: /Schedule?storeId=1&weekStart=2025-06-09
    public async Task<IActionResult> Index(int? storeId, string? weekStart)
    {
        var stores = await _scheduleService.GetStoresAsync();
        if (!stores.Any())
        {
            TempData["Error"] = "Chưa có chi nhánh nào trong hệ thống.";
            return View(null);
        }

        var selectedStore = storeId ?? stores.First().StoreId;

        // Tính đầu tuần (Thứ 2) — dùng DateOnly xuyên suốt
        DateOnly monday;
        if (!string.IsNullOrEmpty(weekStart) && DateOnly.TryParse(weekStart, out var parsed))
        {
            monday = parsed;
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            // DayOfWeek: Monday=1 ... Sunday=0; tính về thứ 2 gần nhất
            int diff = ((int)today.DayOfWeek + 6) % 7; // 0=Mon,1=Tue,...,6=Sun
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

        await _scheduleService.AssignShiftAsync(storeId, parsedDate, shiftId, userIds);

        TempData["Success"] = "Đã cập nhật lịch làm việc thành công.";
        // Quay về tuần chứa ngày vừa assign
        int diff = ((int)parsedDate.DayOfWeek + 6) % 7;
        var weekMonday = parsedDate.AddDays(-diff);
        return RedirectToAction(nameof(Index),
            new { storeId, weekStart = weekMonday.ToString("yyyy-MM-dd") });
    }

    // GET: /Schedule/GetUsers?storeId=1  (dùng cho AJAX reload danh sách user khi đổi store)
    [HttpGet]
    public async Task<IActionResult> GetUsers(int storeId)
    {
        var users = await _scheduleService.GetUsersByStoreAsync(storeId);
        return Json(users);
    }
}
