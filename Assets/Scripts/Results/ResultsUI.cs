using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    public class ResultsUI : MonoBehaviour
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text standingsText;
        [SerializeField] TMP_Text leaderboardText;
        [SerializeField] Button playAgainButton;
        [SerializeField] Button quitButton;

        async void Start()
        {
            playAgainButton.onClick.AddListener(() => SceneLoader.LoadLobby());
            quitButton.onClick.AddListener(() => SceneLoader.LoadLogin());

            // Show match results
            var myId = GameConfig.Client.PlayerId;
            var isWinner = MatchResult.Winner == myId;
            titleText.text = isWinner ? "VICTORY!" : "DEFEAT";
            titleText.color = isWinner ? NavalTheme.Tertiary : NavalTheme.Error;

            var sb = new System.Text.StringBuilder();
            foreach (var s in MatchResult.Standings)
            {
                var marker = s.player_id == myId ? " (YOU)" : "";
                sb.AppendLine($"#{s.rank}  {s.player_id[..8]}...{marker}  K:{s.kills} D:{s.deaths}");
            }
            standingsText.text = sb.ToString();

            // Submit score and show leaderboard
            leaderboardText.text = "Loading leaderboard...";
            try
            {
                var myStanding = MatchResult.Standings.Find(s => s.player_id == myId);
                if (myStanding != null)
                    await GameConfig.Client.Leaderboards.SubmitScoreAsync(
                        GameConfig.LeaderboardId, myStanding.kills);

                var lb = await GameConfig.Client.Leaderboards.GetTopAsync(GameConfig.LeaderboardId, 10);
                var lbSb = new System.Text.StringBuilder("--- TOP 10 ---\n");
                for (int i = 0; i < lb.entries.Length; i++)
                {
                    var e = lb.entries[i];
                    var me = e.player_id == myId ? " *" : "";
                    lbSb.AppendLine($"{i + 1}. {e.player_id[..8]}... - {e.score} kills{me}");
                }
                leaderboardText.text = lbSb.ToString();
            }
            catch
            {
                leaderboardText.text = "Could not load leaderboard";
            }
        }
    }
}
