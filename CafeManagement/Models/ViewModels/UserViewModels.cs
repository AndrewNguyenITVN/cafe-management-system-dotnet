using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Models.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Chức vụ")]
    public int? PositionId { get; set; }

    [Display(Name = "Chi nhánh")]
    public int? StoreId { get; set; }

    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã PIN phải đúng 6 chữ số")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã PIN chỉ gồm 6 chữ số")]
    [Display(Name = "Mã PIN (6 số)")]
    public string? PinCode { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "Staff";
}

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Chức vụ")]
    public int? PositionId { get; set; }

    [Display(Name = "Chi nhánh")]
    public int? StoreId { get; set; }

    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã PIN phải đúng 6 chữ số")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã PIN chỉ gồm 6 chữ số")]
    [Display(Name = "Mã PIN (6 số)")]
    public string? PinCode { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "Staff";

    [Display(Name = "Đang hoạt động")]
    public bool IsActive { get; set; } = true;
}
