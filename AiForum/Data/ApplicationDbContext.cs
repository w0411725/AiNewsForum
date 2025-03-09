using Microsoft.EntityFrameworkCore;
using AiForum.Models;

namespace AiForum.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Discussion)
                .WithMany(d => d.Comments)
                .HasForeignKey(c => c.DiscussionId);
        }
    }
}
