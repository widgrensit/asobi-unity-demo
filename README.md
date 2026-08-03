# Asobi Arena Demo

A multiplayer top-down arena shooter demo built with [Asobi](https://github.com/widgrensit/asobi) game backend and the [Asobi Unity SDK](https://github.com/widgrensit/asobi-unity).

Players match up, spawn into an arena, move with WASD, aim with mouse, and shoot with left click. Best of 3 rounds, 90 seconds each, with a boon pick and a modifier vote in between. Kills accumulate across the match and are submitted to a global leaderboard.

## Prerequisites

- Unity 2021.3+ (LTS recommended)
- The [asobi CLI](https://github.com/widgrensit/asobi-cli) and Docker (for `asobi dev`)

## Backend Setup

The full arena game logic (boons, modifiers, voting, bots) is bundled in `lua/`,
kept in sync with [asobi_arena_lua](https://github.com/widgrensit/asobi_arena_lua).
Run it locally with one command:

```bash
asobi dev
```

The server listens on `http://localhost:8084` - the host and port the client connects to in `Assets/Scripts/Shared/GameConfig.cs`. On Windows and macOS this needs Docker Desktop running; on Windows use the WSL2 backend. Leave it running.

## Unity Setup

1. Open this project in Unity
2. The SDK is pulled automatically via Package Manager (see `Packages/manifest.json`)
3. Hit Play

The four scenes (`Login`, `Lobby`, `Arena`, `Results`) are committed under
`Assets/Scenes/` and already registered in Build Settings with **Login** at
index 0. Each is a single empty GameObject carrying its `*Bootstrap`
component - all UI is built programmatically at runtime.

### SDK version

`Packages/manifest.json` pins the SDK to a tag
(`asobi-unity.git#v0.13.1`) so the demo builds reproducibly. To move to a
newer SDK, bump the tag in `manifest.json` and the matching
`version`/`hash` in `Packages/packages-lock.json`.

## Controls

- **WASD** - Move
- **Mouse** - Aim
- **Left Click** - Shoot
- Best of 3 rounds, 90 seconds each; most cumulative kills wins

## Architecture

```
Login → Lobby → [Matchmaker] → Arena → Results → Lobby
                                          ↓
                                     Leaderboard
```

- **Login**: Register/login via REST API
- **Lobby**: Connect WebSocket, enter matchmaker queue
- **Arena**: Real-time game state sync at 10 ticks/sec via WebSocket
- **Results**: Show standings, submit kills to leaderboard

## Tests

The parsing layer is covered by a licence-free .NET test project so CI can
run it without a Unity seat:

```bash
dotnet test Tests/AsobiDemo.NET/AsobiDemo.Tests.csproj
```

See `Tests/AsobiDemo.NET/README.md`.
