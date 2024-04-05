using Microsoft.AspNetCore.Mvc;

namespace WeatherSeaWebApplication.Controllers
{
    public class ModulesController : Controller
    {
        public IActionResult FieldList()
        {
            return View();
        }

        public IActionResult FieldInfo()
        {
            return View();
        }
    }
}
