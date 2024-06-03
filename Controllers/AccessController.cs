using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Controllers
{
    [Route("Access")]
    [Authorize]
    public class AccessController : Controller
    {

        private readonly ModulesContext _context;
        private readonly AuthorizationContext _authContext;
        private readonly IdentificationResponseHandler _identificationResponseHandler;

        // Инициализируйте _context через конструктор
        public AccessController(ModulesContext context, AuthorizationContext authContext, IdentificationResponseHandler identificationResponseHandler)
        {
            _context = context;
            _authContext = authContext;
            _identificationResponseHandler = identificationResponseHandler;
        }

        [AllowAnonymous]
        [HttpGet("AccessList")]
        public IActionResult AccessList()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet("AccessInfo")]
        public IActionResult AccessInfo()
        {
            return View();
        }

        [HttpGet("GetAccessesAndFields")]
        public IActionResult GetAccessesAndFields([FromHeader(Name = "Authorization")] string authorization)
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

            var user = _authContext.UserList.FirstOrDefault(u => u.Login == login);

            if (user.Type == UserType.Standart)
            {
                return Ok("noAccess");
            }

            var myFields = _context.FieldList.Where(f => f.OwnerLogin == login).ToList();
            var fieldIds = myFields.Select(f => f.FieldId).ToList();

            var accessList = _context.AccessList.Where(a => fieldIds.Contains(a.FieldId)).ToList();

            var summaries = myFields.Select(f => new FieldAccessSummary
            {
                FieldId = f.FieldId,
                FieldName = f.Name,
                EditCount = accessList.Count(a => a.FieldId == f.FieldId && a.Type == AccessType.Edit),
                ViewCount = accessList.Count(a => a.FieldId == f.FieldId && a.Type == AccessType.View)
            }).ToList();

            return Ok(summaries);
        }

        [HttpGet("GetAccessesOnField")]
        public IActionResult GetAccessesOnField([FromHeader(Name = "Authorization")] string authorization, int fieldId)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            var field = _context.FieldList.FirstOrDefault(f => f.FieldId == fieldId && f.OwnerLogin == login);

            if (login == null || field == null)
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            var accesses = _context.AccessList.Where(f => f.FieldId == fieldId).ToList();

            return Ok(accesses);
        }

        [HttpPost("AddAccess")]
        public async Task<IActionResult> AddAccess([FromHeader(Name = "Authorization")] string authorization, [FromForm] AccessModel accessModel)
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

            var field = _context.FieldList.FirstOrDefault(f => f.FieldId == accessModel.FieldId && f.OwnerLogin == login);
            if (field == null)
            {
                return NotFound("Поле не найдено или пользователь не имеет к нему доступа.");
            }

            var existingAccess = _context.AccessList.FirstOrDefault(f => f.FieldId == accessModel.FieldId && f.UserLogin == accessModel.UserLogin);
            if (existingAccess != null)
            {
                return Conflict("Такой доступ уже существует.");
            }

            if(accessModel.UserLogin == login)
            {
                return Conflict("Нельзя выдать доступ самому себе.");
            }

            _context.AccessList.Add(accessModel);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("UpdateAccess")]
        public async Task<IActionResult> UpdateAccess([FromHeader(Name = "Authorization")] string authorization, [FromForm] AccessModel updatedAccess)
        {
            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            string token = authorization.Substring("Bearer ".Length);
            string? login = _identificationResponseHandler.GetLoginFromJwt(token);

            var field = _context.FieldList.FirstOrDefault(f => f.FieldId == updatedAccess.FieldId && f.OwnerLogin == login);

            if (login == null || field == null)
            {
                return BadRequest("Неверный токен или пользователь не авторизован.");
            }

            var access = _context.AccessList.FirstOrDefault(f => f.FieldId == updatedAccess.FieldId && f.UserLogin == updatedAccess.UserLogin);

            if (access == null)
            {
                return BadRequest("Доступ не найден.");
            }

            access.Type = updatedAccess.Type;

            _context.AccessList.Update(access);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("DeleteAccess")]
        public async Task<IActionResult> DeleteAccess([FromHeader(Name = "Authorization")] string authorization, int fieldId, string userLogin)
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

            // Поиск поля по идентификатору и логину владельца
            var field = _context.FieldList.FirstOrDefault(e => e.FieldId == fieldId && e.OwnerLogin == login);
            var access = _context.AccessList.FirstOrDefault(e => e.FieldId == fieldId && e.UserLogin == userLogin);

            if (field == null || access == null)
            {
                return NotFound("Доступ не найден или пользователь не имеет к ней доступа.");
            }

            _context.AccessList.Remove(access);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
