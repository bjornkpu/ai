## **MCP Client CLI Tool**

### **Overview**

`MCP Client CLI` is a cross-platform AI-powered .NET CLI tool designed to connect to and utilize Model Context Protocol (MCP) servers. Key features of the tool include:

- **AI Chat**: Start conversations with an AI assistant using MCP tools.
- **Configuration**: Connect to MCP Servers though the config in you `~/.llm/config.json`.

### **Installation**

To install the tool globally, simply run the following command:

```shell script
  dotnet tool install --global bjornkpu.ai
```

After installing, you can run the tool using the `ai` command.

To update the tool, use:

```shell script
  dotnet tool update --global bjornkpu.ai
```

To uninstall it, use:

```shell script
  dotnet tool uninstall --global bjornkpu.ai
```

---

### **How to Use**

#### **Chat with AI**
Start a conversation with the AI assistant:
```shell script
  ai
```
- Once the command starts it connects to the servers, you can type your messages directly into the console.
- Type `"exit"` or empty return to quit the conversation.
- The tool integrates with MCP servers to enhance the AI with tools and data.

#### **Check functionality**
Check what the tool can do through the help menu:
```shell script
  ai --help
```

---

### **Getting Started with Development**

If you're contributing to or testing the tool locally:

1. Clone the repository:
```shell script
  git clone https://github.com/bjornkpu/ai.git
  cd ai
```

2. Build the project:
```shell script
  cd src/McpClient
  dotnet build
```

3. Run the tool locally:
```shell script
  dotnet run
```

4. Run unit tests:
```shell script
  dotnet test
```
