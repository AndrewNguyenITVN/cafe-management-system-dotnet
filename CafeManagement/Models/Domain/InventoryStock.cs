namespace CafeManagement.Models.Domain;

public class InventoryStock
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int IngredientId { get; set; }
    public decimal CurrentQuantity { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public Store Store { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
