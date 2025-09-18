using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SoftwareDashboardAPI.Models;

namespace SoftwareDashboardAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public DashboardController(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<Software>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM software_dashboard", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Software
                {
                    Id = Convert.ToInt32(reader["id"]),
                    AppName = reader["app_name"].ToString()!,
                    Version = reader["version"].ToString()!,
                    Status = reader["status"].ToString()!,
                    OpenIssues = Convert.ToInt32(reader["open_issues"]),
                    ResolvedTickets = Convert.ToInt32(reader["resolved_tickets"])
                });
            }

            return Ok(list);
        }

        [HttpPost]
        public IActionResult Add([FromBody] Software software)
        {
            if (string.IsNullOrEmpty(software.AppName) || string.IsNullOrEmpty(software.Version))
                return BadRequest(new { error = "App name and version are required" });

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(
                "INSERT INTO software_dashboard (app_name, version, status, open_issues, resolved_tickets) VALUES (@app, @ver, @status, @open, @resolved)",
                conn
            );
            cmd.Parameters.AddWithValue("@app", software.AppName);
            cmd.Parameters.AddWithValue("@ver", software.Version);
            cmd.Parameters.AddWithValue("@status", software.Status);
            cmd.Parameters.AddWithValue("@open", software.OpenIssues);
            cmd.Parameters.AddWithValue("@resolved", software.ResolvedTickets);

            cmd.ExecuteNonQuery();
            return Ok(new { success = true, id = cmd.LastInsertedId });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("DELETE FROM software_dashboard WHERE id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            var affected = cmd.ExecuteNonQuery();

            if (affected == 0)
                return NotFound(new { error = "Record not found" });

            return Ok(new { success = true, message = "Software deleted" });
        }
    }
}
