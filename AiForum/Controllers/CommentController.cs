using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiForum.Data;
using AiForum.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AiForum.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ApplicationDbContext context, ILogger<CommentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Comment/Create
        public IActionResult Create(int discussionId)
        {
            _logger.LogInformation("GET Create action called with discussionId: {DiscussionId}", discussionId);

            // Verify that the discussion exists
            var discussionExists = _context.Discussions.Any(d => d.DiscussionId == discussionId);
            if (!discussionExists)
            {
                _logger.LogWarning("Attempted to create comment for non-existent discussion ID: {DiscussionId}", discussionId);
                return NotFound();
            }

            var comment = new Comment { DiscussionId = discussionId };
            ViewData["DiscussionId"] = discussionId;
            return View(comment);
        }

        // POST: Comment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,DiscussionId")] Comment comment)
        {
            _logger.LogInformation("POST Create action called with Content: {Content}, DiscussionId: {DiscussionId}",
                comment.Content, comment.DiscussionId);

            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation($"Retrieved User ID: {userId ?? "NULL"}");

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized comment submission attempt");
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed");
                ViewData["DiscussionId"] = comment.DiscussionId;
                return View(comment);
            }

            try
            {
                var discussionExists = await _context.Discussions.AnyAsync(d => d.DiscussionId == comment.DiscussionId);
                if (!discussionExists)
                {
                    _logger.LogWarning("Attempted to create comment for non-existent discussion ID: {DiscussionId}", comment.DiscussionId);
                    ModelState.AddModelError("DiscussionId", "The specified discussion does not exist.");
                    return View(comment);
                }

                // Assign user ID and creation date
                comment.UserId = userId;
                comment.CreateDate = DateTime.UtcNow;

                _context.Add(comment);
                var result = await _context.SaveChangesAsync();
                _logger.LogInformation("Comment successfully saved with ID: {CommentId}", comment.CommentId);

                return RedirectToAction("GetDiscussion", "Home", new { id = comment.DiscussionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving comment: {ErrorMessage}", ex.Message);
                ModelState.AddModelError("", $"Error saving comment: {ex.Message}");
                ViewData["DiscussionId"] = comment.DiscussionId;
                return View(comment);
            }
        }

        //// GET: Comment
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.Comments.Include(c => c.Discussion);
        //    return View(await applicationDbContext.ToListAsync());
        //}

        //// GET: Comment/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var comment = await _context.Comments
        //        .Include(c => c.Discussion)
        //        .FirstOrDefaultAsync(m => m.CommentId == id);
        //    if (comment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(comment);
        //}

        //// GET: Comment/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var comment = await _context.Comments.FindAsync(id);
        //    if (comment == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["DiscussionId"] = new SelectList(_context.Discussions, "DiscussionId", "Content", comment.DiscussionId);
        //    return View(comment);
        //}

        //// POST: Comment/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("CommentId,Content,CreateDate,DiscussionId")] Comment comment)
        //{
        //    if (id != comment.CommentId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(comment);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CommentExists(comment.CommentId))
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
        //    ViewData["DiscussionId"] = new SelectList(_context.Discussions, "DiscussionId", "Content", comment.DiscussionId);
        //    return View(comment);
        //}

        //// GET: Comment/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var comment = await _context.Comments
        //        .Include(c => c.Discussion)
        //        .FirstOrDefaultAsync(m => m.CommentId == id);
        //    if (comment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(comment);
        //}

        //// POST: Comment/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var comment = await _context.Comments.FindAsync(id);
        //    if (comment != null)
        //    {
        //        _context.Comments.Remove(comment);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool CommentExists(int id)
        //{
        //    return _context.Comments.Any(e => e.CommentId == id);
        //}
    }
}
