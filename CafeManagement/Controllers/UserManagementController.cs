using CafeManagement.Models.ViewModels;
using CafeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CafeManagement.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class UserManagementController : Controller
{
    private readonly UserService _userService;
    public UserManagementController(UserService userService) => _userService = userService;

    public async Task<IActionResult> Index()
        => View(await _userService.GetAllWithDetailsAsync());

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new CreateUserViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        var result = await _userService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            await PopulateDropdownsAsync();
            return View(model);
        }

        TempData["Success"] = "Đã thêm nhân viên thành công.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userService.GetRolesAsync(user);
        await PopulateDropdownsAsync();

        return View(new EditUserViewModel
        {
            Id         = user.Id,
            FullName   = user.FullName,
            Email      = user.Email ?? string.Empty,
            PositionId = user.PositionId,
            StoreId    = user.StoreId,
            PinCode    = user.PinCode,
            Role       = roles.FirstOrDefault() ?? "Staff",
            IsActive   = user.IsActive
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync();
            return View(model);
        }

        await _userService.UpdateAsync(id, model);
        TempData["Success"] = "Đã cập nhật nhân viên thành công.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync()
    {
        var (positions, stores) = await _userService.GetDropdownDataAsync();
        ViewBag.Positions = new SelectList(positions, "Id", "PositionName");
        ViewBag.Stores    = new SelectList(stores,    "Id", "Name");
        ViewBag.Roles     = new SelectList(new[] { "Admin", "Manager", "Staff" });
    }
}
