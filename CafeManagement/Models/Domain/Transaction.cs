namespace CafeManagement.Models.Domain;

public class Transaction
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountTendered { get; set; } = 0;  // Tiền khách đưa (tiền mặt)
    public decimal ChangeAmount { get; set; } = 0;    // Tiền thối
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public Order Order { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
}
