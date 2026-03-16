using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class TimekeepingService
{
    private readonly AppDbContext _context;

public TimekeepingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> CheckInOrOutAsync(string pinCode)
    {
        if (string.IsNullOrWhiteSpace(pinCode))
            return "Vui lòng nhập mã PIN";

        var user = await _context.Users
            .Include(x => x.Position)
            .FirstOrDefaultAsync(x =>
                x.PinCode == pinCode &&
                x.IsActive &&
                x.PinCode != null);

        if (user == null)
            return "Sai mã PIN hoặc tài khoản không hoạt động";

        var today = DateOnly.FromDateTime(DateTime.Now);

        var record = await _context.Timekeepings
            .FirstOrDefaultAsync(x =>
                x.UserId == user.Id &&
                x.Date == today);

        // CHECK IN
        if (record == null)
        {
            var now = DateTime.Now;

            record = new Timekeeping
            {
                UserId = user.Id,
                StoreId = user.StoreId ?? 1,
                Date = today,
                CheckInTime = now,
                IsLate = now.TimeOfDay > new TimeSpan(6, 15, 0)
            };

            _context.Timekeepings.Add(record);
            await _context.SaveChangesAsync();

            return $"Xin chào {user.FullName}. Check-in thành công lúc {now:HH:mm}";
        }

        // CHECK OUT
        if (record.CheckOutTime == null)
        {
            var now = DateTime.Now;

            record.CheckOutTime = now;

            var hours = (now - record.CheckInTime).TotalHours;

            record.TotalHours = Math.Round((decimal)hours, 2);

            if (user.Position != null)
                record.HourlyRateAtTime = user.Position.HourlyRate;

            await _context.SaveChangesAsync();

            return $"Check-out thành công lúc {now:HH:mm}. Tổng giờ làm: {record.TotalHours}";
        }

        return "Bạn đã chấm công xong hôm nay";
    }

}
