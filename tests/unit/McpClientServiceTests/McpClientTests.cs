using System.Collections;
using McpClient;

namespace McpClientServiceTests;

public class McpClientTests
{
    [Theory]
    [ClassData(typeof(CreateClientTestData))]
    public async Task CreateClientTest(Config config)
    {
        config.SetEnv();
        var service = new McpClientService(config);
        var clients = await service.CreateClientsAsync();
        Assert.NotNull(clients);
        Assert.Single(clients);
    }

    [Fact]
    public async Task CreateClient_ShouldBeEmpty_WhenNotEnabled()
    {
        var config = new Config
        {
            McpServers = new Dictionary<string, McpConfig>
            {
                ["everything"] = new()
                {
                    Enabled = false,
                    Command = "npx",
                    Args =
                    [
                        "-y",
                        "@modelcontextprotocol/server-everything"
                    ]
                }
            }
        };
        var service = new McpClientService(config);
        var clients = await service.CreateClientsAsync();
        Assert.NotNull(clients);
        Assert.Empty(clients);
    }
}

public class CreateClientTestData : IEnumerable<TheoryDataRow<Config>>
{
    public IEnumerator<TheoryDataRow<Config>> GetEnumerator()
    {
        yield return new TheoryDataRow<Config>(new Config
        {
            McpServers = new Dictionary<string, McpConfig>
            {
                ["everything"] = new()
                {
                    Command = "npx",
                    Args =
                    [
                        "-y",
                        "@modelcontextprotocol/server-everything"
                    ]
                }
            }
        });
        yield return new TheoryDataRow<Config>(new Config
        {
            McpServers = new Dictionary<string, McpConfig>
            {
                ["time"] = new()
                {
                    Command = "podman",
                    Args =
                    [
                        "run",
                        "-i",
                        "--rm",
                        "mcp/fetch"
                    ]
                }
            }
        });
        yield return new TheoryDataRow<Config>(new Config
        {
            McpServers = new Dictionary<string, McpConfig>
            {
                ["fetch"] = new()
                {
                    Command = "podman",
                    Args =
                    [
                        "run",
                        "-i",
                        "--rm",
                        "mcp/fetch"
                    ]
                }
            }
        });
        yield return new TheoryDataRow<Config>(new Config
        {
            McpServers = new Dictionary<string, McpConfig>
            {
                ["filesystem"] = new()
                {
                    Command = "npx",
                    Args =
                    [
                        "-y",
                        "@modelcontextprotocol/server-filesystem",
                        "\"~/.llm\""
                    ]
                }
            }
        });
        yield return new TheoryDataRow<Config>(new Config
        {
            McpServers = new Dictionary<string, McpConfig>
            {
                ["memory"] = new()
                {
                    Command = "podman",
                    Args =
                    [
                        "run",
                        "-i",
                        "-v",
                        "claude-memory:/app/dist",
                        "--rm",
                        "mcp/memory"
                    ]
                }
            }
        });
        yield return new TheoryDataRow<Config>(new Config
        {
            McpServers = new Dictionary<string, McpConfig>
            {
                ["git"] = new()
                {
                    Command = "python",
                    Args =
                    [
                        "-m",
                        "mcp_server_git",
                        "--repository",
                        "\"C:\\Users\\punsvbjo\\personal\\ai\""
                    ]
                }
            }
        });
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}