using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using EIMS.Client;
using EIMS.Client.Services;
using EIMS.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure service mode - prefer real API but fall back to local mode if needed
bool useLocalMode = false; // Try to use the real API through our proxy
ServiceConfig.UseLocalMode = useLocalMode;

// Configure server URL for API calls - using the proxy to avoid CSP issues
string serverBaseAddress = "http://localhost:5063";
string proxyEndpoint = $"{serverBaseAddress}/client-api";
Console.WriteLine($"API proxy URL: {proxyEndpoint}");

// Log the local mode status
Console.WriteLine($"Operating in local mode: {ServiceConfig.UseLocalMode}");

// Register HttpClient with base address for easier API calls
builder.Services.AddScoped(sp => 
{
    var client = new HttpClient { BaseAddress = new Uri(proxyEndpoint) };
    Console.WriteLine($"Created HttpClient with base address: {client.BaseAddress}");
    return client;
});

// Register services
builder.Services.AddScoped<IPartService, PartService>();

// Create a separate HttpClient instance for Octopart that won't be used for other services
builder.Services.AddScoped<IOctopartService>(sp => {
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClient = new HttpClient(); // This client will be configured by OctopartService
    return new OctopartService(httpClient, config);
});

await builder.Build().RunAsync();
