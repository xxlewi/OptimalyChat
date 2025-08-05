using Microsoft.AspNetCore.Mvc;
using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.PresentationLayer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestAIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILMStudioClient _lmStudioClient;
    private readonly ILogger<TestAIController> _logger;

    public TestAIController(IAIService aiService, ILMStudioClient lmStudioClient, ILogger<TestAIController> logger)
    {
        _aiService = aiService;
        _lmStudioClient = lmStudioClient;
        _logger = logger;
    }

    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        var isConnected = await _aiService.TestConnectionAsync();
        return Ok(new { connected = isConnected });
    }

    [HttpGet("test-models")]
    public async Task<IActionResult> TestModels()
    {
        var models = await _lmStudioClient.GetModelsAsync();
        return Ok(models);
    }

    [HttpPost("test-chat")]
    public async Task<IActionResult> TestChat([FromBody] TestChatRequest request)
    {
        // Get available models from LM Studio first
        var models = await _lmStudioClient.GetModelsAsync();
        var firstModel = models.FirstOrDefault();
        
        if (firstModel == null)
        {
            return BadRequest(new { error = "No models available in LM Studio" });
        }
        
        _logger.LogInformation("Using model: {ModelId}", firstModel.Id);
        
        var chatRequest = new ChatCompletionRequest
        {
            Model = firstModel.Id,
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = "You are a helpful assistant." },
                new ChatMessage { Role = "user", Content = request.Message }
            },
            Stream = false
        };

        var response = await _lmStudioClient.CreateChatCompletionAsync(chatRequest);
        return Ok(response);
    }

    [HttpGet("available-models")]
    public async Task<IActionResult> GetAvailableModels()
    {
        try
        {
            var models = await _aiService.GetAvailableModelsAsync();
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("test-stream")]
    public async IAsyncEnumerable<string> TestStream([FromBody] TestChatRequest request)
    {
        _logger.LogInformation("Test stream called with message: {Message}", request.Message);
        
        // Use the AIService directly to test streaming
        await foreach (var chunk in _aiService.StreamResponseAsync(1, 1, request.Message))
        {
            yield return chunk;
        }
    }
}

public class TestChatRequest
{
    public string Message { get; set; } = string.Empty;
}