using CafeManagement.Data;
using CafeManagement.Models.Domain;

namespace CafeManagement.Services;

public class TransactionService
{
    private readonly AppDbContext _db;
    public TransactionService(AppDbContext db) => _db = db;

    /// <summary>
    /// Ghi nhận giao dịch thanh toán vào bảng Transactions.
    /// amountTendered = tiền khách đưa (tiền mặt). Nếu chuyển khoản có thể = amount.
    /// </summary>
    public async Task RecordPaymentAsync(
        int orderId,
        int paymentMethodId,
        decimal amount,
        decimal amountTendered)
    {
        var tx = new Transaction
        {
            OrderId = orderId,
            PaymentMethodId = paymentMethodId,
            Amount = amount,
            AmountTendered = amountTendered,
            ChangeAmount = amountTendered - amount,    // tiền thối
            TransactionDate = DateTime.Now
        };

        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
    }
}