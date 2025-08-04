using Microsoft.AspNetCore.Mvc;
using OptimalyChat.ServiceLayer.Exceptions;

namespace OptimalyChat.PresentationLayer.Controllers;

/// <summary>
/// Test controller for exception handling - DEVELOPMENT ONLY
/// This controller is automatically excluded from production builds
/// </summary>
#if DEBUG
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("test-not-found")]
    public IActionResult TestNotFoundException()
    {
        _logger.LogInformation("Testování NotFoundException");
        throw new NotFoundException("TestEntity", 123);
    }

    [HttpGet("test-validation")]
    public IActionResult TestValidationException()
    {
        _logger.LogInformation("Testování ValidationException");
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email je povinný", "Email není ve správném formátu" } },
            { "Name", new[] { "Jméno je povinné" } }
        };
        throw new ValidationException(errors);
    }

    [HttpGet("test-business")]
    public IActionResult TestBusinessException()
    {
        _logger.LogInformation("Testování BusinessException");
        throw new BusinessException("Nelze vykonat tuto operaci z business důvodů", "BUSINESS_RULE_VIOLATION");
    }

    [HttpGet("test-general")]
    public IActionResult TestGeneralException()
    {
        _logger.LogInformation("Testování obecné výjimky");
        throw new InvalidOperationException("Obecná výjimka pro testování");
    }

    [HttpGet("test-success")]
    public IActionResult TestSuccess()
    {
        _logger.LogInformation("Test úspěšného volání");
        return Ok(new { Message = "Everything works correctly!", Timestamp = DateTime.UtcNow });
    }

    [HttpGet("test-users")]
    public async Task<IActionResult> TestUsers([FromServices] OptimalyChat.ServiceLayer.Interfaces.IUserService userService)
    {
        try
        {
            _logger.LogInformation("Testování uživatelů");
            var users = await userService.GetAllAsync();
            return Ok(new { Count = users.Count(), Users = users.Take(2) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při testování uživatelů");
            return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }
}
#endif