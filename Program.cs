using McpDemo.Tools;

var builder = WebApplication.CreateBuilder(args);

// Register the MCP server with HTTP (Streamable HTTP) transport and
// discover every tool defined in this assembly.
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// The MCP endpoint an AI client connects to.
app.MapMcp("/mcp");

// A plain landing page so a human can confirm the server is up.
app.MapGet("/", () => "MCP demo server is running. Connect an MCP client to /mcp.");

app.Run();
