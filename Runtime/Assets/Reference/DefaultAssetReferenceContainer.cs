using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameFrame
{
    public sealed class DefaultAssetReferenceContainer : MonoBehaviour
    {
        [SerializeField, HideInInspector] private DefaultAssetReference assetReference;
        
        public DefaultAssetReference AssetReference
        {
            get
            {
                if (!initialized)
                    return null;
                return assetReference;
            }
        }

        private bool initialized;

        private void Awake()
        {
            if (assetReference != null)
            {
                if (!assetReference.RefInitialize())
                {
                    Debug.LogError($"AssetReference initialize failed, caused by inactive gameobject clone generally", this);
                    return;
                }
            }

            initialized = true;
        }

        private void OnDestroy()
        {
            if (assetReference != null)
            {
                if (initialized)
                    assetReference.Dispose();
                assetReference = null;
            }
        }

        public static void Bind(GameObject target, DefaultAssetReference reference)
        {
            var go = new GameObject("Reference");
            var container = go.AddComponent<DefaultAssetReferenceContainer>();
            container.assetReference = reference;
            go.transform.SetParent(target.transform);
        }

#if UNITY_EDITOR
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

        [ShowInInspector]
        private List<string> DirectReference
        {
            get
            {
                var drs = GetRef(AssetReference);
                if (drs == null) return null;
                var list = new List<string>();
                foreach (var path in drs)
                {
                    list.Add(FormatAssetPath(path));
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
                if (drs == null) return null;
                var list = new List<string>();
                foreach (var path in drs)
                {
                    if (AssetManager.Instance.TryGetAssetHandle(path, out var handle) && handle.AssetReference != null)
                    {
                        var drs2 = GetRef(handle.AssetReference as DefaultAssetReference);
                        if (drs2 == null) continue;
                        foreach (var path2 in drs2)
                        {
                            list.Add($"{path}  -  {FormatAssetPath(path2)}");
                        }
                    }
                }

                return list;
            }
        }
#endif
    }
}