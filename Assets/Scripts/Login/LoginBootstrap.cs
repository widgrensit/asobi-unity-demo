using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AsobiDemo
{
    /// <summary>
    /// Drop on an empty GameObject in the Login scene. Creates the full login UI at runtime.
    /// </summary>
    public class LoginBootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Panel
            var panel = CreatePanel(canvasGo.transform);

            // Title
            var title = CreateText(panel.transform, "ASOBI ARENA", 42, NavalTheme.Primary,
                new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, 120), new Vector2(0, 50));

            // Username
            var usernameField = CreateInputField(panel.transform, "Username",
                new Vector2(0, 30));

            // Password
            var passwordField = CreateInputField(panel.transform, "Password",
                new Vector2(0, -30));
            passwordField.contentType = TMP_InputField.ContentType.Password;

            // Login Button
            var loginBtn = CreateButton(panel.transform, "LOGIN", NavalTheme.Primary,
                new Vector2(-80, -100));

            // Register Button
            var registerBtn = CreateButton(panel.transform, "REGISTER", NavalTheme.Tertiary,
                new Vector2(80, -100));

            // Status
            var statusText = CreateText(panel.transform, "", 18, NavalTheme.Secondary,
                new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, -160), new Vector2(0, 30));

            // Wire up LoginUI
            var loginUI = gameObject.AddComponent<LoginUI>();
            SetField(loginUI, "usernameField", usernameField);
            SetField(loginUI, "passwordField", passwordField);
            SetField(loginUI, "loginButton", loginBtn);
            SetField(loginUI, "registerButton", registerBtn);
            SetField(loginUI, "statusText", statusText);

            // Background camera
            Camera.main.backgroundColor = NavalTheme.Background;
        }

        GameObject CreatePanel(Transform parent)
        {
            var go = new GameObject("Panel");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 350);
            rect.anchoredPosition = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = NavalTheme.PanelBg;
            return go;
        }

        TMP_InputField CreateInputField(Transform parent, string placeholder, Vector2 pos)
        {
            var go = new GameObject(placeholder + "Field");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300, 40);
            rect.anchoredPosition = pos;

            var img = go.AddComponent<Image>();
            img.color = NavalTheme.InputBg;

            // Text area
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(go.transform);
            var taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(10, 0);
            taRect.offsetMax = new Vector2(-10, 0);

            // Input text
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(textArea.transform);
            var tRect = textGo.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.fontSize = 20;
            text.color = Color.white;

            // Placeholder
            var phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(textArea.transform);
            var phRect = phGo.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            var ph = phGo.AddComponent<TextMeshProUGUI>();
            ph.text = placeholder;
            ph.fontSize = 20;
            ph.color = new Color(0.5f, 0.5f, 0.5f);
            ph.fontStyle = FontStyles.Italic;

            var input = go.AddComponent<TMP_InputField>();
            input.textViewport = taRect;
            input.textComponent = text;
            input.placeholder = ph;

            return input;
        }

        Button CreateButton(Transform parent, string label, Color color, Vector2 pos)
        {
            var go = new GameObject(label + "Button");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(130, 40);
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
            text.fontSize = 20;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;

            return go.AddComponent<Button>();
        }

        TMP_Text CreateText(Transform parent, string content, int fontSize, Color color,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size == Vector2.zero ? new Vector2(400, 40) : new Vector2(400, size.y);
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
