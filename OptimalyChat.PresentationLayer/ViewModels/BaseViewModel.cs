namespace OptimalyChat.PresentationLayer.ViewModels;

public abstract class BaseViewModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}