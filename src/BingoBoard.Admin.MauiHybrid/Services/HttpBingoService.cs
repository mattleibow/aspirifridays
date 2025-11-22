using BingoBoard.Admin.Models;
using BingoBoard.Admin.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BingoBoard.Admin.MauiHybrid.Services;

/// <summary>
/// HTTP-based implementation of IBingoService for MAUI app
/// Calls the admin web app API endpoints
/// </summary>
public class HttpBingoService : IBingoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpBingoService> _logger;

    public HttpBingoService(HttpClient httpClient, ILogger<HttpBingoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BingoSet?> GetClientBingoSetAsync(string clientId)
    {
        try
        {
            _logger.LogInformation("Getting bingo set for client {ClientId}", clientId);
            
            var response = await _httpClient.GetAsync($"{clientId}/bingo-set");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Bingo set not found for client {ClientId}", clientId);
                    return null;
                }
                
                _logger.LogError("Failed to get bingo set for client {ClientId}: {StatusCode}", 
                    clientId, response.StatusCode);
                return null;
            }

            var bingoSet = await response.Content.ReadFromJsonAsync<BingoSet>();
            _logger.LogInformation("Retrieved bingo set for client {ClientId}", clientId);
            return bingoSet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bingo set for client {ClientId}", clientId);
            return null;
        }
    }

    #region Not Implemented Methods
    // These methods are not needed for the MAUI admin app based on the user's request
    // They would need to be implemented if the MAUI app requires full functionality

    public Task<List<BingoSquare>> GetAllSquaresAsync()
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app. Use IBingoSquareService instead.");
    }

    public Task<BingoSet> GenerateRandomBingoSetAsync(string clientId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<bool> UpdateSquareStatusAsync(string clientId, string squareId, bool isChecked)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<bool> CheckForWinAsync(string clientId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<List<BingoSet>> GetAllClientSetsAsync()
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<bool> UpdateSquareGloballyAsync(string squareId, bool isChecked)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<bool> UpdateSquareForAdminAsync(string adminClientId, string squareId, bool isChecked)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<List<string>> GetGloballyCheckedSquaresAsync()
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<string> RequestSquareApprovalAsync(string clientId, string squareId, bool requestedState)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<List<PendingApproval>> GetPendingApprovalsAsync()
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<bool> ApproveSquareRequestAsync(string approvalId, string adminId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<bool> DenySquareRequestAsync(string approvalId, string adminId, string? reason = null)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<PendingApproval?> GetPendingApprovalAsync(string approvalId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task CleanupExpiredApprovalsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task SetLiveModeAsync(bool isLiveMode)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<bool> GetLiveModeAsync()
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<(bool needsApproval, string? approvalId)> HandleSquareRequestAsync(string clientId, string squareId, bool requestedState)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }

    public Task<int> ApproveAllPendingRequestsAsync(string adminId)
    {
        throw new NotImplementedException("This method is not implemented in the MAUI admin app.");
    }
    #endregion
}
