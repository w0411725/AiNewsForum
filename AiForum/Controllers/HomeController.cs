using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AiForum.Models;
using AiForum.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AiForum.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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

    [Authorize] // Ensure only logged-in users can view profiles
    public async Task<IActionResult> Profile(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.Users
            .Where(u => u.Id == id)
            .Select(u => new ApplicationUser
            {
                Id = u.Id,
                Name = u.Name, 
                Location = u.Location, 
                ImageFilename = u.ImageFilename 
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        var userDiscussions = await _context.Discussions
            .Where(d => d.UserId == id)
            .OrderByDescending(d => d.CreateDate)
            .ToListAsync();

        var viewModel = new ProfileViewModel
        {
            User = user,
            Discussions = userDiscussions
        };

        return View(viewModel);
    }


    public async Task<IActionResult> GetDiscussion(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var discussion = await _context.Discussions
            .Include(d => d.User) // Load discussion author
            .Include(d => d.Comments)
                .ThenInclude(c => c.User) // Load comment authors
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
