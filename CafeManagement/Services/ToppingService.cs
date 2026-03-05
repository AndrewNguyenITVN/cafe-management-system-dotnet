using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class ToppingService
{
    private readonly AppDbContext _db;
    public ToppingService(AppDbContext db) => _db = db;

    public Task<List<Topping>> GetAllAsync()
        => _db.Toppings.OrderBy(t => t.Name).ToListAsync();

    public Task<List<Topping>> GetActiveAsync()
        => _db.Toppings.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync();

    public Task<Topping?> GetByIdAsync(int id)
        => _db.Toppings.FindAsync(id).AsTask();

    public async Task CreateAsync(Topping model)
    {
        _db.Toppings.Add(model);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Topping model)
    {
        _db.Toppings.Update(model);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var topping = await _db.Toppings.FindAsync(id);
        if (topping == null) return false;
        topping.IsActive = !topping.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}
