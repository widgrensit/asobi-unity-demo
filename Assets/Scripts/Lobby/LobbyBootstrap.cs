using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    /// <summary>
    /// Drop on an empty GameObject in the Lobby scene. Creates the lobby UI at runtime.
    /// </summary>
    public class LobbyBootstrap : MonoBehaviour
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

            // Player info
            var playerText = CreateText(canvasGo.transform, "Player: ...", 18, Color.grey,
                new Vector2(0, 180));

            // Status
            var statusText = CreateText(canvasGo.transform, "Connecting...", 24, Color.white,
                new Vector2(0, 40));

            // Find Match button
            var findBtn = CreateButton(canvasGo.transform, "FIND MATCH", Color.cyan,
                new Vector2(0, -40), new Vector2(250, 60));

            // Cancel button
            var cancelBtn = CreateButton(canvasGo.transform, "CANCEL", Color.red,
                new Vector2(0, -40), new Vector2(250, 60));

            // Wire up
            var lobbyUI = gameObject.AddComponent<LobbyUI>();
            SetField(lobbyUI, "findMatchButton", findBtn);
            SetField(lobbyUI, "cancelButton", cancelBtn);
            SetField(lobbyUI, "statusText", statusText);
            SetField(lobbyUI, "playerInfoText", playerText);

            Camera.main.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        }

        Button CreateButton(Transform parent, string label, Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
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
            text.fontSize = 28;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;

            return go.AddComponent<Button>();
        }

        TMP_Text CreateText(Transform parent, string content, int fontSize, Color color, Vector2 pos)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(500, 40);
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
