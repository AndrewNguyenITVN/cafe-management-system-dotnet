namespace CafeManagement.Models.Domain;

public class OrderDetailTopping
{
    public int Id { get; set; }
    public int OrderDetailId { get; set; }
    public int ToppingId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }  // Snapshot giá Topping tại thời điểm bán

    public OrderDetail OrderDetail { get; set; } = null!;
    public Topping Topping { get; set; } = null!;
}
