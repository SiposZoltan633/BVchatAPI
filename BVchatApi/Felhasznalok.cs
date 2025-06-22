using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BVchatApi
{
    [Table("felhasznalok")]
    public class Felhasznalo
    {
        [Key]
        [Column("felhasznaloid")]
        public int FelhasznaloId { get; set; }

        [Required]
        [Column("felhasznalonev")]
        public string FelhasznaloNev { get; set; } = string.Empty;

        [Required]
        [Column("jelszo")]
        public string Jelszo { get; set; } = string.Empty;

        [Required]
        [Column("regisztracioidopont")]
        public DateTime RegisztracioIdopont { get; set; }
    }
}
