using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    public class VoteUI : MonoBehaviour
    {
        GameObject _root;
        TMP_Text _titleText;
        TMP_Text _timerText;
        TMP_Text _resultText;
        readonly List<GameObject> _optionButtons = new();
        readonly Dictionary<string, TMP_Text> _tallyTexts = new();
        bool _voted;
        string _currentVoteId;

        public event Action<string, string> OnVoteCast; // voteId, optionId

        public void ShowVote(string voteId, List<VoteOption> options, float windowMs)
        {
            if (_root == null) Build();
            _root.SetActive(true);
            _voted = false;
            _currentVoteId = voteId;
            _resultText.gameObject.SetActive(false);

            _titleText.text = "VOTE";
            _timerText.text = $"{windowMs / 1000f:F0}s";

            foreach (var b in _optionButtons) Destroy(b);
            _optionButtons.Clear();
            _tallyTexts.Clear();

            float startX = -(options.Count - 1) * 100f;
            for (int i = 0; i < options.Count; i++)
            {
                var opt = options[i];
                var btn = CreateOptionButton(opt, new Vector2(startX + i * 200f, 0));
                _optionButtons.Add(btn);
            }
        }

        public void UpdateTally(string rawJson, float timeRemainingMs)
        {
            _timerText.text = $"{timeRemainingMs / 1000f:F0}s";

            // Parse tallies array from raw JSON
            var talliesBlock = ExtractArray(rawJson, "tallies");
            if (talliesBlock == null) return;

            var matches = Regex.Matches(talliesBlock, "\\{[^}]+\\}");
            foreach (Match m in matches)
            {
                var optId = ArenaState.ParseString(m.Value, "option_id");
                var count = 0;
                var countMatch = Regex.Match(m.Value, "\"count\"\\s*:\\s*(\\d+)");
                if (countMatch.Success) int.TryParse(countMatch.Groups[1].Value, out count);

                if (_tallyTexts.TryGetValue(optId, out var text))
                    text.text = count.ToString();
            }
        }

        public void ShowResult(string winnerLabel)
        {
            _resultText.gameObject.SetActive(true);
            _resultText.text = $"Winner: {winnerLabel}";

            // Auto-hide after 3 seconds
            Invoke(nameof(Hide), 3f);
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        void Build()
        {
            var canvasGo = new GameObject("VoteCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dim overlay
            var overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasGo.transform);
            var oRect = overlay.AddComponent<RectTransform>();
            oRect.anchorMin = Vector2.zero;
            oRect.anchorMax = Vector2.one;
            oRect.offsetMin = Vector2.zero;
            oRect.offsetMax = Vector2.zero;
            var oImg = overlay.AddComponent<Image>();
            oImg.color = new Color(0, 0, 0, 0.6f);

            _titleText = CreateText(canvasGo.transform, "VOTE", 36,
                NavalTheme.Primary, new Vector2(0, 200));
            _timerText = CreateText(canvasGo.transform, "", 24,
                NavalTheme.Secondary, new Vector2(0, 160));
            _resultText = CreateText(canvasGo.transform, "", 28,
                NavalTheme.Tertiary, new Vector2(0, -120));
            _resultText.gameObject.SetActive(false);

            _root = canvasGo;
        }

        GameObject CreateOptionButton(VoteOption opt, Vector2 pos)
        {
            var go = new GameObject("Vote_" + opt.id);
            go.transform.SetParent(_root.transform);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(180, 100);
            rect.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = NavalTheme.PanelBg;

            var labelText = CreateText(go.transform, opt.label, 20,
                NavalTheme.Primary, new Vector2(0, 15), new Vector2(160, 30));

            var tallyText = CreateText(go.transform, "0", 28,
                NavalTheme.Tertiary, new Vector2(0, -20), new Vector2(160, 40));
            _tallyTexts[opt.id] = tallyText;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = NavalTheme.Secondary;
            btn.colors = colors;

            var capturedId = opt.id;
            btn.onClick.AddListener(() =>
            {
                if (_voted) return;
                _voted = true;
                img.color = NavalTheme.Primary * 0.4f;
                OnVoteCast?.Invoke(_currentVoteId, capturedId);
            });

            return go;
        }

        TMP_Text CreateText(Transform parent, string content, int fontSize,
            Color color, Vector2 pos, Vector2? size = null)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(400, 40);
            rect.anchoredPosition = pos;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            return text;
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

        void OnDestroy()
        {
            if (_root != null) Destroy(_root);
        }
    }
}
