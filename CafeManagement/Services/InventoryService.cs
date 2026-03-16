using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

/// <summary>TV3: Inventory Engine – DeductStock, Purchase, Stocktake.</summary>
public class InventoryService
{
    private readonly AppDbContext _db;
    public InventoryService(AppDbContext db) => _db = db;

    // ── 1.2 Deduct Stock ──────────────────────────────────────────────────────
    public async Task DeductStockAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null || order.OrderDetails == null) return;

        int storeId = order.StoreId;

        foreach (var detail in order.OrderDetails)
        {
            var recipes = await _db.Recipes
                .Where(r => r.MenuItemId == detail.MenuItemId)
                .ToListAsync();

            foreach (var recipe in recipes)
            {
                decimal totalRequired = recipe.Quantity * detail.Quantity;

                var stock = await _db.InventoryStocks
                    .FirstOrDefaultAsync(s => s.StoreId == storeId && s.IngredientId == recipe.IngredientId);

                if (stock == null)
                {
                    stock = new InventoryStock { StoreId = storeId, IngredientId = recipe.IngredientId, CurrentQuantity = 0 };
                    _db.InventoryStocks.Add(stock);
                }

                stock.CurrentQuantity -= totalRequired;

                _db.InventoryLogs.Add(new InventoryLog
                {
                    StoreId = storeId,
                    IngredientId = recipe.IngredientId,
                    ChangeQuantity = -totalRequired,
                    Type = "Sale",
                    ReferenceId = orderId
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    // ── 1.3 Purchase ──────────────────────────────────────────────────────────
    public async Task CreatePurchaseOrderAsync(PurchaseOrder po, List<PurchaseOrderDetail> details)
    {
        po.Status = "Received";
        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        foreach (var detail in details)
        {
            detail.PurchaseOrderId = po.Id;
            _db.PurchaseOrderDetails.Add(detail);

            var stock = await _db.InventoryStocks
                .FirstOrDefaultAsync(s => s.StoreId == po.StoreId && s.IngredientId == detail.IngredientId);

            if (stock == null)
            {
                stock = new InventoryStock { StoreId = po.StoreId, IngredientId = detail.IngredientId, CurrentQuantity = 0 };
                _db.InventoryStocks.Add(stock);
            }

            stock.CurrentQuantity += detail.Quantity;

            _db.InventoryLogs.Add(new InventoryLog
            {
                StoreId = po.StoreId,
                IngredientId = detail.IngredientId,
                ChangeQuantity = detail.Quantity,
                Type = "Purchase",
                ReferenceId = po.Id
            });
        }

        await _db.SaveChangesAsync();
    }

    // ── 1.4 Stocktake ─────────────────────────────────────────────────────────
    public async Task<List<string>> HandleStocktakeAsync(int storeId, List<StocktakeDto> items)
    {
        var warnings = new List<string>();

        foreach (var item in items)
        {
            var stock = await _db.InventoryStocks
                .Include(s => s.Ingredient)
                .FirstOrDefaultAsync(s => s.StoreId == storeId && s.IngredientId == item.IngredientId);

            if (stock == null)
            {
                stock = new InventoryStock { StoreId = storeId, IngredientId = item.IngredientId, CurrentQuantity = 0 };
                _db.InventoryStocks.Add(stock);
            }

            decimal theoretical = stock.CurrentQuantity;
            decimal delta = item.ActualQuantity - theoretical;

            if (delta != 0)
            {
                stock.CurrentQuantity = item.ActualQuantity;

                _db.InventoryLogs.Add(new InventoryLog
                {
                    StoreId = storeId,
                    IngredientId = item.IngredientId,
                    ChangeQuantity = delta,
                    Type = "Adjustment",
                    ReferenceId = null
                });

                if (theoretical > 0 && delta < 0)
                {
                    decimal percent = Math.Abs(delta) / theoretical * 100;
                    if (percent > 5)
                    {
                        string name = stock.Ingredient?.Name ?? $"ID {item.IngredientId}";
                        warnings.Add($"Nguyên liệu '{name}' hao hụt {percent:F1}%, vượt mức cho phép.");
                    }
                }
            }
        }

        await _db.SaveChangesAsync();
        return warnings;
    }
}

public class StocktakeDto
{
    public int IngredientId { get; set; }
    public decimal ActualQuantity { get; set; }
}
