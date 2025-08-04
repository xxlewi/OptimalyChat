using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OptimalyChat.PresentationLayer.Models;

namespace OptimalyChat.PresentationLayer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Redirect authenticated users to Chat
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Chat");
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
