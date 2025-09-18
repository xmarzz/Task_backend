using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SoftwareDashboardAPI.Models;

namespace SoftwareDashboardAPI.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                return BadRequest(new { success = false, message = "Username and password required" });

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("INSERT INTO users (username, password) VALUES (@username, @password)", conn);
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@password", hashedPassword);

            try
            {
                cmd.ExecuteNonQuery();
                return Ok(new { success = true, message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Database error", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM users WHERE username=@username", conn);
            cmd.Parameters.AddWithValue("@username", user.Username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return Unauthorized(new { success = false, message = "Invalid username or password" });

            var dbPassword = reader["password"].ToString();
            if (!BCrypt.Net.BCrypt.Verify(user.Password, dbPassword!))
                return Unauthorized(new { success = false, message = "Invalid username or password" });

            return Ok(new { success = true, message = "Login successful" });
        }
    }
}
