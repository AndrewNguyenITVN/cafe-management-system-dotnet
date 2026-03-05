using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class SupplierService
{
    private readonly AppDbContext _db;
    public SupplierService(AppDbContext db) => _db = db;

    public Task<List<Supplier>> GetAllAsync()
        => _db.Suppliers.OrderBy(s => s.Name).ToListAsync();

    public Task<List<Supplier>> GetActiveAsync()
        => _db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

    public Task<Supplier?> GetByIdAsync(int id)
        => _db.Suppliers.FindAsync(id).AsTask();

    public async Task CreateAsync(Supplier model)
    {
        _db.Suppliers.Add(model);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supplier model)
    {
        _db.Suppliers.Update(model);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return false;
        supplier.IsActive = !supplier.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }
}
