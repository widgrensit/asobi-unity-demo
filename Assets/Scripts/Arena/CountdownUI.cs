using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    public class CountdownUI : MonoBehaviour
    {
        GameObject _root;
        TMP_Text _countText;
        float _timer;
        int _current;
        bool _active;

        public event Action OnCountdownComplete;

        public void StartCountdown()
        {
            if (_root == null) Build();
            _root.SetActive(true);
            _timer = 0;
            _current = 3;
            _active = true;
            _countText.text = "3";
            _countText.color = NavalTheme.Error;
        }

        void Update()
        {
            if (!_active) return;

            _timer += Time.deltaTime;

            if (_timer >= 1f && _current == 3)
            {
                _current = 2;
                _countText.text = "2";
                _countText.color = NavalTheme.Primary;
            }
            else if (_timer >= 2f && _current == 2)
            {
                _current = 1;
                _countText.text = "1";
                _countText.color = NavalTheme.Secondary;
            }
            else if (_timer >= 3f && _current == 1)
            {
                _current = 0;
                _countText.text = "GO!";
                _countText.color = NavalTheme.Tertiary;
            }
            else if (_timer >= 3.5f && _current == 0)
            {
                _active = false;
                _root.SetActive(false);
                OnCountdownComplete?.Invoke();
            }

            // Pulse scale
            float pulse = 1f + 0.2f * Mathf.Sin(_timer * 8f);
            _countText.transform.localScale = Vector3.one * pulse;
        }

        void Build()
        {
            var canvasGo = new GameObject("CountdownCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            // Dim background
            var overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasGo.transform);
            var oRect = overlay.AddComponent<RectTransform>();
            oRect.anchorMin = Vector2.zero;
            oRect.anchorMax = Vector2.one;
            oRect.offsetMin = Vector2.zero;
            oRect.offsetMax = Vector2.zero;
            var oImg = overlay.AddComponent<Image>();
            oImg.color = new Color(0, 0, 0, 0.5f);

            var textGo = new GameObject("CountdownText");
            textGo.transform.SetParent(canvasGo.transform);
            var rect = textGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300, 200);
            rect.anchoredPosition = Vector2.zero;

            _countText = textGo.AddComponent<TextMeshProUGUI>();
            _countText.fontSize = 120;
            _countText.alignment = TextAlignmentOptions.Center;
            _countText.fontStyle = FontStyles.Bold;

            _root = canvasGo;
        }

        void OnDestroy()
        {
            if (_root != null) Destroy(_root);
        }
    }
}
