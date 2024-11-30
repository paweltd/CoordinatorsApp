using CoordinatorsAppAPI.AppSettings;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoordinatorsAppAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly OAuthCredentials _oAuthCredentials;
        private readonly string _issuer;

        public AuthController(IOptions<OAuthCredentials> oAuthCredentials)
        {
            if (oAuthCredentials?.Value == null)
            {
                throw new ArgumentNullException(nameof(oAuthCredentials), "OAuthCredentials or its Value cannot be null.");
            }

            _oAuthCredentials = oAuthCredentials.Value;
            _issuer = "https://your-app.example.com"; // Poprawna wartoœæ issuer, ustawiona na URL serwera
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.IdToken))
            {
                return BadRequest(new { message = "IdToken is required." });
            }

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _oAuthCredentials.ClientId }
                });

                var token = CreateJwtToken(payload.Email);

                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // W œrodowisku produkcyjnym ustaw na true
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1)
                });

                return Ok(new { message = "User authenticated successfully." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = "Invalid token.", error = ex.Message });
            }
        }

        [HttpGet("check-status")]
        public IActionResult CheckStatus()
        {
            var token = Request.Cookies["access_token"];
            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            try
            {
                var claimsPrincipal = ValidateJwtToken(token);

                var userName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                var userEmail = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

                return Ok(new { user = new { name = userName, email = userEmail } });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { message = "Invalid token." });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok(new { message = "User logged out successfully." });
        }

        private string CreateJwtToken(string email)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_oAuthCredentials.ClientSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _oAuthCredentials.ClientId,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_oAuthCredentials.ClientSecret);

            var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer, // Weryfikacja issuer
                ValidateAudience = true,
                ValidAudience = _oAuthCredentials.ClientId,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out _);

            return claimsPrincipal;
        }
    }

    public class GoogleLoginRequest
    {
        public string IdToken { get; set; }
    }
}
