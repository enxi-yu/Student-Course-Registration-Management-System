using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using StudentCourse.Infrastructure;
using StudentCourse.Shared.Security;

namespace StudentCourse.Controllers;

[ApiController]
public sealed class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("api/auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || !request.Username.Trim().StartsWith("T", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Please use a teacher account beginning with T." });
        }

        const string sql = @"SELECT u.user_id, u.username, u.password, u.real_name, t.teacher_no, t.title, t.department FROM ""user"" u JOIN teacher t ON t.user_id = u.user_id WHERE u.username = :username AND u.role_id = 1 AND u.status = 1";
        using OracleConnection connection = DbConnectionFactory.OpenConnection();
        using OracleCommand command = new OracleCommand(sql, connection) { BindByName = true };
        command.Parameters.Add("username", OracleDbType.Varchar2).Value = request.Username.Trim();
        using OracleDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return Unauthorized(new { message = "Invalid username or password." });

        int userId = Convert.ToInt32(reader["user_id"]);
        string storedPassword = Convert.ToString(reader["password"]) ?? string.Empty;
        if (!PasswordHash.Verify(storedPassword, request.Password, out bool needsUpgrade))
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, Convert.ToString(reader["username"]) ?? string.Empty),
            new Claim(ClaimTypes.GivenName, Convert.ToString(reader["real_name"]) ?? string.Empty),
            new Claim(ClaimTypes.Role, "Teacher"),
            new Claim("teacher_no", Convert.ToString(reader["teacher_no"]) ?? string.Empty),
            new Claim("title", Convert.ToString(reader["title"]) ?? string.Empty),
            new Claim("department", Convert.ToString(reader["department"]) ?? string.Empty)
        };

        reader.Close();
        if (needsUpgrade) UpgradePassword(connection, userId, storedPassword, request.Password);
        await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));
        return Ok(new { username = request.Username.Trim(), role = "Teacher" });
    }

    [AllowAnonymous]
    [HttpPost("api/auth/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok(new { loggedOut = true });
    }

    private static void UpgradePassword(OracleConnection connection, int userId, string oldHash, string password)
    {
        const string sql = @"UPDATE ""user"" SET password = :newPassword WHERE user_id = :userId AND password = :oldPassword";
        using OracleCommand command = new OracleCommand(sql, connection) { BindByName = true };
        command.Parameters.Add("newPassword", OracleDbType.Varchar2).Value = PasswordHash.Hash(password);
        command.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
        command.Parameters.Add("oldPassword", OracleDbType.Varchar2).Value = oldHash;
        command.ExecuteNonQuery();
    }
}

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
