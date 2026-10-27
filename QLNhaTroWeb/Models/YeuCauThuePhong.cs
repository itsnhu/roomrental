using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("YeuCauThuePhong")]
    public class YeuCauThuePhong
    {
        [Key]
        public int MaYeuCau { get; set; }

        // Chỉ định rõ kiểu NCHAR(10) để đồng bộ với SQL
        [StringLength(10)]
        [Column(TypeName = "nchar(10)")]
        public string MaKhachTro { get; set; }

        [StringLength(10)]
        [Column(TypeName = "nchar(10)")]
        public string MaPhong { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? NgayDuKienThue { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; } = "Chờ duyệt";

        [StringLength(255)]
        public string? PhanHoiAdmin { get; set; }

        [ForeignKey("MaKhachTro")]
        public virtual ThongTinKhach? KhachTro { get; set; }

        [ForeignKey("MaPhong")]
        public virtual Phong? Phong { get; set; }
    }
}