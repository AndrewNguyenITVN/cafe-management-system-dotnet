using CafeManagement.Data;
using CafeManagement.Models.Domain;

namespace CafeManagement.Services;

public class TransactionService
{
    private readonly AppDbContext _db;
    public TransactionService(AppDbContext db) => _db = db;

    /// <summary>
    /// Ghi bản ghi thanh toán cho 1 order (phục vụ đối soát/kết ca).
    /// amountTendered = tiền khách đưa; chuyển khoản thường bằng amount.
    /// </summary>
    public async Task RecordPaymentAsync(
        int orderId,
        int paymentMethodId,
        decimal amount,
        decimal amountTendered)
    {
        // Tạo giao dịch: amount là thành tiền, change = tiền thối.
        var tx = new Transaction
        {
            OrderId = orderId,
            PaymentMethodId = paymentMethodId,
            Amount = amount,
            AmountTendered = amountTendered,
            ChangeAmount = amountTendered - amount,    // tiền thối
            TransactionDate = DateTime.Now
        };

        // Lưu ngay vì cần dữ liệu transaction cho các nghiệp vụ sau (vd: kết ca).
        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync();
    }
}