using BingoBoard.Admin.Models;
using BingoBoard.Admin.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BingoBoard.Admin.MauiHybrid.Services;

/// <summary>
/// HTTP-based implementation of IClientConnectionService for MAUI app
/// Calls the admin web app API endpoints
/// </summary>
public class HttpClientConnectionService : IClientConnectionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientConnectionService> _logger;

    public HttpClientConnectionService(HttpClient httpClient, ILogger<HttpClientConnectionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ConnectedClient>> GetAllClientsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all connected clients");
            
            var response = await _httpClient.GetAsync("");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get connected clients: {StatusCode}", response.StatusCode);
                return new List<ConnectedClient>();
            }

            var clients = await response.Content.ReadFromJsonAsync<List<ConnectedClient>>();
            _logger.LogInformation("Retrieved {Count} connected clients", clients?.Count ?? 0);
            return clients ?? new List<ConnectedClient>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connected clients");
            return new List<ConnectedClient>();
        }
    }

    public async Task<string?> GetPersistentClientIdAsync(string connectionId)
    {
        try
        {
            _logger.LogInformation("Getting persistent client ID for connection {ConnectionId}", connectionId);
            
            var response = await _httpClient.GetAsync($"{connectionId}/persistent-id");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Persistent client ID not found for connection {ConnectionId}", connectionId);
                    return null;
                }
                
                _logger.LogError("Failed to get persistent client ID for connection {ConnectionId}: {StatusCode}", 
                    connectionId, response.StatusCode);
                return null;
            }

            var persistentClientId = await response.Content.ReadFromJsonAsync<string>();
            _logger.LogInformation("Retrieved persistent client ID for connection {ConnectionId}", connectionId);
            return persistentClientId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting persistent client ID for connection {ConnectionId}", connectionId);
            return null;
        }
    }

    #region Not Implemented Methods
    // These methods are not needed for the MAUI admin app based on the user's request
    // They would need to be implemented if the MAUI app requires full functionality

    public Task AddClientAsync(ConnectedClient client)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task RemoveClientAsync(string connectionId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<ConnectedClient?> GetClientAsync(string connectionId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task UpdateClientActivityAsync(string connectionId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task AssociateBingoSetAsync(string connectionId, string bingoSetId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task MapConnectionToPersistentClientAsync(string connectionId, string persistentClientId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<string?> GetConnectionIdFromPersistentClientAsync(string persistentClientId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }
    #endregion
}
