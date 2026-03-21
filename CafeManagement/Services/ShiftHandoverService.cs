using CafeManagement.Data;
using CafeManagement.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class ShiftHandoverService
{
    private readonly AppDbContext _db;

    public ShiftHandoverService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ShiftHandoverPreviewDto?> GetPreviewAsync(int storeId, int shiftId, DateOnly date)
    {
        var store = await _db.Stores.FindAsync(storeId);
        var shift = await _db.Shifts.FindAsync(shiftId);
        if (store == null || shift == null)
        {
            return null;
        }

        var (from, to) = GetShiftDateRange(shift, date);

        // Xác định PaymentMethod cho "Tiền mặt"
        var cashMethod = await _db.PaymentMethods
            .Where(p => p.IsActive)
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync(p => EF.Functions.Like(p.MethodName, "%tiền mặt%"));

        if (cashMethod == null)
        {
            return new ShiftHandoverPreviewDto
            {
                StoreId = storeId,
                StoreName = store.Name,
                ShiftId = shiftId,
                ShiftName = shift.ShiftName,
                Date = date,
                TotalRevenue = 0,
                TotalCash = 0
            };
        }

        // Doanh thu ca theo Orders.FinalAmount
        var totalRevenue = await _db.Orders
            .Where(o => o.StoreId == storeId
                        && o.OrderDate >= from
                        && o.OrderDate < to)
            .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;

        // Tổng tiền mặt theo Transactions (join với Orders để lọc Store)
        var totalCash = await _db.Transactions
            .Where(t => t.PaymentMethodId == cashMethod.Id
                        && t.TransactionDate >= from
                        && t.TransactionDate < to
                        && t.Order.StoreId == storeId)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return new ShiftHandoverPreviewDto
        {
            StoreId = storeId,
            StoreName = store.Name,
            ShiftId = shiftId,
            ShiftName = shift.ShiftName,
            Date = date,
            TotalRevenue = totalRevenue,
            TotalCash = totalCash
        };
    }

    public async Task<ShiftHandoverResultDto> CloseShiftAsync(int storeId, int shiftId, DateOnly date,
        decimal openingCash, decimal actualCashCounted, string? note, string? userId)
    {
        var preview = await GetPreviewAsync(storeId, shiftId, date);
        if (preview == null)
        {
            return new ShiftHandoverResultDto
            {
                Success = false,
                Message = "Không tìm thấy chi nhánh hoặc ca làm việc."
            };
        }

        var expectedCash = openingCash + preview.TotalCash;
        var difference = actualCashCounted - expectedCash;

        var handover = new ShiftHandover
        {
            StoreId = storeId,
            ShiftId = shiftId,
            HandoverDate = date,
            OpeningCash = openingCash,
            ExpectedCash = expectedCash,
            ActualCashCounted = actualCashCounted,
            Difference = difference,
            Note = note,
            ConfirmedByUserId = userId
        };

        _db.ShiftHandovers.Add(handover);
        await _db.SaveChangesAsync();

        return new ShiftHandoverResultDto
        {
            Success = true,
            StoreId = storeId,
            ShiftId = shiftId,
            Date = date,
            OpeningCash = openingCash,
            TotalCash = preview.TotalCash,
            ExpectedCash = expectedCash,
            ActualCashCounted = actualCashCounted,
            Difference = difference
        };
    }

    private static (DateTime from, DateTime to) GetShiftDateRange(Shift shift, DateOnly date)
    {
        var from = date.ToDateTime(shift.StartTime);
        // Nếu EndTime <= StartTime → ca qua đêm, kết thúc vào ngày hôm sau
        var endDate = shift.EndTime <= shift.StartTime ? date.AddDays(1) : date;
        var to = endDate.ToDateTime(shift.EndTime);
        return (from, to);
    }
}

public class ShiftHandoverPreviewDto
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCash { get; set; }
}

public class ShiftHandoverResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public int StoreId { get; set; }
    public int ShiftId { get; set; }
    public DateOnly Date { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal TotalCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ActualCashCounted { get; set; }
    public decimal Difference { get; set; }
}

