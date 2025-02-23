using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LMS.Models;
using LMS.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: ClassRooms
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.ClassRooms.Include(c => c.Topic);
        return View(await applicationDbContext.ToListAsync());
    }

    [Route("template")]
    public IActionResult Template()
    {
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
