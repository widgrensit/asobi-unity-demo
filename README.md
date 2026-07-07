# Asobi Arena Demo

A multiplayer top-down arena shooter demo built with [Asobi](https://github.com/widgrensit/asobi) game backend and the [Asobi Unity SDK](https://github.com/widgrensit/asobi-unity).

Players match up, spawn into an arena, move with WASD, aim with mouse, and shoot with left click. 90-second rounds, most kills wins. Scores submit to a global leaderboard.

## Prerequisites

- Unity 2021.3+ (LTS recommended)
- The [asobi CLI](https://github.com/widgrensit/asobi-cli) and Docker (for `asobi dev`)

## Backend Setup

The full arena game logic (boons, modifiers, voting, bots) is bundled in `lua/`.
Run it locally with one command:

```bash
asobi dev
```

The server listens on `http://localhost:8084` - the host and port the client connects to in `Assets/Scripts/Shared/GameConfig.cs`. On Windows and macOS this needs Docker Desktop running; on Windows use the WSL2 backend. Leave it running.

## Unity Setup

1. Open this project in Unity
2. The SDK is pulled automatically via Package Manager (see `Packages/manifest.json`)
3. Create 4 scenes in `Assets/Scenes/`:

| Scene | Setup |
|-------|-------|
| **Login** | Create empty GameObject, add `LoginBootstrap` component |
| **Lobby** | Create empty GameObject, add `LobbyBootstrap` component |
| **Arena** | Create empty GameObject, add `ArenaBootstrap` component |
| **Results** | Create empty GameObject, add `ResultsBootstrap` component |

4. Add all 4 scenes to Build Settings (File > Build Settings > Add Open Scenes)
5. Make sure **Login** is the first scene (index 0)
6. Hit Play

## Controls

- **WASD** - Move
- **Mouse** - Aim
- **Left Click** - Shoot
- Match lasts 90 seconds, most kills wins

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

All UI is created programmatically by the Bootstrap scripts — no prefabs or scene setup needed beyond adding a single component to an empty GameObject.
