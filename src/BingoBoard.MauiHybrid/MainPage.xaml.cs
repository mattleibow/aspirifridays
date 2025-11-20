using System.Text;

namespace BingoBoard.MauiHybrid;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
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
}
