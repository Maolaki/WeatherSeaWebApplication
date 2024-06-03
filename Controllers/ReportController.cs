using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql.Internal;
using WeatherSeaWebApplication.Models;
using static WeatherSeaWebApplication.Controllers.FieldController;

namespace WeatherSeaWebApplication.Controllers
{
    [Route("Report")]
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ModulesContext _context;
        private readonly IdentificationResponseHandler _identificationResponseHandler;

        // Инициализируйте _context через конструктор
        public ReportController(ModulesContext context, IdentificationResponseHandler identificationResponseHandler)
        {
            _context = context;
            _identificationResponseHandler = identificationResponseHandler;
        }

        [AllowAnonymous]
        [HttpGet("ReportList")]
        public IActionResult ReportList()
        {
            return View();
        }

        [HttpGet("GetReports")]
        public IActionResult GetReports([FromHeader(Name = "Authorization")] string authorization)
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

            // Получаем поля, принадлежащие текущему пользователю
            var userFields = _context.FieldList.Where(f => f.OwnerLogin == login).Select(f => f.FieldId);

            // Получаем отчеты, связанные с полями текущего пользователя
            var userReports = _context.ReportList.Where(r => userFields.Contains(r.FieldId));

            return Ok(userReports);
        }

        [HttpGet("GetPlants")]
        public IActionResult GetPlants([FromHeader(Name = "Authorization")] string authorization)
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

            // Получаем поля, принадлежащие текущему пользователю
            var plants = _context.EntityList.Where(e => e.Origin == EntityOrigin.Standart && e.Class == EntityClass.Plant || e.OwnerLogin == login && e.Class == EntityClass.Plant).ToList();

            return Ok(plants);
        }

        [HttpGet("GetAnimals")]
        public IActionResult GetAnimals([FromHeader(Name = "Authorization")] string authorization)
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

            // Получаем поля, принадлежащие текущему пользователю
            var animals = _context.EntityList.Where(e => e.Origin == EntityOrigin.Standart && e.Class == EntityClass.Animal || e.OwnerLogin == login && e.Class == EntityClass.Animal).ToList();

            return Ok(animals);
        }

        [HttpGet("GetFields")]
        public IActionResult GetFields([FromHeader(Name = "Authorization")] string authorization)
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

            // Получаем поля, принадлежащие текущему пользователю
            var fields = _context.FieldList.Where(e => e.OwnerLogin == login).ToList();

            return Ok(fields);
        }

        [HttpGet("GetReport")]
        public IActionResult GetReport([FromHeader(Name = "Authorization")] string authorization, int reportId)
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



            return Ok();
        }

        [HttpGet("DownloadReport")]
        public IActionResult DownloadReport([FromHeader(Name = "Authorization")] string authorization, int reportId)
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

            // Найдите отчет по ID
            var report = _context.ReportList.FirstOrDefault(r => r.Id == reportId);

            if (report == null)
            {
                return NotFound("Отчет не найден.");
            }

            // Проверьте права доступа пользователя (если нужно) потом

            // Путь к файлу PDF
            string pdfPath = Path.Combine("E:/Семестр4/Курсач/pdfs", $"{report.Id}.pdf");

            if (!System.IO.File.Exists(pdfPath))
            {
                return NotFound("Файл не найден.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(pdfPath);
            return File(fileBytes, "application/pdf", $"{report.Name}.pdf");
        }

        [HttpPost("AddReport")]
        public async Task<IActionResult> AddReport([FromHeader(Name = "Authorization")] string authorization, [FromForm] ReportModel report, int plantId = 0, int animalId = 0)
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

            // Добавляем новый отчет в базу данных
            _context.ReportList.Add(report);
            await _context.SaveChangesAsync();

            // Получаем данные поля, связанного с отчетом
            var field = _context.FieldList.FirstOrDefault(f => f.FieldId == report.FieldId);
            if (field == null)
            {
                return NotFound("Поле не найдено.");
            }

            // Запрос к OpenWeatherMap API для получения данных о погоде
            var apiKey = "d70ee2c7097a6708357e647b25b32615";
            var lat = field.Latitude;
            var lon = field.Longitude;
            var url = $"https://api.openweathermap.org/data/2.5/forecast?lat={lat}&lon={lon}&appid={apiKey}&units=metric";

            WeatherForecastResponse weatherForecast;

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Ошибка при получении данных о погоде.");
                }

                var weatherData = await response.Content.ReadAsStringAsync();
                weatherForecast = JsonConvert.DeserializeObject<WeatherForecastResponse>(weatherData);

                if (weatherForecast == null || weatherForecast.List == null)
                {
                    return StatusCode(500, "Ошибка при обработке данных о погоде.");
                }
            }

            // Путь для сохранения PDF файла
            string pdfPath = Path.Combine("E:/Семестр4/Курсач/pdfs", $"{report.Id}.pdf");

            // Создаем PDF документ
            using (var stream = new FileStream(pdfPath, FileMode.Create))
            {
                using (var document = new Document())
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();

                    // Загружаем шрифт, поддерживающий кириллицу
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    var font6 = new Font(baseFont, 8, Font.NORMAL);
                    var font14 = new Font(baseFont, 14, Font.NORMAL);
                    var font20 = new Font(baseFont, 20, Font.BOLD);

                    // Добавляем основную информацию об отчете
                    document.Add(new Paragraph($"ОТЧЁТ №-{report.Id}", font6));
                    document.Add(new Paragraph($"Об отчете", font20) { Alignment = Element.ALIGN_CENTER });
                    document.Add(new Paragraph($"Название отчета: {report.Name}", font14));
                    document.Add(new Paragraph($"ID поля: {field.FieldId}", font14));
                    document.Add(new Paragraph($"Тип отчета: {report.Type}", font14));

                    // Добавляем информацию о погоде
                    document.Add(new Paragraph($"Погода", font20) { Alignment = Element.ALIGN_CENTER });

                    // Пример извлечения данных о погоде (можно адаптировать под вашу структуру данных)
                    var currentWeather = weatherForecast.List.First();
                    document.Add(new Paragraph($"Температура: {currentWeather.Main.Temp} °C", font14));
                    document.Add(new Paragraph($"Влажность: {currentWeather.Main.Humidity} %", font14));
                    document.Add(new Paragraph($"Скорость ветра: {currentWeather.Wind.Speed} м/с", font14));

                    // Добавляем информацию о сущности (растении или животном) в зависимости от типа отчета
                    if (report.Type == ReportType.Plant || report.Type == ReportType.Combined)
                    {
                        var plant = _context.EntityList.FirstOrDefault(e => e.Id == plantId && e.Class == EntityClass.Plant);
                        if (plant != null)
                        {
                            document.Add(new Paragraph($"Информация о растении", font20) { Alignment = Element.ALIGN_CENTER });
                            document.Add(new Paragraph($"Название: {plant.Name}", font14));
                            document.Add(new Paragraph($"Описание: {plant.Description}", font14));
                            document.Add(new Paragraph($"Рекомендуемая температура: {plant.RecommendedTemperature} °C", font14));
                            document.Add(new Paragraph($"Рекомендуемая влажность: {plant.RecommendedHumidity} %", font14));
                            document.Add(new Paragraph($"Рекомендуемая скорость ветра: {plant.RecommendedWindSpeed} м/с", font14));

                            // Сравнение рекомендуемых условий с текущими погодными условиями
                            document.Add(new Paragraph($"Соответствие текущих условий рекомендуемым:", font14));
                            document.Add(new Paragraph($"Температура: {currentWeather.Main.Temp} °C (Рекомендуемая: {plant.RecommendedTemperature} °C)", font14));
                            document.Add(new Paragraph($"Влажность: {currentWeather.Main.Humidity} % (Рекомендуемая: {plant.RecommendedHumidity} %)", font14));
                            document.Add(new Paragraph($"Скорость ветра: {currentWeather.Wind.Speed} м/с (Рекомендуемая: {plant.RecommendedWindSpeed} м/с)", font14));
                        }
                    }

                    if (report.Type == ReportType.Animal || report.Type == ReportType.Combined)
                    {
                        var animal = _context.EntityList.FirstOrDefault(e => e.Id == animalId && e.Class == EntityClass.Animal);
                        if (animal != null)
                        {
                            document.Add(new Paragraph($"Информация о животном", font20) { Alignment = Element.ALIGN_CENTER });
                            document.Add(new Paragraph($"Название: {animal.Name}", font14));
                            document.Add(new Paragraph($"Описание: {animal.Description}", font14));
                            document.Add(new Paragraph($"Рекомендуемая температура: {animal.RecommendedTemperature} °C", font14));
                            document.Add(new Paragraph($"Рекомендуемая влажность: {animal.RecommendedHumidity} %", font14));
                            document.Add(new Paragraph($"Рекомендуемая скорость ветра: {animal.RecommendedWindSpeed} м/с", font14));

                            // Сравнение рекомендуемых условий с текущими погодными условиями
                            document.Add(new Paragraph($"Соответствие текущих условий рекомендуемым:", font14));
                            document.Add(new Paragraph($"Температура: {currentWeather.Main.Temp} °C (Рекомендуемая: {animal.RecommendedTemperature} °C)", font14));
                            document.Add(new Paragraph($"Влажность: {currentWeather.Main.Humidity} % (Рекомендуемая: {animal.RecommendedHumidity} %)", font14));
                            document.Add(new Paragraph($"Скорость ветра: {currentWeather.Wind.Speed} м/с (Рекомендуемая: {animal.RecommendedWindSpeed} м/с)", font14));
                        }
                    }
                }
            }

            return Ok();
        }



        [HttpDelete("DeleteReport")]
        public async Task<IActionResult> DeleteReport([FromHeader(Name = "Authorization")] string authorization, int reportId)
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
            var report = _context.ReportList.FirstOrDefault(e => e.Id == reportId);

            // Проверка участка на существование и на доступ к нему
            if (report == null)
            {
                return NotFound("Отчета не существует.");
            }

            // Удаление самого поля
            _context.ReportList.Remove(report);
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
