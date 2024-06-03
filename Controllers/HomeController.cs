using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherSeaWebApplication.Models;

namespace WeatherSeaWebApplication.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("PriceInfo")]
        public IActionResult PriceInfo()
        {
            return View();
        }
    }
}
