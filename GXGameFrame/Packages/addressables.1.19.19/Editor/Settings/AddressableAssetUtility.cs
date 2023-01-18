using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Utilities;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.VersionControl;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Settings
{
    using Object = UnityEngine.Object;

    internal static class AddressableAssetUtility
    {
        internal static bool IsInResources(string path)
        {
            return path.Replace('\\', '/').ToLower().Contains("/resources/");
        }

        internal static bool TryGetPathAndGUIDFromTarget(Object target, out string path, out string guid)
        {
            guid = string.Empty;
            path = string.Empty;
            if (target == null)
                return false;
            path = AssetDatabase.GetAssetOrScenePath(target);
            if (!IsPathValidForEntry(path))
                return false;
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out guid, out long id))
                return false;
            return true;
        }

        static HashSet<string> excludedExtensions = new HashSet<string>(new string[] { ".cs", ".js", ".boo", ".exe", ".dll", ".meta", ".preset", ".asmdef" });
        internal static bool IsPathValidForEntry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            path = path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
            if (!path.StartsWith("assets", StringComparison.OrdinalIgnoreCase) && !IsPathValidPackageAsset(path))
                return false;
            if (path == CommonStrings.UnityEditorResourcePath ||
                path == CommonStrings.UnityDefaultResourcePath ||
                path == CommonStrings.UnityBuiltInExtraPath)
                return false;
            if (path.EndsWith($"{Path.DirectorySeparatorChar}Editor") || path.Contains($"{Path.DirectorySeparatorChar}Editor{Path.DirectorySeparatorChar}") 
                || path.EndsWith("/Editor") || path.Contains("/Editor/"))
                return false;
            if (path == "Assets")
                return false;
            var settings = AddressableAssetSettingsDefaultObject.SettingsExists ? AddressableAssetSettingsDefaultObject.Settings : null;
            if (settings != null && path.StartsWith(settings.ConfigFolder) || path.StartsWith(AddressableAssetSettingsDefaultObject.kDefaultConfigFolder))
                return false;
            return !excludedExtensions.Contains(Path.GetExtension(path));
        }

        internal static bool IsPathValidPackageAsset(string path)
        {
            string[] splitPath = path.ToLower().Split(Path.DirectorySeparatorChar);

            if (splitPath.Length < 3)
                return false;
            if (splitPath[0] != "packages")
                return false;
            if (splitPath[2] == "package.json")
                return false;
            return true;
        }

        static HashSet<Type> validTypes = new HashSet<Type>();
        internal static Type MapEditorTypeToRuntimeType(Type t, bool allowFolders)
        {
            //type is valid and already seen (most common)
            if (validTypes.Contains(t))
                return t;

            //removes the need to check this outside of this call
            if (t == null)
                return t;

            //check for editor type, this will get hit once for each new type encountered
            if (!t.Assembly.IsDefined(typeof(AssemblyIsEditorAssembly), true) && !Build.BuildUtility.IsEditorAssembly(t.Assembly))
            {
                validTypes.Add(t);
                return t;
            }

            if (t == typeof(DefaultAsset))
                return typeof(DefaultAsset);

            //try to remap the editor type to a runtime type
            return MapEditorTypeToRuntimeTypeInternal(t);
        }

        static Type MapEditorTypeToRuntimeTypeInternal(Type t)
        {
            if (t == typeof(UnityEditor.Animations.AnimatorController))
                return typeof(RuntimeAnimatorController);
            if (t == typeof(UnityEditor.SceneAsset))
                return typeof(UnityEngine.ResourceManagement.ResourceProviders.SceneInstance);
            if (t.FullName == "UnityEditor.Audio.AudioMixerController")
                return typeof(UnityEngine.Audio.AudioMixer);
            if (t.FullName == "UnityEditor.Audio.AudioMixerGroupController")
                return typeof(UnityEngine.Audio.AudioMixerGroup);
            return null;
        }

        internal static void ConvertAssetBundlesToAddressables()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            var bundleList = AssetDatabase.GetAllAssetBundleNames();

            float fullCount = bundleList.Length;
            int currCount = 0;

            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            foreach (var bundle in bundleList)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Converting Legacy Asset Bundles", bundle, currCount / fullCount))
                    break;

                currCount++;
                var group = settings.CreateGroup(bundle, false, false, false, null);
                var schema = group.AddSchema<BundledAssetGroupSchema>();
                schema.Validate();
                schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
                group.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

                var assetList = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);

                foreach (var asset in assetList)
                {
                    var guid = AssetDatabase.AssetPathToGUID(asset);
                    settings.CreateOrMoveEntry(guid, group, false, false);
                    var imp = AssetImporter.GetAtPath(asset);
                    if (imp != null)
                        imp.SetAssetBundleNameAndVariant(string.Empty, string.Empty);
                }
            }

            if (fullCount > 0)
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            EditorUtility.ClearProgressBar();
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        /// <summary>
        /// Get all types that can be assigned to type T
        /// </summary>
        /// <typeparam name="T">The class type to use as the base class or interface for all found types.</typeparam>
        /// <returns>A list of types that are assignable to type T.  The results are cached.</returns>
        public static List<Type> GetTypes<T>()
        {
            return TypeManager<T>.Types;
        }

        /// <summary>
        /// Get all types that can be assigned to type rootType.
        /// </summary>
        /// <param name="rootType">The class type to use as the base class or interface for all found types.</param>
        /// <returns>A list of types that are assignable to type T.  The results are not cached.</returns>
        public static List<Type> GetTypes(Type rootType)
        {
            return TypeManager.GetManagerTypes(rootType);
        }

        class TypeManager
        {
            public static List<Type> GetManagerTypes(Type rootType)
            {
                var types = new List<Type>();
                try
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (a.IsDynamic)
                            continue;
                        foreach (var t in a.ExportedTypes)
                        {
                            if (t != rootType && rootType.IsAssignableFrom(t) && !t.IsAbstract)
                                types.Add(t);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return types;
            }
        }

        class TypeManager<T> : TypeManager
        {
            // ReSharper disable once StaticMemberInGenericType
            static List<Type> s_Types;
            public static List<Type> Types
            {
                get
                {
                    if (s_Types == null)
                        s_Types = GetManagerTypes(typeof(T));

                    return s_Types;
                }
            }
        }

        internal static bool SafeMoveResourcesToGroup(AddressableAssetSettings settings, AddressableAssetGroup targetGroup, List<string> paths, List<string> guids, bool showDialog = true)
        {
            if (targetGroup == null)
            {
                Debug.LogWarning("No valid group to move Resources to");
                return false;
            }

            if (paths == null || paths.Count == 0)
            {
                Debug.LogWarning("No valid Resources found to move");
                return false;
            }

            if (guids == null)
            {
                guids = new List<string>();
                foreach (var p in paths)
                    guids.Add(AssetDatabase.AssetPathToGUID(p));
            }

            Dictionary<string, string> guidToNewPath = new Dictionary<string, string>();

            var message = "Any assets in Resources that you wish to mark as Addressable must be moved within the project. We will move the files to:\n\n";
            for (int i = 0; i < guids.Count; i++)
            {
                var newName = paths[i].Replace("\\", "/");
                newName = newName.Replace("Resources", "Resources_moved");
                newName = newName.Replace("resources", "resources_moved");
                if (newName == paths[i])
                    continue;

                guidToNewPath.Add(guids[i], newName);
                message += newName + "\n";
            }
            message += "\nAre you sure you want to proceed?";
            if (!showDialog || EditorUtility.DisplayDialog("Move From Resources", message, "Yes", "No"))
            {
                settings.MoveAssetsFromResources(guidToNewPath, targetGroup);
                return true;
            }
            return false;
        }

        static Dictionary<Type, string> s_CachedDisplayNames = new Dictionary<Type, string>();
        internal static string GetCachedTypeDisplayName(Type type)
        {
            string result = "<none>";
            if (type != null)
            {
                if (!s_CachedDisplayNames.TryGetValue(type, out result))
                {
                    var displayNameAtr = type.GetCustomAttribute<DisplayNameAttribute>();
                    if (displayNameAtr != null)
                    {
                        result = (string)displayNameAtr.DisplayName;
                    }
                    else
                        result = type.Name;

                    s_CachedDisplayNames.Add(type, result);
                }
            }

            return result;
        }
        
        struct PackageData
        {
            public string version;
        }
        private static string m_Version = null;
        internal static string GetVersionFromPackageData()
        {
            if (string.IsNullOrEmpty(m_Version))
            {
                var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.unity.addressables/package.json");
                var packageData = JsonUtility.FromJson<PackageData>(jsonFile.text);
                var split = packageData.version.Split('.');
                if (split.Length < 2)
                    throw new Exception("Could not get correct version data for Addressables package");
                m_Version = $"{split[0]}.{split[1]}";
            }
            return m_Version;
        }

        public static string GenerateDocsURL(string page)
        {
            return $"https://docs.unity3d.com/Packages/com.unity.addressables@{GetVersionFromPackageData()}/manual/{page}";
        }

        internal static bool IsUsingVCIntegration()
        {
            return Provider.isActive && Provider.enabled;
        }

        internal static bool IsVCAssetOpenForEdit(string path)
        {
            AssetList VCAssets = GetVCAssets(path);
            foreach (Asset vcAsset in VCAssets)
            {
                if (vcAsset.path == path)
                    return Provider.IsOpenForEdit(vcAsset);
            }
            return false;
        }

        internal static AssetList GetVCAssets(string path)
        {
            VersionControl.Task op = Provider.Status(path);
            op.Wait();
            return op.assetList;
        }

        private static bool MakeAssetEditable(Asset asset)
        {
            if (!AssetDatabase.IsOpenForEdit(asset.path))
                return AssetDatabase.MakeEditable(asset.path);
            return false;
        }

        internal static bool OpenAssetIfUsingVCIntegration(Object target, bool exitGUI = false)
        {
            if (!IsUsingVCIntegration() || target == null)
                return false;
            return OpenAssetIfUsingVCIntegration(AssetDatabase.GetAssetOrScenePath(target), exitGUI);
        }

        internal static bool OpenAssetIfUsingVCIntegration(string path, bool exitGUI = false)
        {
            if (!IsUsingVCIntegration() || string.IsNullOrEmpty(path))
                return false;

            AssetList assets = GetVCAssets(path);
            var uneditableAssets = new List<Asset>();
            string message = "Check out file(s)?\n\n";
            foreach (Asset asset in assets)
            {
                if (!Provider.IsOpenForEdit(asset))
                {
                    uneditableAssets.Add(asset);
                    message += $"{asset.path}\n";
                }
            }

            if (uneditableAssets.Count == 0)
                return false;

            bool openedAsset = true;
            if (EditorUtility.DisplayDialog("Attempting to modify files that are uneditable", message, "Yes", "No"))
            {
                foreach (Asset asset in uneditableAssets)
                {
                    if (!MakeAssetEditable(asset))
                        openedAsset = false;
                }
            }
            else
                openedAsset = false;

            if (exitGUI)
                GUIUtility.ExitGUI();
            return openedAsset;
        }

        internal static bool InstallCCDPackage()
        {
#if !ENABLE_CCD
            var confirm = EditorUtility.DisplayDialog("Install CCD Management SDK Package", "Are you sure you want to install the CCD Management SDK pre-release package and enable experimental CCD features within Addressables?\nTo remove this package and its related features, please use the Package manager.  Alternatively, uncheck the Addressable Asset Settings > Cloud Content Delivery > Enable Experimental CCD Features toggle to remove the package.", "Yes", "No");
            if (confirm)
            {
                Client.Add("com.unity.services.ccd.management@1.0.0-pre.2");
                AddressableAssetSettingsDefaultObject.Settings.CCDEnabled = true;
            }
#endif
            return AddressableAssetSettingsDefaultObject.Settings.CCDEnabled;
        }

        internal static bool RemoveCCDPackage()
        {
            var confirm = EditorUtility.DisplayDialog("Remove CCD Management SDK Package", "Are you sure you want to remove the CCD Management SDK package?", "Yes", "No");
            if (confirm)
            {
#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
                Client.Remove("com.unity.services.ccd.management");
                AddressableAssetSettingsDefaultObject.Settings.CCDEnabled = false;
#endif
            }
            return AddressableAssetSettingsDefaultObject.Settings.CCDEnabled;
        }

        internal static string GetMd5Hash(string path)
        {
            string hashString;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    var hash = md5.ComputeHash(stream);
                    hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                }
            }
            return hashString;
        }



        internal static System.Threading.Tasks.Task ParallelForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, System.Threading.Tasks.Task> body)
        {
            async System.Threading.Tasks.Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    { await body(partition.Current); }
                }
            }
            return System.Threading.Tasks.Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(dop)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
        }

        internal class SortedDelegate<T1, T2, T3, T4>
        {
            struct BufferedValues
            {
                public T1 arg1;
                public T2 arg2;
                public T3 arg3;
                public T4 arg4;
            }

            List<BufferedValues> m_Buffer;
            bool m_IsInvoking;
            private List<(int, Delegate)> m_RegisterQueue = new List<(int, Delegate)>();

            public delegate void Delegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
            private SortedList<int, Delegate> m_SortedInvocationList = new SortedList<int, Delegate>();

            public void Unregister(Delegate toUnregister)
            {
                IList<int> keys = m_SortedInvocationList.Keys;
                for (int i = 0; i < keys.Count; ++i)
                {
                    m_SortedInvocationList[keys[i]] -= toUnregister;
                    if (m_SortedInvocationList[keys[i]] == null)
                    {
                        m_SortedInvocationList.Remove(keys[i]);
                        break;
                    }
                }
                
                if (m_IsInvoking)
                {
                    for (int i = m_RegisterQueue.Count - 1; i >= 0; --i)
                    {
                        if (m_RegisterQueue[i].Item2 == toUnregister)
                        {
                            m_RegisterQueue.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            
            public void Register(Delegate toRegister, int order)
            {
                if (m_IsInvoking)
                {
                    m_RegisterQueue.Add((order, toRegister));
                    return;
                }
                
                Unregister(toRegister);
                if (m_SortedInvocationList.ContainsKey(order))
                    m_SortedInvocationList[order] += toRegister;
                else
                    m_SortedInvocationList.Add(order, toRegister);
                InvokeBuffer_Internal();
            }

            public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                if (m_IsInvoking)
                    return;
                
                m_IsInvoking = true;
                foreach (var invocationList in m_SortedInvocationList)
                    invocationList.Value?.Invoke(arg1,arg2,arg3,arg4);

                if (m_RegisterQueue.Count > 0)
                {
                    m_IsInvoking = false;
                    foreach (var toRegister in m_RegisterQueue)
                        Register(toRegister.Item2, toRegister.Item1);
                    m_RegisterQueue.Clear();
                    m_IsInvoking = true;
                }
                
                InvokeBuffer_Internal();
                m_IsInvoking = false;
            }

            void InvokeBuffer_Internal()
            {
                if (m_Buffer != null)
                {
                    foreach (var b in m_Buffer)
                        Invoke(b.arg1, b.arg2, b.arg3, b.arg4);
                    m_Buffer = null;
                }
            }
            
            public void BufferInvoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                if (m_SortedInvocationList.Count == 0 || m_IsInvoking)
                {
                    if (m_Buffer == null)
                        m_Buffer = new List<BufferedValues>();
                    m_Buffer.Add(new BufferedValues { arg1 = arg1, arg2 = arg2, arg3 = arg3, arg4 = arg4 });
                }
                else
                {
                    Invoke(arg1, arg2, arg3, arg4);
                }
            }

            public static SortedDelegate<T1, T2, T3, T4> operator +(SortedDelegate<T1, T2, T3, T4> self, Delegate delegateToAdd)
            {
                int lastInOrder = self.m_SortedInvocationList.Keys[self.m_SortedInvocationList.Count - 1];
                self.Register(delegateToAdd, lastInOrder + 1);
                return self;
            }
            
            public static SortedDelegate<T1, T2, T3, T4> operator -(SortedDelegate<T1, T2, T3, T4> self, Delegate delegateToRemove)
            {
                self.Unregister(delegateToRemove);
                return self;
            }
            
            public static bool operator ==(SortedDelegate<T1, T2, T3, T4> obj1, SortedDelegate<T1, T2, T3, T4> obj2)
            {
                bool aNull = ReferenceEquals(obj1, null);
                bool bNull = ReferenceEquals(obj2, null);
                
                if (aNull && bNull)
                    return true;
                if (!aNull && bNull)
                    return obj1.m_SortedInvocationList.Count == 0;
                if (aNull && !bNull)
                    return obj2.m_SortedInvocationList.Count == 0;
                if (ReferenceEquals(obj1, obj2))
                    return true;
                return obj1.Equals(obj2);
            }
            
            public static bool operator !=(SortedDelegate<T1, T2, T3, T4> lhs, SortedDelegate<T1, T2, T3, T4> rhs)
            {
                return !(lhs == rhs);
            }
            
            protected bool Equals(SortedDelegate<T1, T2, T3, T4> other)
            {
                return Equals(m_SortedInvocationList, other.m_SortedInvocationList);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((SortedDelegate<T1, T2, T3, T4>) obj);
            }

            public override int GetHashCode()
            {
                return (m_SortedInvocationList != null ? m_SortedInvocationList.GetHashCode() : 0);
            }
        }
    }
}
