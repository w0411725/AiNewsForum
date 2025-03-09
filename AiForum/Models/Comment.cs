using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AiForum.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public int DiscussionId { get; set; }
        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }

        // Navigation Properties
        [ForeignKey("DiscussionId")]
        public virtual Discussion? Discussion { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}

