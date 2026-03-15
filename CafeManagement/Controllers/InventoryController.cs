using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly AppDbContext _db;
    private readonly InventoryService _inventoryService;

    public InventoryController(AppDbContext db, InventoryService inventoryService)
    {
        _db = db;
        _inventoryService = inventoryService;
    }

    // GET: /Inventory/Index
    public async Task<IActionResult> Index(int? storeId)
    {
        var stores = await _db.Stores.Where(s => s.IsActive).ToListAsync();
        ViewBag.Stores = stores;

        if (!storeId.HasValue && stores.Any())
            storeId = stores.First().Id;

        ViewBag.SelectedStoreId = storeId;

        if (storeId.HasValue)
        {
            var stocks = await _db.InventoryStocks
                .Include(s => s.Ingredient)
                .Where(s => s.StoreId == storeId)
                .ToListAsync();

            var allIngredients = await _db.Ingredients.Where(i => i.IsActive).ToListAsync();
            var result = new List<InventoryStock>();

            foreach (var ing in allIngredients)
            {
                var stock = stocks.FirstOrDefault(s => s.IngredientId == ing.Id);
                if (stock == null)
                    stock = new InventoryStock { Ingredient = ing, CurrentQuantity = 0 };
                result.Add(stock);
            }

            return View(result.OrderBy(s => s.Ingredient.Name).ToList());
        }

        return View(new List<InventoryStock>());
    }

    // GET: /Inventory/Purchase
    public async Task<IActionResult> Purchase()
    {
        ViewBag.Stores = await _db.Stores.Where(s => s.IsActive).ToListAsync();
        ViewBag.Suppliers = await _db.Suppliers.Where(s => s.IsActive).ToListAsync();
        ViewBag.Ingredients = await _db.Ingredients.Where(i => i.IsActive).ToListAsync();
        return View();
    }

    // POST: /Inventory/CreatePurchase
    [HttpPost]
    public async Task<IActionResult> CreatePurchase(int storeId, int supplierId,
        List<int> ingredientIds, List<decimal> quantities, List<decimal> prices)
    {
        if (ingredientIds == null || !ingredientIds.Any())
        {
            TempData["Error"] = "Đơn nhập kho phải có ít nhất 1 nguyên liệu.";
            return RedirectToAction("Purchase");
        }

        var po = new PurchaseOrder { StoreId = storeId, SupplierId = supplierId };
        var details = new List<PurchaseOrderDetail>();

        for (int i = 0; i < ingredientIds.Count; i++)
        {
            if (quantities[i] > 0)
            {
                details.Add(new PurchaseOrderDetail
                {
                    IngredientId = ingredientIds[i],
                    Quantity = quantities[i],
                    CostPrice = prices != null && prices.Count > i ? prices[i] : 0
                });
            }
        }

        await _inventoryService.CreatePurchaseOrderAsync(po, details);
        TempData["Success"] = "Đã nhập hàng vào kho thành công!";
        return RedirectToAction("Index", new { storeId });
    }

    // GET: /Inventory/Stocktake
    public async Task<IActionResult> Stocktake(int? storeId)
    {
        var stores = await _db.Stores.Where(s => s.IsActive).ToListAsync();
        ViewBag.Stores = stores;

        if (!storeId.HasValue && stores.Any())
            storeId = stores.First().Id;

        ViewBag.SelectedStoreId = storeId;

        if (storeId.HasValue)
        {
            var stocks = await _db.InventoryStocks
                .Include(s => s.Ingredient)
                .Where(s => s.StoreId == storeId)
                .ToListAsync();

            var allIngredients = await _db.Ingredients.Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync();
            var result = new List<InventoryStock>();

            foreach (var ing in allIngredients)
            {
                var stock = stocks.FirstOrDefault(s => s.IngredientId == ing.Id);
                if (stock == null)
                    stock = new InventoryStock { IngredientId = ing.Id, Ingredient = ing, CurrentQuantity = 0 };
                result.Add(stock);
            }

            return View(result);
        }

        return View(new List<InventoryStock>());
    }

    // POST: /Inventory/SubmitStocktake
    [HttpPost]
    public async Task<IActionResult> SubmitStocktake(int storeId,
        List<int> ingredientIds, List<decimal> actualQuantities)
    {
        var dtos = new List<StocktakeDto>();
        for (int i = 0; i < ingredientIds.Count; i++)
        {
            dtos.Add(new StocktakeDto
            {
                IngredientId = ingredientIds[i],
                ActualQuantity = actualQuantities[i]
            });
        }

        var warnings = await _inventoryService.HandleStocktakeAsync(storeId, dtos);

        if (warnings.Any())
            TempData["Warning"] = string.Join("<br/>", warnings);
        else
            TempData["Success"] = "Chốt kiểm kê thành công!";

        return RedirectToAction("Stocktake", new { storeId });
    }
}
