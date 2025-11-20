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