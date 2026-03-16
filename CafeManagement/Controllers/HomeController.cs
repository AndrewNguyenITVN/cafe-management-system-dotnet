using System.Diagnostics;
using CafeManagement.Data;
using CafeManagement.Models;
using CafeManagement.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public HomeController(
            ILogger<HomeController> logger,
            AppDbContext db,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API: Lấy danh sách Manager/Admin có PIN (hiện trên màn hình chọn)
        [HttpGet]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await (
                from u in _db.Users
                join ur in _db.UserRoles on u.Id equals ur.UserId
                join r in _db.Roles on ur.RoleId equals r.Id
                where u.IsActive
                      && (r.Name == "Manager" || r.Name == "Admin")
                      && u.PinCode != null && u.PinCode != ""
                select new { id = u.Id, name = u.FullName ?? u.UserName ?? "" }
            ).Distinct().ToListAsync();

            return Ok(managers);
        }

        // API: Xác thực PIN cho Manager/Admin, đăng nhập session rồi redirect Inventory
        [HttpPost]
        public async Task<IActionResult> VerifyManagerPin([FromBody] ManagerPinRequest request)
        {
            if (string.IsNullOrEmpty(request?.PinCode) || string.IsNullOrEmpty(request?.UserId))
                return BadRequest(new { success = false, message = "Thiếu thông tin PIN hoặc nhân viên." });

            var user = await _db.Users.FirstOrDefaultAsync(
                u => u.Id == request.UserId && u.PinCode == request.PinCode && u.IsActive);

            if (user == null)
                return BadRequest(new { success = false, message = "Mã PIN không đúng hoặc tài khoản đã bị khóa." });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Manager") && !roles.Contains("Admin"))
                return BadRequest(new { success = false, message = "Tài khoản không có quyền truy cập Kho hàng (yêu cầu Manager hoặc Admin)." });

            // Đăng nhập thật sự để tạo cookie session, isPersistent=false (đóng tab là hết)
            await _signInManager.SignInAsync(user, isPersistent: false);

            return Ok(new
            {
                success = true,
                userName = user.FullName ?? user.UserName,
                redirectUrl = "/Inventory/Index"
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public class ManagerPinRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string PinCode { get; set; } = string.Empty;
        }
    }
}
