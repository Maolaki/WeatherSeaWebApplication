using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Controllers
{
    [Route("Field")]
    [Authorize]
    public class FieldController : Controller
    {
        // records для FieldInfo
        public record WeatherForecastResponse(List<WeatherData> List);

        public record WeatherData(
            MainData Main,
            WindData Wind,
            List<WeatherDescription> Weather,
            RainData? Rain,
            CloudData? Clouds,
            string dt_txt);

        public record MainData(float Temp, float Humidity);

        public record WindData(float Speed, float Deg);

        public record WeatherDescription(string Description);

        public record RainData([property: JsonProperty("3h")] float? Volume);

        public record CloudData(int All);


        // Дальше records для FieldList

        public record WeatherResponse(Main Main, List<Weather> Weather, Wind Wind);
        public record Main(double Temp);
        public record Weather(string Description);
        public record Wind(double Speed, int Deg);

        private readonly ModulesContext _context;
        private readonly IdentificationResponseHandler _identificationResponseHandler;

        // Инициализируйте _context через конструктор
        public FieldController(ModulesContext context, IdentificationResponseHandler identificationResponseHandler)
        {
            _context = context;
            _identificationResponseHandler = identificationResponseHandler;
        }

        [AllowAnonymous]
        [HttpGet("FieldList")]
        public IActionResult FieldList()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet("FieldInfo")]
        public IActionResult FieldInfo()
        {
            return View();
        }

        [HttpGet("GetFields")]
        public async Task<IActionResult> GetFields([FromHeader(Name = "Authorization")] string authorization)
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

            var myFields = _context.FieldList.Where(f => f.OwnerLogin == login).ToList();
            var accessList = _context.AccessList.Where(a => a.UserLogin == login).ToList();
            var accessibleFieldIds = accessList.Select(a => a.FieldId).Distinct().ToList();
            var accessibleFields = _context.FieldList.Where(f => accessibleFieldIds.Contains(f.FieldId)).ToList();

            var apiKey = ""; // InsertYourAPIKeyHere
            var httpClient = new HttpClient();

            async Task<object> GetFieldWithWeather(FieldModel field)
            {
                var url = $"https://api.openweathermap.org/data/2.5/weather?lat={field.Latitude}&lon={field.Longitude}&appid={apiKey}&units=metric";
                var response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var weatherData = await response.Content.ReadFromJsonAsync<WeatherResponse>();
                    return new
                    {
                        field.FieldId,
                        field.Name,
                        field.Latitude,
                        field.Longitude,
                        Weather = new
                        {
                            Temperature = weatherData.Main.Temp,
                            WindSpeed = weatherData.Wind.Speed,
                            WindDirection = weatherData.Wind.Deg,
                            Description = weatherData.Weather[0].Description
                        }
                    };
                }
                else
                {
                    return new
                    {
                        field.FieldId,
                        field.Name,
                        field.Latitude,
                        field.Longitude,
                        Weather = new
                        {
                            Temperature = 30,
                            WindSpeed = 6,
                            WindDirection = 180,
                            Description = "sunny"
                        }
                    };
                }
            }

            var myFieldsWithWeather = await Task.WhenAll(myFields.Select(GetFieldWithWeather));
            var accessibleFieldsWithWeather = await Task.WhenAll(accessibleFields.Select(GetFieldWithWeather));

            var response = new
            {
                MyFields = myFieldsWithWeather.Where(f => f != null),
                AccessibleFields = accessibleFieldsWithWeather.Where(f => f != null)
            };

            return Ok(response);
        }

        [HttpGet("GetFieldInfo")]
        public IActionResult GetField([FromHeader(Name = "Authorization")] string authorization, int fieldId)
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
            var isEdit = 0;

            // Проверка участка на существование и на доступ к нему
            if (field == null)
            {
                // Если поле не найдено или не принадлежит пользователю, проверим доступ к редактированию
                var hasEditAccess = _context.AccessList.Any(a => a.FieldId == fieldId && a.UserLogin == login && a.Type == AccessType.Edit);

                if (!hasEditAccess)
                {
                    // Если поле не найдено или не принадлежит пользователю, проверим доступ к просмотру
                    var hasViewAccess = _context.AccessList.Any(a => a.FieldId == fieldId && a.UserLogin == login && a.Type == AccessType.View);

                    if (!hasViewAccess)
                    {
                        return NotFound("Поле не найдено или пользователь не имеет к нему доступа.");
                    }
                }
                else
                {
                    isEdit = 1;
                }

                field = _context.FieldList.FirstOrDefault(e => e.FieldId == fieldId);

                if (field == null)
                {
                    return NotFound("Поля не существует.");
                }
            }
            else
            {
                isEdit = 1;
            }

            return Ok(new
            {
                Field = field,
                IsEdit = isEdit
            });
        }

        [HttpGet("GetFieldWeatherData")]
        public async Task<IActionResult> GetFieldWeatherData([FromHeader(Name = "Authorization")] string authorization, int fieldId, int day)
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

            // Проверка участка на существование и на доступ к нему
            if (field == null)
            {
                // Если поле не найдено или не принадлежит пользователю, проверим доступ к редактированию
                var hasEditAccess = _context.AccessList.Any(a => a.FieldId == fieldId && a.UserLogin == login && a.Type == AccessType.View);

                if (!hasEditAccess)
                {
                    return NotFound("Поле не найдено или пользователь не имеет к нему доступа.");
                }

                field = _context.FieldList.FirstOrDefault(e => e.FieldId == fieldId);

                if (field == null)
                {
                    return NotFound("Поля не существует.");
                }
            }

            // Определяем нужную дату
            var targetDate = DateTime.UtcNow.AddDays(day).Date;

            // Запрос к OpenWeatherMap API
            var apiKey = "d70ee2c7097a6708357e647b25b32615";
            var lat = field.Latitude;
            var lon = field.Longitude;
            var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={apiKey}&units=metric"; // Добавляем units=metric для получения температуры в градусах Цельсия

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Ошибка при получении данных о погоде.");
                }

                var weatherData = await response.Content.ReadAsStringAsync();

                // Парсим ответ и извлекаем необходимые данные
                var weatherForecast = JsonConvert.DeserializeObject<WeatherForecastResponse>(weatherData);
                if (weatherForecast == null || weatherForecast.List == null)
                {
                    return StatusCode(500, "Ошибка при обработке данных о погоде.");
                }

                // Фильтруем данные по targetDate и преобразуем DtTxt в DateTime
                var filteredData = weatherForecast.List
                    .Select(item => new
                    {
                        DateTime = DateTime.Parse(item.dt_txt),
                        item.Main.Temp,
                        item.Wind.Speed,
                        item.Wind.Deg,
                        Description = item.Weather.FirstOrDefault()?.Description,
                        Precipitation = item.Rain?.Volume ?? 0,
                        ICV = item.Clouds?.All ?? 0
                    })
                    .Where(item => item.DateTime.Date == targetDate && item.DateTime.Hour % 3 == 0)
                    .ToList();

                var result = filteredData
                    .Select(item => new
                    {
                        Time = item.DateTime.ToString("HH:mm"),
                        Temperature = item.Temp,
                        WindSpeed = item.Speed,
                        WindDirection = item.Deg,
                        Description = item.Description
                    })
                    .ToList();

                var averageTemperature = filteredData.Average(item => item.Temp);
                var averageWindSpeed = filteredData.Average(item => item.Speed);
                var averagePrecipitation = filteredData.Average(item => item.Precipitation);
                var averageICV = filteredData.Average(item => item.ICV);

                return Ok(new
                {
                    WeatherData = result,
                    Averages = new
                    {
                        Temperature = averageTemperature,
                        WindSpeed = averageWindSpeed,
                        Precipitation = averagePrecipitation,
                        ICV = averageICV
                    }
                });
            }
        }

        [HttpPost("AddField")]
        public async Task<IActionResult> AddField([FromHeader(Name = "Authorization")] string authorization, [FromForm] FieldModel field)
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

            // Устанавливаем ID владельца для поля
            field.OwnerLogin = login;

            // Добавляем новое поле в базу данных
            _context.FieldList.Add(field);
            await _context.SaveChangesAsync();

            // Перезагружаем страницу со списком полей
            return Ok();
        }

        [HttpPost("UpdateField")]
        public async Task<IActionResult> UpdateField([FromHeader(Name = "Authorization")] string authorization, [FromForm] FieldModel updatedField)
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
            var field = _context.FieldList.FirstOrDefault(e => e.FieldId == updatedField.FieldId && e.OwnerLogin == login);

            // Проверка участка на существование и на доступ к нему
            if (field == null)
            {
                // Если поле не найдено или не принадлежит пользователю, проверим доступ к редактированию
                var hasEditAccess = _context.AccessList.Any(a => a.FieldId == updatedField.FieldId && a.UserLogin == login && a.Type == AccessType.Edit);

                if (!hasEditAccess)
                {
                    return NotFound("Поле не найдено или пользователь не имеет к нему доступа.");
                }

                field = _context.FieldList.FirstOrDefault(e => e.FieldId == updatedField.FieldId);

                if (field == null)
                {
                    return NotFound("Поля не существует.");
                }
            }

            field.Name = updatedField.Name;
            field.Type = updatedField.Type;
            field.Description = updatedField.Description;
            field.Latitude = updatedField.Latitude;
            field.Longitude = updatedField.Longitude;

            _context.FieldList.Update(field);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("DeleteField")]
        public async Task<IActionResult> DeleteAccess([FromHeader(Name = "Authorization")] string authorization, int fieldId)
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

            // Проверка участка на существование и на доступ к нему
            if (field == null)
            {
                // Если поле не найдено или не принадлежит пользователю, проверим доступ к редактированию
                var hasEditAccess = _context.AccessList.Any(a => a.FieldId == fieldId && a.UserLogin == login && a.Type == AccessType.Edit);

                if (!hasEditAccess)
                {
                    return NotFound("Поле не найдено или пользователь не имеет к нему доступа.");
                }

                field = _context.FieldList.FirstOrDefault(e => e.FieldId == fieldId);

                if (field == null)
                {
                    return NotFound("Поля не существует.");
                }
            }

            // Удаление всех AccessModel с соответствующим fieldId
            var accessEntries = _context.AccessList.Where(a => a.FieldId == fieldId).ToList();
            _context.AccessList.RemoveRange(accessEntries);

            // Удаление самого поля
            _context.FieldList.Remove(field);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
