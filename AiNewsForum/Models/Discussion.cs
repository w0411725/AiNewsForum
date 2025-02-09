using System;
using System.Collections.Generic;

namespace AiNewsForum.Models;

public partial class Discussion
{
    public int DiscussionId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? ImageFilename { get; set; }  // Nullable
    public DateTime CreateDate { get; set; } = DateTime.Now;

    // Navigation Property
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
