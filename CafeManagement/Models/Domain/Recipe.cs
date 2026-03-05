namespace CafeManagement.Models.Domain;

public class Recipe
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public int IngredientId { get; set; }
    public decimal Quantity { get; set; }       // Lượng cần cho 1 đơn vị món
    public decimal WastePercent { get; set; } = 0;  // % hao hụt trong pha chế

    public MenuItem MenuItem { get; set; } = null!;
    public Ingredient Ingredient { get; set; } = null!;
}
