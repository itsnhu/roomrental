using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("Phong")]
    public class Phong
    {
        [Key]
        [StringLength(10)]
        public string MaPhong { get; set; } // Sẽ được tự động tạo ở Controller

        [StringLength(10)]
        [Required(ErrorMessage = "Vui lòng chọn loại phòng")]
        public string? MaLoaiPhong { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Vui lòng chọn khu vực")]
        public string? MaKhuVuc { get; set; }

        [StringLength(200)]
        [Required(ErrorMessage = "Tên phòng không được để trống")]
        [RegularExpression(@"^[a-zA-Z0-9À-ỹ\s]+$", ErrorMessage = "Tên phòng không được chứa ký tự đặc biệt")]
        public string? TenPhong { get; set; }

        [StringLength(200)]
        [Required(ErrorMessage = "Dãy không được để trống")]
        [RegularExpression(@"^[a-zA-Z0-9À-ỹ\s]+$", ErrorMessage = "Dãy không được chứa ký tự đặc biệt")]
        public string? Day { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string? TrangThai { get; set; }

        [StringLength(50)]
        public string? GhiChu { get; set; }

        [ForeignKey("MaLoaiPhong")]
        public virtual LoaiPhong? LoaiPhong { get; set; }

        [ForeignKey("MaKhuVuc")]
        public virtual KhuVuc? KhuVuc { get; set; }
    }
}