namespace CafeManagement.Models.ViewModels;

// Class này dùng để nhận dữ liệu từ Giao diện Thu Ngân (Frontend) gửi xuống Server
public class PosOrderRequestDto
{
    public int StoreId { get; set; }
    public string? UserId { get; set; } // Người thu ngân (Cashier)
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }  // Dành cho khách hàng chưa có SĐT
    public int PointsUsed { get; set; }
    public string OrderType { get; set; } = "EatIn"; // EatIn (Tại quán) hoặc TakeAway (Mang đi)
    
    // Mảng danh sách các món ăn khách đã chọn
    public List<PosOrderItemDto> OrderItems { get; set; } = new List<PosOrderItemDto>();
}

// Class con mô tả 1 dòng món ăn trong Giỏ hàng
public class PosOrderItemDto
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Lấy từ giao diện truyền xuống là giá bán
    public string? Note { get; set; }
}
