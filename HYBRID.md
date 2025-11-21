# Adding Hybrid Apps

## JavaScript Hybrid

### 1. Create the .NET MAUI app

> [!NOTE]
> This project uses centrally managed packages, so we will have to remove the version
> attributes from the new .csproj file.

1.  Create a normal/native .NET MAUI app project

    ```
    dotnet new maui -o src/BingoBoard.MauiHybrid
    ```

2.  Add the project to the solution
    ```
    dotnet sln add src/BingoBoard.MauiHybrid --in-root
    ```

3.  Build the project
    ```
    dotnet build src/BingoBoard.MauiHybrid
    ```

### 2. (Optional) Add a public dev tunnel

If we are wanting to target apps on devices or simulators, then we will need to
set up a Dev Tunnel so that we can expose our localhost as an external HTTPS endpoint.

This is needed because localhost on a device represents the _device's localhost_, not
the host machine's localhost.

1.  Add the package reference to `apphost.cs`
    ```cs
    #:package Aspire.Hosting.DevTunnels
    ```

2.  Add the dev tunnel
    ```cs
    // Add a dev tunnel so devices and simulators can access the localhost
    var publicDevTunnel = builder.AddDevTunnel("devtunnel-public")
        .WithAnonymousAccess() // All ports on this tunnel default to allowing anonymous access
        .WithReference(admin.GetEndpoint("https"));
    ```

### 3. Add the .NET MAUI app to the Aspire app host

1.  Add the initial .NET MAUI app resource builder to the app host

    ```cs
    // Add the .NET MAUI app builder
    var mauiapp = builder.AddMauiProject("mauiapp", @"BingoBoard.MauiHybrid/BingoBoard.MauiHybrid.csproj");
    ```

2.  Add the desired mobile devices, remembering to use the dev tunnel
    ```cs
    // Add iOS simulator with default simulator (uses running or default simulator)
    var ios = mauiapp.AddiOSSimulator()
        .ExcludeFromManifest()
        .WithOtlpDevTunnel() // Needed to get the OpenTelemetry data to "localhost"
        .WithReference(admin, publicDevTunnel); // Needs a dev tunnel to reach "localhost"

    // Add Android emulator with default emulator (uses running or default emulator)
    mauiapp.AddAndroidEmulator()
        .ExcludeFromManifest()
        .WithOtlpDevTunnel() // Needed to get the OpenTelemetry data to "localhost"
        .WithReference(admin, publicDevTunnel); // Needs a dev tunnel to reach "localhost"
    ```

3.  Add the desired desktop devices
    ```cs
    // Add Mac Catalyst desktop
    mauiapp.AddMacCatalystDevice()
        .ExcludeFromManifest()
        .WithReference(admin);

    // Add Windows desktop
    mauiapp.AddWindowsDevice()
        .ExcludeFromManifest()
        .WithReference(admin);
    ```

### 4. Add the compiled JS app into the .NET MAUI app

1.  Include the output from `vite build` into the app under the `ResourcesRaw\wwwroot` folder,
    making sure to set the logical name to start at `wwwroot\`

    ```xml
    <!-- Link Vue app build output without copying -->
    <MauiAsset
        Include="..\bingo-board\dist\**"
        Link="Resources\Raw\wwwroot\%(RecursiveDir)%(Filename)%(Extension)"
        LogicalName="wwwroot\%(RecursiveDir)%(Filename)%(Extension)" />
    ```

2.  Include the `<HybridWebView` control in the page, removing the existing XAML and C#
    code from the page
    ```xml
    <HybridWebView
        x:Name="hybridWebView" />
    ```

3.  Enable web developer tools in the `<HybridWebView>` for `DEBUG` builds

    ```cs
    builder.Services.AddHybridWebViewDeveloperTools();
    ```

### 5. Inject the HybridWebView.js file

1.  Add the `WebResourceRequested` event to the XAML

    ```xml
    <HybridWebView
        ...
        WebResourceRequested="OnWebResourceRequested" />
    ```

2.  In the codebehind, add the event handler to inject the `_framework/hybridwebview.js` script
    for the root URL.

    The implementation requires that we return imediately and rather pass a `Task<Stream>`
    to the response.

    ```cs
    private void OnWebResourceRequested(object? sender, WebViewWebResourceRequestedEventArgs e)
    {
        // Intercept the root request to inject the HybridWebView script
        // app:// is for macOS and iOS, https:// is for Android and Windows
        if (e.Uri.ToString() == "app://0.0.0.1/" || e.Uri.ToString() == "https://0.0.0.1/")
        {
            e.Handled = true;
            e.SetResponse(200, "OK", "text/html", GetModifiedHtmlStreamAsync());
        }

        async Task<Stream?> GetModifiedHtmlStreamAsync()
        {
            // Read the original HTML from the app package
            using var stream = await FileSystem.OpenAppPackageFileAsync("wwwroot/index.html");
            using var reader = new StreamReader(stream);
            var originalHtml = await reader.ReadToEndAsync();

            // Define the scripts to inject
            const string newScripts = 
                "<script src=\"_framework/hybridwebview.js\"></script>";

            // Inject scripts as the first script in <head>
            const string headTag = "<head>";
            var headStart = originalHtml.IndexOf(headTag) + headTag.Length;
            var modifiedHtml = originalHtml.Insert(headStart, newScripts);

            // Return modified HTML
            return new MemoryStream(Encoding.UTF8.GetBytes(modifiedHtml));
        }
    }
    ```

### 6. Inject the backend config

When we run the bingo board web app, we can see the SignalR port is _the same_ as the
web frontend, however, this is a result of a proxy that redirects to the admin backend
which is set on an environment variable `services__boardadmin__http__0`.

When we run the MAUI app, the web app assumes the proxy exists. We could update the
web app to not use a proxy and instead use the admin port, but there is a better way.

> !{NOTE]
> We can test this but removing the `server: { ... }` config from the 
> `src/bingo-board/vite.config.js` file. When the Aspire app is runnng, we can copy 
> the URI from the Aspire dashboard into the
> `src/bingo-board/services/signalrService.js` file so the hub URL is absolute.

1.  Add the nuget packages to the MAUI app
    ```xml
    <PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" />
    <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" />
    ```
2.  Register the services in `MauiProgram.cs`
    ```cs
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    ...
    
    builder.Configuration.AddEnvironmentVariables();
    builder.Services.AddServiceDiscovery();
    ```
3.  Fetch the `ServiceEndpointResolver` service in the `MainPage` constructor
    ```cs
    private readonly ServiceEndpointResolver _resolver;

    public MainPage(ServiceEndpointResolver serviceEndpointResolver)
    {
        _resolver = serviceEndpointResolver;

    ...
    ```
4.  Fetch the correct endpoint for the admin using the resolver
    ```cs
    async Task<Stream?> GetModifiedHtmlStreamAsync()
    {
        // Resolve the admin endpoint via service discovery
        var endpoints = await _resolver.GetEndpointsAsync("https://boardadmin", default);
        var endpoint = endpoints.Endpoints[0].EndPoint;

    ...
    ```

5.  Pass the endpoint into the web view using script injection
    ```cs
    // Define the scripts to inject
    var newScripts = 
        $"<script>window.BACKEND_CONFIG={{adminUrl:'{endpoint}'}};</script>" +
        "<script src=\"_framework/hybridwebview.js\"></script>";
    ```

6.  Update the `connect()` function in `src/bingo-board/services/signalrService.js`
    ```js
    let hubUrl = 'bingohub'; // Default relative URL for web

    // If running in an environment with BACKEND_CONFIG, use that to set the hub URL
    if (typeof window !== 'undefined' && window.BACKEND_CONFIG && window.BACKEND_CONFIG.adminUrl) {
        const baseUrl = window.BACKEND_CONFIG.adminUrl.replace(/\/$/, '');
        hubUrl = `${baseUrl}/bingohub`;
    }
    ```

### 7. Invoke JavaScript from C#

1.  Expose a `window.bingoBoard.requestNewBoard` function in `BingoBoard.vue`'s `mounted` function
    ```js
    // Expose requestNewBoard to the global window object for hybrid app access
    window.bingoBoard = {
        requestNewBoard: () => this.requestNewBoard()
    }
    ```

2.  Clean up when the component is unloaded in the `beforeUnmount` function
    ```js
    // Clean up global window object
    if (window.bingoBoard) {
        delete window.bingoBoard
    }
    ```

3.  Add a new toolbar item in `MainPage` XAML
    ```xml
    <ContentPage.ToolbarItems>
        <ToolbarItem Text="New Board" Clicked="OnNewBoardClicked" />
    </ContentPage.ToolbarItems>
    ```

4.  In the code behind, invoke the JavaScript
    ```cs
    private async void OnNewBoardClicked(object sender, EventArgs e)
    {
        // Invoke the requestNewBoard function defined in the BingoBoard component
        await hybridWebView.InvokeJavaScriptAsync("window.bingoBoard.requestNewBoard");
    }
    ```

### 8. Invoke C# from JavaScript

1.  Invoke .NET code in `downloadCanvas` from `imageGenerator.js`
    ```js
    // Check if we're in a HybridWebView by looking for the HybridWebView object
    if (typeof window.HybridWebView !== 'undefined') {
        // In HybridWebView, ask the app to display the image
        return new Promise((resolve, reject) => {
            const data = canvas.toDataURL('image/png');
            window.HybridWebView.InvokeDotNet('DownloadBoard', data);
        });
    }
    ```

2.  Create a "target" for the JavaScript function calls
    ```cs
    class JavaScriptTarget
    {
        // only public instance methods are avaiable to JavaScript
        public async void DownloadBoard(string imageDataUrl)
        {
            // Write the image data to a file
            var base64Data = imageDataUrl[(imageDataUrl.IndexOf(',') + 1)..];
            var tempFile = Path.Combine(FileSystem.CacheDirectory, "bingo-board.png");
            await File.WriteAllBytesAsync(tempFile, Convert.FromBase64String(base64Data));

            // Share the file
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "AspiriFridays Bingo Board",
                File = new ShareFile(tempFile)
            });
        }
    }
    ```

3.  Register the target with the web view in `MainPage`
    ```cs
    hybridWebView.SetInvokeJavaScriptTarget(new JavaScriptTarget());
    ```
