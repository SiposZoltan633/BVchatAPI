namespace BVchatApi.Models
{
    public class Felhasznalo
    {
        public int FelhasznaloId { get; set; }
        public string FelhasznaloNev { get; set; } = null!;
        public string Jelszo { get; set; } = null!;
        public DateTime RegisztracioIdopont { get; set; }
    }
}
