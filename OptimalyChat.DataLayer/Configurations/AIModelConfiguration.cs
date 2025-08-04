using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Configurations;

public class AIModelConfiguration : IEntityTypeConfiguration<AIModel>
{
    public void Configure(EntityTypeBuilder<AIModel> builder)
    {
        builder.ToTable("AIModels");
        
        // Primary key
        builder.HasKey(m => m.Id);
        
        // Properties
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(m => m.ModelId)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(m => m.Provider)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(m => m.Endpoint)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(m => m.ApiKey)
            .HasMaxLength(500);
            
        builder.Property(m => m.MaxTokens)
            .HasDefaultValue(4096);
            
        builder.Property(m => m.Temperature)
            .HasDefaultValue(0.7);
            
        builder.Property(m => m.IsDefault)
            .HasDefaultValue(false);
            
        builder.Property(m => m.IsActive)
            .HasDefaultValue(true);
            
        builder.Property(m => m.CostPer1KInput)
            .HasPrecision(10, 6);
            
        builder.Property(m => m.CostPer1KOutput)
            .HasPrecision(10, 6);
            
        builder.Property(m => m.Capabilities)
            .HasColumnType("text");
            
        // Indexes
        builder.HasIndex(m => m.ModelId)
            .HasDatabaseName("IX_AIModels_ModelId");
            
        builder.HasIndex(m => m.IsDefault)
            .HasDatabaseName("IX_AIModels_IsDefault");
            
        builder.HasIndex(m => m.IsActive)
            .HasDatabaseName("IX_AIModels_IsActive");
            
        // Constraints
        builder.HasCheckConstraint("CK_AIModels_Temperature", 
            "\"Temperature\" >= 0 AND \"Temperature\" <= 2");
            
        builder.HasCheckConstraint("CK_AIModels_MaxTokens", 
            "\"MaxTokens\" > 0");
            
        // Seed data
        builder.HasData(
            new AIModel
            {
                Id = 1,
                Name = "Local LM Studio Model",
                ModelId = "local-model",
                Provider = "LMStudio",
                Endpoint = "http://localhost:1234/v1",
                MaxTokens = 4096,
                Temperature = 0.7,
                IsDefault = true,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}