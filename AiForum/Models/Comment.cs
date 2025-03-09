using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiForum.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int DiscussionId { get; set; }

        // Navigation Property
        [ForeignKey("DiscussionId")]
        public Discussion? Discussion { get; set; }
    }
}
