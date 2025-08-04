using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;
using System.Linq.Expressions;

namespace OptimalyChat.DataLayer.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets pro naše entity
    // Poznámka: Users je již definován v IdentityDbContext<User>
    
    // AI Chat entities
    public DbSet<Project> Projects { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<AIModel> AIModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Aplikuj všechny konfigurace
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Globální query filtry pro soft delete
        ConfigureGlobalQueryFilters(modelBuilder);
    }

    private static void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Query filter pro všechny entity dědící z BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IBaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(IBaseEntity.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}