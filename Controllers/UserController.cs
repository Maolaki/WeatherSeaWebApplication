using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using WeatherSeaWebApplication.Models;
using System.Text.Json;

namespace WeatherSeaWebApplication.Controllers
{
    [Route("User")]
    [Authorize]
    public class UserController : Controller
    {
        public class Amount
        {
            public string Value { get; set; } = "";
            public string Currency { get; set; } = "";
        }

        public class RefundRequest
        {
            public Amount Amount { get; set; }
            public string PaymentId { get; set; } = "";
        }





        public record ChangeRequestModel(
            string NewName,
            string NewPassword,
            string NewEmail,
            string Password);

        private readonly AuthorizationContext _context;
        private readonly IdentificationResponseHandler _identificationResponseHandler;
        private readonly IPasswordHasher _passwordHasher;

        // Инициализируйте _context через конструктор
        public UserController(AuthorizationContext context, IdentificationResponseHandler identificationResponseHandler, IPasswordHasher passwordHasher)
        {
            _context = context;
            _identificationResponseHandler = identificationResponseHandler;
            _passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        [HttpGet("Profile")]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile([FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            if (login == null)
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            var dbUser = _context.UserList.FirstOrDefault(u => u.Login == login);

            if (dbUser == null)
            {
                return BadRequest("Неверный токен.");
            }

            return Ok(dbUser);
        }

        [HttpGet("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            if (login == null)
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }


            string shopId = "396607";
            string secretKey = "test_93RCT_PYr3cbwU7IPTLbkp5JMilwbk7mi6HEjiDiqyg";

            var refundRequest = new RefundRequest
            {
                Amount = new Amount
                {
                    Value = "1.00", // Сумма возврата
                    Currency = "RUB" // Валюта
                },
                PaymentId = "215d8da0-000f-50be-b000-0003308c89be" // ID платежа
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(refundRequest), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.yookassa.ru/v3/refunds")
            {
                Content = requestContent
            };

            // Добавление заголовков
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shopId}:{secretKey}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            request.Headers.Add("Idempotence-Key", Guid.NewGuid().ToString());

            // Отправка запроса
            var httpClient = new HttpClient();
            var response = await httpClient.SendAsync(request);

            var dbUser = _context.UserList.FirstOrDefault(u => u.Login == login);
            if (dbUser == null)
            {
                // Если пользователь не найден, возвращаем ошибку
                return BadRequest(new { error = "Неверный логин или пароль." });
            }

            dbUser.Type = UserType.Premium;
            dbUser.Days += 5;

            // Добавляем новое поле в базу данных
            _context.UserList.Update(dbUser);
            await _context.SaveChangesAsync();

            // Возвращаем URL для перенаправления пользователя на страницу оплаты
            return Ok("https://example.com/payment/success");
        }

        [HttpPost("UpdateUsername")]
        public async Task<IActionResult> UpdateUsername([FromHeader(Name = "Authorization")] string authorization, [FromForm] ChangeRequestModel model)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            if (login == null)
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            var dbUser = _context.UserList.FirstOrDefault(u => u.Login == login);
            if (dbUser == null || !_passwordHasher.Verify(model.Password, dbUser.Password))
            {
                // Если пользователь не найден, возвращаем ошибку
                return BadRequest(new { error = "Неверный логин или пароль." });
            }

            dbUser.Username = model.NewName;

            // Добавляем новое поле в базу данных
            _context.UserList.Update(dbUser);
            await _context.SaveChangesAsync();

            // Перезагружаем страницу со списком полей
            return Ok();
        }

        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromHeader(Name = "Authorization")] string authorization, [FromForm] ChangeRequestModel model)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            if (login == null)
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            var dbUser = _context.UserList.FirstOrDefault(u => u.Login == login);
            if (dbUser == null || model.NewPassword == model.Password)
            {
                // Если пользователь не найден, возвращаем ошибку
                return BadRequest(new { error = "Неверный логин или разные пароли." });
            }

            dbUser.Password = _passwordHasher.Generate(model.NewPassword);

            // Добавляем новое поле в базу данных
            _context.UserList.Update(dbUser);
            await _context.SaveChangesAsync();

            // Перезагружаем страницу со списком полей
            return Ok();
        }

        [HttpPost("UpdateEmail")]
        public async Task<IActionResult> UpdateEmail([FromHeader(Name = "Authorization")] string authorization, [FromForm] ChangeRequestModel model)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            if (login == null)
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            var dbUser = _context.UserList.FirstOrDefault(u => u.Login == login);
            if (dbUser == null || !_passwordHasher.Verify(model.Password, dbUser.Password))
            {
                // Если пользователь не найден, возвращаем ошибку
                return BadRequest(new { error = "Неверный логин или пароль." });
            }

            dbUser.Email = model.NewEmail;

            // Добавляем новое поле в базу данных
            _context.UserList.Update(dbUser);
            await _context.SaveChangesAsync();

            // Перезагружаем страницу со списком полей
            return Ok();
        }
    }
}
