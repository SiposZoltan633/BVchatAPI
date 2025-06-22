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

                string sql = "SELECT \"felhasznaloid\", TRIM(\"felhasznalonev\"), \"regisztracioidopont\" FROM felhasznalok";
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
            if (string.IsNullOrWhiteSpace(uj?.FelhasznaloNev) || string.IsNullOrWhiteSpace(uj?.Jelszo))
                return BadRequest(new { uzenet = "Hiányzó felhasználónév vagy jelszó" });

            var connStr = _config.GetConnectionString("Default");
            try
            {
                using var conn = new NpgsqlConnection(connStr);
                conn.Open();

                var felhasznalonev = uj.FelhasznaloNev.Trim();

                using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM felhasznalok WHERE TRIM(\"felhasznalonev\") = @nev", conn);
                checkCmd.Parameters.AddWithValue("@nev", felhasznalonev);
                var result = checkCmd.ExecuteScalar();
                long count = (result != null && result != DBNull.Value) ? Convert.ToInt64(result) : 0;

                if (count > 0)
                    return BadRequest(new { uzenet = "Ez a felhasználónév már foglalt" });

                var jelszoHash = BCrypt.Net.BCrypt.HashPassword(uj.Jelszo);

                using var cmd = new NpgsqlCommand("INSERT INTO felhasznalok (\"felhasznalonev\", \"jelszo\", \"regisztracioidopont\") VALUES (@nev, @jelszo, @ido)", conn);
                cmd.Parameters.AddWithValue("@nev", felhasznalonev);
                cmd.Parameters.AddWithValue("@jelszo", jelszoHash);
                cmd.Parameters.AddWithValue("@ido", DateTime.UtcNow);

                cmd.ExecuteNonQuery();

                return Ok(new { uzenet = "Sikeres regisztráció" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uzenet = "Szerverhiba", hiba = ex.ToString() });
            }
        }

        [HttpPost("bejelentkezes")]
        public IActionResult Bejelentkezes([FromBody] FelhasznaloDto login)
        {
            if (string.IsNullOrWhiteSpace(login?.FelhasznaloNev) || string.IsNullOrWhiteSpace(login?.Jelszo))
                return BadRequest(new { uzenet = "Hiányzó felhasználónév vagy jelszó" });

            var connStr = _config.GetConnectionString("Default");
            try
            {
                using var conn = new NpgsqlConnection(connStr);
                conn.Open();

                var felhasznalonev = login.FelhasznaloNev.Trim();

                using var cmd = new NpgsqlCommand("SELECT \"jelszo\" FROM felhasznalok WHERE TRIM(\"felhasznalonev\") = @nev", conn);
                cmd.Parameters.AddWithValue("@nev", felhasznalonev);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return Unauthorized(new { uzenet = "Hibás felhasználónév vagy jelszó" });

                var hash = reader.GetString(0);

                if (!BCrypt.Net.BCrypt.Verify(login.Jelszo, hash))
                    return Unauthorized(new { uzenet = "Hibás felhasználónév vagy jelszó" });

                return Ok(new { uzenet = "Sikeres bejelentkezés" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { uzenet = "Szerverhiba", hiba = ex.Message });
            }
        }

        public class FelhasznaloDto
        {
            public string FelhasznaloNev { get; set; } = string.Empty;
            public string Jelszo { get; set; } = string.Empty;
        }
    }
}
