using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiNewsForum.Models;

namespace AiNewsForum.Controllers
{
    public class DiscussionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscussionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Discussion
        public async Task<IActionResult> Index()
        {
            return View(await _context.Discussions.ToListAsync());
        }

        // GET: Discussion/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Discussion/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Discussion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discussion discussion, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload if an image was provided
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Make sure wwwroot or images directory exists
                    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    // Generate a unique filename (GUID + original extension)
                    string uniqueFilename = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(imagesPath, uniqueFilename);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Save filename to the database
                    discussion.ImageFilename = uniqueFilename;
                }

                _context.Add(discussion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(discussion);
        }
    }
}
