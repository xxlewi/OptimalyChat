using AutoMapper;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.ServiceLayer.Mapping;

/// <summary>
/// Service layer mapping profile - Entity to DTO mappings
/// Maps between domain entities and data transfer objects
/// </summary>
public class EntityToDtoMappingProfile : Profile
{
    public EntityToDtoMappingProfile()
    {
        // User entity mapping
        CreateMap<User, UserDto>()
            .ReverseMap()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // ID se nenastavuje při vytváření
            
        // AI Chat entity mappings
        CreateMap<Project, ProjectDto>()
            .ReverseMap()
            .ForMember(dest => dest.Conversations, opt => opt.Ignore())
            .ForMember(dest => dest.Documents, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
            
        CreateMap<Conversation, ConversationDto>()
            .ReverseMap()
            .ForMember(dest => dest.Messages, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore());
            
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content ?? src.DisplayContent))
            .ReverseMap()
            .ForMember(dest => dest.Conversation, opt => opt.Ignore())
            .ForMember(dest => dest.Embedding, opt => opt.Ignore())
            .ForMember(dest => dest.EncryptedContent, opt => opt.Ignore())
            .ForMember(dest => dest.Nonce, opt => opt.Ignore())
            .ForMember(dest => dest.Tag, opt => opt.Ignore());
            
        CreateMap<AIModel, AIModelDto>()
            .ReverseMap();
    }
}