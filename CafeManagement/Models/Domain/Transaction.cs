namespace CafeManagement.Models.Domain;

public class Transaction
{
    public int Id { get; set; }
    public int OrderId { get; set; }          // Đơn hàng gốc.
    public int PaymentMethodId { get; set; }  // Phương thức thanh toán.
    public decimal Amount { get; set; }       // Thành tiền thực thu.
    public decimal AmountTendered { get; set; } = 0;  // Tiền khách đưa.
    public decimal ChangeAmount { get; set; } = 0;    // Tiền thối lại cho khách.
    public DateTime TransactionDate { get; set; } = DateTime.Now; // Thời điểm ghi nhận.

    // Navigation phục vụ join để báo cáo/kết ca.
    public Order Order { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
}
