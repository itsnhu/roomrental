using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("LoaiPhong")]
    public class LoaiPhong
    {
        [Key]
        [StringLength(10)]
        public string MaLoaiPhong { get; set; }

        [StringLength(50)]
        public string? TenLoaiPhong { get; set; }

        public double? DienTichPhong { get; set; }
        public double? DonGia { get; set; }

        [StringLength(50)]
        public string? GhiChu { get; set; }
    }
}