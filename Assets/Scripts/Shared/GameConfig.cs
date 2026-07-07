using Asobi;
using UnityEngine;

namespace AsobiDemo
{
    public static class GameConfig
    {
        public const string Host = "localhost";
        public const int Port = 8084;
        public const string GameMode = "arena";
        public const string LeaderboardId = "arena_kills";

        public const float ArenaWidth = 800f;
        public const float ArenaHeight = 600f;
        public const float PixelsPerUnit = 50f;

        static AsobiClient _client;

        public static AsobiClient Client
        {
            get
            {
                _client ??= new AsobiClient(Host, Port);
                return _client;
            }
        }
    }
}
