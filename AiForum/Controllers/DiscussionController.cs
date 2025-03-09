using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiForum.Data;
using AiForum.Models;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AiForum.Controllers
{
    public class DiscussionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscussionController> _logger;

        public DiscussionController(ApplicationDbContext context, ILogger<DiscussionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Discussion
        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussions.ToListAsync();
            Console.WriteLine($"Found {discussions.Count} discussions in database.");
            return View(discussions);
        }

        // GET: Discussion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions
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
        public async Task<IActionResult> Create([Bind("Title,Content,ImageFilename")] Discussion discussion, IFormFile? ImageFile)
        {
            // Check what's being received
            _logger.LogInformation("Received discussion: Title={Title}, Content={ContentPreview}...",
                discussion.Title,
                discussion.Content?.Substring(0, Math.Min(20, discussion.Content?.Length ?? 0)));

            // Remove any validation errors related to ImageFilename
            ModelState.Remove("ImageFilename");

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        _logger.LogInformation("Processing image: {FileName}, size: {FileSize} bytes",
                            ImageFile.FileName, ImageFile.Length);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                            _logger.LogInformation("Created directory: {UploadPath}", uploadPath);
                        }

                        string filePath = Path.Combine(uploadPath, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        discussion.ImageFilename = uniqueFileName;
                        _logger.LogInformation("Image saved as: {ImageFilename}", uniqueFileName);
                    }
                    else
                    {
                        _logger.LogInformation("No image file uploaded");
                        // If ImageFilename is non-nullable in the database, set a default value
                        discussion.ImageFilename = string.Empty;
                    }

                    // Explicitly initialize Comments if it's null
                    if (discussion.Comments == null)
                    {
                        discussion.Comments = new List<Comment>();
                    }

                    _context.Add(discussion);

                    _logger.LogInformation("Attempting to save discussion to database...");
                    int result = await _context.SaveChangesAsync();
                    _logger.LogInformation("SaveChangesAsync returned: {RowsAffected}", result);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving discussion: {ErrorMessage}", ex.Message);
                    ModelState.AddModelError("", "Error saving to database: " + ex.Message);
                }
            }
            else
            {
                // Log model validation errors
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        _logger.LogWarning("Validation error for {PropertyName}: {ErrorMessages}",
                            key,
                            string.Join(", ", state.Errors.Select(e => e.ErrorMessage)));
                    }
                }
            }

            return View(discussion);
        }

        //// GET: Discussion/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var discussion = await _context.Discussions.FindAsync(id);
        //    if (discussion == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(discussion);
        //}

        //// POST: Discussion/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("DiscussionId,Title,Content,ImageFilename,CreateDate")] Discussion discussion)
        //{
        //    if (id != discussion.DiscussionId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(discussion);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!DiscussionExists(discussion.DiscussionId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(discussion);
        //}

        //// GET: Discussion/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var discussion = await _context.Discussions
        //        .FirstOrDefaultAsync(m => m.DiscussionId == id);
        //    if (discussion == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(discussion);
        //}

        //// POST: Discussion/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var discussion = await _context.Discussions.FindAsync(id);
        //    if (discussion != null)
        //    {
        //        _context.Discussions.Remove(discussion);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool DiscussionExists(int id)
        //{
        //    return _context.Discussions.Any(e => e.DiscussionId == id);
        //}
    }
}
