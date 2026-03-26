namespace CafeManagement.Models.Domain;

public class PaymentMethod
{
    public int Id { get; set; }
    public string MethodName { get; set; } = string.Empty; // Ví dụ: Tiền mặt, Chuyển khoản.
    public bool IsActive { get; set; } = true; // Ẩn/hiện phương thức ở POS.

    // Danh sách giao dịch đã dùng phương thức này.
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
