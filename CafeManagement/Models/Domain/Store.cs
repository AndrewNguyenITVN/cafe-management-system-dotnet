namespace CafeManagement.Models.Domain;

public class Store
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Timekeeping> Timekeepings { get; set; } = new List<Timekeeping>();
}
