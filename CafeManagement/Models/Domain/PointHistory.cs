namespace CafeManagement.Models.Domain;

public class PointHistory
{
    public int Id { get; set; }
    public int CustomerId { get; set; }       // Khách hàng bị thay đổi điểm.
    public int? OrderId { get; set; }         // Đơn hàng phát sinh thay đổi điểm (nếu có).
    public int PointsChanged { get; set; }      // Dương: tích điểm | Âm: dùng điểm
    public string Type { get; set; } = string.Empty;  // Earn | Redeem
    public DateTime CreatedAt { get; set; } = DateTime.Now; // Thời điểm ghi lịch sử.

    public Customer Customer { get; set; } = null!;
    public Order? Order { get; set; }
}
