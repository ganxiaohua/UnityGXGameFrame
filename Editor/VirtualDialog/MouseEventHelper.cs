using UnityEngine;

namespace GameFrame.Editor
{
    public static class MouseEventHelper
    {
        private static string currentControlName = string.Empty;

        public static bool TryGetMouseDrag(string controlName, Rect rect, out Vector2 mousePosition, out Vector2 delta, int button = 0)
        {
            mousePosition = Vector2.zero;
            delta = Vector2.zero;
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (GUI.enabled && rect.Contains(Event.current.mousePosition) && Event.current.button == button)
                    {
                        currentControlName = controlName;
                        mousePosition = Event.current.mousePosition;
                        delta = Vector2.zero;
                        Event.current.Use();
                        return true;
                    }

                    break;
                case EventType.MouseUp:
                    currentControlName = string.Empty;
                    break;
                case EventType.MouseDrag:
                    if (GUI.enabled && currentControlName == controlName && Event.current.button == button)
                    {
                        mousePosition = Event.current.mousePosition;
                        delta = Event.current.delta;
                        Event.current.Use();
                        return true;
                    }

                    break;
            }

            return false;
        }

        public static bool TryGetMouseScroll(string controlName, Rect rect, out Vector2 mousePosition, out float delta)
        {
            mousePosition = Vector2.zero;
            delta = 0;
            switch (Event.current.type)
            {
                case EventType.ScrollWheel:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        mousePosition = Event.current.mousePosition;
                        delta = Event.current.delta.y;
                        Event.current.Use();
                        return true;
                    }

                    break;
            }

            return false;
        }
    }
}