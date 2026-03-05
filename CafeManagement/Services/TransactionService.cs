using CafeManagement.Data;

namespace CafeManagement.Services;

/// <summary>TV5 implement class này.</summary>
public class TransactionService
{
    private readonly AppDbContext _db;
    public TransactionService(AppDbContext db) => _db = db;

    /// <summary>
    /// Ghi nhận giao dịch thanh toán vào bảng Transactions.
    /// amountTendered = tiền khách đưa (chỉ dùng cho tiền mặt, truyền 0 nếu chuyển khoản).
    /// </summary>
    public Task RecordPaymentAsync(int orderId, int paymentMethodId, decimal amount, decimal amountTendered)
    {
        // TODO: TV5 implement
        throw new NotImplementedException("TV5: TransactionService.RecordPaymentAsync");
    }
}
