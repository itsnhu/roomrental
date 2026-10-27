using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaTroWeb.Models
{
    [Table("GhiNho")]
    public class GhiNho
    {
        [Key]
        [StringLength(50)]
        public string ID { get; set; }

        [StringLength(10)]
        public string Pass { get; set; }
    }
}