using BingoBoard.Admin.Models;
using BingoBoard.Admin.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BingoBoard.Admin.Endpoints;

/// <summary>
/// Extension methods for mapping bingo client management endpoints
/// </summary>
public static class BingoClientEndpoints
{
    /// <summary>
    /// Maps all bingo client management endpoints to the web application
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <returns>The web application instance for method chaining</returns>
    public static WebApplication MapBingoClientEndpoints(this WebApplication app)
    {
        var clientGroup = app.MapGroup("/api/bingo-clients")
            .RequireAuthorization()
            .WithTags("Bingo Clients");

        clientGroup.MapGet("/", GetAllClientsHandler)
            .WithName("GetAllConnectedClients")
            .WithSummary("Get all connected clients")
            .WithDescription("Retrieves all currently connected clients");

        clientGroup.MapGet("/{connectionId}/persistent-id", GetPersistentClientIdHandler)
            .WithName("GetPersistentClientId")
            .WithSummary("Get persistent client ID from connection ID")
            .WithDescription("Retrieves the persistent client ID associated with a connection ID");

        clientGroup.MapGet("/{persistentClientId}/bingo-set", GetClientBingoSetHandler)
            .WithName("GetClientBingoSet")
            .WithSummary("Get a client's bingo set")
            .WithDescription("Retrieves the bingo set for a specific client by their persistent ID");

        return app;
    }

    /// <summary>
    /// Gets all connected clients
    /// </summary>
    /// <param name="clientService">The client connection service</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>A list of all connected clients</returns>
    internal static async Task<Results<Ok<List<ConnectedClient>>, ProblemHttpResult>> GetAllClientsHandler(
        IClientConnectionService clientService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("API request: Getting all connected clients");
            
            var clients = await clientService.GetAllClientsAsync();

            logger.LogInformation("Retrieved {Count} connected clients", clients.Count);
            return TypedResults.Ok(clients);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving connected clients");
            return TypedResults.Problem("Failed to retrieve connected clients", statusCode: 500);
        }
    }

    /// <summary>
    /// Gets the persistent client ID from a connection ID
    /// </summary>
    /// <param name="connectionId">The connection ID</param>
    /// <param name="clientService">The client connection service</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>The persistent client ID or not found</returns>
    internal static async Task<Results<Ok<string>, NotFound, ProblemHttpResult>> GetPersistentClientIdHandler(
        string connectionId,
        IClientConnectionService clientService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("API request: Getting persistent client ID for connection {ConnectionId}", connectionId);
            
            var persistentClientId = await clientService.GetPersistentClientIdAsync(connectionId);

            if (string.IsNullOrEmpty(persistentClientId))
            {
                logger.LogWarning("No persistent client ID found for connection {ConnectionId}", connectionId);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Retrieved persistent client ID {PersistentClientId} for connection {ConnectionId}", 
                persistentClientId, connectionId);
            return TypedResults.Ok(persistentClientId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving persistent client ID for connection {ConnectionId}", connectionId);
            return TypedResults.Problem("Failed to retrieve persistent client ID", statusCode: 500);
        }
    }

    /// <summary>
    /// Gets a client's bingo set
    /// </summary>
    /// <param name="persistentClientId">The persistent client ID</param>
    /// <param name="bingoService">The bingo service</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>The client's bingo set or not found</returns>
    internal static async Task<Results<Ok<BingoSet>, NotFound, ProblemHttpResult>> GetClientBingoSetHandler(
        string persistentClientId,
        IBingoService bingoService,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("API request: Getting bingo set for client {ClientId}", persistentClientId);
            
            var bingoSet = await bingoService.GetClientBingoSetAsync(persistentClientId);

            if (bingoSet == null)
            {
                logger.LogWarning("No bingo set found for client {ClientId}", persistentClientId);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Retrieved bingo set {BingoSetId} for client {ClientId}", 
                bingoSet.Id, persistentClientId);
            return TypedResults.Ok(bingoSet);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving bingo set for client {ClientId}", persistentClientId);
            return TypedResults.Problem("Failed to retrieve client bingo set", statusCode: 500);
        }
    }
}
