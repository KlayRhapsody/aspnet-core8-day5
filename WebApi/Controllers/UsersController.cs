
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService user) : ControllerBase
{
    public sealed record LoginRequest(string email, string password);
    
    [HttpGet("login", Name = "LoginAsync")]
    public async Task<ActionResult<string>> LoginAsync([FromQuery] LoginRequest request)
    {
        var (email, password) = request;

        var token = await user.UserLogin(email, password);

        return token;
    }

    [Authorize]
    [HttpGet("claims", Name = "GetAsync")]
    public async Task<IActionResult> GetAsync()
    {
        var user = HttpContext.User;

        var claims = user.Claims.ToDictionary(c => c.Type, c => c.Value);

        return Ok(new
        {
            id = claims.GetValueOrDefault(ClaimTypes.NameIdentifier, string.Empty),
            name = claims.GetValueOrDefault(ClaimTypes.Name, string.Empty),
            jti = claims.GetValueOrDefault(JwtRegisteredClaimNames.Jti, string.Empty),
            email = claims.GetValueOrDefault(ClaimTypes.Email, string.Empty),
            emailVerified = claims.TryGetValue("email_verified", out var verified) && bool.TryParse(verified, out var isVerified) && isVerified,
            allClaims = claims // 🔍 附加完整 Claim 列表，方便 Debug
        });
    }
    
}


