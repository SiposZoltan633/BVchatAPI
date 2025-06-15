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

                try
                {
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
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { uzenet = "Olvasási hiba", hiba = ex.Message });
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

                var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM felhasznalok WHERE TRIM(\"felhasznalonev\") = @nev", conn);
                checkCmd.Parameters.AddWithValue("@nev", uj.felhasznalonev);
                long count = Convert.ToInt64(checkCmd.ExecuteScalar() ?? 0);

                if (count > 0)
                    return BadRequest(new { uzenet = "Ez a felhasználónév már foglalt" });

                var jelszoHash = BCrypt.Net.BCrypt.HashPassword(uj.jelszo);

                string sql = "INSERT INTO felhasznalok (\"felhasznalonev\", \"jelszo\", \"regisztracioidopont\") VALUES (@nev, @jelszo, @ido)";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nev", uj.felhasznalonev);
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

                string sql = "SELECT \"jelszo\" FROM felhasznalok WHERE TRIM(\"felhasznalonev\") = @nev";
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@nev", login.felhasznalonev);
                var reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return Unauthorized("Hibás felhasználónév vagy jelszó");

                string hash = reader.GetString(0);

                if (!BCrypt.Net.BCrypt.Verify(login.jelszo, hash))
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
            public required string felhasznalonev { get; set; }
            public required string jelszo { get; set; }
        }
    }
}
