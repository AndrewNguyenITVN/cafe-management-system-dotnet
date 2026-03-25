using CafeManagement.Data;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin")]
public class ShiftHandoverController : Controller
{
    private readonly StoreService _storeService;
    private readonly AppDbContext _db;

    public ShiftHandoverController(StoreService storeService, AppDbContext db)
    {
        _storeService = storeService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date, int? storeId)
    {
        // Không truyền ngày thì mặc định xem báo cáo của hôm nay.
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        // Query danh sách kết ca và load đủ dữ liệu để hiển thị tên chi nhánh/ca/người xác nhận.
        var query = _db.ShiftHandovers
            .Include(h => h.Store)
            .Include(h => h.Shift)
            .Include(h => h.ConfirmedByUser)
            .Where(h => h.HandoverDate == targetDate);

        // Chỉ áp dụng lọc chi nhánh khi có storeId.
        if (storeId.HasValue)
        {
            query = query.Where(h => h.StoreId == storeId.Value);
        }

        // Sắp theo chi nhánh rồi theo ca để dễ rà soát.
        var list = await query
            .OrderBy(h => h.StoreId)
            .ThenBy(h => h.ShiftId)
            .ToListAsync();

        // Trả dữ liệu filter để view giữ trạng thái form.
        ViewBag.Date = targetDate;
        ViewBag.Stores = await _storeService.GetActiveAsync();
        ViewBag.StoreId = storeId;

        return View(list);
    }
}

