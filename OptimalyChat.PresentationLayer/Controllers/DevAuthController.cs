using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OptimalyChat.DataLayer.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OptimalyChat.PresentationLayer.Controllers;

#if DEBUG
[ApiController]
[Route("api/dev/[controller]")]
public class DevAuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DevAuthController> _logger;

    public DevAuthController(
        UserManager<User> userManager,
        IConfiguration configuration,
        ILogger<DevAuthController> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("test-token")]
    public async Task<IActionResult> GetTestToken([FromBody] TestTokenRequest request)
    {
        _logger.LogWarning("DEV ONLY: Creating test token for user {Email}", request.Email);

        // Vytvoří nebo najde test uživatele
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };
            
            var createResult = await _userManager.CreateAsync(user, request.Password ?? "Test123!");
            if (!createResult.Succeeded)
            {
                return BadRequest(new { errors = createResult.Errors });
            }

            // Přidá role pokud jsou specifikované
            if (request.Roles?.Any() == true)
            {
                await _userManager.AddToRolesAsync(user, request.Roles);
            }
        }

        // Generuje JWT token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            token = tokenString,
            email = user.Email,
            userId = user.Id,
            roles = roles,
            expiresAt = token.ValidTo,
            curlExample = $"curl -H \"Authorization: Bearer {tokenString}\" https://localhost:5001/api/your-endpoint"
        });
    }

    [HttpDelete("test-users")]
    public async Task<IActionResult> CleanupTestUsers()
    {
        var testUsers = _userManager.Users.Where(u => u.Email.StartsWith("test"));
        foreach (var user in testUsers)
        {
            await _userManager.DeleteAsync(user);
        }
        return Ok(new { message = $"Deleted {testUsers.Count()} test users" });
    }
}

public class TestTokenRequest
{
    public string Email { get; set; } = "test@example.com";
    public string? Password { get; set; }
    public List<string>? Roles { get; set; }
}
#endif