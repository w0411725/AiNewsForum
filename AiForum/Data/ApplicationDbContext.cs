using Microsoft.EntityFrameworkCore;
using AiForum.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AiForum.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            // Optional: Enable more detailed database logging
            // This can help diagnose relationship and query issues
        }

        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Essential base configuration for Identity tables
            base.OnModelCreating(builder);

            // Ensure IdentityUser is NOT being added separately
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers"); // Maps to existing Identity table

            // Simplified relationship configuration
            builder.Entity<Discussion>()
                .HasOne(d => d.User)           // Each discussion has one user
                .WithMany()                    // User can have multiple discussions
                .HasForeignKey(d => d.UserId)  // Use the explicit foreign key
                .OnDelete(DeleteBehavior.Restrict);

            // Existing Comment relationship
            builder.Entity<Discussion>()
                .HasMany(d => d.Comments)
                .WithOne(c => c.Discussion)
                .HasForeignKey(c => c.DiscussionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
