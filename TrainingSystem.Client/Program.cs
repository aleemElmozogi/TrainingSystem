using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TrainingSystem.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IClientService, ClientService>();

await builder.Build().RunAsync();
