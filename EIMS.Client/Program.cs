using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EIMS.Client;
using EIMS.Client.Services;
using EIMS.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add configuration from appsettings.json
try
{
    var client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var response = await client.GetAsync("appsettings.json");
    if (response.IsSuccessStatusCode)
    {
        using var stream = await response.Content.ReadAsStreamAsync();
        builder.Configuration.AddJsonStream(stream);
        
        // Log full credentials for debugging
        var clientId = builder.Configuration["Octopart:ClientId"];
        var clientSecret = builder.Configuration["Octopart:ClientSecret"];
        
        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            Console.WriteLine($"Successfully loaded Octopart credentials:");
            Console.WriteLine($"ClientId: '{clientId}'");
            Console.WriteLine($"ClientSecret: '{clientSecret}'");
        }
        else
        {
            Console.Error.WriteLine("Missing Octopart credentials in configuration");
        }
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error loading configuration: {ex.Message}");
}

// Load services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IOctopartService, OctopartService>();

await builder.Build().RunAsync();
