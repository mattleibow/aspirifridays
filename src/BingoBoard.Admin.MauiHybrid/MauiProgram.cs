using BingoBoard.Admin.MauiHybrid.Endpoints;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using BingoBoard.Admin.Services;
using BingoBoard.Admin.MauiHybrid.Services;

namespace BingoBoard.Admin.MauiHybrid;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.AddServiceDefaults();

        // Configure HttpClient for BingoBoard.Admin API calls WITH authentication
        // Get base URL from AddressResolver
        var addressResolver = new AddressResolver();
        
        // Register authentication services
        builder.Services.AddAccountServices(addressResolver.Resolve("/identity/"));

        // Register needed elements for authentication:
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddAuthorizationCore();

        // Register custom services
        builder.Services.AddSingleton<IAddressResolver>(addressResolver);
        
        builder.Services.AddHttpClient<IBingoService, HttpBingoService>(client =>
            {
                client.BaseAddress = new Uri(addressResolver.Resolve("/api/bingo-clients/"));
            })
            .AddIdentityAuthorizationHandler();

        builder.Services.AddHttpClient<IClientConnectionService, HttpClientConnectionService>(client =>
            {
                client.BaseAddress = new Uri(addressResolver.Resolve("/api/bingo-clients/"));
            })
            .AddIdentityAuthorizationHandler();

        builder.Services.AddHttpClient<IBingoSquareService, HttpBingoSquareService>(client =>
            {
                client.BaseAddress = new Uri(addressResolver.Resolve("/api/bingo-squares/"));
            })
            .AddIdentityAuthorizationHandler();

        return builder.Build();
    }
}
