using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class CategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db) => _db = db;

    public Task<List<Category>> GetAllAsync()
        => _db.Categories.OrderBy(c => c.Name).ToListAsync();

    public Task<List<Category>> GetActiveAsync()
        => _db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

    public Task<Category?> GetByIdAsync(int id)
        => _db.Categories.FindAsync(id).AsTask();

    public async Task CreateAsync(Category model)
    {
        _db.Categories.Add(model);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category model)
    {
        _db.Categories.Update(model);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return false;
        category.IsActive = !category.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}
