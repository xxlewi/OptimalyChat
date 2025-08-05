using OptimalyChat.ServiceLayer.DTOs;

namespace OptimalyChat.PresentationLayer.ViewModels;

/// <summary>
/// Main chat view model
/// </summary>
public class ChatViewModel : BaseViewModel
{
    public ProjectDto? CurrentProject { get; set; }
    public ConversationDto? CurrentConversation { get; set; }
    public List<ProjectDto> Projects { get; set; } = new();
    public List<ConversationDto> Conversations { get; set; } = new();
    public List<MessageDto> Messages { get; set; } = new();
    public bool IsConnected { get; set; }
    public string? ConnectionError { get; set; }
    public List<AIModelDto> AvailableModels { get; set; } = new();
    public int? SelectedModelId { get; set; }
}