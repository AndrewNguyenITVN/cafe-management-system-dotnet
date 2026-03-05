using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class StoreService
{
    private readonly AppDbContext _db;
    public StoreService(AppDbContext db) => _db = db;

    public Task<List<Store>> GetAllAsync()
        => _db.Stores.OrderBy(s => s.Name).ToListAsync();

    public Task<List<Store>> GetActiveAsync()
        => _db.Stores.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

    public Task<Store?> GetByIdAsync(int id)
        => _db.Stores.FindAsync(id).AsTask();

    public async Task CreateAsync(Store model)
    {
        _db.Stores.Add(model);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Store model)
    {
        _db.Stores.Update(model);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var store = await _db.Stores.FindAsync(id);
        if (store == null) return false;
        store.IsActive = !store.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}
