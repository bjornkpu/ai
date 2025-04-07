using System.Globalization;
using ModelContextProtocol.Client;
using ModelContextProtocol.Configuration;
using ModelContextProtocol.Protocol.Transport;

namespace McpClient;

public class McpClientService(Config config)
{
    public async Task<List<IMcpClient>> CreateClientsAsync()
    {
        var clients = new List<IMcpClient>();
        var tasks = config.McpServers
            .Where(serverConfig => serverConfig.Value.Enabled)
            .Select(CreateClientAsync)
            .ToList();

        var results = await Task.WhenAll(tasks);

        clients.AddRange(results.Where(client => client != null)!);

        return clients;
    }

    private static async Task<IMcpClient?> CreateClientAsync(KeyValuePair<string, McpConfig> serverConfig)
    {
        try
        {
            var mcpServerConfig = new McpServerConfig
            {
                Id = serverConfig.Key,
                Name = string.Join(" ", serverConfig.Key.Split('-')
                    .Select(word => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word))),
                TransportType = TransportTypes.StdIo,
                Location = null,
                Arguments = [],
                TransportOptions = new Dictionary<string, string>
                {
                    ["command"] = serverConfig.Value.Command,
                    ["arguments"] = string.Join(" ", serverConfig.Value.Args)
                }
            };

            var client = await McpClientFactory.CreateAsync(mcpServerConfig);
            Console.WriteLine($"MCP client for server '{serverConfig.Key}' initialized.");
            return client;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize MCP server '{serverConfig.Key}': {ex.Message}");
            return null;
        }
    }
}