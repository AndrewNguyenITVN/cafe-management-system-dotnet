namespace CafeManagement.Models.Domain;

public class Schedule
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int StoreId { get; set; }

    public int ShiftId { get; set; }

    public DateTime WorkDate { get; set; }

    // Navigation properties (nullable để tránh ModelState lỗi)
    public AppUser? User { get; set; }

    public Store? Store { get; set; }

    public Shift? Shift { get; set; }
}