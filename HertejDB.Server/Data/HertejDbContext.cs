using HertejDB.Common;
using Microsoft.EntityFrameworkCore;

namespace HertejDB.Server.Data; 

public class HertejDbContext : DbContext {
	public DbSet<Image> Images { get; set; }
	public DbSet<ImageRating> ImageRatings { get; set; }

	public HertejDbContext(DbContextOptions<HertejDbContext> dbco) : base(dbco) { }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.Entity<Image>().HasIndex(image => image.RatingStatus);
		modelBuilder.Entity<Image>().HasIndex(image => image.Category);
		modelBuilder.Entity<Image>().HasIndex(image => new {image.RatingStatus, image.Category});
		modelBuilder.Entity<Image>().OwnsOne(image => image.SourceAttribution);
		
		modelBuilder.Entity<ImageRating>().HasKey(ir => new {ir.ImageId, ir.UserId});
		modelBuilder.Entity<ImageRating>().HasIndex(ir => ir.UserId);
		
	}
}
