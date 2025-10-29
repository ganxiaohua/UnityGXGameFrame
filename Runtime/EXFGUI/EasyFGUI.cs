using FairyGUI;
using UnityEngine;

namespace UnityGXGameFrame.Runtime
{
    public static class EasyFGUI
    {
        public static Rect Rect2Ins(this GObject gObject)
        {
            var offset = new Vector2(-gObject.pivotX * gObject.width, (1 - gObject.pivotY) * gObject.height);
            return gObject.TransformRect(new Rect(offset.x, offset.y, gObject.width, gObject.height), null);
        }
    }
}