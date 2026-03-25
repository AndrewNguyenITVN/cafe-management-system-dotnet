namespace CafeManagement.Models.Domain;

public class Shift
{
    public int Id { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; } // Giờ bắt đầu ca.
    public TimeOnly EndTime { get; set; }   // Giờ kết thúc ca.

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
