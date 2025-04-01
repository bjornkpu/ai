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

        foreach (var serverConfig in config.McpServers)
        {
            if (!serverConfig.Value.Enabled)
            {
                Console.WriteLine($"MCP server '{serverConfig.Key}' is disabled. Skipping...");
                continue;
            }

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
                clients.Add(client);
                Console.WriteLine($"MCP client for server '{serverConfig.Key}' initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize MCP server '{serverConfig.Key}': {ex.Message}");
            }
        }

        return clients;
    }
}