using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using System.Drawing;

namespace Task_6_C.Controllers
{

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["SERVER_ADRESS"] = Environment.GetEnvironmentVariable("SERVER_ADRESS");

            return View();
        }
    }
}
