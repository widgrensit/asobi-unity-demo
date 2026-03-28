using UnityEngine;
using TMPro;

namespace AsobiDemo
{
    public static class PrefabFactory
    {
        public static GameObject CreatePlayerPrefab(Color color)
        {
            var go = new GameObject("Player");

            // Body circle
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = color;
            go.transform.localScale = Vector3.one * 0.6f;

            // Name label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            labelGo.transform.localPosition = new Vector3(0, 0.8f, 0);
            var label = labelGo.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 3;
            label.sortingOrder = 10;

            // HP bar background
            var hpBg = new GameObject("HPBarBg");
            hpBg.transform.SetParent(go.transform);
            hpBg.transform.localPosition = new Vector3(0, -0.6f, 0);
            hpBg.transform.localScale = new Vector3(1.2f, 0.15f, 1);
            var bgSr = hpBg.AddComponent<SpriteRenderer>();
            bgSr.sprite = CreateSquareSprite();
            bgSr.color = Color.black;
            bgSr.sortingOrder = 5;

            // HP bar fill
            var hpBar = new GameObject("HPBar");
            hpBar.transform.SetParent(go.transform);
            hpBar.transform.localPosition = new Vector3(0, -0.6f, 0);
            hpBar.transform.localScale = new Vector3(1.2f, 0.15f, 1);
            var hpSr = hpBar.AddComponent<SpriteRenderer>();
            hpSr.sprite = CreateSquareSprite();
            hpSr.color = Color.green;
            hpSr.sortingOrder = 6;

            return go;
        }

        public static GameObject CreateProjectilePrefab()
        {
            var go = new GameObject("Projectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = Color.yellow;
            sr.sortingOrder = 3;
            go.transform.localScale = Vector3.one * 0.15f;
            return go;
        }

        public static GameObject CreateCrosshairPrefab()
        {
            var go = new GameObject("Crosshair");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCrosshairSprite();
            sr.color = Color.white;
            sr.sortingOrder = 100;
            go.transform.localScale = Vector3.one * 0.3f;
            go.AddComponent<Crosshair>();
            return go;
        }

        static Sprite CreateCircleSprite()
        {
            int size = 64;
            var tex = new Texture2D(size, size);
            var center = size / 2f;
            var radius = size / 2f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }

        static Sprite CreateSquareSprite()
        {
            int size = 4;
            var tex = new Texture2D(size, size);
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }

        static Sprite CreateCrosshairSprite()
        {
            int size = 32;
            var tex = new Texture2D(size, size);
            var center = size / 2;

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    tex.SetPixel(x, y, Color.clear);

            // Draw cross
            for (int i = 0; i < size; i++)
            {
                tex.SetPixel(center, i, Color.white);
                tex.SetPixel(i, center, Color.white);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }
    }
}
