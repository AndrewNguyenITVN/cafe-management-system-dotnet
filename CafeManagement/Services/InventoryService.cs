using CafeManagement.Data;

namespace CafeManagement.Services;

/// <summary>TV3 implement class này.</summary>
public class InventoryService
{
    private readonly AppDbContext _db;
    public InventoryService(AppDbContext db) => _db = db;

    /// <summary>
    /// Trừ nguyên liệu theo Recipe + WastePercent khi đơn hàng hoàn tất.
    /// </summary>
    public Task DeductStockAsync(int orderId)
    {
        // TODO: TV3 implement
        throw new NotImplementedException("TV3: InventoryService.DeductStockAsync");
    }
}
