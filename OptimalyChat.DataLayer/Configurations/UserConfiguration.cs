using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Základní konfigurace (převezme od IdentityUser)
        builder.ToTable("Users");
        
        // Naše vlastní properties
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired(false);
            
        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired(false);
            
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);
            
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(u => u.LastLoginAt)
            .IsRequired(false);
            
        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexy
        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");
            
        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");
            
        builder.HasIndex(u => new { u.FirstName, u.LastName })
            .HasDatabaseName("IX_Users_FullName");

        // Poznámka: Query filter pro soft delete je aplikován globálně v DbContext
    }
}