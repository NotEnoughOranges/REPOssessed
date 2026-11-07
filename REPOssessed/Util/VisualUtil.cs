using Newtonsoft.Json;
using System.Globalization;
using UnityEngine;

namespace REPOssessed.Util
{
    public class RGBAColor
    {
        public static RGBAColor Default { get; set; } = new RGBAColor(1f, 1f, 1f, 1f);

        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        [JsonConstructor]
        public RGBAColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public RGBAColor(int r, int g, int b, float a)
        {
            this.r = r / 255f;
            this.g = g / 255f;
            this.b = b / 255f;
            this.a = a;
        }

        public RGBAColor(string hexCode, float alpha = 1f)
        {
            try
            {
                if (hexCode.StartsWith("#")) hexCode = hexCode.Substring(1);

                int rgb = int.Parse(hexCode, NumberStyles.HexNumber);

                r = hexCode.Length == 8 ? ((rgb >> 24) & 0xFF) / 255f : ((rgb >> 16) & 0xFF) / 255f;
                g = hexCode.Length == 8 ? ((rgb >> 16) & 0xFF) / 255f : ((rgb >> 8) & 0xFF) / 255f;
                b = hexCode.Length == 8 ? ((rgb >> 8) & 0xFF) / 255f : (rgb & 0xFF) / 255f;
                a = hexCode.Length == 8 ? (rgb & 0xFF) / 255f : alpha;
            }
            catch
            {
                r = 1f;
                g = 1f;
                b = 1f;
                a = 1f;
            }
        }

        public Color GetColor()
        {
            return new Color(r, g, b, a);
        }

        public string GetHexCode()
        {
            if (a < 1f) return GetHexCodeAlpha();

            int red = Mathf.Clamp((int)(r * 255), 0, 255);
            int green = Mathf.Clamp((int)(g * 255), 0, 255);
            int blue = Mathf.Clamp((int)(b * 255), 0, 255);

            int rgb = (red << 16) | (green << 8) | blue;

            string hexCode = rgb.ToString("X6");

            return hexCode;
        }

        public string GetHexCodeAlpha()
        {
            int red = Mathf.Clamp((int)(r * 255), 0, 255);
            int green = Mathf.Clamp((int)(g * 255), 0, 255);
            int blue = Mathf.Clamp((int)(b * 255), 0, 255);
            int alpha = Mathf.Clamp((int)(a * 255), 0, 255);

            int rgb = (red << 24) | (green << 16) | (blue << 8) | alpha;

            string hexCode = rgb.ToString("X8");

            return hexCode;
        }

        public string AsString(string text)
        {
            return $"<color=#{GetHexCode()}>{TranslationUtil.Translate(text)}</color>";
        }

    }
    public class VisualUtil
    {
        public static void DrawString(Vector2 position, string label, Color color, bool centered = true, bool alignMiddle = false, bool bold = false, int fontSize = -1, float verticalPadding = 0f)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            GUIContent content = new GUIContent(label);
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize > 0 ? fontSize : Settings.i_menuFontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                alignment = alignMiddle ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft,
                clipping = verticalPadding > 0f ? TextClipping.Overflow : TextClipping.Clip
            };
            Vector2 size = style.CalcSize(content);
            if (verticalPadding > 0f) size.y = Mathf.Max(size.y, style.CalcHeight(content, size.x)) + verticalPadding;
            GUI.Label(new Rect(centered ? position - size / 2f : position, size), content, style);
            GUI.color = oldColor;
        }

        public static void DrawString(Vector2 position, string label, RGBAColor color, bool centered = true, bool alignMiddle = false, bool bold = false, int fontSize = -1, float verticalPadding = 0f)
        {
            DrawString(position, label, color.GetColor(), centered, alignMiddle, bold, fontSize, verticalPadding);
        }
    }
}
