using Microsoft.AspNetCore.Components;

namespace BingoBoard.Admin.Services;

/// <summary>
/// AddressResolver for the web app - resolves relative paths to absolute URLs
/// </summary>
class AddressResolver(NavigationManager navigationManager) : IAddressResolver
{
    public string Resolve(string path) => navigationManager.ToAbsoluteUri(path).ToString();
}
