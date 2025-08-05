using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace OptimalyChat.PresentationLayer.Services;

/// <summary>
/// Custom UserIdProvider for SignalR to properly identify users
/// </summary>
public class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Get the user ID from the ClaimsPrincipal
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}