using Microsoft.EntityFrameworkCore;
using EIMS.Shared.Models;
using System.Text.Json;

namespace EIMS.Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Part> Parts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Configure dictionary properties with JSON conversion
            entity.Property(e => e.Dimensions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null));
            
            entity.Property(e => e.TechnicalSpecs)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null));
            
            entity.Property(e => e.PhysicalSpecs)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null));

            // Configure list properties with JSON conversion
            entity.Property(e => e.UsedInProjects)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
            
            entity.Property(e => e.UsedInMetaParts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
            
            entity.Property(e => e.CadKeys)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
            
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));

            // Configure Documents as owned type collection
            entity.OwnsMany(e => e.Documents, d =>
            {
                d.WithOwner().HasForeignKey("PartId");
                d.Property<int>("Id").ValueGeneratedOnAdd();
                d.HasKey("Id");
            });

            // Configure Substitutes as many-to-many relationship
            entity.HasMany(e => e.Substitutes)
                .WithMany()
                .UsingEntity(j => j.ToTable("PartSubstitutes"));
        });
    }
} 