using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class InventoryController : Controller
{
    private readonly AppDbContext _db;
    private readonly InventoryService _inventoryService;
    private readonly UserManager<AppUser> _userManager;

    public InventoryController(AppDbContext db, InventoryService inventoryService, UserManager<AppUser> userManager)
    {
        _db = db;
        _inventoryService = inventoryService;
        _userManager = userManager;
    }

    // Trả về StoreId nếu user là Manager (bị khoá 1 store), null nếu Admin (tuỳ chọn)
    private async Task<int?> GetManagerStoreIdAsync()
    {
        if (User.IsInRole("Admin")) return null;
        var user = await _userManager.GetUserAsync(User);
        return user?.StoreId;
    }

    private async Task SetStoreViewBagAsync(int? managerStoreId)
    {
        if (managerStoreId.HasValue)
        {
            var store = await _db.Stores.FindAsync(managerStoreId.Value);
            ViewBag.Stores = store != null ? new List<Store> { store } : new List<Store>();
            ViewBag.ManagerStoreId = managerStoreId.Value;
            ViewBag.ManagerStoreName = store?.Name ?? "";
        }
        else
        {
            ViewBag.Stores = await _db.Stores.Where(s => s.IsActive).ToListAsync();
            ViewBag.ManagerStoreId = null;
        }
    }

    // GET: /Inventory/Index
    public async Task<IActionResult> Index(int? storeId)
    {
        var managerStoreId = await GetManagerStoreIdAsync();
        await SetStoreViewBagAsync(managerStoreId);

        // Manager bị khoá vào store của họ
        if (managerStoreId.HasValue)
            storeId = managerStoreId.Value;

        var stores = ViewBag.Stores as List<Store>;
        if (!storeId.HasValue && stores != null && stores.Any())
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
        var managerStoreId = await GetManagerStoreIdAsync();
        await SetStoreViewBagAsync(managerStoreId);
        ViewBag.Suppliers = await _db.Suppliers.Where(s => s.IsActive).ToListAsync();
        ViewBag.Ingredients = await _db.Ingredients.Where(i => i.IsActive).ToListAsync();
        return View();
    }

    // POST: /Inventory/CreatePurchase
    [HttpPost]
    public async Task<IActionResult> CreatePurchase(int storeId, int? supplierId,
        List<int> ingredientIds, List<decimal> quantities, List<decimal> prices)
    {
        // Manager không được nhập kho hộ store khác
        var managerStoreId = await GetManagerStoreIdAsync();
        if (managerStoreId.HasValue && storeId != managerStoreId.Value)
        {
            TempData["Error"] = "Bạn không có quyền nhập kho cho chi nhánh này.";
            return RedirectToAction("Purchase");
        }

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

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _inventoryService.CreatePurchaseOrderAsync(po, details, userId);
        TempData["Success"] = "Đã nhập hàng vào kho thành công!";
        return RedirectToAction("Index", new { storeId });
    }

    // GET: /Inventory/Stocktake
    public async Task<IActionResult> Stocktake(int? storeId)
    {
        var managerStoreId = await GetManagerStoreIdAsync();
        await SetStoreViewBagAsync(managerStoreId);

        if (managerStoreId.HasValue)
            storeId = managerStoreId.Value;

        var stores = ViewBag.Stores as List<Store>;
        if (!storeId.HasValue && stores != null && stores.Any())
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

            var historyLogs = await _db.InventoryLogs
                .Include(l => l.Ingredient)
                .Include(l => l.User)
                .Where(l => l.StoreId == storeId && l.Type == "Adjustment")
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .ToListAsync();
            
            ViewBag.HistoryLogs = historyLogs;

            return View(result);
        }

        ViewBag.HistoryLogs = new List<InventoryLog>();
        return View(new List<InventoryStock>());
    }

    // POST: /Inventory/SubmitStocktake
    [HttpPost]
    public async Task<IActionResult> SubmitStocktake(int storeId,
        List<int> ingredientIds, List<decimal> actualQuantities)
    {
        // Manager không được chốt kiểm kê hộ store khác
        var managerStoreId = await GetManagerStoreIdAsync();
        if (managerStoreId.HasValue && storeId != managerStoreId.Value)
        {
            TempData["Error"] = "Bạn không có quyền kiểm kê cho chi nhánh này.";
            return RedirectToAction("Stocktake");
        }

        var dtos = new List<StocktakeDto>();
        for (int i = 0; i < ingredientIds.Count; i++)
        {
            dtos.Add(new StocktakeDto
            {
                IngredientId = ingredientIds[i],
                ActualQuantity = actualQuantities[i]
            });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var warnings = await _inventoryService.HandleStocktakeAsync(storeId, dtos, userId);

        if (warnings.Any())
            TempData["Warning"] = string.Join("<br/>", warnings);
        else
            TempData["Success"] = "Chốt kiểm kê thành công!";

        return RedirectToAction("Stocktake", new { storeId });
    }
}
