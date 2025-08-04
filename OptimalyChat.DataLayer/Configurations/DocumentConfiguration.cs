using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        
        // Primary key
        builder.HasKey(d => d.Id);
        
        // Properties
        builder.Property(d => d.ProjectId)
            .IsRequired();
            
        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(d => d.ContentType)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(d => d.FileSize)
            .IsRequired();
            
        builder.Property(d => d.StoragePath)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(d => d.ExtractedText)
            .HasColumnType("text");
            
        builder.Property(d => d.IsIndexed)
            .HasDefaultValue(false);
            
        builder.Property(d => d.ChunkCount)
            .HasDefaultValue(0);
            
        // Indexes
        builder.HasIndex(d => d.ProjectId)
            .HasDatabaseName("IX_Documents_ProjectId");
            
        builder.HasIndex(d => d.IsIndexed)
            .HasDatabaseName("IX_Documents_IsIndexed");
            
        builder.HasIndex(d => d.ContentType)
            .HasDatabaseName("IX_Documents_ContentType");
            
        // Relationships
        builder.HasOne(d => d.Project)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}