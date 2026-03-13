using CafeManagement.Data;

namespace CafeManagement.Services;

/// <summary>TV5 implement class này.</summary>
public class PointService
{
    private readonly AppDbContext _db;
    public PointService(AppDbContext db) => _db = db;

    /// <summary>
    /// Xử lý điểm sau khi đơn hàng hoàn tất:
    /// - Trừ pointsUsed khỏi Customer.TotalPoints
    /// - Cộng điểm mới = FLOOR(TotalAmount * 1%)
    /// - Ghi PointHistory (Redeem + Earn)
    /// </summary>
    public async Task ProcessOrderPointsAsync(int orderId, int pointsUsed)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null || order.CustomerId == null) return;

        var customer = await _db.Customers.FindAsync(order.CustomerId);
        if (customer == null) return;

        // 1. Trừ điểm đã sử dụng (Redeem)
        if (pointsUsed > 0 && customer.TotalPoints >= pointsUsed)
        {
            customer.TotalPoints -= pointsUsed;
        }

        // 2. Tích điểm mới (Earn) - Ví dụ 1% của số tiền sau giảm giá
        int pointsEarned = (int)Math.Floor(order.FinalAmount * 0.01m);
        if (pointsEarned > 0)
        {
            customer.TotalPoints += pointsEarned;
        }

        // (Thực tế của TV5 sẽ cần ghi Log vào bảng PointHistories ở đây nữa)

        await _db.SaveChangesAsync();
        Console.WriteLine($"[PointService] Khách {customer.FullName}: Dùng {pointsUsed} - Tích {pointsEarned} -> Tổng: {customer.TotalPoints}");
    }
}
