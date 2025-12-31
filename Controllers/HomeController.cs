using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nass.Models;

namespace Nass.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult login()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Logout()
        {
            // Optionally clear server-side session here if used
            return RedirectToAction("Index", "Home"); // redirect to login page
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Index(string email, string password, string UserType)
        {
            if (UserType == "Agency")
            {
                ViewBag.Message = $"Logging in as Agency: {email}";

                // Add your Agency login validation here
            }
            else if (UserType == "Customer")
            {
                ViewBag.Message = $"Logging in as Customer: {email}";
                // Add your Customer login validation here
            }

            return View();
        }

        // ✅ ADD THESE ACTIONS
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult Refund()
        {
            return View();
        }

    }
}
