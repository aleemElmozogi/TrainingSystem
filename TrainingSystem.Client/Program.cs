using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TrainingSystem.Client.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
