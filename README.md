# MCP Demo Server — ASP.NET Core + Model Context Protocol

A small, working demo of the [Model Context Protocol (MCP)](https://modelcontextprotocol.io/)
built with ASP.NET Core and the MCP C# SDK (v2.0). It shows, in code anyone can
read in minutes, how an AI assistant discovers and calls tools on a server:
the assistant asks the server what tools exist, picks one, sends inputs, and
gets a real result back — computed by ordinary C# code running on the server.

This is a personal portfolio project. It contains no client or employer code,
no credentials, and no secrets. Everything runs locally; the tools make no
network calls.

## What it does

The server exposes three tools over one HTTP endpoint (`/mcp`):

| Tool | What it does |
| --- | --- |
| `text_insights` | Analyzes text: word and sentence counts, average word length, estimated reading time, and the five most frequent meaningful words. |
| `slugify_title` | Turns a title into a clean URL slug (`"My First MCP Demo!"` → `my-first-mcp-demo`) and reports whether the result is URL-safe. |
| `plan_task` | Breaks a goal into a short ordered plan (2–6 steps) with rough time estimates. |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or newer)

## Run it

```bash
dotnet run
```

The server starts on `http://localhost:5238` by default (the exact port is
shown in the console output). Visiting `/` in a browser shows a short
"server is running" message. The MCP endpoint itself lives at `/mcp`.

## Try it with an MCP client

Any MCP client that supports Streamable HTTP can connect to
`http://localhost:5238/mcp` and will see the three tools above. For a quick
check without a client, the repository's tools can be exercised with plain
HTTP requests (initialize → list tools → call a tool); see the examples below.

```bash
# 1. Start a session
curl -s -i -X POST http://localhost:5238/mcp \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-03-26","capabilities":{},"clientInfo":{"name":"curl-check","version":"1.0"}}}'
# Copy the session id from the `mcp-session-id` response header, then use it below as SESSION.

# 2. List the tools
curl -s -X POST http://localhost:5238/mcp \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -H "Mcp-Session-Id: SESSION" \
  -d '{"jsonrpc":"2.0","method":"notifications/initialized"}' -o /dev/null
curl -s -X POST http://localhost:5238/mcp \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -H "Mcp-Session-Id: SESSION" \
  -d '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'

# 3. Call a tool
curl -s -X POST http://localhost:5238/mcp \
  -H "Content-Type: application/json" \
  -H "Accept: application/json, text/event-stream" \
  -H "Mcp-Session-Id: SESSION" \
  -d '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"slugify_title","arguments":{"title":"My First MCP Demo!"}}}'
```

## Project layout

- `Program.cs` — wires up the MCP server and maps the `/mcp` endpoint.
- `DemoTools.cs` — the three tools, each a plain C# method with a description.
- `mcp-genai-demo.csproj` — references `ModelContextProtocol.AspNetCore` v2.0.0.

## Push it to GitHub

```bash
git init
git add .
git commit -m "Add ASP.NET Core MCP demo server"
# Create an empty repository on github.com (no README, no .gitignore — this
# project already has both), then:
git remote add origin https://github.com/<your-username>/<your-repo>.git
git branch -M main
git push -u origin main
```

## Why this exists

Resumes can claim AI-tooling experience; this repo lets anyone run the proof.
It is deliberately small: one server file, one tools file, three tools, no
framework ceremony — readable in one sitting.
