using System;
using System.Collections.Generic;

namespace AsobiDemo
{
    public static class MatchResult
    {
        public static string Winner;
        public static List<PlayerStanding> Standings = new();

        [Serializable]
        public class PlayerStanding
        {
            public string player_id;
            public int kills;
            public int deaths;
            public int rank;
        }
    }
}
