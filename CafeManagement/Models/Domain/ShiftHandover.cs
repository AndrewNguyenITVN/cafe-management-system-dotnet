namespace CafeManagement.Models.Domain;

public class ShiftHandover
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ShiftId { get; set; }
    public DateOnly HandoverDate { get; set; }
    public decimal OpeningCash { get; set; }       // Tiền đầu ca.
    public decimal ExpectedCash { get; set; }      // Tiền lý thuyết = OpeningCash + TotalCash.
    public decimal ActualCashCounted { get; set; } // Tiền thực tế nhân viên đếm.
    public decimal Difference { get; set; }        // Chênh lệch = Actual - Expected.
    public string? Note { get; set; }              // Lý do chênh lệch (nếu có).
    public DateTime ConfirmedAt { get; set; } = DateTime.Now; // Thời điểm chốt ca.
    public string? ConfirmedByUserId { get; set; } // Người xác nhận kết ca.

    // Navigation để hiển thị tên chi nhánh/ca/người xác nhận.
    public Store Store { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
    public AppUser? ConfirmedByUser { get; set; }
}

