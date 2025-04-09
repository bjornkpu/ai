using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpClient;

public class Config
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".llm",
        "config.json"
    );

    public static readonly string PromptsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".llm",
        "prompts"
    );

    [JsonPropertyName("systemPrompt")] public string SystemPrompt { get; set; }

    [JsonPropertyName("llm")] public LlmConfig Llm { get; set; }

    [JsonPropertyName("mcpServers")] public Dictionary<string, McpConfig> McpServers { get; set; }

    /// <summary>
    ///     Gets the path to the configuration file.
    /// </summary>
    public static string GetConfigPath()
    {
        return ConfigPath;
    }

    /// <summary>
    ///     Loads the configuration from the specified file path. If it doesn't exist, creates a default config.
    /// </summary>
    /// <returns>The configuration object</returns>
    public static async Task<Config> LoadOrCreateAsync()
    {
        // Ensure the directory exists
        var configDir = Path.GetDirectoryName(ConfigPath)
                        ?? throw new InvalidOperationException("Invalid config path");
        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        if (!Directory.Exists(PromptsDir))
        {
            Directory.CreateDirectory(PromptsDir);
        }

        // If the config file does not exist, create it with default values
        if (!File.Exists(ConfigPath))
        {
            var defaultConfig = CreateDefault();
            await SaveAsync(defaultConfig);
            Console.WriteLine("Default configuration file created at " + ConfigPath);
            Environment.Exit(0);
        }

        // Read and deserialize the file into a Config object
        var json = await File.ReadAllTextAsync(ConfigPath);
        var config = JsonSerializer.Deserialize<Config>(json)
                     ?? throw new InvalidOperationException("Failed to deserialize the config");

        config.SetEnv();

        return config;
    }

    public void SetEnv()
    {
        foreach (var server in McpServers)
        {
            foreach (var (key, value) in server.Value.Env)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    /// <summary>
    ///     Saves the current configuration to the specified file.
    /// </summary>
    /// <param name="configPath">The path to the configuration file</param>
    public static async Task SaveAsync(Config config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ConfigPath, json);
    }


    /// <summary>
    ///     Creates a default configuration object with default values.
    /// </summary>
    /// <returns>The default configuration</returns>
    public static Config CreateDefault()
    {
        return new Config
        {
            SystemPrompt = "You are an AI assistant helping a software engineer...",
            Llm = new LlmConfig
            {
                Provider = "azureOpenAI",
                Model = "gpt-4o-mini",
                ApiKey = "your-azure-openai-api-key",
                BaseUrl = "https://your_service.cognitiveservices.azure.com"
            },
            McpServers = new Dictionary<string, McpConfig>
            {
                ["time"] = new()
                {
                    Enabled = true,
                    Command = "podman",
                    Args = ["run", "-i", "--rm", "mcp/time", "--local-timezone=Europe/Oslo"]
                }
            }
        };
    }
}

public class McpConfig
{
    /// <summary>
    ///     Whether the server is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     Command to run the server
    /// </summary>
    [JsonPropertyName("command")]
    public required string Command { get; set; }

    /// <summary>
    ///     Command-line arguments
    /// </summary>
    [JsonPropertyName("args")]
    public string[] Args { get; set; } = [];

    /// <summary>
    ///     Environment variables
    /// </summary>
    [JsonPropertyName("env")]
    public Dictionary<string, string> Env { get; set; } = new();
}

public class LlmConfig
{
    [JsonPropertyName("provider")] public required string Provider { get; set; }

    [JsonPropertyName("model")] public required string Model { get; set; }

    [JsonPropertyName("api_key")] public required string ApiKey { get; set; }

    [JsonPropertyName("base_url")] public required string BaseUrl { get; set; } // Optional
}