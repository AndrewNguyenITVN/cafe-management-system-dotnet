using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

/// <summary>TV5: xử lý tích / tiêu điểm + log lịch sử.</summary>
public class PointService
{
    private readonly AppDbContext _db;
    public PointService(AppDbContext db) => _db = db;

    public async Task ProcessOrderPointsAsync(int orderId, int pointsUsed)
    {
        // Lấy order kèm Customer
        var order = await _db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null || order.Customer == null)
            return; // Không có order hoặc không gắn khách => bỏ qua

        var customer = order.Customer;

        // 1. Xác định điểm được cộng (Earn)
        int pointsEarned = (int)Math.Floor(order.FinalAmount * 0.01m);

        // 2. Xử lý Redeem (dùng điểm)
        int pointsActuallyUsed = 0;

        if (pointsUsed > 0)
        {
            // Bảo vệ: không cho âm điểm
            if (customer.TotalPoints >= pointsUsed)
            {
                customer.TotalPoints -= pointsUsed;
                pointsActuallyUsed = pointsUsed;
            }
            else
            {
                // Không đủ điểm: chỉ trừ phần hợp lệ
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

        // 3. Xử lý Earn (tích điểm)
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
        Console.WriteLine(
            $"[PointService] {customer.FullName}: Redeem {pointsActuallyUsed}, Earn {pointsEarned} -> Total {customer.TotalPoints}");
    }
}