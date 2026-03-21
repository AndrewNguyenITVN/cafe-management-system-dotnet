namespace CafeManagement.Models.Domain;

public class InventoryLog
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int IngredientId { get; set; }
    public decimal ChangeQuantity { get; set; }    // Dương: nhập vào | Âm: xuất ra
    public string Type { get; set; } = string.Empty;  // Purchase | Sale | Adjustment
    public int? ReferenceId { get; set; }          // OrderId hoặc PurchaseOrderId
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? UserId { get; set; }

    public Store Store { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
    public AppUser? User { get; set; }
}
