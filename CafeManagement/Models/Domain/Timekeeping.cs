namespace CafeManagement.Models.Domain;

public class Timekeeping
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public decimal? TotalHours { get; set; }
    public decimal? HourlyRateAtTime { get; set; }  // Snapshot lương tại thời điểm check-out
    public bool IsLate { get; set; } = false;

    public AppUser User { get; set; } = null!;
    public Store Store { get; set; } = null!;
}
