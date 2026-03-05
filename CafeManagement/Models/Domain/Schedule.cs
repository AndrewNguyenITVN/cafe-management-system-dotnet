namespace CafeManagement.Models.Domain;

public class Schedule
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public int ShiftId { get; set; }
    public DateOnly WorkDate { get; set; }

    public AppUser User { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
}
