using System.Collections.Generic;
using System.Text.RegularExpressions;
using Asobi;
using UnityEngine;

namespace AsobiDemo
{
    public class ArenaManager : MonoBehaviour
    {
        [SerializeField] GameObject playerPrefab;
        [SerializeField] GameObject localPlayerPrefab;
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] TMPro.TMP_Text timerText;
        [SerializeField] TMPro.TMP_Text killsText;
        [SerializeField] TMPro.TMP_Text hpText;
        [SerializeField] TMPro.TMP_Text roundText;
        [SerializeField] TMPro.TMP_Text modifierText;
        [SerializeField] TMPro.TMP_Text boonsText;

        readonly Dictionary<string, GameObject> _playerObjects = new();
        readonly Dictionary<int, GameObject> _projectileObjects = new();
        Camera _cam;
        string _myId;
        ArenaState _latestState;
        bool _matchEnded;
        bool _countdownDone;
        string _lastPhase;

        BoonPickUI _boonPickUI;
        VoteUI _voteUI;
        CountdownUI _countdownUI;

        void Start()
        {
            _cam = Camera.main;
            _cam.backgroundColor = NavalTheme.Background;
            _myId = GameConfig.Client.PlayerId;

            _cam.transform.position = new Vector3(
                GameConfig.ArenaWidth / (2 * GameConfig.PixelsPerUnit),
                GameConfig.ArenaHeight / (2 * GameConfig.PixelsPerUnit),
                -10);
            _cam.orthographicSize = GameConfig.ArenaHeight / (2 * GameConfig.PixelsPerUnit) + 0.5f;

            // Countdown
            _countdownUI = gameObject.AddComponent<CountdownUI>();
            _countdownUI.OnCountdownComplete += () => _countdownDone = true;
            _countdownUI.StartCountdown();

            // Boon pick UI
            _boonPickUI = gameObject.AddComponent<BoonPickUI>();
            _boonPickUI.OnBoonPicked += OnBoonPicked;

            // Vote UI
            _voteUI = gameObject.AddComponent<VoteUI>();
            _voteUI.OnVoteCast += OnVoteCast;

            var client = GameConfig.Client;
            client.Realtime.OnMatchState += OnMatchState;
            client.Realtime.OnMatchEvent += OnMatchEvent;
            client.Realtime.OnVoteStart += OnVoteStart;
            client.Realtime.OnVoteTally += OnVoteTally;
            client.Realtime.OnVoteResult += OnVoteResult;
        }

        void OnMatchState(string rawJson)
        {
            var payload = ExtractPayload(rawJson);
            var state = ArenaState.Parse(payload);
            if (state != null)
                _latestState = state;
        }

        void OnMatchEvent(string eventName, string rawJson)
        {
            if (eventName == "finished")
            {
                var payload = ExtractPayload(rawJson);
                var result = JsonUtility.FromJson<ArenaMatchResult>(payload);
                if (result != null)
                {
                    _matchEnded = true;
                    UnityMainThread.Enqueue(() =>
                    {
                        MatchResult.Winner = result.winner;
                        MatchResult.Standings = result.standings ?? new();
                        // Show results for 3s then auto-queue
                        Invoke(nameof(AutoQueue), 3f);
                    });
                }
            }
        }

        void AutoQueue()
        {
            SceneLoader.LoadLobby();
        }

        void OnVoteStart(string rawJson)
        {
            UnityMainThread.Enqueue(() =>
            {
                var payload = ExtractPayload(rawJson);
                var voteId = ArenaState.ParseString(payload, "vote_id");
                var windowMs = 0f;
                var wMatch = Regex.Match(payload, "\"window_ms\"\\s*:\\s*(\\d+)");
                if (wMatch.Success) float.TryParse(wMatch.Groups[1].Value, out windowMs);

                var options = ParseVoteOptions(payload);
                _voteUI.ShowVote(voteId, options, windowMs);
            });
        }

        void OnVoteTally(string rawJson)
        {
            UnityMainThread.Enqueue(() =>
            {
                var payload = ExtractPayload(rawJson);
                var timeRem = 0f;
                var tMatch = Regex.Match(payload, "\"time_remaining_ms\"\\s*:\\s*(\\d+)");
                if (tMatch.Success) float.TryParse(tMatch.Groups[1].Value, out timeRem);
                _voteUI.UpdateTally(payload, timeRem);
            });
        }

        void OnVoteResult(string rawJson)
        {
            UnityMainThread.Enqueue(() =>
            {
                var payload = ExtractPayload(rawJson);
                var winner = ArenaState.ParseString(payload, "winner");
                _voteUI.ShowResult(winner);
            });
        }

        List<VoteOption> ParseVoteOptions(string json)
        {
            var options = new List<VoteOption>();
            var arrPattern = "\"options\"\\s*:\\s*\\[";
            var arrMatch = Regex.Match(json, arrPattern);
            if (!arrMatch.Success) return options;

            var start = json.IndexOf('[', arrMatch.Index);
            int depth = 0, end = start;
            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '[') depth++;
                else if (json[i] == ']') depth--;
                if (depth == 0) { end = i; break; }
            }
            var block = json.Substring(start + 1, end - start - 1);

            var objMatches = Regex.Matches(block, "\\{[^}]+\\}");
            foreach (Match m in objMatches)
            {
                options.Add(new VoteOption
                {
                    id = ArenaState.ParseString(m.Value, "id"),
                    label = ArenaState.ParseString(m.Value, "label")
                });
            }
            return options;
        }

        void OnBoonPicked(string boonId)
        {
            var json = $"{{\"type\":\"boon_pick\",\"boon_id\":\"{boonId}\"}}";
            _ = GameConfig.Client.Realtime.SendMatchInputAsync(json);
        }

        async void OnVoteCast(string voteId, string optionId)
        {
            try
            {
                await GameConfig.Client.Realtime.CastVoteAsync(voteId, optionId);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Arena] Vote cast failed: {ex.Message}");
            }
        }

        void Update()
        {
            if (_matchEnded) return;
            if (!_countdownDone) return;

            if (_latestState == null) return;

            HandlePhaseTransitions(_latestState);

            if (_latestState.phase == "playing" || string.IsNullOrEmpty(_latestState.phase))
            {
                SendInput();
                UpdatePlayers(_latestState);
                UpdateProjectiles(_latestState);
            }

            UpdateHUD(_latestState);
        }

        void HandlePhaseTransitions(ArenaState state)
        {
            if (state.phase == _lastPhase) return;
            var prev = _lastPhase;
            _lastPhase = state.phase;

            if (state.phase == "boon_pick")
            {
                _boonPickUI.Show(state.boon_offers, state.time_remaining, state.picks_done);
            }
            else if (prev == "boon_pick")
            {
                _boonPickUI.Hide();
            }
        }

        void SendInput()
        {
            var mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            var input = new ArenaInput
            {
                up = Input.GetKey(KeyCode.W),
                down = Input.GetKey(KeyCode.S),
                left = Input.GetKey(KeyCode.A),
                right = Input.GetKey(KeyCode.D),
                shoot = Input.GetMouseButton(0),
                aim_x = mouseWorld.x * GameConfig.PixelsPerUnit,
                aim_y = mouseWorld.y * GameConfig.PixelsPerUnit
            };

            if (input.up || input.down || input.left || input.right || input.shoot)
            {
                var json = JsonUtility.ToJson(input);
                _ = GameConfig.Client.Realtime.SendMatchInputAsync(json);
            }
        }

        void UpdatePlayers(ArenaState state)
        {
            if (state.players == null) return;

            var seen = new HashSet<string>();
            foreach (var kv in state.players)
            {
                seen.Add(kv.Key);
                if (!_playerObjects.TryGetValue(kv.Key, out var go))
                {
                    var prefab = kv.Key == _myId ? localPlayerPrefab : playerPrefab;
                    go = Instantiate(prefab);
                    go.SetActive(true);
                    go.name = $"Player_{kv.Key[..8]}";
                    _playerObjects[kv.Key] = go;
                }

                // Update name label
                var label = go.GetComponentInChildren<TMPro.TMP_Text>();
                if (label != null)
                {
                    var displayName = !string.IsNullOrEmpty(kv.Value.name) ? kv.Value.name :
                        (kv.Key == _myId ? "YOU" : kv.Key[..6]);
                    label.text = displayName;
                }

                var pos = new Vector3(
                    kv.Value.x / GameConfig.PixelsPerUnit,
                    kv.Value.y / GameConfig.PixelsPerUnit,
                    0);
                go.transform.position = Vector3.Lerp(go.transform.position, pos, 0.3f);

                // HP bar color based on health fraction
                float hpFraction = kv.Value.hp / 100f;
                var hpBar = go.transform.Find("HPBar");
                if (hpBar != null)
                {
                    hpBar.localScale = new Vector3(Mathf.Max(0, hpFraction) * 1.2f, 0.12f, 1);
                    var hpSr = hpBar.GetComponent<SpriteRenderer>();
                    if (hpSr != null)
                        hpSr.color = kv.Value.hp <= 0 ? Color.grey : NavalTheme.HpColor(hpFraction);
                }

                // Grey out dead ships
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null && kv.Value.hp <= 0)
                    sr.color = Color.grey;
                else if (sr != null)
                    sr.color = Color.white;
            }

            var toRemove = new List<string>();
            foreach (var kv in _playerObjects)
            {
                if (!seen.Contains(kv.Key))
                {
                    Destroy(kv.Value);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var id in toRemove)
                _playerObjects.Remove(id);
        }

        void UpdateProjectiles(ArenaState state)
        {
            var seen = new HashSet<int>();

            if (state.projectiles != null)
            {
                foreach (var proj in state.projectiles)
                {
                    seen.Add(proj.id);
                    if (!_projectileObjects.TryGetValue(proj.id, out var go))
                    {
                        go = Instantiate(projectilePrefab);
                        go.SetActive(true);
                        _projectileObjects[proj.id] = go;

                        var sr = go.GetComponent<SpriteRenderer>();
                        if (sr != null)
                            sr.color = proj.owner == _myId ? NavalTheme.Primary : NavalTheme.Secondary;
                    }

                    go.transform.position = new Vector3(
                        proj.x / GameConfig.PixelsPerUnit,
                        proj.y / GameConfig.PixelsPerUnit,
                        0);
                }
            }

            var toRemove = new List<int>();
            foreach (var kv in _projectileObjects)
            {
                if (!seen.Contains(kv.Key))
                {
                    Destroy(kv.Value);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var id in toRemove)
                _projectileObjects.Remove(id);
        }

        void UpdateHUD(ArenaState state)
        {
            var remaining = state.time_remaining / 1000f;
            timerText.text = $"{(int)remaining / 60}:{(int)remaining % 60:D2}";

            if (state.players != null && state.players.TryGetValue(_myId, out var me))
            {
                killsText.text = $"Kills: {me.kills}";
                hpText.text = $"HP: {me.hp}";
            }

            // Round + modifier
            if (roundText != null && state.round > 0)
                roundText.text = $"Round {state.round}";
            if (modifierText != null && !string.IsNullOrEmpty(state.modifier))
                modifierText.text = state.modifier;

            // Boons display
            if (boonsText != null && state.my_boons != null && state.my_boons.Count > 0)
                boonsText.text = string.Join(" | ", state.my_boons);
            else if (boonsText != null)
                boonsText.text = "";

            // Update boon pick timer if in that phase
            if (state.phase == "boon_pick")
                _boonPickUI.UpdateTimer(state.time_remaining);
        }

        string ExtractPayload(string rawJson)
        {
            var idx = rawJson.IndexOf("\"payload\":");
            if (idx < 0) return rawJson;
            var start = rawJson.IndexOf('{', idx);
            if (start < 0) return rawJson;

            int depth = 0;
            for (int i = start; i < rawJson.Length; i++)
            {
                if (rawJson[i] == '{') depth++;
                else if (rawJson[i] == '}') depth--;
                if (depth == 0) return rawJson.Substring(start, i - start + 1);
            }
            return rawJson;
        }

        void OnDestroy()
        {
            var client = GameConfig.Client;
            client.Realtime.OnMatchState -= OnMatchState;
            client.Realtime.OnMatchEvent -= OnMatchEvent;
            client.Realtime.OnVoteStart -= OnVoteStart;
            client.Realtime.OnVoteTally -= OnVoteTally;
            client.Realtime.OnVoteResult -= OnVoteResult;
        }
    }
}
