#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace GameFrame
{
    public sealed partial class AssetManager
    {
        private struct DebugInfo
        {
            public AssetManager Instance { get; set; }

            [ShowInInspector, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)]
            private List<string> AssetCaches
            {
                get
                {
                    var list = new List<string>();
                    foreach (var pair in Instance.assetCaches)
                    {
                        var path = pair.Key;
                        var cache = pair.Value;
                        list.Add($"{path}<{cache.referenceCount}, {cache.lifeTime}>");
                    }

                    list.Sort();
                    return list;
                }
                set => _ = value;
            }


            private static string tempKey = " ";

            [ShowInInspector, HorizontalGroup("Test"), HideLabel]
            private string TempKey
            {
                get => tempKey;
                set => tempKey = value;
            }

            [ShowInInspector, HorizontalGroup("Test", Width = 40)]
            private void Ref()
            {
                if (!string.IsNullOrEmpty(tempKey))
                    Instance.IncrementReferenceCount(tempKey);
            }

            [ShowInInspector, HorizontalGroup("Test", Width = 40)]
            private void Unref()
            {
                if (!string.IsNullOrEmpty(tempKey))
                    Instance.DecrementReferenceCount(tempKey, true);
            }
        }

        [ShowInInspector, HideLabel, HideInEditorMode]
        private DebugInfo _DebugInfo
        {
            get => new DebugInfo {Instance = this};
            set => _ = value;
        }
    }
}
#endif