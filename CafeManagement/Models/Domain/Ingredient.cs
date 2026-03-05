namespace CafeManagement.Models.Domain;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;  // gram | ml | cái | túi | hộp
    public decimal MinStockLevel { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
    public ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();
    public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
}
