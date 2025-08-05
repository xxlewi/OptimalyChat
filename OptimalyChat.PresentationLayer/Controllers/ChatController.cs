using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Interfaces;
using OptimalyChat.PresentationLayer.ViewModels;
using System.Security.Claims;

namespace OptimalyChat.PresentationLayer.Controllers;

/// <summary>
/// Controller for AI chat functionality
/// </summary>
[Authorize]
public class ChatController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IConversationService _conversationService;
    private readonly IAIService _aiService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IProjectService projectService,
        IConversationService conversationService,
        IAIService aiService,
        ILogger<ChatController> logger)
    {
        _projectService = projectService;
        _conversationService = conversationService;
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Main chat interface
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int? projectId = null, int? conversationId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var viewModel = new ChatViewModel
        {
            Title = "AI Chat",
            PageTitle = "AI Chat"
        };

        try
        {
            // Get user's projects
            var projectsResult = await _projectService.GetUserProjectsAsync(userId, 1, 100);
            viewModel.Projects = projectsResult.Items.ToList();

            // If no project specified, use the first one
            if (!projectId.HasValue && viewModel.Projects.Any())
            {
                projectId = viewModel.Projects.First().Id;
            }

            if (projectId.HasValue)
            {
                // Get current project
                viewModel.CurrentProject = await _projectService.GetByIdAsync(projectId.Value);
                
                if (viewModel.CurrentProject != null && viewModel.CurrentProject.UserId == userId)
                {
                    // Get conversations for the project
                    var conversationsResult = await _conversationService.GetProjectConversationsAsync(projectId.Value, 1, 100);
                    viewModel.Conversations = conversationsResult.Items.ToList();

                    // If no conversation specified, use the first one
                    if (!conversationId.HasValue && viewModel.Conversations.Any())
                    {
                        conversationId = viewModel.Conversations.First().Id;
                    }

                    if (conversationId.HasValue)
                    {
                        // Get current conversation with messages
                        viewModel.CurrentConversation = await _conversationService.GetConversationWithMessagesAsync(conversationId.Value);
                        viewModel.Messages = viewModel.CurrentConversation?.Messages ?? new List<MessageDto>();
                    }
                }
            }

            // Test AI connection
            viewModel.IsConnected = await _aiService.TestConnectionAsync();
            if (!viewModel.IsConnected)
            {
                viewModel.ConnectionError = "Unable to connect to LM Studio. Please ensure it's running on http://localhost:1234";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading chat interface");
            viewModel.ErrorMessage = "An error occurred loading the chat interface.";
        }

        return View(viewModel);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpGet]
    public IActionResult CreateProject()
    {
        var viewModel = new CreateProjectViewModel
        {
            Title = "Create Project",
            PageTitle = "Create New Project"
        };
        
        return View(viewModel);
    }

    /// <summary>
    /// Create a new project (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProject(CreateProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var projectDto = new ProjectDto
            {
                Name = model.Name,
                Description = model.Description,
                IsEncrypted = model.IsEncrypted,
                EncryptionLevel = model.EncryptionLevel,
                UserId = userId
            };

            var created = await _projectService.CreateProjectAsync(userId, projectDto);
            
            // Create initial conversation
            await _conversationService.CreateConversationAsync(created.Id, "General");
            
            return RedirectToAction(nameof(Index), new { projectId = created.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            ModelState.AddModelError("", "An error occurred creating the project.");
            return View(model);
        }
    }

    /// <summary>
    /// Get available AI models
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Models()
    {
        try
        {
            var models = await _aiService.GetAvailableModelsAsync();
            return Json(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI models");
            return StatusCode(500, new { error = "Failed to get AI models" });
        }
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ProjectStats(int projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null || project.UserId != userId)
            {
                return NotFound();
            }

            var stats = await _projectService.GetProjectStatisticsAsync(projectId);
            return Json(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project statistics");
            return StatusCode(500, new { error = "Failed to get project statistics" });
        }
    }
    
    /// <summary>
    /// Delete a conversation
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteConversation(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var conversation = await _conversationService.GetByIdAsync(id);
            if (conversation == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetByIdAsync(conversation.ProjectId);
            if (project == null || project.UserId != userId)
            {
                return Unauthorized();
            }

            await _conversationService.DeleteConversationAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation");
            return StatusCode(500, new { error = "Failed to delete conversation" });
        }
    }
}