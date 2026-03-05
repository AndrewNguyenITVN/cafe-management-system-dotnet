using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class CategoryController : Controller
{
    private readonly CategoryService _categoryService;
    public CategoryController(CategoryService categoryService) => _categoryService = categoryService;

    public async Task<IActionResult> Index()
        => View(await _categoryService.GetAllAsync());

    public IActionResult Create() => View(new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (!ModelState.IsValid) return View(model);
        await _categoryService.CreateAsync(model);
        TempData["Success"] = "Đã thêm danh mục thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _categoryService.UpdateAsync(model);
        TempData["Success"] = "Đã cập nhật danh mục thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _categoryService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
