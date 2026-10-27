using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("PhieuThu")]
    public class PhieuThu
    {
        [Key]
        public double MaPT { get; set; } // Trong SQL là float, C# dùng double tương đương

        [StringLength(10)]
        public string? MaPhong { get; set; }

        [DataType(DataType.Date)]
        public DateTime NgayLap { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayThu { get; set; }

        public long? TienNha { get; set; }
        public int? SkDien { get; set; }
        public long? TienDien { get; set; }
        public int? SkNuoc { get; set; }
        public long? TienNuoc { get; set; }
        public long? TongTien { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }

        [ForeignKey("MaPhong")]
        public virtual Phong? Phong { get; set; }
    }
}