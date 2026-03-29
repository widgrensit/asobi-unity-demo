using System.Collections.Generic;
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

        readonly Dictionary<string, GameObject> _playerObjects = new();
        readonly Dictionary<int, GameObject> _projectileObjects = new();
        Camera _cam;
        string _myId;
        ArenaState _latestState;
        bool _matchEnded;

        void Start()
        {
            _cam = Camera.main;
            _myId = GameConfig.Client.PlayerId;

            // Set camera to cover arena
            _cam.transform.position = new Vector3(
                GameConfig.ArenaWidth / (2 * GameConfig.PixelsPerUnit),
                GameConfig.ArenaHeight / (2 * GameConfig.PixelsPerUnit),
                -10);
            _cam.orthographicSize = GameConfig.ArenaHeight / (2 * GameConfig.PixelsPerUnit) + 0.5f;

            var client = GameConfig.Client;
            Debug.Log($"[Arena] Start - PlayerId: {_myId}, WS connected: {client.Realtime.IsConnected}");
            client.Realtime.OnMatchState += OnMatchState;
            client.Realtime.OnMatchEvent += OnMatchEvent;
        }

        void OnMatchState(string rawJson)
        {
            Debug.Log($"[Arena] OnMatchState received: {rawJson.Substring(0, System.Math.Min(200, rawJson.Length))}");
            var payload = ExtractPayload(rawJson);
            Debug.Log($"[Arena] Extracted payload: {payload.Substring(0, System.Math.Min(200, payload.Length))}");
            var state = ArenaState.Parse(payload);
            if (state != null)
            {
                Debug.Log($"[Arena] Parsed state: {state.players?.Count ?? -1} players, {state.projectiles?.Count ?? -1} projectiles");
                _latestState = state;
            }
            else
            {
                Debug.LogWarning("[Arena] Failed to parse state");
            }
        }

        void OnMatchEvent(string eventName, string rawJson)
        {
            if (eventName == "finished")
            {
                var result = JsonUtility.FromJson<ArenaMatchResult>(ExtractPayload(rawJson));
                if (result != null)
                {
                    _matchEnded = true;
                    UnityMainThread.Enqueue(() =>
                    {
                        MatchResult.Winner = result.winner;
                        MatchResult.Standings = result.standings ?? new();
                        SceneLoader.LoadResults();
                    });
                }
            }
        }

        void Update()
        {
            if (_matchEnded) return;

            SendInput();

            if (_latestState == null) return;

            UpdatePlayers(_latestState);
            UpdateProjectiles(_latestState);
            UpdateHUD(_latestState);
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

                    var label = go.GetComponentInChildren<TMPro.TMP_Text>();
                    if (label != null)
                        label.text = kv.Key == _myId ? "YOU" : kv.Key[..6];
                }

                var pos = new Vector3(
                    kv.Value.x / GameConfig.PixelsPerUnit,
                    kv.Value.y / GameConfig.PixelsPerUnit,
                    0);
                go.transform.position = Vector3.Lerp(go.transform.position, pos, 0.3f);

                // Grey out dead players
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = kv.Value.hp <= 0 ? Color.grey : (kv.Key == _myId ? Color.cyan : Color.red);

                // HP bar
                var hpBar = go.transform.Find("HPBar");
                if (hpBar != null)
                    hpBar.localScale = new Vector3(kv.Value.hp / 100f, 1, 1);
            }

            // Remove disconnected players
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
                            sr.color = proj.owner == _myId ? Color.yellow : Color.white;
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
        }

        string ExtractPayload(string rawJson)
        {
            // Quick extraction of "payload" value from WS message JSON
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
        }
    }
}
