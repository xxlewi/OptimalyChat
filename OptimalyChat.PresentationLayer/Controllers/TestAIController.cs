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
        var chatRequest = new ChatCompletionRequest
        {
            Model = "qwen2.5-coder-14b-instruct",
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
}

public class TestChatRequest
{
    public string Message { get; set; } = string.Empty;
}