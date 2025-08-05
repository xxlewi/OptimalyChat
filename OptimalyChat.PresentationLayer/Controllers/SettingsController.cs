using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OptimalyChat.ServiceLayer.Interfaces;
using OptimalyChat.ServiceLayer.Exceptions;
using OptimalyChat.PresentationLayer.ViewModels;
using AutoMapper;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.DataLayer.Entities;

namespace OptimalyChat.PresentationLayer.Controllers;

/// <summary>
/// Controller for managing AI settings
/// </summary>
[Authorize]
public class SettingsController : Controller
{
    private readonly IAIService _aiService;
    private readonly ILMStudioClient _lmStudioClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SettingsController> _logger;
    private readonly IMapper _mapper;

    public SettingsController(
        IAIService aiService,
        ILMStudioClient lmStudioClient,
        IConfiguration configuration,
        ILogger<SettingsController> logger,
        IMapper mapper)
    {
        _aiService = aiService;
        _lmStudioClient = lmStudioClient;
        _configuration = configuration;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Display AI settings page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var models = await _aiService.GetAvailableModelsAsync();
            var isConnected = await _aiService.TestConnectionAsync();
            
            var viewModel = new SettingsViewModel
            {
                Models = models.ToList(),
                IsLMStudioConnected = isConnected,
                ConnectionStatus = isConnected ? "Connected to LM Studio" : "Not connected to LM Studio",
                LMStudioSettings = new LMStudioSettingsViewModel
                {
                    BaseUrl = _configuration["LMStudio:BaseUrl"] ?? "http://localhost:1234/v1",
                    TimeoutSeconds = int.Parse(_configuration["LMStudio:TimeoutSeconds"] ?? "120")
                }
            };
            
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
            TempData["Error"] = "Error loading settings: " + ex.Message;
            return View(new SettingsViewModel());
        }
    }

    /// <summary>
    /// Test connection to LM Studio
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var isConnected = await _aiService.TestConnectionAsync();
            
            if (isConnected)
            {
                TempData["Success"] = "Successfully connected to LM Studio!";
            }
            else
            {
                TempData["Error"] = "Failed to connect to LM Studio. Please check the server URL and ensure LM Studio is running.";
            }
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection");
            TempData["Error"] = "Error testing connection: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Sync models from LM Studio
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SyncModels()
    {
        try
        {
            // Force sync models from LM Studio
            await _aiService.SyncModelsFromLMStudioAsync();
            
            var models = await _aiService.GetAvailableModelsAsync();
            var loadedCount = models.Count(m => m.IsLoadedInLMStudio);
            TempData["Success"] = $"Found {loadedCount} models loaded in LM Studio!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing models");
            TempData["Error"] = "Error syncing models: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Set default model
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SetDefaultModel(int id)
    {
        try
        {
            await _aiService.SetDefaultModelAsync(id);
            TempData["Success"] = "Default model updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default model");
            TempData["Error"] = "Error setting default model: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Toggle model active status
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleModelStatus(int id)
    {
        try
        {
            await _aiService.ToggleModelStatusAsync(id);
            TempData["Success"] = "Model status updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating model status");
            TempData["Error"] = "Error updating model status: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Delete model
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteModel(int id)
    {
        try
        {
            await _aiService.DeleteModelAsync(id);
            TempData["Success"] = "Model deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting model");
            TempData["Error"] = "Error deleting model: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}