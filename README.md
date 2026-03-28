# Asobi Arena Demo

A multiplayer top-down arena shooter demo built with [Asobi](https://github.com/widgrensit/asobi) game backend and the [Asobi Unity SDK](https://github.com/widgrensit/asobi-unity).

Players match up, spawn into an arena, move with WASD, aim with mouse, and shoot with left click. 90-second rounds, most kills wins. Scores submit to a global leaderboard.

## Prerequisites

- Unity 2021.3+ (LTS recommended)
- Asobi backend running locally (`rebar3 shell`)
- PostgreSQL (via `docker compose up -d` in the asobi project)

## Backend Setup

1. Configure the arena game mode in your asobi `sys.config`:

```erlang
{asobi, [
    {game_modes, #{
        <<"arena">> => asobi_arena
    }}
]}
```

2. Start the backend:

```sh
cd /path/to/asobi
docker compose up -d
rebar3 shell
```

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
