using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Models.ViewModels;

public class JobPositionViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên chức vụ")]
    [Display(Name = "Tên chức vụ")]
    public string PositionName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mức lương")]
    [Range(1000, 10000000, ErrorMessage = "Mức lương phải từ 1,000 VNĐ/giờ trở lên")]
    [Display(Name = "Mức lương/giờ (VNĐ)")]
    public decimal HourlyRate { get; set; }

    [Display(Name = "Đang dùng")]
    public bool IsActive { get; set; } = true;
}
