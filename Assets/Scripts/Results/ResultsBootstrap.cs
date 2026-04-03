using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    public class ResultsBootstrap : MonoBehaviour
    {
        void Awake()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            var titleText = CreateText(canvasGo.transform, "RESULTS", 48, NavalTheme.Primary,
                new Vector2(0, 250));

            var standingsText = CreateText(canvasGo.transform, "", 20, NavalTheme.Secondary,
                new Vector2(0, 80), new Vector2(500, 200));

            var lbText = CreateText(canvasGo.transform, "Loading...", 18, NavalTheme.TextDim,
                new Vector2(0, -120), new Vector2(400, 200));

            var playBtn = CreateButton(canvasGo.transform, "PLAY AGAIN", NavalTheme.Primary,
                new Vector2(-100, -280));

            var quitBtn = CreateButton(canvasGo.transform, "QUIT", NavalTheme.Error,
                new Vector2(100, -280));

            var ui = gameObject.AddComponent<ResultsUI>();
            SetField(ui, "titleText", titleText);
            SetField(ui, "standingsText", standingsText);
            SetField(ui, "leaderboardText", lbText);
            SetField(ui, "playAgainButton", playBtn);
            SetField(ui, "quitButton", quitBtn);

            Camera.main.backgroundColor = NavalTheme.Background;
        }

        Button CreateButton(Transform parent, string label, Color color, Vector2 pos)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(160, 50);
            rect.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = color;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform);
            var tRect = textGo.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 22;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;

            return go.AddComponent<Button>();
        }

        TMP_Text CreateText(Transform parent, string content, int fontSize, Color color,
            Vector2 pos, Vector2? size = null)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size ?? new Vector2(500, 50);
            rect.anchoredPosition = pos;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        static void SetField(object obj, string name, object value)
        {
            var field = obj.GetType().GetField(name,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
