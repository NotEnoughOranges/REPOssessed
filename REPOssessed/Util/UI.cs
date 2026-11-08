using REPOssessed.Cheats.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace REPOssessed.Util
{
    public class UIButton
    {
        public string label;
        public Action action;

        public UIButton(string label, Action action)
        {
            this.label = TranslationUtil.Translate(label);
            this.action = action;
        }

        public void Draw()
        {
            if (GUILayout.Button(label)) action.Invoke();
        }
    }

    public class UIOption
    {
        public string label;
        public Action? action;

        public UIOption(string label, Action action)
        {
            this.label = TranslationUtil.Translate(label);
            this.action = action;
        }

        public void Draw()
        {
            if (GUILayout.Button(label)) action?.Invoke();
        }
    }

    public class UI
    {
        // Grey out host features if not host
        // Ignore if Single Player
        public static void HostOnly(Action draw, bool allowWhenNotHost = false)
        {
            bool wasEnabled = GUI.enabled;
            bool hasHostAuthority = !SemiFunc.IsMultiplayer() || GameUtil.IsMasterClient();

            try
            {
                GUI.enabled = wasEnabled && (hasHostAuthority || allowWhenNotHost);
                draw();
            }
            finally
            {
                GUI.enabled = wasEnabled;
            }
        }

        public static void Label(string header, string? label = null, bool bold = false, int fontSize = -1, bool middleAlignment = false, RGBAColor? rGBAColor = null)
        {
            HorizontalGroup(() =>
            {
                GUILayout.Label(rGBAColor != null ? rGBAColor.AsString(TranslationUtil.Translate(header)) : TranslationUtil.Translate(header), new GUIStyle(GUI.skin.label)
                {
                    alignment = middleAlignment ? TextAnchor.MiddleCenter : TextAnchor.UpperLeft,
                    fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                    fontSize = fontSize > 0 ? fontSize : Settings.i_menuFontSize,
                });
                if (!string.IsNullOrEmpty(label))
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(rGBAColor != null ? rGBAColor.AsString(TranslationUtil.Translate(header)) : TranslationUtil.Translate(label));
                }
            });
        }

        public static void Label(string[] header, string? label = null, bool bold = false, int fontSize = -1, bool middleAlignment = false)
        {
            Label(TranslationUtil.Translate(header), label, bold, fontSize, middleAlignment);
        }

        public static void Checkbox(string header, ref bool value)
        {
            bool tempValue = value;
            HorizontalGroup(() =>
            {
                Label(header);
                GUILayout.FlexibleSpace();
                tempValue = GUILayout.Toggle(tempValue, "");
            });
            value = tempValue;
        }

        public static void Checkbox(string[] header, ref bool value)
        {
            Checkbox(TranslationUtil.Translate(header), ref value);
        }

        public static void Checkbox(string header, ToggleCheat toggleCheat)
        {
            bool tempValue = toggleCheat.Enabled;
            Checkbox(header, ref tempValue);
            toggleCheat.Enabled = tempValue;
        }

        public static void Checkbox(string[] header, ToggleCheat toggleCheat)
        {
            Checkbox(TranslationUtil.Translate(header), toggleCheat);
        }

        public static void Textbox(string label, ref string value, string regex, int length, params UIButton[] buttons)
        {
            string tempValue = value;
            HorizontalGroup(() =>
            {
                Label(TranslationUtil.Translate(label));
                GUILayout.FlexibleSpace();
                tempValue = GUILayout.TextField(tempValue, length, GUILayout.Width(Settings.i_textboxWidth));
                if (!string.IsNullOrEmpty(regex)) tempValue = Regex.Replace(tempValue, regex, "");
                buttons.ToList().ForEach(b => b?.Draw());
            });
            value = tempValue;
        }

        public static void Textbox(string[] label, ref string value, string regex, int length, params UIButton[] buttons)
        {
            Textbox(TranslationUtil.Translate(label), ref value, regex, length, buttons);
        }

        public static void VerticalGroup(ref Vector2 ScrollPosition, Action action, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(options);
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
            action.Invoke();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public static void HorizontalGroup(ref Vector2 ScrollPosition, Action action, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
            action.Invoke();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        public static void VerticalGroup(Action action, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(options);
            action.Invoke();
            GUILayout.EndVertical();
        }

        public static void HorizontalGroup(Action action, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(options);
            action.Invoke();
            GUILayout.EndHorizontal();
        }

        public static void Button(string header, Action action, string buttonText = "General.Execute", params GUILayoutOption[] options)
        {
            HorizontalGroup(() =>
            {
                string headerTranslated = TranslationUtil.Translate(header);
                string buttonTranslated = TranslationUtil.Translate(buttonText);
                if (!string.IsNullOrEmpty(buttonTranslated))
                {
                    Label(headerTranslated);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(buttonTranslated, options)) action();
                }
                else if (GUILayout.Button(headerTranslated, options)) action();
            });
        }

        public static void Button(string[] header, Action action, string buttonText = "General.Execute", params GUILayoutOption[] options)
        {
            Button(TranslationUtil.Translate(header), action, buttonText, options);
        }

        public static void Toggle(string header, ref bool value, string enabled = "General.Enable", string disabled = "General.Disable", params GUILayoutOption[] options)
        {
            bool tempValue = value;
            HorizontalGroup(() =>
            {
                Label(header);
                GUILayout.FlexibleSpace();
                if (!GUILayout.Button(TranslationUtil.Translate(tempValue ? disabled : enabled), options)) return;
                tempValue = !tempValue;
            });
            value = tempValue;
        }

        public static void Toggle(string[] header, ref bool value, string enabled = "General.Enable", string disabled = "General.Disable", params GUILayoutOption[] options)
        {
            Toggle(TranslationUtil.Translate(header), ref value, enabled, disabled, options);
        }

        public static void ToggleSlider(string header, string displayValue, ref bool enable, ref float value, float min, float max)
        {
            bool tempEnable = enable;
            float tempValue = value;
            HorizontalGroup(() =>
            {
                Label($"{TranslationUtil.Translate(header)} ( {displayValue} )");
                GUILayout.FlexibleSpace();
                tempValue = GUILayout.HorizontalSlider(tempValue, min, max, new GUIStyle(GUI.skin.horizontalSlider)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fixedWidth = Settings.i_sliderWidth
                }, GUI.skin.horizontalSliderThumb);
                Checkbox("", ref tempEnable);
            });
            enable = tempEnable;
            value = tempValue;
        }

        public static void ToggleSlider(string[] header, string displayValue, ref bool enable, ref float value, float min, float max)
        {
            ToggleSlider(TranslationUtil.Translate(header), displayValue, ref enable, ref value, min, max);
        }

        public static void ToggleSlider(ToggleCheat toggleCheat, string header, string displayValue, ref float value, float min, float max)
        {
            bool tempValue = toggleCheat.Enabled;
            ToggleSlider(header, displayValue, ref tempValue, ref value, min, max);
            toggleCheat.Enabled = tempValue;
        }

        public static void ToggleSlider(ToggleCheat toggleCheat, string[] header, string displayValue, ref float value, float min, float max)
        {
            ToggleSlider(toggleCheat, header, displayValue, ref value, min, max);
        }

        public static void Select(string header, ref int index, params UIOption[] options)
        {
            GUILayout.BeginHorizontal();
            Label(header);
            GUILayout.FlexibleSpace();
            options[index].Draw();
            if (GUILayout.Button(TranslationUtil.Translate("-"))) index = Mathf.Clamp(index - 1, 0, options.Length - 1);
            if (GUILayout.Button(TranslationUtil.Translate("+"))) index = Mathf.Clamp(index + 1, 0, options.Length - 1);
            GUILayout.EndHorizontal();
        }

        public static void ButtonGrid<T>(List<T> objects, Func<T, string> textSelector, string search, Action<T> action, int numPerRow, int width = 175, bool hostOnly = false)
        {
            List<T> filtered = objects.Where(x => textSelector(x).Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            for (int i = 0; i < filtered.Count; i += numPerRow)
            {
                HorizontalGroup(() =>
                {
                    foreach (T obj in filtered.Skip(i).Take(numPerRow))
                    {
                        if (hostOnly)
                        {
                            HostOnly(() =>
                            {
                                if (GUILayout.Button(textSelector(obj), GUILayout.Width(width))) action(obj);
                            });
                        }
                        else if (GUILayout.Button(textSelector(obj), GUILayout.Width(width))) action(obj);
                    }
                });
            }
        }
    }
}
