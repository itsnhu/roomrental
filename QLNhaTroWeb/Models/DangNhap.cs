using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thêm dòng này

namespace QLNhaTroWeb.Models
{
    [Table("DangNhap")] // Ch? ??nh ?úng tên b?ng trong SQL Server
    public class DangNhap
    {
        [Key]
        [StringLength(50)]
        public string ID { get; set; }

        [StringLength(10)]
        public string PassWord { get; set; }

        [StringLength(20)]
        public string? Role { get; set; }
    }
}