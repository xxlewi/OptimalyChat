using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        
        // Primary key
        builder.HasKey(p => p.Id);
        
        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Description)
            .HasMaxLength(1000);
            
        builder.Property(p => p.IsEncrypted)
            .HasDefaultValue(false);
            
        builder.Property(p => p.EncryptionLevel)
            .HasDefaultValue(EncryptionLevel.None)
            .HasConversion<int>();
            
        builder.Property(p => p.EncryptionKeyId)
            .HasMaxLength(500);
            
        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(450); // ASP.NET Identity default
            
        // Indexes
        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_Projects_UserId");
            
        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Projects_Name");
            
        // Relationships
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(p => p.Conversations)
            .WithOne(c => c.Project)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(p => p.Documents)
            .WithOne(d => d.Project)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Seed data will be added after users are created
    }
}