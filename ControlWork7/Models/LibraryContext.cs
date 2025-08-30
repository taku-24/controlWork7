using Microsoft.EntityFrameworkCore;

namespace ControlWork7.Models;

public class LibraryContext : DbContext
{
    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options){}

    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Issue> Issues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Issue>()
            .HasIndex(i => new { i.BookId, i.ReturnedAt })
            .HasFilter("\"ReturnedAt\" IS NULL")
            .IsUnique();
    }

}
