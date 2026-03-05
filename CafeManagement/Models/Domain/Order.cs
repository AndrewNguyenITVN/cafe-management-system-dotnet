namespace CafeManagement.Models.Domain;

public class Order
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string? UserId { get; set; }
    public int? CustomerId { get; set; }
    public int QueueNumber { get; set; }       // Số thứ tự trong ngày, per Store (reset mỗi ngày)
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string OrderType { get; set; } = "EatIn";  // EatIn | TakeAway
    public decimal TotalAmount { get; set; }
    public int PointsUsed { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal FinalAmount { get; set; }
    public int Status { get; set; } = 0;
    // 0: Pending | 1: Processing | 2: Ready | 3: Completed

    public Store Store { get; set; } = null!;
    public AppUser? User { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
}
