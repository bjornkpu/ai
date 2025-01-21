using System.Text.Json;
using Application;
using Cocona;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var builder = CoconaApp.CreateBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.local.json", false, true);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.AddCommand("config",
    (
        IOptions<ObsidianConfig> obsidianConfig
    ) =>
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        Console.WriteLine(JsonSerializer.Serialize(obsidianConfig.Value, options));
    });

app.Run();