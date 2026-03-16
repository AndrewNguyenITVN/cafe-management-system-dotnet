using CafeManagement.Models.Domain;
using CafeManagement.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CafeManagement.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser>   _userManager;

    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager   = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            return Redirect(await GetDefaultRedirectAsync(user));
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return Redirect(await GetDefaultRedirectAsync(user));
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/");
    }

    public IActionResult AccessDenied() => View();

    /// <summary>
    /// Trả về URL mặc định sau khi đăng nhập theo role:
    ///   Admin   → /Dashboard/Index
    ///   Manager → /Home/Index  (landing page, vào Inventory bằng PIN)
    ///   Staff   → /Pos/Index
    /// </summary>
    private async Task<string> GetDefaultRedirectAsync(AppUser? user)
    {
        if (user == null) return "/Account/Login";
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase)))
            return "/Dashboard/Index";
        if (roles.Any(r => string.Equals(r, "Manager", StringComparison.OrdinalIgnoreCase)))
            return "/Home/Index";
        return "/Pos/Index";
    }
}
