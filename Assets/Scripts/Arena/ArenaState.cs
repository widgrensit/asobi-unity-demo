using System;
using System.Collections.Generic;

namespace AsobiDemo
{
    [Serializable]
    public class ArenaState
    {
        public Dictionary<string, ArenaPlayer> players;
        public List<ArenaProjectile> projectiles;
        public float time_remaining;
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
