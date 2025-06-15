using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BVchatApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("osszes")]
        public IActionResult OsszesFelhasznalo()
        {
            var connStr = _config.GetConnectionString("Default");
            try
            {
                var felhasznalok = new List<object>();
                using var conn = new NpgsqlConnection(connStr);
                conn.Open();

                string sql = "SELECT \"felhasznaloId\", \"felhasznaloNev\", \"regisztracioIdopont\" FROM felhasznalok";
                using var cmd = new NpgsqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    felhasznalok.Add(new
                    {
                        Id = reader.GetInt32(0),
                        Nev = reader.GetString(1),
                        Regisztracio = reader.GetDateTime(2)
                    });
                }

                return Ok(felhasznalok);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uzenet = "Szerverhiba", hiba = ex.Message });
            }
        }

        [HttpPost("uj")]
        public IActionResult UjFelhasznalo([FromBody] FelhasznaloDto uj)
        {
            var connStr = _config.GetConnectionString("Default");
            try
            {
                using var conn = new NpgsqlConnection(connStr);
                conn.Open();

                var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM felhasznalok WHERE \"felhasznaloNev\" = @nev", conn);
                checkCmd.Parameters.AddWithValue("@nev", uj.FelhasznaloNev);
                long count = (long)checkCmd.ExecuteScalar();

                if (count > 0)
                    return BadRequest(new { uzenet = "Ez a felhasználónév már foglalt" });

                var jelszoHash = BCrypt.Net.BCrypt.HashPassword(uj.Jelszo);

                string sql = "INSERT INTO felhasznalok (\"felhasznaloNev\", \"jelszo\", \"regisztracioIdopont\") VALUES (@nev, @jelszo, @ido)";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nev", uj.FelhasznaloNev);
                cmd.Parameters.AddWithValue("@jelszo", jelszoHash);
                cmd.Parameters.AddWithValue("@ido", DateTime.Now);

                cmd.ExecuteNonQuery();

                return Ok(new { uzenet = "Sikeres regisztráció" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uzenet = "Szerverhiba", hiba = ex.Message });
            }
        }

        [HttpPost("bejelentkezes")]
        public IActionResult Bejelentkezes([FromBody] FelhasznaloDto login)
        {
            var connStr = _config.GetConnectionString("Default");
            try
            {
                using var conn = new NpgsqlConnection(connStr);
                conn.Open();

                string sql = "SELECT \"jelszo\" FROM felhasznalok WHERE \"felhasznaloNev\" = @nev";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nev", login.FelhasznaloNev);
                var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return Unauthorized("Hibás felhasználónév vagy jelszó");

                string hash = reader.GetString(0);

                if (!BCrypt.Net.BCrypt.Verify(login.Jelszo, hash))
                    return Unauthorized("Hibás felhasználónév vagy jelszó");

                return Ok(new { uzenet = "Sikeres bejelentkezés" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uzenet = "Szerverhiba", hiba = ex.Message });
            }
        }

        public class FelhasznaloDto
        {
            public required string FelhasznaloNev { get; set; }
            public required string Jelszo { get; set; }
        }
    }
}
