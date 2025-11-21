using System.Text;
using Microsoft.Extensions.ServiceDiscovery;

namespace BingoBoard.MauiHybrid;

public partial class MainPage : ContentPage
{
	private readonly ServiceEndpointResolver _resolver;

	public MainPage(ServiceEndpointResolver serviceEndpointResolver)
	{
		_resolver = serviceEndpointResolver;
		InitializeComponent();
	}

	private async void OnNewBoardClicked(object sender, EventArgs e)
	{
		// Invoke the requestNewBoard function defined in the BingoBoard component
		await hybridWebView.InvokeJavaScriptAsync("window.bingoBoard.requestNewBoard");
	}

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
			// Resolve the admin endpoint via service discovery
			var endpoints = await _resolver.GetEndpointsAsync("https://boardadmin", default);
			var endpoint = endpoints.Endpoints[0].EndPoint;

			// Read the original HTML from the app package
			using var stream = await FileSystem.OpenAppPackageFileAsync("wwwroot/index.html");
			using var reader = new StreamReader(stream);
			var originalHtml = await reader.ReadToEndAsync();

			// Define the scripts to inject
			var newScripts = 
				$"<script>window.BACKEND_CONFIG={{adminUrl:'{endpoint}'}};</script>" +
				"<script src=\"_framework/hybridwebview.js\"></script>";

			// Inject scripts as the first script in <head>
			const string headTag = "<head>";
			var headStart = originalHtml.IndexOf(headTag) + headTag.Length;
			var modifiedHtml = originalHtml.Insert(headStart, newScripts);

			// Return modified HTML
			return new MemoryStream(Encoding.UTF8.GetBytes(modifiedHtml));
		}
	}
}
