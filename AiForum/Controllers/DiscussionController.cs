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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AiForum.Controllers
{
    [Authorize] // Ensures only logged-in users can access "My Threads"
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation($"Fetching threads for User ID: {userId}");

            var discussion = await _context.Discussions
                //Filter to only the logged -in user's discussions
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();

            _logger.LogInformation($"Found {discussion.Count} discussions for this user");

            return View(discussion);
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
            // Comprehensive authentication and user verification
            _logger.LogInformation("Authentication Context Diagnostic:");

            // Log all available claims
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"Claim - Type: {claim.Type}, Value: {claim.Value}");
            }

            // Retrieve user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Retrieved User ID: {userId ?? "NULL"}");

            // Verify user exists in database
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                _logger.LogWarning($"No user found with ID: {userId}");
                return Unauthorized();
            }

            // Enhanced logging for diagnostic purposes
            _logger.LogInformation("Create Discussion Attempt");
            _logger.LogInformation($"Title: {discussion.Title}");
            _logger.LogInformation($"Content: {discussion.Content}");

            // Remove any validation errors related to ImageFilename
            ModelState.Remove("ImageFilename");

            // Explicitly check model state
            if (!ModelState.IsValid)
            {
                // Log specific validation errors
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Model Validation Failed. Errors: {Errors}", string.Join(", ", errors));
                return View(discussion);
            }


            try
            {
                // Log user authentication details
                _logger.LogInformation($"User ID: {userId ?? "Not Found"}");

                // Validate user authentication
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized discussion creation attempt");
                    return Unauthorized();
                }

                // Populate discussion details
                discussion.UserId = userId;
                discussion.CreateDate = DateTime.UtcNow;

                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    try
                    {
                        // Generate unique filename
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);

                        // Ensure upload directory exists
                        string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                        Directory.CreateDirectory(uploadPath);

                        // Save file
                        string filePath = Path.Combine(uploadPath, uniqueFileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }

                        discussion.ImageFilename = uniqueFileName;
                        _logger.LogInformation($"Image saved: {uniqueFileName}");
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Image upload failed");
                        ModelState.AddModelError("ImageFile", "Failed to upload image");
                        return View(discussion);
                    }
                }

                // Save to database with explicit tracking
                _context.Discussions.Add(discussion);
                int saveResult = await _context.SaveChangesAsync();

                _logger.LogInformation($"Discussion saved. Rows affected: {saveResult}");

                // Redirect to home page or discussion index
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Comprehensive error logging
                _logger.LogError(ex, $"Unexpected error in Create method: {ex.Message}");

                // Add a generic error message
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while saving the discussion.");

                return View(discussion);
            }
        }


        // GET: Discussion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions.FindAsync(id);
            if (discussion == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Restrict access: Only allow the owner to edit
            if (discussion.UserId != userId)
            {
                return NotFound();
            }
            return View(discussion);
        }

        // POST: Discussion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DiscussionId,Title,Content,ImageFilename,CreateDate")] Discussion discussion)
        {
            if (id != discussion.DiscussionId)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingDiscussion = await _context.Discussions.FindAsync(id);

            // Ensure only the owner can edit
            if (existingDiscussion == null || existingDiscussion.UserId != userId)
            {
                return Forbid(); // Return HTTP 403 Forbidden
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Only allow updating permitted fields
                    existingDiscussion.Title = discussion.Title;
                    existingDiscussion.Content = discussion.Content;
                    existingDiscussion.ImageFilename = discussion.ImageFilename;

                    _context.Update(existingDiscussion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscussionExists(discussion.DiscussionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(discussion);
        }


        // GET: Discussion/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Discussion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var discussion = await _context.Discussions.FindAsync(id);

            // Ensure only the owner can delete
            if (discussion == null || discussion.UserId != userId)
            {
                return Forbid(); // Return HTTP 403 Forbidden
            }

            _context.Discussions.Remove(discussion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool DiscussionExists(int id)
        {
            return _context.Discussions.Any(e => e.DiscussionId == id);
        }
    }
}
