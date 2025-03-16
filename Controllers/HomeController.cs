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
    public async Task<IActionResult> Index(string searchString, string sortBy, string sortOrder)
    {
        var classRooms = await _context.ClassRooms
        .Include(c => c.Topic)
        .ToListAsync();

        var studentCounts = await _context.ClassDetails
            .GroupBy(cd => cd.ClassRoomId!)
            .Select(g => new { ClassRoomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassRoomId, x => x.Count);

        ViewBag.MembersCount = studentCounts;

        foreach (var classRoom in classRooms)
        {
            classRoom.Students = studentCounts.ContainsKey(classRoom.Id!) ? studentCounts[classRoom.Id!] : 0;
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            classRooms = classRooms.Where(c => c.Name!.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        classRooms = sortBy switch
        {
            "name" => sortOrder == "asc" ? classRooms.OrderBy(c => c.Name).ToList() : classRooms.OrderByDescending(c => c.Name).ToList(),
            "price" => sortOrder == "asc" ? classRooms.OrderBy(c => c.Price).ToList() : classRooms.OrderByDescending(c => c.Price).ToList(),
            "students" => sortOrder == "asc" ? classRooms.OrderBy(c => c.Students).ToList() : classRooms.OrderByDescending(c => c.Students).ToList(),
            _ => classRooms.OrderByDescending(c => c.CreateDate).ToList(), // Mặc định: Lớp mới nhất lên đầu
        };

        return View(classRooms);
    }

    [HttpPost]
    public async Task<IActionResult> JoinClass(string code)
    {
        var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(x => x.Code == code);
        if (classRoom != null)
        {
            return RedirectToAction("Introduction", "ClassRooms", new { id = classRoom!.Id });
        }
        return RedirectToAction(nameof(Index),"Home");
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
