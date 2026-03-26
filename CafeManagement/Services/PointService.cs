using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

/// <summary>Xử lý loyalty theo đơn: trừ điểm dùng + cộng điểm mới + ghi lịch sử.</summary>
public class PointService
{
    private readonly AppDbContext _db;
    public PointService(AppDbContext db) => _db = db;

    public async Task ProcessOrderPointsAsync(int orderId, int pointsUsed)
    {
        // Lấy order kèm khách hàng để thao tác trực tiếp trên số điểm hiện tại.
        var order = await _db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null || order.Customer == null)
            return; // Không có order hoặc không gắn khách => bỏ qua

        var customer = order.Customer;

        // B1: Tính điểm cộng mới (1% FinalAmount, làm tròn xuống).
        int pointsEarned = (int)Math.Floor(order.FinalAmount * 0.01m);

        // B2: Xử lý Redeem (trừ điểm khách muốn dùng).
        int pointsActuallyUsed = 0;

        if (pointsUsed > 0)
        {
            // Không cho TotalPoints âm.
            if (customer.TotalPoints >= pointsUsed)
            {
                customer.TotalPoints -= pointsUsed;
                pointsActuallyUsed = pointsUsed;
            }
            else
            {
                // Không đủ điểm thì trừ hết phần đang có.
                pointsActuallyUsed = customer.TotalPoints;
                customer.TotalPoints = 0;
            }

            if (pointsActuallyUsed > 0)
            {
                _db.PointHistories.Add(new PointHistory
                {
                    CustomerId = customer.Id,
                    OrderId = order.Id,
                    PointsChanged = -pointsActuallyUsed,
                    Type = "Redeem",
                    CreatedAt = DateTime.Now
                });
            }
        }

        // B3: Cộng điểm Earn sau khi đơn hoàn tất.
        if (pointsEarned > 0)
        {
            customer.TotalPoints += pointsEarned;

            _db.PointHistories.Add(new PointHistory
            {
                CustomerId = customer.Id,
                OrderId = order.Id,
                PointsChanged = pointsEarned,
                Type = "Earn",
                CreatedAt = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        // Log nhanh để theo dõi khi debug local.
        Console.WriteLine(
            $"[PointService] {customer.FullName}: Redeem {pointsActuallyUsed}, Earn {pointsEarned} -> Total {customer.TotalPoints}");
    }
}