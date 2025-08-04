using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        
        // Primary key
        builder.HasKey(c => c.Id);
        
        // Properties
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(c => c.ProjectId)
            .IsRequired();
            
        builder.Property(c => c.LastMessageAt);
        
        builder.Property(c => c.TotalTokensUsed)
            .HasDefaultValue(0);
            
        // Indexes
        builder.HasIndex(c => c.ProjectId)
            .HasDatabaseName("IX_Conversations_ProjectId");
            
        builder.HasIndex(c => c.LastMessageAt)
            .HasDatabaseName("IX_Conversations_LastMessageAt");
            
        // Relationships
        builder.HasOne(c => c.Project)
            .WithMany(p => p.Conversations)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Seed data will be added after projects are created
    }
}