namespace CafeManagement.Models.Domain;

public class PurchaseOrderDetail
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
