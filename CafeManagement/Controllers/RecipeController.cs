using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Controllers;

[Authorize]
public class RecipeController : Controller
{
    private readonly RecipeService _recipeService;

    public RecipeController(RecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    // GET: /Recipe/Index?menuItemId=1
    public async Task<IActionResult> Index(int? menuItemId)
    {
        ViewBag.MenuItems = await _recipeService.GetActiveMenuItemsAsync();
        ViewBag.Ingredients = await _recipeService.GetActiveIngredientsAsync();
        ViewBag.SelectedMenuItemId = menuItemId;

        var recipes = menuItemId.HasValue
            ? await _recipeService.GetRecipesByMenuItemAsync(menuItemId.Value)
            : new List<Recipe>();

        return View(recipes);
    }

    // POST: /Recipe/Save
    [HttpPost]
    public async Task<IActionResult> Save(int menuItemId,
        List<int> ingredientIds,
        List<decimal> quantities,
        List<decimal> wastePercents)
    {
        if (menuItemId == 0)
        {
            TempData["Error"] = "Vui lòng chọn món ăn trước khi lưu công thức.";
            return RedirectToAction("Index");
        }

        var items = new List<(int, decimal, decimal)>();
        for (int i = 0; i < ingredientIds.Count; i++)
        {
            decimal qty = quantities != null && quantities.Count > i ? quantities[i] : 0;
            decimal waste = wastePercents != null && wastePercents.Count > i ? wastePercents[i] : 0;
            items.Add((ingredientIds[i], qty, waste));
        }

        await _recipeService.UpdateRecipesAsync(menuItemId, items);
        TempData["Success"] = "Đã lưu công thức thành công!";
        return RedirectToAction("Index", new { menuItemId });
    }
}
