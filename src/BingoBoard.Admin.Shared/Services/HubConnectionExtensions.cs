using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;

namespace BingoBoard.Admin.Services;

/// <summary>
/// Extensions for HubConnectionBuilder to support service discovery via IHttpMessageHandlerFactory
/// </summary>
public static class HubConnectionExtensions
{
    /// <summary>
    /// Configures the hub connection URL with an IHttpMessageHandlerFactory for service discovery support
    /// </summary>
    public static IHubConnectionBuilder WithUrl(
        this IHubConnectionBuilder builder, 
        string url, 
        IHttpMessageHandlerFactory handlerFactory)
    {
        return builder.WithUrl(url, options =>
        {
            options.HttpMessageHandlerFactory = _ => handlerFactory.CreateHandler();
        });
    }
}
