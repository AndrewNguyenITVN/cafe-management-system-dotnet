using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class SupplierController : Controller
{
    private readonly SupplierService _supplierService;
    public SupplierController(SupplierService supplierService) => _supplierService = supplierService;

    public async Task<IActionResult> Index()
        => View(await _supplierService.GetAllAsync());

    public IActionResult Create() => View(new Supplier());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier model)
    {
        if (!ModelState.IsValid) return View(model);
        await _supplierService.CreateAsync(model);
        TempData["Success"] = "Đã thêm nhà cung cấp thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        if (supplier == null) return NotFound();
        return View(supplier);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Supplier model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _supplierService.UpdateAsync(model);
        TempData["Success"] = "Đã cập nhật nhà cung cấp thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _supplierService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
