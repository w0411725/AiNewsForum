using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiForum.Models
{
    public class Discussion
    {
        public int DiscussionId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string? ImageFilename { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public List<Comment> Comments { get; set; } = new();
    }
}
