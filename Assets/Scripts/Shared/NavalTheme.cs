using UnityEngine;

namespace AsobiDemo
{
    public static class NavalTheme
    {
        public static readonly Color Background = new(0.05f, 0.12f, 0.24f);
        public static readonly Color Primary = HexColor("c9beff");
        public static readonly Color Secondary = HexColor("91cdff");
        public static readonly Color Tertiary = HexColor("4ae183");
        public static readonly Color Error = HexColor("ffb4ab");
        public static readonly Color PanelBg = new(0.08f, 0.16f, 0.30f, 0.95f);
        public static readonly Color InputBg = new(0.06f, 0.14f, 0.28f);
        public static readonly Color TextDim = new(0.6f, 0.7f, 0.85f);

        static Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        public static Color HpColor(float fraction)
        {
            if (fraction > 0.6f) return Tertiary;
            if (fraction > 0.3f) return Color.Lerp(Error, Tertiary, (fraction - 0.3f) / 0.3f);
            return Error;
        }
    }
}
