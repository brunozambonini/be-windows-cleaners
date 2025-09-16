using Microsoft.EntityFrameworkCore;
using be_windows_cleaners.Models;

namespace be_windows_cleaners.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.Type).IsRequired().HasConversion<string>();
                
                // Ensure email is unique
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configure Image entity
            modelBuilder.Entity<Image>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Created_At).IsRequired();
                entity.Property(e => e.ImageData).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                
                // Configure relationship: User has many Images, Image belongs to one User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Images)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
