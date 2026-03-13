namespace CafeManagement.Services;

using CafeManagement.Models.ViewModels;

public interface IOrderService
{
    /// <summary>
    /// Tạo đơn hàng mới từ dữ liệu POS gửi lên.
    /// Trả về object chứa orderId và queueNumber.
    /// </summary>
    Task<OrderResultDto> CreateOrderAsync(PosOrderRequestDto request);

    /// <summary>
    /// Thay đổi trạng thái đơn hàng (0: Pending -> 1: Processing -> 2: Ready)
    /// </summary>
    Task<UpdateOrderStatusResultDto> UpdateOrderStatusAsync(int orderId, int newStatus);
}

public class UpdateOrderStatusResultDto
{
    public bool Success { get; set; }
    public int StoreId { get; set; }
    public int QueueNumber { get; set; }
}

public class OrderResultDto
{
    public bool Success { get; set; }
    public int OrderId { get; set; }
    public int QueueNumber { get; set; }
    public string? Message { get; set; }   // Thông báo lỗi (nếu có)
    
    public List<OrderResultItemDto> Items { get; set; } = new();
}

public class OrderResultItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Note { get; set; }
}
