using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Controllers
{
    [AllowAnonymous]
    [Route("Identification")]
    public class IdentificationController : Controller
    {
        private readonly AuthorizationContext _context;
        private readonly IdentificationResponseHandler _identificationResponseHandler;
        private readonly IPasswordHasher _passwordHasher;

        public IdentificationController(AuthorizationContext context, IdentificationResponseHandler identificationResponseHandler, IPasswordHasher passwordHasher)
        {
            _context = context;
            _identificationResponseHandler = identificationResponseHandler;
            _passwordHasher = passwordHasher;
        }

        [HttpGet("Registration")]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpGet("Authorization")]
        public IActionResult Authorization()
        {
            return View();
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> Registration([FromBody] UserModel user)
        {
            // Проверка, существует ли уже пользователь с таким логином
            var userExists = await _context.UserList.AnyAsync(u => u.Login == user.Login);
            if (userExists)
            {
                return BadRequest(new { error = "Пользователь с таким логином уже существует." });
            }

            user.Password = _passwordHasher.Generate(user.Password);

            // Создание новой учетной записи
            _context.UserList.Add(user);
            await _context.SaveChangesAsync();

            return _identificationResponseHandler.AuthorizeUser(user);
        }

        [HttpPost("Authorization")]
        public async Task<IActionResult> Authorization([FromBody] UserModel user)
        {
            // Поиск пользователя с указанным логином и паролем
            var dbUser = await _context.UserList.FirstOrDefaultAsync(u => u.Login == user.Login);
            if (dbUser == null || !_passwordHasher.Verify(user.Password, dbUser.Password))
            {
                // Если пользователь не найден, возвращаем ошибку
                return BadRequest(new { error = "Неверный логин или пароль." });
            }

            return _identificationResponseHandler.AuthorizeUser(dbUser);
        }

    }
}
