using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCourse.Services;
using StudentCourse.Models;
namespace StudentCourse.Controllers;

[ApiController]
public sealed class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("api/auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
       try
        {
            AdminCurrentDto current = _adminAuthService.Login(request, ClientIp());
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, current.UserId.ToString()),
                new Claim(ClaimTypes.Name, current.Username),
                new Claim(ClaimTypes.GivenName, current.RealName),
                new Claim(ClaimTypes.Role, "Admin")
            };
            await HttpContext.SignInAsync(new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));
            return Ok(current);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        
    }
    private readonly AdminAuthService _adminAuthService;

    public AuthController(AdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    private string ClientIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
    
    [AllowAnonymous]
    [HttpPost("api/auth/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok(new { loggedOut = true });
    }
}


