using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WeatherSeaWebApplication.Models
{
    public class IdentificationResponseHandler
    {
        private readonly JwtGenerator _jwtGenerator;
        private readonly IConfiguration _configuration;

        public IdentificationResponseHandler(JwtGenerator jwtGenerator, IConfiguration configuration)
        {
            _jwtGenerator = jwtGenerator;
            _configuration = configuration;
        }

        public IActionResult AuthorizeUser(UserModel user)
        {
            // Генерация JWT токена для авторизованного пользователя
            var token = _jwtGenerator.GenerateJwt(user.Login);

            var response = new
            {
                access_token = token,

            };

            return new JsonResult(response);
        }

        public string? GetLoginFromJwt(string jwt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Секретный ключ не найден в конфигурации.");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userLoginClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

                if (userLoginClaim != null)
                {
                    return userLoginClaim.Value;
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибки, если токен невалидный
                Console.WriteLine($"Token validation failed: {ex.Message}");
            }

            return null;
        }


    }
}
