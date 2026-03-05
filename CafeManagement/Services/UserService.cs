using CafeManagement.Data;
using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Services;

public class UserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext         _db;

    public UserService(UserManager<AppUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db          = db;
    }

    public Task<List<AppUser>> GetAllWithDetailsAsync()
        => _db.Users
            .Include(u => u.Position)
            .Include(u => u.Store)
            .OrderBy(u => u.FullName)
            .ToListAsync();

    public Task<AppUser?> GetByIdAsync(string id)
        => _userManager.FindByIdAsync(id);

    public Task<IList<string>> GetRolesAsync(AppUser user)
        => _userManager.GetRolesAsync(user);

    public async Task<IdentityResult> CreateAsync(CreateUserViewModel model)
    {
        var user = new AppUser
        {
            UserName       = model.Email,
            Email          = model.Email,
            FullName       = model.FullName,
            PositionId     = model.PositionId,
            StoreId        = model.StoreId,
            PinCode        = model.PinCode,
            IsActive       = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(user, model.Role);

        return result;
    }

    public async Task<bool> UpdateAsync(string id, EditUserViewModel model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        user.FullName   = model.FullName;
        user.PositionId = model.PositionId;
        user.StoreId    = model.StoreId;
        user.PinCode    = model.PinCode;
        user.IsActive   = model.IsActive;

        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, model.Role);

        return true;
    }

    public async Task<(List<JobPosition> positions, List<Store> stores)> GetDropdownDataAsync()
    {
        var positions = await _db.JobPositions
            .Where(p => p.IsActive).OrderBy(p => p.PositionName).ToListAsync();
        var stores = await _db.Stores
            .Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        return (positions, stores);
    }
}
