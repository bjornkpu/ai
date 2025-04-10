﻿using System.Reflection;
using Cocona;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace McpClient;

public class Commands(McpClientService mcpClientService, AzureOpenAIService azureOpenAiService, Config config)
{
    private IChatClient? chatClient;
    private List<ChatMessage> conversationHistory;
    private ChatOptions options;

    [PrimaryCommand]
    [Command(Description = "Start a conversation with the AI assistant.")]
    public async Task Chat()
    {
        await LoadConfig();

        conversationHistory = new List<ChatMessage>
        {
            new(ChatRole.System, config.SystemPrompt)
        };
        conversationHistory.AddRange(LoadSystemPrompts());

        Console.WriteLine("Start typing your messages below. Type 'exit' to quit.");

        while (true)
        {
            Console.Write("\n>>> ");
            var userInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting. Goodbye!");
                break;
            }

            // if (userInput.Equals("reload", StringComparison.OrdinalIgnoreCase))
            // {
            //     Console.WriteLine("Reloading configuration...");
            //     await LoadConfig();
            //     Console.WriteLine("Done.");
            //     continue;
            // }

            if (userInput.Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("Resetting conversation history...");
                conversationHistory = new List<ChatMessage>
                {
                    new(ChatRole.System, config.SystemPrompt)
                };
                conversationHistory.AddRange(LoadSystemPrompts());
                Console.WriteLine("Done.");
                continue;
            }

            conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));

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
                conversationHistory.Add(new ChatMessage(ChatRole.Assistant, streamedResponse));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling input: {ex.Message}");
            }
        }
    }

    private async Task LoadConfig()
    {
        chatClient = azureOpenAiService.GetChatClient();
        if (chatClient is null)
        {
            Console.WriteLine("Azure OpenAI configuration is missing. Exiting.");
            Environment.Exit(0);
        }

        var mcpClients = await mcpClientService.CreateClientsAsync();
        var tools = new List<AITool>();
        foreach (var client in mcpClients)
        {
            tools.AddRange(await client.ListToolsAsync());
        }

        options = new ChatOptions
        {
            Tools = tools.ToArray()
        };
    }

    [Command(Description = "Display the current version of the CLI.")]
    public void Version()
    {
        var version = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version?.ToString() ?? "unknown";

        Console.WriteLine($"ai v{version}");
    }


    [Command(Description = "Test MCP client connections.")]
    public async Task TestMcpClients()
    {
        var mcpClients = await mcpClientService.CreateClientsAsync();
        foreach (var client in mcpClients)
        {
            Console.WriteLine($"MCP client '{client.ServerInfo?.Name}' is connected.");
        }
    }

    private List<ChatMessage> LoadSystemPrompts()
    {
        var promptFiles = Directory.GetFiles(Config.PromptsDir, "*.md");
        return promptFiles.Select(File.ReadAllText)
            .Where(content => !string.IsNullOrWhiteSpace(content))
            .Select(content => new ChatMessage(ChatRole.System, content))
            .ToList();
    }
}