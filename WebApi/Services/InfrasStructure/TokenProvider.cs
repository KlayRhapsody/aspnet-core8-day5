using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Services.InfrasStructure;

public class TokenProvider(IConfiguration config)
{
    public string Create(User user)
    {
        SymmetricSecurityKey key = new (Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]));

        SigningCredentials credentials = new (key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor descriptor = new ()
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("email_verified", user.EmailVerified.ToString()),
            ]),
            Expires = DateTime.UtcNow.AddMinutes(config.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"]
        };

        JsonWebTokenHandler handler = new ();
        
        return handler.CreateToken(descriptor);
    }
}