using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.DAL.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Property> Properties { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<PropertyImage> PropertyImages { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<PropertyAmenity> PropertyAmenities { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Inquiry> Inquiries { get; set; }
    public DbSet<SiteSettings> SiteSettings { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Property configuration
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasOne(p => p.User)
                .WithMany(u => u.Properties)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Properties)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.PropertyType);
            entity.HasIndex(p => p.ListingType);
            entity.HasIndex(p => p.City);
        });
        
        // PropertyAmenity - Many to Many
        modelBuilder.Entity<PropertyAmenity>(entity =>
        {
            entity.HasKey(pa => new { pa.PropertyId, pa.AmenityId });
            
            entity.HasOne(pa => pa.Property)
                .WithMany(p => p.PropertyAmenities)
                .HasForeignKey(pa => pa.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(pa => pa.Amenity)
                .WithMany(a => a.PropertyAmenities)
                .HasForeignKey(pa => pa.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Favorite configuration
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(f => new { f.UserId, f.PropertyId }).IsUnique();
            
            entity.HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(f => f.Property)
                .WithMany(p => p.Favorites)
                .HasForeignKey(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Inquiry configuration
        modelBuilder.Entity<Inquiry>(entity =>
        {
            entity.HasOne(i => i.Sender)
                .WithMany(u => u.SentInquiries)
                .HasForeignKey(i => i.SenderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(i => i.Receiver)
                .WithMany(u => u.ReceivedInquiries)
                .HasForeignKey(i => i.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(i => i.Property)
                .WithMany(p => p.Inquiries)
                .HasForeignKey(i => i.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // PropertyImage configuration
        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.HasOne(pi => pi.Property)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Seed data
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Residential", Description = "Residential properties including houses and apartments", IconClass = "fa-home" },
            new Category { Id = 2, Name = "Commercial", Description = "Commercial properties for business use", IconClass = "fa-building" },
            new Category { Id = 3, Name = "Industrial", Description = "Industrial properties and warehouses", IconClass = "fa-industry" },
            new Category { Id = 4, Name = "Land", Description = "Vacant land and plots", IconClass = "fa-map" }
        );
        
        // Seed Amenities
        modelBuilder.Entity<Amenity>().HasData(
            new Amenity { Id = 1, Name = "Swimming Pool", IconClass = "fa-swimming-pool" },
            new Amenity { Id = 2, Name = "Garage", IconClass = "fa-car" },
            new Amenity { Id = 3, Name = "Garden", IconClass = "fa-tree" },
            new Amenity { Id = 4, Name = "Air Conditioning", IconClass = "fa-snowflake" },
            new Amenity { Id = 5, Name = "Heating", IconClass = "fa-fire" },
            new Amenity { Id = 6, Name = "Security System", IconClass = "fa-shield-alt" },
            new Amenity { Id = 7, Name = "Gym", IconClass = "fa-dumbbell" },
            new Amenity { Id = 8, Name = "Elevator", IconClass = "fa-elevator" },
            new Amenity { Id = 9, Name = "Balcony", IconClass = "fa-door-open" },
            new Amenity { Id = 10, Name = "Fireplace", IconClass = "fa-fire-alt" }
        );
    }
}
