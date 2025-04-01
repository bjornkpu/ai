using Cocona;
using McpClient;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Welcome to MCP Client CLI");

var builder = CoconaApp.CreateBuilder(args);

builder.Services.AddSingleton(await Config.LoadOrCreateAsync());

builder.Services.AddSingleton<McpClientService>();
builder.Services.AddSingleton<AzureOpenAIService>();

var app = builder.Build();

app.AddCommands<Commands>();

app.Run();