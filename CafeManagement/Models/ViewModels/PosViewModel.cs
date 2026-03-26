namespace CafeManagement.Models.ViewModels;

// DTO nhận payload tạo đơn từ POS.
public class PosOrderRequestDto
{
    public int StoreId { get; set; }
    public string? UserId { get; set; } // Cashier thao tác đơn hàng.
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }  // Dành cho khách hàng chưa có SĐT
    public int PointsUsed { get; set; }
    public string OrderType { get; set; } = "EatIn"; // EatIn (Tại quán) hoặc TakeAway (Mang đi)

    /// <summary>Phương thức thanh toán được chọn ở POS.</summary>
    public int PaymentMethodId { get; set; }
    /// <summary>Tiền khách đưa; dùng để tính tiền thối cho giao dịch tiền mặt.</summary>
    public decimal AmountTendered { get; set; }

    // Danh sách món cần tạo vào OrderDetails.
    public List<PosOrderItemDto> OrderItems { get; set; } = new List<PosOrderItemDto>();
}

// DTO cho từng dòng món trong giỏ hàng.
public class PosOrderItemDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Giá bán tại thời điểm checkout.
    public string? Note { get; set; }
}
