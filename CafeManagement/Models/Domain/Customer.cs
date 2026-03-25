namespace CafeManagement.Models.Domain;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int TotalPoints { get; set; } = 0; // Số điểm loyalty hiện tại của khách.
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation phục vụ tra cứu lịch sử mua và lịch sử điểm.
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
}