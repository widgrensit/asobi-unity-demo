using UnityEngine;
using TMPro;

namespace AsobiDemo
{
    public static class PrefabFactory
    {
        const int SheetCols = 3;
        const int SheetRows = 4;
        const int FrameW = 52;
        const int FrameH = 53;

        static Sprite[] SliceSpriteSheet(Texture2D tex)
        {
            var sprites = new Sprite[SheetRows * SheetCols];
            for (int row = 0; row < SheetRows; row++)
            {
                for (int col = 0; col < SheetCols; col++)
                {
                    // Texture Y is bottom-up; row 0 (down) is top of image
                    int y = tex.height - (row + 1) * FrameH;
                    var rect = new Rect(col * FrameW, y, FrameW, FrameH);
                    sprites[row * SheetCols + col] = Sprite.Create(
                        tex, rect, new Vector2(0.5f, 0.5f), FrameW);
                }
            }
            return sprites;
        }

        static Texture2D LoadShipTexture(string name)
        {
            var tex = Resources.Load<Texture2D>(name);
            if (tex != null) return tex;

            // Fallback: load from Sprites folder via path
            var sprite = Resources.Load<Sprite>(name);
            if (sprite != null) return sprite.texture;

            return null;
        }

        public static GameObject CreateShipPrefab(bool isLocal)
        {
            var texName = isLocal ? "ship_player" : "ship_enemy";
            var tex = LoadShipTexture(texName);

            var go = new GameObject("Ship");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            go.transform.localScale = Vector3.one * 1.5f;

            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                var frames = SliceSpriteSheet(tex);
                sr.sprite = frames[1]; // default: down, center frame

                var anim = go.AddComponent<ShipAnimator>();
                anim.frames = frames;
            }
            else
            {
                // Fallback to colored circle
                sr.sprite = CreateCircleSprite();
                sr.color = isLocal ? NavalTheme.Secondary : NavalTheme.Error;
                go.transform.localScale = Vector3.one * 0.6f;
            }

            // Name label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            labelGo.transform.localPosition = new Vector3(0, 0.8f, 0);
            var label = labelGo.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 3;
            label.color = isLocal ? NavalTheme.Secondary : NavalTheme.Primary;
            label.sortingOrder = 10;

            // HP bar background
            var hpBg = new GameObject("HPBarBg");
            hpBg.transform.SetParent(go.transform);
            hpBg.transform.localPosition = new Vector3(0, -0.7f, 0);
            hpBg.transform.localScale = new Vector3(1.2f, 0.12f, 1);
            var bgSr = hpBg.AddComponent<SpriteRenderer>();
            bgSr.sprite = CreateSquareSprite();
            bgSr.color = new Color(0, 0, 0, 0.7f);
            bgSr.sortingOrder = 5;

            // HP bar fill
            var hpBar = new GameObject("HPBar");
            hpBar.transform.SetParent(go.transform);
            hpBar.transform.localPosition = new Vector3(0, -0.7f, 0);
            hpBar.transform.localScale = new Vector3(1.2f, 0.12f, 1);
            var hpSr = hpBar.AddComponent<SpriteRenderer>();
            hpSr.sprite = CreateSquareSprite();
            hpSr.color = NavalTheme.Tertiary;
            hpSr.sortingOrder = 6;

            return go;
        }

        public static GameObject CreatePlayerPrefab(Color color)
        {
            return CreateShipPrefab(color == Color.cyan);
        }

        public static GameObject CreateProjectilePrefab()
        {
            var go = new GameObject("Projectile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = NavalTheme.Primary;
            sr.sortingOrder = 3;
            go.transform.localScale = Vector3.one * 0.15f;
            return go;
        }

        public static GameObject CreateCrosshairPrefab()
        {
            var go = new GameObject("Crosshair");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCrosshairSprite();
            sr.color = NavalTheme.Secondary;
            sr.sortingOrder = 100;
            go.transform.localScale = Vector3.one * 1f;
            go.AddComponent<Crosshair>();
            return go;
        }

        public static Sprite CreateCircleSprite()
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

        public static Sprite CreateSquareSprite()
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
