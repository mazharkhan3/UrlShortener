using Microsoft.EntityFrameworkCore;
using UrlShortener.Entities;

namespace UrlShortener;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortenedUrl>().HasIndex(x => x.Code).IsUnique();
    }
}