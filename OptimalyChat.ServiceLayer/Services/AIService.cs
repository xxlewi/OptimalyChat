using System.Runtime.CompilerServices;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OptimalyChat.DataLayer.Entities;
using OptimalyChat.DataLayer.Interfaces;
using OptimalyChat.ServiceLayer.DTOs;
using OptimalyChat.ServiceLayer.Exceptions;
using OptimalyChat.ServiceLayer.Interfaces;

namespace OptimalyChat.ServiceLayer.Services;

/// <summary>
/// AI service implementation for chat operations
/// </summary>
public class AIService : IAIService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILMStudioClient _lmStudioClient;
    private readonly IMapper _mapper;
    private readonly ILogger<AIService> _logger;
    private readonly IConfiguration _configuration;

    public AIService(
        IUnitOfWork unitOfWork,
        ILMStudioClient lmStudioClient,
        IMapper mapper,
        ILogger<AIService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _lmStudioClient = lmStudioClient;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Get AI response for a message
    /// </summary>
    public async Task<MessageDto> GetResponseAsync(int projectId, int conversationId, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get conversation and validate
            var conversationRepo = _unitOfWork.GetRepository<Conversation, int>();
            var conversation = await conversationRepo.GetByIdAsync(conversationId, cancellationToken);
            
            if (conversation == null || conversation.ProjectId != projectId)
                throw new NotFoundException($"Conversation {conversationId} not found in project {projectId}");
            
            // Save user message
            var userMessage = new Message
            {
                ConversationId = conversationId,
                Role = "user",
                Content = message,
                TokenCount = EstimateTokenCount(message)
            };
            
            var messageRepo = _unitOfWork.GetRepository<Message, int>();
            await messageRepo.AddAsync(userMessage, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Get context
            var context = await GetConversationContextAsync(conversationId, cancellationToken);
            
            // Build prompt with context
            var messages = BuildMessagesWithContext(context, message);
            
            // Get AI model
            var model = await GetDefaultModelAsync(cancellationToken);
            
            // Call LM Studio
            var request = new ChatCompletionRequest
            {
                Model = model.ModelId,
                Messages = messages,
                Temperature = model.Temperature,
                MaxTokens = model.MaxTokens,
                Stream = false
            };
            
            var startTime = DateTime.UtcNow;
            var response = await _lmStudioClient.CreateChatCompletionAsync(request, cancellationToken);
            var responseTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            
            // Extract response content
            var responseContent = response.Choices.FirstOrDefault()?.Message?.Content ?? "Sorry, I couldn't generate a response.";
            
            // Save AI response
            var aiMessage = new Message
            {
                ConversationId = conversationId,
                Role = "assistant",
                Content = responseContent,
                TokenCount = response.Usage?.CompletionTokens ?? EstimateTokenCount(responseContent),
                Model = model.ModelId,
                ResponseTimeMs = responseTime
            };
            
            await messageRepo.AddAsync(aiMessage, cancellationToken);
            
            // Update conversation stats
            conversation.LastMessageAt = DateTime.UtcNow;
            conversation.TotalTokensUsed += response.Usage?.TotalTokens ?? 0;
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return _mapper.Map<MessageDto>(aiMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI response for conversation {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// Stream AI response for a message
    /// </summary>
    public async IAsyncEnumerable<string> StreamResponseAsync(
        int projectId, 
        int conversationId, 
        string message, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("StreamResponseAsync called - ProjectId: {ProjectId}, ConversationId: {ConversationId}, Message: {Message}", 
            projectId, conversationId, message);
            
        // Get conversation and validate
        var conversationRepo = _unitOfWork.GetRepository<Conversation, int>();
        var conversation = await conversationRepo.GetByIdAsync(conversationId, cancellationToken);
        
        if (conversation == null || conversation.ProjectId != projectId)
        {
            _logger.LogError("Conversation not found or project mismatch - ConversationId: {ConversationId}, ProjectId: {ProjectId}", 
                conversationId, projectId);
            throw new NotFoundException($"Conversation {conversationId} not found in project {projectId}");
        }
        
        // Save user message
        var userMessage = new Message
        {
            ConversationId = conversationId,
            Role = "user",
            Content = message,
            TokenCount = EstimateTokenCount(message)
        };
        
        var messageRepo = _unitOfWork.GetRepository<Message, int>();
        await messageRepo.AddAsync(userMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Get context
        var context = await GetConversationContextAsync(conversationId, cancellationToken);
        
        // Build prompt with context
        var messages = BuildMessagesWithContext(context, message);
        
        // Get AI model
        var model = await GetDefaultModelAsync(cancellationToken);
        _logger.LogInformation("Using AI model: {Model}", model.ModelId);
        
        // Call LM Studio with streaming
        var request = new ChatCompletionRequest
        {
            Model = model.ModelId,
            Messages = messages,
            Temperature = model.Temperature,
            MaxTokens = model.MaxTokens,
            Stream = true
        };
        
        _logger.LogInformation("Sending request to LM Studio - Model: {Model}, Messages count: {Count}", 
            request.Model, messages.Count);
        
        var startTime = DateTime.UtcNow;
        var responseBuilder = new StringBuilder();
        var tokenCount = 0;
        
        await foreach (var chunk in _lmStudioClient.StreamChatCompletionAsync(request, cancellationToken))
        {
            var content = chunk.Choices.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(content))
            {
                _logger.LogDebug("Received chunk: {Content}", content);
                responseBuilder.Append(content);
                tokenCount++;
                yield return content;
            }
        }
        
        var responseTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
        
        // Save complete AI response
        var aiMessage = new Message
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = responseBuilder.ToString(),
            TokenCount = tokenCount,
            Model = model.ModelId,
            ResponseTimeMs = responseTime
        };
        
        await messageRepo.AddAsync(aiMessage, cancellationToken);
        
        // Update conversation stats
        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.TotalTokensUsed += tokenCount;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Get relevant context from project history
    /// </summary>
    public async Task<IEnumerable<MessageDto>> GetRelevantContextAsync(int projectId, string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        // For now, return recent messages from the project
        // TODO: Implement vector search when vector database is added
        
        var messageRepo = _unitOfWork.GetRepository<Message, int>();
        var messages = await messageRepo.Query
            .Include(m => m.Conversation)
            .Where(m => m.Conversation.ProjectId == projectId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(topK * 2) // Get more to filter
            .ToListAsync(cancellationToken);
        
        // Simple keyword matching for now
        var relevantMessages = messages
            .Where(m => m.Content != null && m.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(topK);
        
        return _mapper.Map<IEnumerable<MessageDto>>(relevantMessages);
    }

    /// <summary>
    /// Test AI connection
    /// </summary>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _lmStudioClient.TestConnectionAsync(cancellationToken);
    }

    /// <summary>
    /// Get available AI models
    /// </summary>
    public async Task<IEnumerable<AIModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        // Get models from database
        var modelRepo = _unitOfWork.GetRepository<AIModel, int>();
        var dbModels = await modelRepo.Query.ToListAsync(cancellationToken);
        
        // Get currently loaded models from LM Studio using the v0 API
        var loadedModels = await _lmStudioClient.GetLoadedModelsAsync(cancellationToken);
        var loadedModelIds = loadedModels
            .Where(m => m.State == "loaded")
            .Select(m => m.Id)
            .ToHashSet();
        
        // Map to DTOs and mark which ones are loaded
        var modelDtos = _mapper.Map<List<AIModelDto>>(dbModels);
        foreach (var dto in modelDtos)
        {
            dto.IsLoadedInLMStudio = loadedModelIds.Contains(dto.ModelId);
        }
        
        return modelDtos;
    }

    private async Task<IEnumerable<Message>> GetConversationContextAsync(int conversationId, CancellationToken cancellationToken)
    {
        var messageRepo = _unitOfWork.GetRepository<Message, int>();
        
        // Get last N messages from conversation
        var messages = await messageRepo.Query
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10) // Last 10 messages
            .OrderBy(m => m.CreatedAt) // Reorder chronologically
            .ToListAsync(cancellationToken);
        
        return messages;
    }

    private List<ChatMessage> BuildMessagesWithContext(IEnumerable<Message> context, string currentMessage)
    {
        var messages = new List<ChatMessage>();
        
        // Add system prompt
        messages.Add(new ChatMessage
        {
            Role = "system",
            Content = "You are OptimalyChat, a helpful AI assistant. Provide clear, concise, and accurate responses."
        });
        
        // Add context messages
        foreach (var msg in context)
        {
            if (msg.Content != null)
            {
                messages.Add(new ChatMessage
                {
                    Role = msg.Role,
                    Content = msg.Content
                });
            }
        }
        
        // Add current message
        messages.Add(new ChatMessage
        {
            Role = "user",
            Content = currentMessage
        });
        
        return messages;
    }

    private async Task<AIModel> GetDefaultModelAsync(CancellationToken cancellationToken)
    {
        var modelRepo = _unitOfWork.GetRepository<AIModel, int>();
        
        // First sync models from LM Studio
        await SyncModelsFromLMStudioAsync(cancellationToken);
        
        var model = await modelRepo.Query
            .Where(m => m.IsDefault && m.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (model == null)
        {
            model = await modelRepo.Query
                .Where(m => m.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        }
        
        if (model == null)
            throw new BusinessException("No active AI model found", "NO_AI_MODEL");
        
        _logger.LogInformation("Using AI model: {ModelId} - {ModelName}", model.ModelId, model.Name);
        return model;
    }
    
    public async Task SyncModelsFromLMStudioAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var lmModels = await _lmStudioClient.GetModelsAsync(cancellationToken);
            var modelRepo = _unitOfWork.GetRepository<AIModel, int>();
            
            foreach (var lmModel in lmModels)
            {
                var existingModel = await modelRepo.Query
                    .Where(m => m.ModelId == lmModel.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                
                if (existingModel == null)
                {
                    var newModel = new AIModel
                    {
                        Name = lmModel.Id,
                        ModelId = lmModel.Id,
                        Provider = "LMStudio",
                        Endpoint = _configuration["LMStudio:BaseUrl"] ?? "http://localhost:1234/v1",
                        MaxTokens = 4096,
                        Temperature = 0.7,
                        IsActive = true,
                        IsDefault = !await modelRepo.Query.AnyAsync(m => m.IsDefault, cancellationToken)
                    };
                    
                    await modelRepo.AddAsync(newModel, cancellationToken);
                    _logger.LogInformation("Added new AI model from LM Studio: {ModelId}", lmModel.Id);
                }
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync models from LM Studio");
        }
    }

    private int EstimateTokenCount(string text)
    {
        // Simple estimation: ~4 characters per token
        return (int)Math.Ceiling(text.Length / 4.0);
    }
    
    /// <summary>
    /// Set default AI model
    /// </summary>
    public async Task SetDefaultModelAsync(int modelId, CancellationToken cancellationToken = default)
    {
        var modelRepo = _unitOfWork.GetRepository<AIModel, int>();
        
        // First, unset all models as default
        var allModels = await modelRepo.Query.ToListAsync(cancellationToken);
        foreach (var model in allModels)
        {
            model.IsDefault = false;
        }
        
        // Set the selected model as default
        var selectedModel = await modelRepo.GetByIdAsync(modelId, cancellationToken);
        if (selectedModel == null)
            throw new NotFoundException($"AI Model with ID {modelId} not found");
            
        selectedModel.IsDefault = true;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Set model {ModelId} as default", modelId);
    }
    
    /// <summary>
    /// Toggle model active status
    /// </summary>
    public async Task ToggleModelStatusAsync(int modelId, CancellationToken cancellationToken = default)
    {
        var modelRepo = _unitOfWork.GetRepository<AIModel, int>();
        var model = await modelRepo.GetByIdAsync(modelId, cancellationToken);
        
        if (model == null)
            throw new NotFoundException($"AI Model with ID {modelId} not found");
            
        // Don't allow deactivating the default model
        if (model.IsDefault && model.IsActive)
            throw new BusinessException("Cannot deactivate the default model", "DEFAULT_MODEL_DEACTIVATION");
            
        model.IsActive = !model.IsActive;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Toggled model {ModelId} status to {Status}", modelId, model.IsActive);
    }
    
    /// <summary>
    /// Delete AI model
    /// </summary>
    public async Task DeleteModelAsync(int modelId, CancellationToken cancellationToken = default)
    {
        var modelRepo = _unitOfWork.GetRepository<AIModel, int>();
        var model = await modelRepo.GetByIdAsync(modelId, cancellationToken);
        
        if (model == null)
            throw new NotFoundException($"AI Model with ID {modelId} not found");
            
        // Don't allow deleting the default model
        if (model.IsDefault)
            throw new BusinessException("Cannot delete the default model", "DEFAULT_MODEL_DELETION");
            
        modelRepo.Delete(model);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Deleted model {ModelId}", modelId);
    }
}