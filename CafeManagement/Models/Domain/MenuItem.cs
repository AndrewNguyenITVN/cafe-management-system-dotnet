namespace CafeManagement.Models.Domain;

public class MenuItem
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public Category Category { get; set; } = null!;
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
