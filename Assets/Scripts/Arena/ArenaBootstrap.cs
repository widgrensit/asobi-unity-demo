using UnityEngine;
using TMPro;

namespace AsobiDemo
{
    public class ArenaBootstrap : MonoBehaviour
    {
        void Awake()
        {
            var playerPrefab = PrefabFactory.CreateShipPrefab(false);
            playerPrefab.SetActive(false);

            var localPlayerPrefab = PrefabFactory.CreateShipPrefab(true);
            localPlayerPrefab.SetActive(false);

            var projectilePrefab = PrefabFactory.CreateProjectilePrefab();
            projectilePrefab.SetActive(false);

            var bounds = new GameObject("ArenaBounds");
            bounds.AddComponent<ArenaBounds>();

            Instantiate(PrefabFactory.CreateCrosshairPrefab());

            // HUD Canvas
            var canvas = CreateHUDCanvas();

            // Left side: timer
            var timerText = CreateHUDText(canvas.transform, "Timer",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(10, -10), "1:30", NavalTheme.Secondary, 28);

            // Right side: kills + HP
            var killsText = CreateHUDText(canvas.transform, "Kills",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-130, -10), "Kills: 0", NavalTheme.Primary, 22);
            var hpText = CreateHUDText(canvas.transform, "HP",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-130, -35), "HP: 100", NavalTheme.Tertiary, 22);

            // Top-right: round + modifier
            var roundText = CreateHUDText(canvas.transform, "Round",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-130, -65), "", NavalTheme.Secondary, 20);
            var modifierText = CreateHUDText(canvas.transform, "Modifier",
                new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-130, -88), "", NavalTheme.Primary, 18);

            // Bottom center: active boons
            var boonsText = CreateHUDText(canvas.transform, "Boons",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 10), "", NavalTheme.Tertiary, 18);

            // ArenaManager
            var manager = gameObject.AddComponent<ArenaManager>();
            SetPrivateField(manager, "playerPrefab", playerPrefab);
            SetPrivateField(manager, "localPlayerPrefab", localPlayerPrefab);
            SetPrivateField(manager, "projectilePrefab", projectilePrefab);
            SetPrivateField(manager, "timerText", timerText);
            SetPrivateField(manager, "killsText", killsText);
            SetPrivateField(manager, "hpText", hpText);
            SetPrivateField(manager, "roundText", roundText);
            SetPrivateField(manager, "modifierText", modifierText);
            SetPrivateField(manager, "boonsText", boonsText);
        }

        GameObject CreateHUDCanvas()
        {
            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            return canvasGo;
        }

        TMP_Text CreateHUDText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offset, string defaultText, Color color, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(250, 30);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = defaultText;
            text.fontSize = fontSize;
            text.color = color;
            return text;
        }

        static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}
