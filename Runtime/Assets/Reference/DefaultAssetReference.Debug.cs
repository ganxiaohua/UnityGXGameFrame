#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;

namespace GameFrame
{
    public sealed partial class DefaultAssetReference
    {
        private struct DebugInfo
        {
            public DefaultAssetReference AssetReference { get; set; }


            [ShowInInspector]
            private List<string> DirectReference
            {
                get
                {
                    var drs = GetRef(AssetReference);
                    var list = new List<string>();
                    if (drs != null)
                    {
                        foreach (var path in drs)
                        {
                            list.Add(FormatAssetPath(path));
                        }
                    }

                    return list;
                }
            }

            [ShowInInspector, GUIColor(1f, 0.8f, 0.2f), PropertySpace(SpaceAfter = 10)]
            private List<string> IndirectReference
            {
                get
                {
                    var drs = GetRef(AssetReference);
                    var list = new List<string>();
                    if (drs != null)
                    {
                        foreach (var path in drs)
                        {
                            if (AssetManager.Instance.TryGetAssetHandle(path, out var handle) &&
                                handle.AssetReference != null)
                            {
                                var drs2 = GetRef(handle.AssetReference as DefaultAssetReference);
                                if (drs2 == null) continue;
                                foreach (var path2 in drs2)
                                {
                                    list.Add($"{path}  -  {FormatAssetPath(path2)}");
                                }
                            }
                        }
                    }

                    return list;
                }
            }

            private List<string> GetRef(DefaultAssetReference reference)
            {
                if (reference == null) return new List<string>();
                return (List<string>) reference.GetType()
                        .GetField("refAssets", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField)
                        ?.GetValue(reference);
            }

            private string FormatAssetPath(string path)
            {
                if (AssetManager.Instance.TryGetAssetStatus(path, out var referenceCount, out var lifeTime))
                {
                    return $"{path}<{referenceCount}, {lifeTime}>";
                }
                else
                {
                    return $"{path}<Not Found>";
                }
            }
        }

        [ShowInInspector, HideLabel, HideInEditorMode]
        private DebugInfo _DebugInfo
        {
            get => new DebugInfo {AssetReference = this};
            set => _ = value;
        }
    }
}
#endif