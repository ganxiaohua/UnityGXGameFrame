using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public static class EditorStylesHelper
    {
        private static List<GUIStyle> labelStyles = new List<GUIStyle>();
        private static List<GUIStyle> buttonStyles = new List<GUIStyle>();

        public static GUIStyle GetLabelStyle(TextAnchor alignment,        bool         richText = true,
                bool                                    wordWrap = false, TextClipping clipping = TextClipping.Overflow)
        {
            foreach (var tmp in labelStyles)
            {
                if (tmp.alignment == alignment && tmp.richText == richText && tmp.wordWrap == wordWrap && tmp.clipping == clipping)
                    return tmp;
            }

            var style = new GUIStyle(GUI.skin.label);
            style.alignment = alignment;
            style.richText = richText;
            style.wordWrap = wordWrap;
            style.clipping = clipping;
            labelStyles.Add(style);
            return style;
        }

        public static GUIStyle GetButtonStyle(TextAnchor alignment, bool richText = true)
        {
            foreach (var tmp in buttonStyles)
            {
                if (tmp.alignment == alignment && tmp.richText == richText)
                    return tmp;
            }

            var style = new GUIStyle(GUI.skin.button);
            style.alignment = alignment;
            style.richText = richText;
            buttonStyles.Add(style);
            return style;
        }

        private static Font _fontMonospace = null;

        public static Font FontMonospace
        {
            get
            {
                if (_fontMonospace == null)
                {
                    _fontMonospace = Font.CreateDynamicFontFromOSFont("新宋体", 16);
                }

                return _fontMonospace;
            }
        }
    }
}