using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.DataLayer.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        
        // Primary key
        builder.HasKey(m => m.Id);
        
        // Properties
        builder.Property(m => m.ConversationId)
            .IsRequired();
            
        builder.Property(m => m.Role)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(m => m.Content)
            .HasColumnType("text");
            
        builder.Property(m => m.EncryptedContent)
            .HasColumnType("text");
            
        builder.Property(m => m.Nonce)
            .HasMaxLength(100);
            
        builder.Property(m => m.Tag)
            .HasMaxLength(100);
            
        builder.Property(m => m.Embedding)
            .HasColumnType("bytea");
            
        builder.Property(m => m.IsIndexed)
            .HasDefaultValue(false);
            
        builder.Property(m => m.TokenCount)
            .HasDefaultValue(0);
            
        builder.Property(m => m.Model)
            .HasMaxLength(100);
            
        builder.Property(m => m.ResponseTimeMs);
        
        // Indexes
        builder.HasIndex(m => m.ConversationId)
            .HasDatabaseName("IX_Messages_ConversationId");
            
        builder.HasIndex(m => m.Role)
            .HasDatabaseName("IX_Messages_Role");
            
        builder.HasIndex(m => m.IsIndexed)
            .HasDatabaseName("IX_Messages_IsIndexed");
            
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt })
            .HasDatabaseName("IX_Messages_ConversationId_CreatedAt");
            
        // Relationships
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Seed data will be added after conversations are created
    }
}