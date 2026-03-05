namespace CafeManagement.Models.Domain;

public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }  // Snapshot giá tại thời điểm bán
    public string? Note { get; set; }

    public Order Order { get; set; } = null!;
    public MenuItem MenuItem { get; set; } = null!;
    public ICollection<OrderDetailTopping> OrderDetailToppings { get; set; } = new List<OrderDetailTopping>();
}
