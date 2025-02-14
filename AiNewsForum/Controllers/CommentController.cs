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
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Comment/Create
        public IActionResult Create(int discussionId)
        {
            var comment = new Comment { DiscussionId = discussionId };
            return View(comment);
        }

        // POST: Comment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,DiscussionId")] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return View(comment);
            }

            comment.CreateDate = DateTime.Now;
            _context.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetDiscussion", "Home", new { id = comment.DiscussionId });
        }
    }
}
