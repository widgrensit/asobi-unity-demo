using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AsobiDemo
{
    [Serializable]
    public class ArenaState
    {
        public Dictionary<string, ArenaPlayer> players;
        public List<ArenaProjectile> projectiles;
        public float time_remaining;

        public static ArenaState Parse(string json)
        {
            var state = new ArenaState
            {
                players = new Dictionary<string, ArenaPlayer>(),
                projectiles = new List<ArenaProjectile>()
            };

            state.time_remaining = ParseFloat(json, "time_remaining");

            // Parse players object: "players":{...}
            var playersBlock = ExtractBlock(json, "players");
            if (playersBlock != null)
            {
                // Each player: "id":{...}
                int pos = 0;
                while (pos < playersBlock.Length)
                {
                    var keyStart = playersBlock.IndexOf('"', pos);
                    if (keyStart < 0) break;
                    var keyEnd = playersBlock.IndexOf('"', keyStart + 1);
                    if (keyEnd < 0) break;
                    var playerId = playersBlock.Substring(keyStart + 1, keyEnd - keyStart - 1);

                    var braceStart = playersBlock.IndexOf('{', keyEnd);
                    if (braceStart < 0) break;
                    var braceEnd = FindMatchingBrace(playersBlock, braceStart);
                    if (braceEnd < 0) break;

                    var playerJson = playersBlock.Substring(braceStart, braceEnd - braceStart + 1);
                    var player = new ArenaPlayer
                    {
                        x = ParseFloat(playerJson, "x"),
                        y = ParseFloat(playerJson, "y"),
                        hp = ParseInt(playerJson, "hp"),
                        kills = ParseInt(playerJson, "kills"),
                        deaths = ParseInt(playerJson, "deaths")
                    };
                    state.players[playerId] = player;
                    pos = braceEnd + 1;
                }
            }

            // Parse projectiles array: "projectiles":[...]
            var projBlock = ExtractArray(json, "projectiles");
            if (projBlock != null)
            {
                int pos = 0;
                while (pos < projBlock.Length)
                {
                    var braceStart = projBlock.IndexOf('{', pos);
                    if (braceStart < 0) break;
                    var braceEnd = FindMatchingBrace(projBlock, braceStart);
                    if (braceEnd < 0) break;

                    var projJson = projBlock.Substring(braceStart, braceEnd - braceStart + 1);
                    var proj = new ArenaProjectile
                    {
                        id = ParseInt(projJson, "id"),
                        x = ParseFloat(projJson, "x"),
                        y = ParseFloat(projJson, "y"),
                        owner = ParseString(projJson, "owner")
                    };
                    state.projectiles.Add(proj);
                    pos = braceEnd + 1;
                }
            }

            return state;
        }

        static string ExtractBlock(string json, string key)
        {
            var pattern = "\"" + key + "\"\\s*:\\s*\\{";
            var match = Regex.Match(json, pattern);
            if (!match.Success) return null;
            var start = json.IndexOf('{', match.Index + match.Length - 1);
            var end = FindMatchingBrace(json, start);
            if (end < 0) return null;
            return json.Substring(start + 1, end - start - 1);
        }

        static string ExtractArray(string json, string key)
        {
            var pattern = "\"" + key + "\"\\s*:\\s*\\[";
            var match = Regex.Match(json, pattern);
            if (!match.Success) return null;
            var start = json.IndexOf('[', match.Index + match.Length - 1);
            var end = FindMatchingBracket(json, start);
            if (end < 0) return null;
            return json.Substring(start + 1, end - start - 1);
        }

        static int FindMatchingBrace(string s, int start)
        {
            int depth = 0;
            for (int i = start; i < s.Length; i++)
            {
                if (s[i] == '{') depth++;
                else if (s[i] == '}') depth--;
                if (depth == 0) return i;
            }
            return -1;
        }

        static int FindMatchingBracket(string s, int start)
        {
            int depth = 0;
            for (int i = start; i < s.Length; i++)
            {
                if (s[i] == '[') depth++;
                else if (s[i] == ']') depth--;
                if (depth == 0) return i;
            }
            return -1;
        }

        static float ParseFloat(string json, string key)
        {
            var match = Regex.Match(json, "\"" + key + "\"\\s*:\\s*([\\d.eE+-]+)");
            if (match.Success && float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                return v;
            return 0;
        }

        static int ParseInt(string json, string key)
        {
            var match = Regex.Match(json, "\"" + key + "\"\\s*:\\s*(-?\\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var v))
                return v;
            return 0;
        }

        static string ParseString(string json, string key)
        {
            var match = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : "";
        }
    }

    [Serializable]
    public class ArenaPlayer
    {
        public float x;
        public float y;
        public int hp;
        public int kills;
        public int deaths;
    }

    [Serializable]
    public class ArenaProjectile
    {
        public int id;
        public float x;
        public float y;
        public string owner;
    }

    [Serializable]
    public class ArenaMatchResult
    {
        public string status;
        public string winner;
        public List<MatchResult.PlayerStanding> standings;
    }

    [Serializable]
    public class ArenaInput
    {
        public bool up;
        public bool down;
        public bool left;
        public bool right;
        public bool shoot;
        public float aim_x;
        public float aim_y;
    }
}
