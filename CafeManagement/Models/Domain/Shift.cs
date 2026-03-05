namespace CafeManagement.Models.Domain;

public class Shift
{
    public int Id { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
