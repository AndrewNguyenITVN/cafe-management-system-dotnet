using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ToppingController : Controller
{
    private readonly ToppingService _toppingService;
    public ToppingController(ToppingService toppingService) => _toppingService = toppingService;

    public async Task<IActionResult> Index()
        => View(await _toppingService.GetAllAsync());

    public IActionResult Create() => View(new Topping());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Topping model)
    {
        if (!ModelState.IsValid) return View(model);
        await _toppingService.CreateAsync(model);
        TempData["Success"] = "Đã thêm topping thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var topping = await _toppingService.GetByIdAsync(id);
        if (topping == null) return NotFound();
        return View(topping);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Topping model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _toppingService.UpdateAsync(model);
        TempData["Success"] = "Đã cập nhật topping thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _toppingService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
