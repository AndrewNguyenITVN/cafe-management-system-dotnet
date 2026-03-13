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
        // Tạm thời Console.WriteLine để test thay vì throw lỗi. TV3 sẽ code tiếp ở đây.
        Console.WriteLine($"[InventoryService] Đã trừ nguyên liệu cho đơn hàng #{orderId}");
        return Task.CompletedTask;
    }
}
