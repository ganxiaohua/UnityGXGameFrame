using UnityEngine;

namespace GameFrame.Runtime
{
    public static class UnityClassExpand
    {
        public static void SetLayerRecursive(this Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0, count = root.childCount; i < count; i++)
            {
                root.GetChild(i).SetLayerRecursive(layer);
            }
        }
    }
}