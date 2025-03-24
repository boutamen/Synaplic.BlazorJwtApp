using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Synaplic.BlazorJwtApp.Client;
using Synaplic.BlazorJwtApp.Client.Authentication;
using Synaplic.BlazorJwtApp.Shared;
using Synaplic.BlazorJwtApp.Shared.Permissions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 📌 Registering services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthHeaderHandler>();

// 📌 Proper configuration of HttpClient with header management
builder.Services.AddHttpClient(AuthenticationConstants.AuthClientName, client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<AuthHeaderHandler>();

// 📌 Adding a default HttpClient (without middleware)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 📌 Authentication and authorization management
// 📌 Authentication and authorization management
builder.Services.AddAuthorizationCore(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim(AuthenticationConstants.AuthPermission, permission));
    }
});




builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();

// 📌 Adding API-related services
// builder.Services.AddScoped<MyApiService>();

await builder.Build().RunAsync();
