using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin")]
public class StoreController : Controller
{
    private readonly StoreService _storeService;
    public StoreController(StoreService storeService) => _storeService = storeService;

    public async Task<IActionResult> Index()
        => View(await _storeService.GetAllAsync());

    public IActionResult Create() => View(new Store());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Store model)
    {
        if (!ModelState.IsValid) return View(model);
        await _storeService.CreateAsync(model);
        TempData["Success"] = "Đã thêm chi nhánh thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var store = await _storeService.GetByIdAsync(id);
        if (store == null) return NotFound();
        return View(store);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Store model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _storeService.UpdateAsync(model);
        TempData["Success"] = "Đã cập nhật chi nhánh thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _storeService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
