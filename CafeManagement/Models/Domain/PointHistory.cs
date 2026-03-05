namespace CafeManagement.Models.Domain;

public class PointHistory
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public int PointsChanged { get; set; }      // Dương: tích điểm | Âm: dùng điểm
    public string Type { get; set; } = string.Empty;  // Earn | Redeem
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Customer Customer { get; set; } = null!;
    public Order? Order { get; set; }
}
