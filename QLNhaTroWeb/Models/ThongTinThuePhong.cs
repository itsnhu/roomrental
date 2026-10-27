using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QLNhaTroWeb.Models
{
    [Table("ThongTinThuePhong")]
    [PrimaryKey(nameof(MaHD), nameof(MaKhachTro))]
    public class ThongTinThuePhong
    {
        [StringLength(10)]
        public string MaHD { get; set; } // Khóa chính (không thể null)

        [StringLength(10)]
        public string MaKhachTro { get; set; } // Khóa chính (không thể null)

        [StringLength(10)]
        public string? MaPhong { get; set; } // Thêm dấu ?

        [DataType(DataType.Date)]
        public DateTime? NgayThue { get; set; } // Thêm dấu ?

        public long? TienDatCoc { get; set; }

        [StringLength(50)]
        public string? GhiChu { get; set; }

        [ForeignKey("MaPhong")]
        public virtual Phong? Phong { get; set; }

        [ForeignKey("MaKhachTro")]
        public virtual ThongTinKhach? ThongTinKhach { get; set; }
    }
}