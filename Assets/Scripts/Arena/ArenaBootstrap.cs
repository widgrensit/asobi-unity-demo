using UnityEngine;
using TMPro;

namespace AsobiDemo
{
    /// <summary>
    /// Drop this on an empty GameObject in the Arena scene.
    /// It creates all required objects at runtime so you don't need prefabs.
    /// </summary>
    public class ArenaBootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Create prefabs at runtime
            var playerPrefab = PrefabFactory.CreatePlayerPrefab(Color.red);
            playerPrefab.SetActive(false);

            var localPlayerPrefab = PrefabFactory.CreatePlayerPrefab(Color.cyan);
            localPlayerPrefab.SetActive(false);

            var projectilePrefab = PrefabFactory.CreateProjectilePrefab();
            projectilePrefab.SetActive(false);

            // Arena bounds
            var bounds = new GameObject("ArenaBounds");
            bounds.AddComponent<ArenaBounds>();

            // Crosshair
            Instantiate(PrefabFactory.CreateCrosshairPrefab());

            // HUD Canvas
            var canvas = CreateHUDCanvas();
            var timerText = CreateHUDText(canvas.transform, "Timer", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, -10), "1:30");
            var killsText = CreateHUDText(canvas.transform, "Kills", new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-120, -10), "Kills: 0");
            var hpText = CreateHUDText(canvas.transform, "HP", new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-120, -40), "HP: 100");

            // ArenaManager
            var manager = gameObject.AddComponent<ArenaManager>();
            SetPrivateField(manager, "playerPrefab", playerPrefab);
            SetPrivateField(manager, "localPlayerPrefab", localPlayerPrefab);
            SetPrivateField(manager, "projectilePrefab", projectilePrefab);
            SetPrivateField(manager, "timerText", timerText);
            SetPrivateField(manager, "killsText", killsText);
            SetPrivateField(manager, "hpText", hpText);
        }

        GameObject CreateHUDCanvas()
        {
            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            return canvasGo;
        }

        TMP_Text CreateHUDText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offset, string defaultText)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(200, 30);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = defaultText;
            text.fontSize = 24;
            text.color = Color.white;
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
