using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AiForum.Models;
using AiForum.Data;
using Microsoft.EntityFrameworkCore;

namespace AiForum.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get all discussions ordered by CreateDate (newest first)
        var discussions = await _context.Discussions
            .Include(d => d.Comments)
            .OrderByDescending(d => d.CreateDate)
            .ToListAsync();

        return View(discussions);
    }

    public async Task<IActionResult> GetDiscussion(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var discussion = await _context.Discussions
            .Include(d => d.Comments)
            .FirstOrDefaultAsync(m => m.DiscussionId == id);

        if (discussion == null)
        {
            return NotFound();
        }

        return View(discussion);
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
