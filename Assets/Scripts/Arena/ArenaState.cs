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
        public string phase;
        public int round;
        public string modifier;
        public List<BoonOffer> boon_offers;
        public int picks_done;
        public List<string> my_boons;
        public List<StandingEntry> standings;

        public static ArenaState Parse(string json)
        {
            var state = new ArenaState
            {
                players = new Dictionary<string, ArenaPlayer>(),
                projectiles = new List<ArenaProjectile>(),
                boon_offers = new List<BoonOffer>(),
                my_boons = new List<string>(),
                standings = new List<StandingEntry>()
            };

            state.time_remaining = ParseFloat(json, "time_remaining");
            state.phase = ParseString(json, "phase");
            state.round = ParseInt(json, "round");
            state.modifier = ParseString(json, "modifier");
            state.picks_done = ParseInt(json, "picks_done");

            // Parse players
            var playersBlock = ExtractBlock(json, "players");
            if (playersBlock != null)
            {
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
                        deaths = ParseInt(playerJson, "deaths"),
                        name = ParseString(playerJson, "name")
                    };
                    state.players[playerId] = player;
                    pos = braceEnd + 1;
                }
            }

            // Parse projectiles
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

            // Parse boon_offers
            var boonBlock = ExtractArray(json, "boon_offers");
            if (boonBlock != null)
            {
                int pos = 0;
                while (pos < boonBlock.Length)
                {
                    var braceStart = boonBlock.IndexOf('{', pos);
                    if (braceStart < 0) break;
                    var braceEnd = FindMatchingBrace(boonBlock, braceStart);
                    if (braceEnd < 0) break;

                    var boonJson = boonBlock.Substring(braceStart, braceEnd - braceStart + 1);
                    state.boon_offers.Add(new BoonOffer
                    {
                        id = ParseString(boonJson, "id"),
                        name = ParseString(boonJson, "name"),
                        description = ParseString(boonJson, "description")
                    });
                    pos = braceEnd + 1;
                }
            }

            // Parse my_boons (string array)
            var myBoonsBlock = ExtractArray(json, "my_boons");
            if (myBoonsBlock != null)
            {
                var matches = Regex.Matches(myBoonsBlock, "\"([^\"]+)\"");
                foreach (Match m in matches)
                    state.my_boons.Add(m.Groups[1].Value);
            }

            // Parse standings
            var standingsBlock = ExtractArray(json, "standings");
            if (standingsBlock != null)
            {
                int pos = 0;
                while (pos < standingsBlock.Length)
                {
                    var braceStart = standingsBlock.IndexOf('{', pos);
                    if (braceStart < 0) break;
                    var braceEnd = FindMatchingBrace(standingsBlock, braceStart);
                    if (braceEnd < 0) break;

                    var sJson = standingsBlock.Substring(braceStart, braceEnd - braceStart + 1);
                    state.standings.Add(new StandingEntry
                    {
                        player_id = ParseString(sJson, "player_id"),
                        kills = ParseInt(sJson, "kills"),
                        deaths = ParseInt(sJson, "deaths"),
                        rank = ParseInt(sJson, "rank")
                    });
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

        public static string ParseString(string json, string key)
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
        public string name;
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

    [Serializable]
    public class BoonOffer
    {
        public string id;
        public string name;
        public string description;
    }

    [Serializable]
    public class VoteOption
    {
        public string id;
        public string label;
    }

    [Serializable]
    public class VoteTally
    {
        public string option_id;
        public int count;
    }

    [Serializable]
    public class StandingEntry
    {
        public string player_id;
        public int kills;
        public int deaths;
        public int rank;
    }
}
