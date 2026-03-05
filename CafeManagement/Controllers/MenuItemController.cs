using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class MenuItemController : Controller
{
    private readonly MenuItemService _menuItemService;
    public MenuItemController(MenuItemService menuItemService) => _menuItemService = menuItemService;

    public async Task<IActionResult> Index()
        => View(await _menuItemService.GetAllWithCategoryAsync());

    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new MenuItemViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(model);
        }
        await _menuItemService.CreateAsync(model);
        TempData["Success"] = "Đã thêm món thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _menuItemService.GetByIdAsync(id);
        if (item == null) return NotFound();
        await PopulateCategoriesAsync();
        return View(new MenuItemViewModel
        {
            Id               = item.Id,
            CategoryId       = item.CategoryId,
            Name             = item.Name,
            Description      = item.Description,
            BasePrice        = item.BasePrice,
            IsActive         = item.IsActive,
            ExistingImageUrl = item.ImageUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MenuItemViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(model);
        }
        await _menuItemService.UpdateAsync(id, model);
        TempData["Success"] = "Đã cập nhật món thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _menuItemService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync()
    {
        var categories = await _menuItemService.GetActiveCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
    }
}
