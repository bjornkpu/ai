using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace McpClient;

// ReSharper disable once InconsistentNaming
public class AzureOpenAIService
{
    private readonly IChatClient _chatClient;

    public AzureOpenAIService(Config config)
    {
        var llmConfig = config.Llm;
        if (llmConfig.Provider != "azureOpenAI")
        {
            throw new InvalidOperationException("LLM configuration is not set to Azure OpenAI.");
        }

        var azureOpenAiClient = new AzureOpenAIClient(
                new Uri(llmConfig.BaseUrl),
                new ApiKeyCredential(llmConfig.ApiKey))
            .AsChatClient(llmConfig.Model);

        _chatClient = azureOpenAiClient
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();
    }

    public IChatClient GetChatClient()
    {
        return _chatClient;
    }
}