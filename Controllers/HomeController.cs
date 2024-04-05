using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
