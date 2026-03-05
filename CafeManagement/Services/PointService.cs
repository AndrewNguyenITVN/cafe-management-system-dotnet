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
    public Task ProcessOrderPointsAsync(int orderId, int pointsUsed)
    {
        // TODO: TV5 implement
        throw new NotImplementedException("TV5: PointService.ProcessOrderPointsAsync");
    }
}
