namespace OptimalyChat.ServiceLayer.DTOs;

/// <summary>
/// Project statistics data transfer object
/// </summary>
public class ProjectStatisticsDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalConversations { get; set; }
    public int TotalMessages { get; set; }
    public int TotalTokensUsed { get; set; }
    public int TotalDocuments { get; set; }
    public long TotalStorageBytes { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, int> MessagesByRole { get; set; } = new();
    public Dictionary<string, int> TokensByModel { get; set; } = new();
    public double AverageResponseTimeMs { get; set; }
}