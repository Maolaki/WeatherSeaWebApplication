using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Controllers
{
    [Route("Entity")]
    [Authorize]
    public class EntityController : Controller
    {
        private readonly AuthorizationContext _authContext;
        private readonly ModulesContext _context;
        private readonly IdentificationResponseHandler _identificationResponseHandler;

        // Инициализируйте _context через конструктор
        public EntityController(ModulesContext context, AuthorizationContext authContext, IdentificationResponseHandler identificationResponseHandler)
        {
            _context = context;
            _authContext = authContext;
            _identificationResponseHandler = identificationResponseHandler;
        }

        [AllowAnonymous]
        [HttpGet("EntityList")]
        public IActionResult EntityList()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet("EntityInfo")]
        public IActionResult EntityInfo()
        {
            return View();
        }

        [HttpGet("GetEntities")]
        public IActionResult GetEntities([FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Invalid token or user not authenticated.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            if (login == null)
            {
                return BadRequest("Invalid token or user not authenticated.");
            }

            var user = _authContext.UserList.FirstOrDefault(u => u.Login == login);

            if (user.Type == UserType.Standart)
            {
                return Ok("noAccess");
            }

            var entities = _context.EntityList.Where(f => f.OwnerLogin == login).ToList();

            // Создаем список анонимных объектов с дополнительным полем
            var modifiedEntities = entities.Select(entity => new
            {
                // Копируем свойства из существующей сущности
                entity,

                // Добавляем новое поле
                CustomClass = entity.Class.ToString()
            }).ToList();

            return Ok(modifiedEntities);
        }

        [HttpGet("GetEntity")]
        public IActionResult GetEntity([FromHeader(Name = "Authorization")] string authorization, int entityId)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Invalid token or user not authenticated.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            if (login == null)
            {
                return BadRequest("Invalid token or user not authenticated.");
            }

            var entity = _context.EntityList.FirstOrDefault(f => f.OwnerLogin == login && f.Id == entityId);

            return Ok(entity);
        }

        [HttpPost("AddEntity")]
        public async Task<IActionResult> AddEntity([FromHeader(Name = "Authorization")] string authorization, [FromForm] EntityModel entity)
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

            entity.OwnerLogin = login;
            entity.Origin = EntityOrigin.Custom;

            // Проверка наличия сущности с тем же именем
            bool entityExists = _context.EntityList
                .Any(e => e.OwnerLogin == login && e.Name == entity.Name);

            if (entityExists)
            {
                return Conflict("Сущность с тем же именем уже существует.");
            }

            _context.EntityList.Add(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("DeleteEntity")]
        public async Task<IActionResult> DeleteEntity([FromHeader(Name = "Authorization")] string authorization, int entityId)
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

            // Поиск сущности по идентификатору и логину владельца
            var entity = _context.EntityList.FirstOrDefault(e => e.Id == entityId && e.OwnerLogin == login);

            if (entity == null)
            {
                return NotFound("Сущность не найдена или пользователь не имеет к ней доступа.");
            }

            _context.EntityList.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("UpdateEntity")]
        public async Task<IActionResult> UpdateEntity([FromHeader(Name = "Authorization")] string authorization, [FromForm] EntityModel updatedEntity)
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

            // Поиск сущности по идентификатору
            var existingEntity = _context.EntityList
                .FirstOrDefault(e => e.Id == updatedEntity.Id && e.OwnerLogin == login);

            if (existingEntity == null)
            {
                return NotFound("Сущность не найдена или пользователь не имеет к ней доступа.");
            }

            // Обновление полей сущности
            existingEntity.Name = updatedEntity.Name;
            existingEntity.Class = updatedEntity.Class;
            existingEntity.Description = updatedEntity.Description;
            existingEntity.RecommendedTemperature = updatedEntity.RecommendedTemperature;
            existingEntity.RecommendedWindSpeed = updatedEntity.RecommendedWindSpeed;
            existingEntity.RecommendedHumidity = updatedEntity.RecommendedHumidity;

            _context.EntityList.Update(existingEntity);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
