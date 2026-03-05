namespace CafeManagement.Models.Domain;

public class JobPosition
{
    public int Id { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }   // Mức lương/giờ hiện tại
    public bool IsActive { get; set; } = true;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
