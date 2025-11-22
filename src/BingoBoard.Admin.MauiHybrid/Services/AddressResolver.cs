using BingoBoard.Admin.Services;

namespace BingoBoard.Admin.MauiHybrid.Services;

/// <summary>
/// AddressResolver for MAUI app - uses service discovery to connect to the admin web app
/// </summary>
class AddressResolver : IAddressResolver
{
    private const string BaseUrl = "https://boardadmin";

    public string Resolve(string path)
    {
        // Combine base URL with the provided path
        return $"{BaseUrl}{path}";
    }
}
