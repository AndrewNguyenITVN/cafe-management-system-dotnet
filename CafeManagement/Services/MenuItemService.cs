using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class MenuItemService
{
    private readonly AppDbContext        _db;
    private readonly IWebHostEnvironment _env;

    public MenuItemService(AppDbContext db, IWebHostEnvironment env)
    {
        _db  = db;
        _env = env;
    }

    public Task<List<MenuItem>> GetAllWithCategoryAsync()
        => _db.MenuItems
            .Include(m => m.Category)
            .OrderBy(m => m.Category.Name).ThenBy(m => m.Name)
            .ToListAsync();

    public Task<MenuItem?> GetByIdAsync(int id)
        => _db.MenuItems.FindAsync(id).AsTask();

    public Task<List<Category>> GetActiveCategoriesAsync()
        => _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

    public async Task CreateAsync(MenuItemViewModel model)
    {
        var item = new MenuItem
        {
            CategoryId  = model.CategoryId,
            Name        = model.Name,
            Description = model.Description,
            BasePrice   = model.BasePrice,
            IsActive    = model.IsActive
        };

        if (model.ImageFile != null && model.ImageFile.Length > 0)
            item.ImageUrl = await SaveImageAsync(model.ImageFile);

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(int id, MenuItemViewModel model)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item == null) return false;

        item.CategoryId  = model.CategoryId;
        item.Name        = model.Name;
        item.Description = model.Description;
        item.BasePrice   = model.BasePrice;
        item.IsActive    = model.IsActive;

        if (model.ImageFile != null && model.ImageFile.Length > 0)
            item.ImageUrl = await SaveImageAsync(model.ImageFile);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item == null) return false;
        item.IsActive = !item.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item == null) return (false, "Món không tồn tại.");

        bool hasOrders = await _db.OrderDetails.AnyAsync(od => od.MenuItemId == id);
        if (hasOrders)
            return (false, "Không thể xóa: món này đã có trong đơn hàng.");

        var recipes = await _db.Recipes.Where(r => r.MenuItemId == id).ToListAsync();
        if (recipes.Count > 0)
            _db.Recipes.RemoveRange(recipes);

        if (!string.IsNullOrEmpty(item.ImageUrl))
        {
            var filePath = Path.Combine(_env.WebRootPath, item.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var dir = Path.Combine(_env.WebRootPath, "images", "menu");
        Directory.CreateDirectory(dir);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(dir, fileName);
        using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);
        return $"/images/menu/{fileName}";
    }
}
