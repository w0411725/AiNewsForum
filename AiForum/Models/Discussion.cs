using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiForum.Models
{
    public class Discussion
    {
        public int DiscussionId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 5000 characters")]
        public string Content { get; set; }

        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public string? ImageFilename { get; set; }




        // Navigation properties
        public List<Comment> Comments { get; set; } = new();
        public virtual ApplicationUser? User { get; set; }
    }
}
