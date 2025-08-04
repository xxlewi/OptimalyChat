using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;

namespace OptimalyChat.DataLayer.Configurations;

/// <summary>
/// Base configuration class for entities inheriting from BaseEntity
/// Provides common audit field configuration following CRM pattern
/// </summary>
/// <typeparam name="TEntity">Entity type inheriting from BaseEntity</typeparam>
public abstract class BaseConfigurableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>, IConfigurableEntityConfiguration<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Main EF Core configuration method - orchestrates all configuration steps
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ConfigureBaseEntity(builder);
        ConfigureEntity(builder);
        ConfigureRelationships(builder);
        ConfigureIndexes(builder);
        SeedData(builder);
    }
    
    /// <summary>
    /// Configure common BaseEntity properties (audit fields, soft delete)
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    protected virtual void ConfigureBaseEntity(EntityTypeBuilder<TEntity> builder)
    {
        // Primary key
        builder.HasKey(e => e.Id);
        
        // Audit fields configuration with UTC timestamps
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'")
            .HasConversion(
                v => v.ToUniversalTime(), // Při ukládání převést na UTC
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Při načítání označit jako UTC
            );
            
        builder.Property(e => e.UpdatedAt)
            .IsRequired(false)
            .HasConversion(
                v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null
            );
            
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(450)
            .IsRequired(false);
            
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(450)
            .IsRequired(false);
            
        // Soft delete configuration
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
            
        builder.Property(e => e.DeletedAt)
            .IsRequired(false)
            .HasConversion(
                v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null
            );
            
        builder.Property(e => e.DeletedBy)
            .HasMaxLength(450)
            .IsRequired(false);
        
        // Global query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
        
        // Indexes for performance
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.IsDeleted);
        builder.HasIndex(e => new { e.IsDeleted, e.CreatedAt });
    }
    
    /// <summary>
    /// Configure entity-specific properties - to be implemented by derived classes
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
    
    /// <summary>
    /// Configure relationships - to be implemented by derived classes
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public virtual void ConfigureRelationships(EntityTypeBuilder<TEntity> builder)
    {
        // Default: no relationships
    }
    
    /// <summary>
    /// Configure indexes - to be implemented by derived classes
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public virtual void ConfigureIndexes(EntityTypeBuilder<TEntity> builder)
    {
        // Default: no additional indexes beyond BaseEntity
    }
    
    /// <summary>
    /// Seed data - to be implemented by derived classes
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    public virtual void SeedData(EntityTypeBuilder<TEntity> builder)
    {
        // Default: no seed data
    }
}