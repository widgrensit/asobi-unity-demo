using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    public class BoonPickUI : MonoBehaviour
    {
        GameObject _root;
        TMP_Text _titleText;
        TMP_Text _timerText;
        readonly List<GameObject> _buttons = new();
        bool _picked;

        public event Action<string> OnBoonPicked;

        public void Show(List<BoonOffer> offers, float timeRemaining, int picksDone)
        {
            if (_root == null) Build();
            _root.SetActive(true);
            _picked = false;

            _titleText.text = $"CHOOSE YOUR BOON ({picksDone} picked)";
            _timerText.text = $"{timeRemaining / 1000f:F0}s";

            foreach (var b in _buttons) Destroy(b);
            _buttons.Clear();

            float startX = -(offers.Count - 1) * 110f;
            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                var btn = CreateBoonButton(offer, new Vector2(startX + i * 220f, 0));
                _buttons.Add(btn);
            }
        }

        public void UpdateTimer(float timeRemaining)
        {
            if (_timerText != null)
                _timerText.text = $"{timeRemaining / 1000f:F0}s";
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
        }

        void Build()
        {
            var canvasGo = new GameObject("BoonPickCanvas");
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
            oImg.color = new Color(0, 0, 0, 0.7f);

            _titleText = CreateText(canvasGo.transform, "CHOOSE YOUR BOON", 32,
                NavalTheme.Primary, new Vector2(0, 200));
            _timerText = CreateText(canvasGo.transform, "", 24,
                NavalTheme.Secondary, new Vector2(0, 160));

            _root = canvasGo;
        }

        GameObject CreateBoonButton(BoonOffer offer, Vector2 pos)
        {
            var go = new GameObject("Boon_" + offer.id);
            go.transform.SetParent(_root.transform);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200, 160);
            rect.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = NavalTheme.PanelBg;

            var nameText = CreateText(go.transform, offer.name, 22,
                NavalTheme.Tertiary, new Vector2(0, 40), new Vector2(180, 30));
            var descText = CreateText(go.transform, offer.description, 16,
                NavalTheme.TextDim, new Vector2(0, -10), new Vector2(180, 60));

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = NavalTheme.Secondary;
            colors.pressedColor = NavalTheme.Primary;
            btn.colors = colors;

            btn.onClick.AddListener(() =>
            {
                if (_picked) return;
                _picked = true;
                OnBoonPicked?.Invoke(offer.id);
                img.color = NavalTheme.Tertiary * 0.5f;
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

        void OnDestroy()
        {
            if (_root != null) Destroy(_root);
        }
    }
}
