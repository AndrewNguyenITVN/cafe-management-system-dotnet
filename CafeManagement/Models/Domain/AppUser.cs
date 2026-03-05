using Microsoft.AspNetCore.Identity;

namespace CafeManagement.Models.Domain;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? PositionId { get; set; }
    public int? StoreId { get; set; }
    public string? PinCode { get; set; }
    public bool IsActive { get; set; } = true;

    public JobPosition? Position { get; set; }
    public Store? Store { get; set; }
}
