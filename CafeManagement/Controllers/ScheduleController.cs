using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ScheduleService _scheduleService;

        public ScheduleController(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // =========================
        // LIST SCHEDULE
        // =========================
        public async Task<IActionResult> Index()
        {
            var schedules = await _scheduleService.GetAllAsync();

            if (schedules == null)
            {
                schedules = new List<Schedule>();
            }

            return View(schedules);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDropdownData();
                    return View(schedule);
                }

                await _scheduleService.CreateAsync(schedule);

                TempData["success"] = "Schedule created successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownData();
                return View(schedule);
            }
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _scheduleService.GetByIdAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }

            await LoadDropdownData();
            return View(schedule);
        }


        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDropdownData();
                    return View(schedule);
                }

                await _scheduleService.UpdateAsync(schedule);

                TempData["success"] = "Schedule updated successfully";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadDropdownData();
                return View(schedule);
            }
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _scheduleService.DeleteAsync(id);

                TempData["success"] = "Schedule deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // LOAD DROPDOWN DATA
        // =========================
        private async Task LoadDropdownData()
        {
            ViewBag.Users = await _scheduleService.GetUsersAsync();
            ViewBag.Shifts = await _scheduleService.GetShiftsAsync();
            ViewBag.Stores = await _scheduleService.GetStoresAsync();
        }
    }
}