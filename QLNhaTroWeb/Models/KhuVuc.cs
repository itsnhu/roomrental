using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("KhuVuc")]
    public class KhuVuc
    {
        [Key]
        [StringLength(10)]
        public string MaKhuVuc { get; set; }

        [StringLength(50)]
        public string? TenKhuVuc { get; set; }
    }
}