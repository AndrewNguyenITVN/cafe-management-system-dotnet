using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin")]
public class JobPositionController : Controller
{
    private readonly JobPositionService _jobPositionService;
    public JobPositionController(JobPositionService jobPositionService) => _jobPositionService = jobPositionService;

    public async Task<IActionResult> Index()
        => View(await _jobPositionService.GetAllAsync());

    public IActionResult Create() => View(new JobPositionViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobPositionViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        await _jobPositionService.CreateAsync(model);
        TempData["Success"] = "Đã thêm chức vụ thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var pos = await _jobPositionService.GetByIdAsync(id);
        if (pos == null) return NotFound();
        return View(new JobPositionViewModel
        {
            Id           = pos.Id,
            PositionName = pos.PositionName,
            HourlyRate   = pos.HourlyRate,
            IsActive     = pos.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JobPositionViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _jobPositionService.UpdateAsync(id, model);
        TempData["Success"] = "Đã cập nhật chức vụ thành công.";
        return RedirectToAction(nameof(Index));
    }
}
