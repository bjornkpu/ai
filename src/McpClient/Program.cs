using McpClient;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;

Console.WriteLine("Welcome to MCP Client CLI");

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

var config = await Config.LoadOrCreateAsync();
builder.Services.AddSingleton(config);
builder.Services.AddSingleton<McpClientService>();
builder.Services.AddSingleton<AzureOpenAIService>();
builder.Services.AddSingleton<IChatClient>(sp =>
    sp.GetRequiredService<AzureOpenAIService>().GetChatClient());

var app = builder.Build();

var mcpClientService = app.Services.GetRequiredService<McpClientService>();
var chatClient = app.Services.GetService<IChatClient>();
if (chatClient == null)
{
    Console.WriteLine("Azure OpenAI configuration is missing. Exiting.");
    return;
}

var mcpClients = await mcpClientService.CreateClientsAsync();
var tools= new List<AITool>();
foreach (var client in mcpClients)
{
    tools.AddRange(await client.ListToolsAsync());
}

var options = new ChatOptions
{
    Tools = [.. tools]
};

var conversationHistory = new List<ChatMessage>
{
    new(ChatRole.System, config.SystemPrompt)
};

while (true)
{
    Console.Write("\n>>> ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Exiting. Goodbye!");
        break;
    }
    conversationHistory.Add(new(ChatRole.User, userInput));

    try
    {
        Console.WriteLine("Thinking...");

        var responseStream = chatClient.GetStreamingResponseAsync(conversationHistory, options);
        var streamedResponse = "";
        await foreach (var update in responseStream)
        {
            Console.Write(update.Text);
            streamedResponse += update.Text;
        }
        Console.WriteLine();

        conversationHistory.Add(new(ChatRole.Assistant, streamedResponse));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error handling input: {ex.Message}");
    }
}