using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("ThongTinDangKyPhong")]
    public class ThongTinDangKyPhong
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ không được để trống")]
        [StringLength(50, ErrorMessage = "Họ không quá 50 ký tự")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ chỉ được chứa chữ cái và khoảng trắng")]
        public string Ho { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(50, ErrorMessage = "Tên không quá 50 ký tự")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên chỉ được chứa chữ cái và khoảng trắng")]
        public string Ten { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Số CMND/CCCD không được để trống")]
        [StringLength(12, MinimumLength = 9, ErrorMessage = "CMND/CCCD phải từ 9 đến 12 chữ số")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số CMND/CCCD chỉ được chứa chữ số")]
        public string CMND { get; set; }

        [Required(ErrorMessage = "Quê quán không được để trống")]
        [StringLength(255, ErrorMessage = "Quê quán không quá 255 ký tự")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Quê quán không được chứa số hoặc ký tự đặc biệt")]
        public string QueQuan { get; set; }

        [Required(ErrorMessage = "Nghề nghiệp không được để trống")]
        [StringLength(255, ErrorMessage = "Nghề nghiệp không quá 255 ký tự")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Nghề nghiệp không được chứa số hoặc ký tự đặc biệt")]
        public string NgheNghiep { get; set; }

        public string? MaKhuVuc { get; set; }
        public string? MaLoaiPhong { get; set; }
        public string? MaPhong { get; set; }

        [ForeignKey("MaKhuVuc")]
        public virtual KhuVuc? KhuVuc { get; set; }

        [ForeignKey("MaLoaiPhong")]
        public virtual LoaiPhong? LoaiPhong { get; set; }

        [ForeignKey("MaPhong")]
        public virtual Phong? Phong { get; set; }
    }
}