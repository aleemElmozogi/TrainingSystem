using TrainingSystem.Client.Pages;
using TrainingSystem.Components;
using MudBlazor.Services;

using Microsoft.EntityFrameworkCore;
using TrainingSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Register HttpClient and ClientService for Server-side rendering
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5258") });
builder.Services.AddScoped<TrainingSystem.Client.Services.IClientService, TrainingSystem.Client.Services.ClientService>();
builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TrainingSystem.Client._Imports).Assembly);

app.Run();
