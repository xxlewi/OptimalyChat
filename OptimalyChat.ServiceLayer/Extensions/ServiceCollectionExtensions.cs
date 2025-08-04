using Microsoft.Extensions.DependencyInjection;
using OptimalyChat.ServiceLayer.Interfaces;
using OptimalyChat.ServiceLayer.Mapping;
using OptimalyChat.ServiceLayer.Services;

namespace OptimalyChat.ServiceLayer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        // ServiceLayer AutoMapper konfigurace
        services.AddAutoMapper(typeof(EntityToDtoMappingProfile));
        
        // User service registrace
        services.AddScoped<IUserService, UserService>();
        
        // Enhanced services following CRM pattern
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IExportService, ExportService>();
        
        // AI Chat services
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IAIService, AIService>();
        
        // LM Studio client
        services.AddHttpClient<ILMStudioClient, LMStudioClient>();

        return services;
    }
}