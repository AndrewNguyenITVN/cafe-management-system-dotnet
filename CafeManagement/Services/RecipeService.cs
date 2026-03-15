using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class RecipeService
{
    private readonly AppDbContext _db;
    public RecipeService(AppDbContext db) => _db = db;

    public async Task<List<MenuItem>> GetActiveMenuItemsAsync()
        => await _db.MenuItems.Include(m => m.Category).Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();

    public async Task<List<Ingredient>> GetActiveIngredientsAsync()
        => await _db.Ingredients.Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync();

    public async Task<List<Recipe>> GetRecipesByMenuItemAsync(int menuItemId)
        => await _db.Recipes.Include(r => r.Ingredient).Where(r => r.MenuItemId == menuItemId).ToListAsync();

    public async Task UpdateRecipesAsync(int menuItemId, List<(int IngredientId, decimal Quantity, decimal WastePercent)> items)
    {
        // Xóa công thức cũ
        var old = await _db.Recipes.Where(r => r.MenuItemId == menuItemId).ToListAsync();
        _db.Recipes.RemoveRange(old);

        // Thêm công thức mới
        foreach (var (ingredientId, quantity, waste) in items)
        {
            if (quantity > 0)
            {
                _db.Recipes.Add(new Recipe
                {
                    MenuItemId = menuItemId,
                    IngredientId = ingredientId,
                    Quantity = quantity,
                    WastePercent = waste
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}
