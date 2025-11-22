using BingoBoard.Admin.Models;
using BingoBoard.Admin.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BingoBoard.Admin.MauiHybrid.Services;

/// <summary>
/// HTTP-based implementation of IBingoSquareService for MAUI app
/// Calls the admin web app API endpoints
/// </summary>
public class HttpBingoSquareService : IBingoSquareService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpBingoSquareService> _logger;

    public HttpBingoSquareService(HttpClient httpClient, ILogger<HttpBingoSquareService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<BingoSquareDto>> GetAllSquaresAsync()
    {
        try
        {
            _logger.LogInformation("Getting all bingo squares");
            
            var response = await _httpClient.GetAsync("");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get bingo squares: {StatusCode}", response.StatusCode);
                return new List<BingoSquareDto>();
            }

            var squares = await response.Content.ReadFromJsonAsync<List<BingoSquareDto>>();
            _logger.LogInformation("Retrieved {Count} bingo squares", squares?.Count ?? 0);
            return squares ?? new List<BingoSquareDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bingo squares");
            return new List<BingoSquareDto>();
        }
    }

    public async Task<BingoSquareDto?> GetSquareByIdAsync(string id)
    {
        try
        {
            _logger.LogInformation("Getting bingo square {Id}", id);
            
            var response = await _httpClient.GetAsync($"{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Bingo square {Id} not found", id);
                    return null;
                }
                
                _logger.LogError("Failed to get bingo square {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var square = await response.Content.ReadFromJsonAsync<BingoSquareDto>();
            _logger.LogInformation("Retrieved bingo square {Id}", id);
            return square;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bingo square {Id}", id);
            return null;
        }
    }

    public async Task<BingoSquareDto> CreateSquareAsync(BingoSquareDto square)
    {
        try
        {
            _logger.LogInformation("Creating bingo square {Id}", square.Id);
            
            var request = new CreateBingoSquareRequest
            {
                Id = square.Id,
                Label = square.Label,
                Type = square.Type
            };

            var response = await _httpClient.PostAsJsonAsync("", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create bingo square: {StatusCode} - {Error}", 
                    response.StatusCode, error);
                throw new HttpRequestException($"Failed to create bingo square: {error}");
            }

            var createdSquare = await response.Content.ReadFromJsonAsync<BingoSquareDto>();
            _logger.LogInformation("Created bingo square {Id}", square.Id);
            return createdSquare ?? square;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bingo square");
            throw;
        }
    }

    public async Task<BingoSquareDto?> UpdateSquareAsync(string id, BingoSquareDto square)
    {
        try
        {
            _logger.LogInformation("Updating bingo square {Id}", id);
            
            var request = new UpdateBingoSquareRequest
            {
                Label = square.Label,
                Type = square.Type
            };

            var response = await _httpClient.PutAsJsonAsync($"{id}", request);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Bingo square {Id} not found for update", id);
                    return null;
                }
                
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update bingo square {Id}: {StatusCode} - {Error}", 
                    id, response.StatusCode, error);
                throw new HttpRequestException($"Failed to update bingo square: {error}");
            }

            var updatedSquare = await response.Content.ReadFromJsonAsync<BingoSquareDto>();
            _logger.LogInformation("Updated bingo square {Id}", id);
            return updatedSquare;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bingo square {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteSquareAsync(string id)
    {
        try
        {
            _logger.LogInformation("Deleting bingo square {Id}", id);
            
            var response = await _httpClient.DeleteAsync($"{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Bingo square {Id} not found for deletion", id);
                    return false;
                }
                
                _logger.LogError("Failed to delete bingo square {Id}: {StatusCode}", id, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Deleted bingo square {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bingo square {Id}", id);
            return false;
        }
    }
}
