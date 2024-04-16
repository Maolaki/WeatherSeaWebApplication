using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Controllers
{
    public class ModulesController : Controller
    {

        private readonly ModulesContext _context;
        private readonly IConfiguration _configuration;

        // Инициализируйте _context через конструктор
        public ModulesController(ModulesContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("FieldList")]
        public async Task<IActionResult> FieldList()
        {
            // Получите JWT из куки или заголовка авторизации
            var jwt = Request.Cookies["jwt"];
            // Расшифруйте JWT и получите имя пользователя или ID
            var userId = GetUserIdFromJwt(jwt);

            // Используйте ID для получения данных из UserContext
            var fields = await _context.FieldList
                .Where(f => f.OwnerId == userId)
                .ToListAsync();

            // Передайте данные в представление
            return View(fields);
        }

        private int? GetUserIdFromJwt(string jwt)
        {
            if (jwt == null) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // Установите параметры валидации, если необходимо
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

                if (userIdClaim != null)
                {
                    return int.Parse(userIdClaim.Value);
                }
            }
            catch
            {
                // Обработка ошибки, если токен невалидный
            }

            return null;
        }


        public IActionResult FieldInfo()
        {
            return View();
        }

        public IActionResult ShareList()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }
    }
}
