using CafeManagement.Data;
using CafeManagement.Hubs;
using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafeManagement.Controllers;

[AllowAnonymous]
public class PosController : Controller
{
    // Dependency Injection

    private readonly CategoryService _categoryService;
    private readonly MenuItemService _menuItemService;
    private readonly IOrderService _orderService;  
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly AppDbContext _db;
    private readonly ShiftHandoverService _shiftHandoverService;

    public PosController(
        CategoryService categoryService,
        MenuItemService menuItemService,
        IOrderService orderService,
        IHubContext<OrderHub> hubContext,
        AppDbContext db,
        ShiftHandoverService shiftHandoverService)
    {
        _categoryService = categoryService;
        _menuItemService = menuItemService;
        _orderService = orderService;      
        _hubContext = hubContext;
        _db = db;
        _shiftHandoverService = shiftHandoverService;
    }


    // Giao diện POS
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // Giao diện KDS Bếp
    [HttpGet]
    public IActionResult Kds()
    {
        return View();
    }

    // Giao diện Customer Display
    [HttpGet]
    public IActionResult CustomerDisplay()
    {
        return View();
    }


    // API: Lấy danh sách Danh mục
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetActiveAsync();
        var result = categories.Select(c => new {
            id = c.Id,
            name = c.Name
        });

        return Ok(result);
    }


    // API: Lấy danh sách Món ăn
    [HttpGet]
    public async Task<IActionResult> GetMenuItems()
    {
        var items = await _menuItemService.GetAllWithCategoryAsync();
        var result = items
            .Where(m => m.IsActive)
            .Select(m => new {
                id = m.Id,
                name = m.Name,
                price = m.BasePrice,
                categoryId = m.CategoryId,
                imageUrl = m.ImageUrl
            });

        return Ok(result);
    }

    // Lấy danh sách nhân viên đang active để hiển thị trên màn PIN
    [HttpGet]
    public async Task<IActionResult> GetCashiers()
    {
        var users = await _db.Users
            .Where(u => u.IsActive)
            .Select(u => new {
                id = u.Id,
                name = u.FullName ?? u.UserName ?? "",
                storeId = u.StoreId
            })
            .ToListAsync();

        return Ok(users);
    }

    // API: Lấy danh sách chi nhánh đang hoạt động (dùng cho KDS / Customer Display)
    [HttpGet]
    public async Task<IActionResult> GetStores()
    {
        var stores = await _db.Stores
            .Where(s => s.IsActive)
            .Select(s => new { id = s.Id, name = s.Name })
            .ToListAsync();
        return Ok(stores);
    }

    // API: Lấy danh sách phương thức thanh toán (cho dropdown POS)
    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var list = await _db.PaymentMethods
            .Where(p => p.IsActive)
            .Select(p => new { id = p.Id, name = p.MethodName })
            .ToListAsync();
        return Ok(list);
    }

    // API: Preview kết ca cho POS (dựa trên ca + ngày của chi nhánh hiện tại)
    [HttpGet]
    public async Task<IActionResult> GetShiftHandoverPreview(int shiftId, DateOnly date)
    {
        // Đọc từ POS session (thay vì User claims)
        var cashierId = HttpContext.Session.GetString("PosCashierId");
        var storeId = HttpContext.Session.GetInt32("PosStoreId");

        if (string.IsNullOrEmpty(cashierId) || !storeId.HasValue || storeId.Value == 0)
        {
            return Unauthorized(new { success = false, message = "Chưa đăng nhập POS (vui lòng nhập PIN)." });
        }

        var dateOnly = date;
        var preview = await _shiftHandoverService.GetPreviewAsync(storeId.Value, shiftId, dateOnly);
        if (preview == null)
        {
            return BadRequest(new { success = false, message = "Không tìm thấy chi nhánh hoặc ca làm việc." });
        }

        return Ok(new
        {
            success = true,
            storeId = preview.StoreId,
            storeName = preview.StoreName,
            shiftId = preview.ShiftId,
            shiftName = preview.ShiftName,
            date = preview.Date,
            totalRevenue = preview.TotalRevenue,
            totalCash = preview.TotalCash
        });
    }

    public class PosCloseShiftRequest
    {
        public int ShiftId { get; set; }
        public DateOnly Date { get; set; }
        public decimal OpeningCash { get; set; }
        public decimal ActualCashCounted { get; set; }
        public string? Note { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> SubmitShiftHandover([FromBody] PosCloseShiftRequest model)
    {
        // Đọc từ POS session (thay vì User claims)
        var cashierId = HttpContext.Session.GetString("PosCashierId");
        var storeId = HttpContext.Session.GetInt32("PosStoreId");

        if (string.IsNullOrEmpty(cashierId) || !storeId.HasValue || storeId.Value == 0)
        {
            return Unauthorized(new { success = false, message = "Chưa đăng nhập POS (vui lòng nhập PIN)." });
        }

        var result = await _shiftHandoverService.CloseShiftAsync(
            storeId.Value,
            model.ShiftId,
            model.Date,
            model.OpeningCash,
            model.ActualCashCounted,
            model.Note,
            cashierId); // userId từ session

        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        return Ok(new
        {
            success = true,
            storeId = result.StoreId,
            shiftId = result.ShiftId,
            date = result.Date,
            openingCash = result.OpeningCash,
            totalCash = result.TotalCash,
            expectedCash = result.ExpectedCash,
            actualCash = result.ActualCashCounted,
            difference = result.Difference
        });
    }

    // API: Tạo đơn hàng
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] PosOrderRequestDto request)
    {
        if (request == null || !request.OrderItems.Any())
        {
            return BadRequest(new { success = false, message = "Giỏ hàng rỗng" });
        }

        // Lấy UserId từ POS session (thay vì User claims)
        if (string.IsNullOrEmpty(request.UserId))
        {
            request.UserId = HttpContext.Session.GetString("PosCashierId");
        }

        if (string.IsNullOrEmpty(request.UserId))
        {
            return Unauthorized(new { success = false, message = "Chưa đăng nhập POS (vui lòng nhập PIN)." });
        }

        // GỌI SERVICE THẬT
        var result = await _orderService.CreateOrderAsync(request);

        if (result.Success)
        {
            // Phát sóng SignalR
            string storeGroup = $"Store_{request.StoreId}";
            await _hubContext.Clients.Group(storeGroup).SendAsync("NewOrderReceived", new {
                orderId = result.OrderId,
                queueNumber = result.QueueNumber,
                itemsCount = request.OrderItems.Count,
                items = result.Items  // Gửi danh sách món sang KDS
            });

            return Ok(new {
                success = true,
                orderId = result.OrderId,
                queueNumber = result.QueueNumber
            });
        }
        else
        {
            return BadRequest(new { success = false, message = result.Message });
        }
    }

    // API: Cập nhật trạng thái đơn hàng (Dành cho KDS)
    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, int status)
    {
        var result = await _orderService.UpdateOrderStatusAsync(orderId, status);
        if (result.Success)
        {
            // Trạng thái 2 = Ready (Xong đơn) -> Bắn tín hiệu sang màn hình Customer Display
            if (status == 2)
            {
                string storeGroup = $"Store_{result.StoreId}";
                await _hubContext.Clients.Group(storeGroup).SendAsync("OrderCompleted", new {
                    queueNumber = result.QueueNumber
                });
            }

            return Ok(new { success = true });
        }
        
        return BadRequest(new { success = false, message = "Không thể cập nhật trạng thái đơn hàng." });
    }

    // API: Lấy danh sách đơn hàng đang hoạt động (Pending + Processing) theo Chi nhánh
    [HttpGet]
    public IActionResult GetActiveOrders(int storeId)
    {
        var today = DateTime.Today;

        var activeOrders = _db.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.MenuItem)
            .Where(o => o.StoreId == storeId
                     && o.OrderDate.Date == today
                     && (o.Status == 0 || o.Status == 1)) // 0 = Pending, 1 = Processing
            .OrderBy(o => o.OrderDate)
            .Select(o => new {
                orderId = o.Id,
                queueNumber = o.QueueNumber,
                status = o.Status,
                items = o.OrderDetails.Select(d => new {
                    name = d.MenuItem != null ? d.MenuItem.Name : "?",
                    quantity = d.Quantity,
                    note = d.Note
                }).ToList()
            })
            .ToList();

        return Ok(activeOrders);
    }

    // API: Hủy đơn hàng (Dành cho KDS khi bếp không thể thực hiện)
    [HttpPost]
    public async Task<IActionResult> CancelOrder(int orderId, string reason = "")
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null)
            return BadRequest(new { success = false, message = "Không tìm thấy đơn hàng." });

        if (order.Status >= 2)
            return BadRequest(new { success = false, message = "Đơn hàng đã hoàn thành, không thể hủy." });

        order.Status = -1; // -1 = Cancelled

        await _db.SaveChangesAsync();

        // Bắn tín hiệu SignalR báo hủy đơn để Customer Display và POS có thể tự xóa / nghe
        string storeGroup = $"Store_{order.StoreId}";
        await _hubContext.Clients.Group(storeGroup).SendAsync("OrderCancelled", new {
            queueNumber = order.QueueNumber,
            reason = string.IsNullOrWhiteSpace(reason) ? "Không có lý do" : reason
        });

        return Ok(new { success = true, storeId = order.StoreId, queueNumber = order.QueueNumber });
    }

    // API: Lấy thông tin khách hàng từ SĐT
    [HttpGet]
    public IActionResult GetCustomerInfo(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) 
            return BadRequest();

        var customer = _db.Customers.FirstOrDefault(c => c.Phone == phone);
        if (customer != null)
        {
            return Ok(new { 
                success = true, 
                name = customer.FullName, 
                points = customer.TotalPoints 
            });
        }
        
        return Ok(new { success = false });
    }

    // public class PinRequest
    // {
    //     public string PinCode { get; set; } = string.Empty;
    // }

    // // API: Xác thực mã PIN của nhân viên
    // [HttpPost]
    // public IActionResult VerifyPin([FromBody] PinRequest request)
    // {
    //     var pinCode = request?.PinCode;
    //     if (string.IsNullOrEmpty(pinCode)) return BadRequest(new { success = false, message = "Mã PIN rỗng!" });

    //     // Trong các hệ thống POS, nhân viên dùng chung 1 máy và khóa màn hình bằng mã PIN.
    //     // Ta sẽ dò trực tiếp người dùng có mã PIN này.
    //     var cashier = _db.Users.FirstOrDefault(u => u.PinCode == pinCode);
        
    //     if (cashier != null && cashier.IsActive)
    //     {
    //         return Ok(new { 
    //             success = true, 
    //             userId = cashier.Id,
    //             cashierName = cashier.FullName ?? cashier.UserName,
    //             storeId = cashier.StoreId ?? 1 // Lấy ID Chi nhánh của Nhân viên này, mặc định 1 nếu chưa gán
    //         });
    //     }

    //     return BadRequest(new { success = false, message = "Mã PIN không đúng hoặc đã bị khóa!" });
    // }
    public class PinRequest
    {
        public string PinCode { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // THÊM
    }

    [HttpPost]
    public IActionResult VerifyPin([FromBody] PinRequest request)
    {
        var pinCode = request?.PinCode;
        var userId  = request?.UserId;

        if (string.IsNullOrEmpty(pinCode) || string.IsNullOrEmpty(userId))
            return BadRequest(new { success = false, message = "Thiếu thông tin nhân viên hoặc PIN!" });

        // Chỉ tìm đúng nhân viên đã chọn
        var cashier = _db.Users.FirstOrDefault(u => u.Id == userId && u.PinCode == pinCode);

        if (cashier != null && cashier.IsActive)
        {
            // Lưu POS session (cookie POS riêng)
            HttpContext.Session.SetString("PosCashierId", cashier.Id);
            HttpContext.Session.SetInt32("PosStoreId", cashier.StoreId ?? 0);
            HttpContext.Session.SetString("PosCashierName", cashier.FullName ?? cashier.UserName ?? "");

            return Ok(new {
                success = true,
                userId = cashier.Id,
                cashierName = cashier.FullName ?? cashier.UserName,
                storeId = cashier.StoreId ?? 1
            });
        }

        return BadRequest(new { success = false, message = "Mã PIN không đúng hoặc nhân viên đã bị khóa!" });
    }
}
