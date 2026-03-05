using System.ComponentModel.DataAnnotations;

namespace CafeManagement.Models.ViewModels;

public class MenuItemViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên món")]
    [Display(Name = "Tên món")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá")]
    [Range(1000, 1000000, ErrorMessage = "Giá phải từ 1,000 đến 1,000,000 VNĐ")]
    [Display(Name = "Giá bán (VNĐ)")]
    public decimal BasePrice { get; set; }

    [Display(Name = "Hình ảnh (ảnh mới)")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImageUrl { get; set; }

    [Display(Name = "Đang bán")]
    public bool IsActive { get; set; } = true;
}
