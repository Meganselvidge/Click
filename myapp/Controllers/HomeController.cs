using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using myapp.Models;
using myapp.Services;

namespace myapp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CounterService _counter;

    public HomeController(ILogger<HomeController> logger, CounterService counter)
    {
        _logger = logger;
        _counter = counter;
    }

   
    public IActionResult Index()
    {
        var (value, lastInst, lastAt) = _counter.ReadAsync().GetAwaiter().GetResult();
        ViewData["Counter"] = value;
        ViewData["LastWriter"] = lastInst;
        ViewData["LastAt"] = lastAt;
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
    
    [HttpPost]
    public async Task<IActionResult> Increment()
    {
        var (value, inst, at) = await _counter.IncrementAsync();
        HttpContext.Session.SetInt32("Counter", value);
        HttpContext.Session.SetString("LastWriter", inst);
        HttpContext.Session.SetString("LastAt", at);
        return RedirectToAction(nameof(Index));
    }
}