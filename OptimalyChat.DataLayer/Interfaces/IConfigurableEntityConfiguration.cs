using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OptimalyChat.DataLayer.Interfaces;

/// <summary>
/// Enhanced entity configuration interface following CRM pattern
/// Provides structured approach to EF Core entity configuration with separation of concerns
/// </summary>
/// <typeparam name="T">Entity type to configure</typeparam>
public interface IConfigurableEntityConfiguration<T> where T : class
{
    /// <summary>
    /// Configure basic entity properties (columns, types, lengths, required fields)
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    void ConfigureEntity(EntityTypeBuilder<T> builder);
    
    /// <summary>
    /// Configure relationships (foreign keys, navigation properties, cascading)
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    void ConfigureRelationships(EntityTypeBuilder<T> builder);
    
    /// <summary>
    /// Configure indexes for performance optimization
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    void ConfigureIndexes(EntityTypeBuilder<T> builder);
    
    /// <summary>
    /// Seed initial data for development and testing
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    void SeedData(EntityTypeBuilder<T> builder);
}