# Wire-contract tests

Standalone .NET tests for the demo's pure parsing layer, so the shapes it
assumes about the server stay pinned without a Unity licence in CI.

Source files are shared via `<Compile Include="..\..\Assets\Scripts\..." Link="..."/>`
in the csproj — no copies. Only files free of `UnityEngine` can be linked:

- `Assets/Scripts/Arena/ArenaState.cs`
- `Assets/Scripts/Shared/MatchResult.cs`

The MonoBehaviour layer (`ArenaManager`, the UI scripts) needs PlayMode and
isn't covered here.

## Run

```sh
dotnet test Tests/AsobiDemo.NET/AsobiDemo.Tests.csproj
```
