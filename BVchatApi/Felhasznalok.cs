using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BVchatApi.Models
{
    [Table("felhasznalok")]
    public class Felhasznalo
    {
        [Key]
        [Column("felhasznaloid")]
        public int felhasznaloid { get; set; }

        [Required]
        [Column("felhasznalonev")]
        public string felhasznalonev { get; set; } = string.Empty;

        [Required]
        [Column("jelszo")]
        public string jelszo { get; set; } = string.Empty;

        [Required]
        [Column("regisztracioidopont")]
        public DateTime regisztracioidopont { get; set; } // ÚJ
    }
}
