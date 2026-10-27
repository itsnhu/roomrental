using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("TraPhong")]
    public class TraPhong
    {
        [Key]
        public int Id { get; set; }

        [StringLength(10)]
        public string? MaPhong { get; set; }

        [StringLength(10)]
        public string? MaKhachTro { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgayTraPhong { get; set; }

        [ForeignKey("MaPhong")]
        public virtual Phong? Phong { get; set; }

        [ForeignKey("MaKhachTro")]
        public virtual ThongTinKhach? ThongTinKhach { get; set; }
    }
}