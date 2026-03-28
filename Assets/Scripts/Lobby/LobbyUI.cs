using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] Button findMatchButton;
        [SerializeField] Button cancelButton;
        [SerializeField] TMP_Text statusText;
        [SerializeField] TMP_Text playerInfoText;

        bool _searching;
        float _searchTimer;

        async void Start()
        {
            findMatchButton.onClick.AddListener(OnFindMatch);
            cancelButton.onClick.AddListener(OnCancel);
            cancelButton.gameObject.SetActive(false);

            playerInfoText.text = $"Player: {GameConfig.Client.PlayerId}";
            statusText.text = "Ready to play!";

            // Connect WebSocket
            statusText.text = "Connecting...";
            try
            {
                var client = GameConfig.Client;
                client.Realtime.OnMatchmakerMatched += OnMatched;
                client.Realtime.OnError += OnError;
                await client.Realtime.ConnectAsync();
                statusText.text = "Connected! Ready to play.";
            }
            catch (Asobi.AsobiException ex)
            {
                statusText.text = $"Connection failed: {ex.Message}";
            }
        }

        async void OnFindMatch()
        {
            findMatchButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(true);
            _searching = true;
            _searchTimer = 0;
            statusText.text = "Searching for match...";

            try
            {
                await GameConfig.Client.Realtime.AddToMatchmakerAsync(GameConfig.GameMode);
            }
            catch (Asobi.AsobiException ex)
            {
                statusText.text = $"Error: {ex.Message}";
                ResetUI();
            }
        }

        async void OnCancel()
        {
            _searching = false;
            statusText.text = "Cancelling...";

            try
            {
                // Cancel via REST as fallback
                await GameConfig.Client.Realtime.RemoveFromMatchmakerAsync("");
            }
            catch { }

            statusText.text = "Cancelled.";
            ResetUI();
        }

        void OnMatched(string rawJson)
        {
            // Matched! The match server auto-joins us.
            // Transition to arena scene on main thread
            _searching = false;
            UnityMainThread.Enqueue(() =>
            {
                statusText.text = "Match found!";
                SceneLoader.LoadArena();
            });
        }

        void OnError(string rawJson)
        {
            UnityMainThread.Enqueue(() =>
            {
                statusText.text = "Error occurred";
                ResetUI();
            });
        }

        void Update()
        {
            if (_searching)
            {
                _searchTimer += Time.deltaTime;
                statusText.text = $"Searching for match... {_searchTimer:F0}s";
            }
        }

        void ResetUI()
        {
            findMatchButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);
            _searching = false;
        }

        void OnDestroy()
        {
            var client = GameConfig.Client;
            client.Realtime.OnMatchmakerMatched -= OnMatched;
            client.Realtime.OnError -= OnError;
        }
    }
}
