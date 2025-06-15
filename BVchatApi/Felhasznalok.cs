using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BVchatApi.Models
{
    [Table("felhasznalok")] // Adatbázistábla neve
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
    }
}
