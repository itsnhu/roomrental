using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("DichVu")]
    public class DichVu
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200, ErrorMessage = "Tên dịch vụ không được vượt quá 200 ký tự")]
        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        // Regex: Chỉ cho phép chữ cái (kể cả có dấu) và khoảng trắng. Không có số, không ký tự đặc biệt.
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên dịch vụ không được chứa số hoặc ký tự đặc biệt")]
        public string? TenDichVu { get; set; }

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(1, long.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public long? Gia { get; set; }
    }
}