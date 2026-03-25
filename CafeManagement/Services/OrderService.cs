using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly InventoryService _inventoryService;
    private readonly PointService _pointService;
    private readonly TransactionService _transactionService;

    // Nhận các service phụ trách: DB, kho, loyalty, transaction.
    public OrderService(AppDbContext db, InventoryService inventoryService, PointService pointService, TransactionService transactionService)
    {
        _db = db;
        _inventoryService = inventoryService;
        _pointService = pointService;
        _transactionService = transactionService;
    }

    public async Task<OrderResultDto> CreateOrderAsync(PosOrderRequestDto request)
    {
        // BƯỚC 1: Mở DB transaction để giữ dữ liệu đơn hàng nhất quán.
        await using var transaction = await _db.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            // BƯỚC 2: Gắn khách theo SĐT; nếu chưa có thì tạo mới.
            int? customerId = null;

            if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
            {
                // Tìm khách hàng theo số điện thoại
                var customer = await _db.Customers
                    .FirstOrDefaultAsync(c => c.Phone == request.CustomerPhone);

                if (customer == null)
                {
                    // Khách hàng mới → tự động tạo
                    customer = new Customer
                    {
                        Phone = request.CustomerPhone,
                        FullName = request.CustomerName ?? "Khách",  // ?? = nếu null thì dùng "Khách"
                        TotalPoints = 0,
                        CreatedAt = DateTime.Now
                    };
                    _db.Customers.Add(customer);
                    await _db.SaveChangesAsync();  // Lưu để có Id
                }

                customerId = customer.Id;
            }


            // BƯỚC 3: Sinh số thứ tự theo ngày/chi nhánh, tránh trùng số.
            var today = DateTime.Today;

            // Lấy số QueueNumber lớn nhất trong ngày hôm nay của chi nhánh này
            var maxQueue = await _db.Orders
                .Where(o => o.StoreId == request.StoreId
                         && o.OrderDate.Date == today)
                .MaxAsync(o => (int?)o.QueueNumber) ?? 0;
            //         ↑ (int?) → nếu chưa có đơn nào → trả về null → ?? 0 = trả về 0

            int nextQueue = maxQueue + 1;


            // BƯỚC 4: Tính tổng tiền, giảm giá điểm, và thành tiền cuối.
            decimal totalAmount = 0;

            // Duyệt từng món trong giỏ hàng, cộng dồn
            foreach (var item in request.OrderItems)
            {
                totalAmount += item.UnitPrice * item.Quantity;
            }

            // Tính giảm giá từ điểm (1 điểm = 1 VNĐ theo đề bài)
            decimal discountAmount = request.PointsUsed;

            // Tổng thanh toán = Tổng tiền - Giảm giá (không âm)
            decimal finalAmount = Math.Max(totalAmount - discountAmount, 0);


            var order = new Order
            {
                StoreId = request.StoreId,
                UserId = request.UserId,
                CustomerId = customerId,
                QueueNumber = nextQueue,
                OrderDate = DateTime.Now,
                OrderType = request.OrderType,
                TotalAmount = totalAmount,
                PointsUsed = request.PointsUsed,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Status = 0   // 0 = Pending (Chờ pha chế)
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();  // Lưu để Order có Id


            // BƯỚC 5: Lưu chi tiết từng món trong đơn.
            var itemDtos = new List<OrderResultItemDto>(); // Lưu danh sách cho KDS

            foreach (var item in request.OrderItems)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.Id,       // Liên kết với Order vừa tạo
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Note = item.Note
                };

                _db.OrderDetails.Add(detail);

                // Lấy tên món từ Database để in ra KDS
                var menuItem = await _db.MenuItems.FindAsync(item.MenuItemId);
                itemDtos.Add(new OrderResultItemDto 
                {
                    Name = menuItem?.Name ?? "Món không xác định",
                    Quantity = item.Quantity,
                    Note = item.Note
                });
            }

            await _db.SaveChangesAsync();  // Lưu tất cả OrderDetails


            // BƯỚC 6: Commit transaction cho phần Order + OrderDetails.
            await transaction.CommitAsync();

            // BƯỚC 7: Chạy hậu xử lý: trừ kho, loyalty, và ghi giao dịch thanh toán.
            await _inventoryService.DeductStockAsync(order.Id);
            await _pointService.ProcessOrderPointsAsync(order.Id, order.PointsUsed);
            if (request.PaymentMethodId > 0)
            {
                // Nếu frontend chưa gửi tiền khách đưa, mặc định bằng thành tiền.
                decimal amountTendered = request.AmountTendered > 0 ? request.AmountTendered : order.FinalAmount;
                await _transactionService.RecordPaymentAsync(order.Id, request.PaymentMethodId, order.FinalAmount, amountTendered);
            }

            // Trả kết quả thành công
            return new OrderResultDto
            {
                Success = true,
                OrderId = order.Id,
                QueueNumber = nextQueue,
                Message = "Tạo đơn hàng thành công",
                Items = itemDtos // Truyền danh sách món về rải lên KDS
            };
        }
        catch (Exception ex)
        {
            // Rollback nếu có lỗi
            await transaction.RollbackAsync();

            return new OrderResultDto
            {
                Success = false,
                Message = $"Lỗi tạo đơn hàng: {ex.Message}"
            };
        }
    }

    public async Task<UpdateOrderStatusResultDto> UpdateOrderStatusAsync(int orderId, int newStatus)
    {
        var order = await _db.Orders.FindAsync(orderId);
        
        // Kiểm tra logic trạng thái: trạng thái mới phải lớn hơn trạng thái cũ
        if (order == null || order.Status >= newStatus) 
            return new UpdateOrderStatusResultDto { Success = false };

        // Cập nhật trạng thái
        order.Status = newStatus;
        await _db.SaveChangesAsync();

        return new UpdateOrderStatusResultDto 
        { 
            Success = true, 
            StoreId = order.StoreId, 
            QueueNumber = order.QueueNumber 
        };
    }
}
