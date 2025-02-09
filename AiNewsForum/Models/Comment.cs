using System;
using System.Collections.Generic;

namespace AiNewsForum.Models;

public partial class Comment
{
    public int CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; } = DateTime.Now;

    // Foreign Key
    public int DiscussionId { get; set; }

    // Navigation Property
    public virtual Discussion? Discussion { get; set; }
}
