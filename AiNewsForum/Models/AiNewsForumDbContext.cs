using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AiNewsForum.Models;

public partial class AiNewsForumDbContext : DbContext
{
    public AiNewsForumDbContext()
    {
    }

    public AiNewsForumDbContext(DbContextOptions<AiNewsForumDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Discussion> Discussions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\ProjectModels;Database=AiNewsForumDb;Trusted_Connection=True;MultipleActiveResultSets=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(e => e.DiscussionId, "IX_Comments_DiscussionId");

            entity.HasOne(d => d.Discussion).WithMany(p => p.Comments).HasForeignKey(d => d.DiscussionId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
