using FairyGUI;
using UnityEngine;

namespace UnityGXGameFrame.Runtime
{
    public static class EasyFGUI
    {
        //由于FGUI Y向下是增加,所以左上角是pos
        public static Rect Rect2Ins(this GObject gObject)
        {
            var offset = new Vector2(-gObject.pivotX * gObject.width, gObject.pivotY * gObject.height);
            return gObject.TransformRect(new Rect(offset.x, -offset.y, gObject.width, gObject.height), null);
        }
    }
}