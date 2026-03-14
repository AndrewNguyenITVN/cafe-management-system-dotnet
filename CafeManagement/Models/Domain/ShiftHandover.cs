namespace CafeManagement.Models.Domain;

public class ShiftHandover
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ShiftId { get; set; }
    public DateOnly HandoverDate { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ActualCashCounted { get; set; }
    public decimal Difference { get; set; }
    public string? Note { get; set; }
    public DateTime ConfirmedAt { get; set; } = DateTime.Now;
    public string? ConfirmedByUserId { get; set; }

    public Store Store { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
    public AppUser? ConfirmedByUser { get; set; }
}

