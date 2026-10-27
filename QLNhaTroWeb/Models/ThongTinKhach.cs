using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("ThongTinKhach")]
    public class ThongTinKhach
    {
        [Key]
        [StringLength(10)]
        public string MaKhachTro { get; set; }

        [StringLength(255)]
        [Required(ErrorMessage = "Họ không được để trống")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ không được chứa số hoặc ký tự đặc biệt")]
        public string? Ho { get; set; }

        [StringLength(255)]
        [Required(ErrorMessage = "Tên không được để trống")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên không được chứa số hoặc ký tự đặc biệt")]
        public string? Ten { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public string? GioiTinh { get; set; }

        public byte[]? AnhNhanDien { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Số CMND/CCCD không được để trống")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "CMND/CCCD phải từ 9 đến 12 chữ số")]
        [StringLength(12)]
        public string? CMND { get; set; }

        [Required(ErrorMessage = "Quê quán không được để trống")]
        [StringLength(255)]
        public string? QueQuan { get; set; }

        [StringLength(50)]
        public string? NgheNghiep { get; set; }

        [StringLength(10)]
        public string? MaPhong { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }

        [ForeignKey("MaPhong")]
        public virtual Phong? Phong { get; set; }
    }
}