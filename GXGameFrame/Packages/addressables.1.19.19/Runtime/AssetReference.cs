using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace UnityEngine.AddressableAssets
{
    /// <summary>
    /// Generic version of AssetReference class.  This should not be used directly as CustomPropertyDrawers do not support generic types.  Instead use the concrete derived classes such as AssetReferenceGameObject.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    [Serializable]
    public class AssetReferenceT<TObject> : AssetReference where TObject : Object
    {
        /// <summary>
        /// Construct a new AssetReference object.
        /// </summary>
        /// <param name="guid">The guid of the asset.</param>
        public AssetReferenceT(string guid)
            : base(guid)
        {
#if UNITY_EDITOR
            m_DerivedClassType = typeof(TObject);
#endif
        }

        /// <summary>
        /// Load the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use <see cref="Addressables.LoadAssetAsync{TObject}(object)"/> and pass your AssetReference in as the key.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <returns>The load operation.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadAssetAsync(*)", true)]
        [Obsolete]
        public AsyncOperationHandle<TObject> LoadAsset()
        {
            return LoadAssetAsync();
        }

        /// <summary>
        /// Load the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use <see cref="Addressables.LoadAssetAsync{TObject}(object)"/> and pass your AssetReference in as the key.
        /// on an AssetReference, use Addressables.LoadAssetAsync&lt;&gt;() and pass your AssetReference in as the key.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <returns>The load operation.</returns>
        public virtual AsyncOperationHandle<TObject> LoadAssetAsync()
        {
            return LoadAssetAsync<TObject>();
        }

        /// <inheritdoc/>
        public override bool ValidateAsset(Object obj)
        {
            var type = obj.GetType();
            return typeof(TObject).IsAssignableFrom(type);
        }

        /// <summary>
        /// Validates that the asset located at a path is allowable for this asset reference. An asset is allowable if
        /// it is of the correct type or if one of its sub-asset is.
        /// </summary>
        /// <param name="mainAssetPath">The path to the asset in question.</param>
        /// <returns>Whether the referenced asset is valid.</returns>
        public override bool ValidateAsset(string mainAssetPath)
        {
#if UNITY_EDITOR
            if (typeof(TObject).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(mainAssetPath)))
                return true;
            
            var repr = AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetPath);
            return repr != null && repr.Any(o => o is TObject);
#else
            return false;
#endif
        }
        
#if UNITY_EDITOR
        internal TObject FetchAsset()
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(AssetGUID);
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TObject));
            return (TObject) asset;
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Type-specific override of parent editorAsset.  Used by the editor to represent the main asset referenced.
        /// </summary>
        /// <returns>Editor Asset as type TObject, else null</returns>
        public new TObject editorAsset
        {
            get
            {
                if (CachedAsset as TObject != null || string.IsNullOrEmpty(AssetGUID))
                    return CachedAsset as TObject;
                TObject asset = FetchAsset();
                if (asset == null)
                    Debug.LogWarning("Assigned editorAsset does not match type " + typeof(TObject) + ". EditorAsset will be null.");
                return asset;
            }
        }
#endif
    }

    /// <summary>
    /// GameObject only asset reference.
    /// </summary>
    [Serializable]
    public class AssetReferenceGameObject : AssetReferenceT<GameObject>
    {
        /// <summary>
        /// Constructs a new reference to a GameObject.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceGameObject(string guid) : base(guid) {}
    }
    /// <summary>
    /// Texture only asset reference.
    /// </summary>
    [Serializable]
    public class AssetReferenceTexture : AssetReferenceT<Texture>
    {
        /// <summary>
        /// Constructs a new reference to a Texture.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceTexture(string guid) : base(guid) {}
    }
    /// <summary>
    /// Texture2D only asset reference.
    /// </summary>
    [Serializable]
    public class AssetReferenceTexture2D : AssetReferenceT<Texture2D>
    {
        /// <summary>
        /// Constructs a new reference to a Texture2D.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceTexture2D(string guid) : base(guid) {}
    }
    /// <summary>
    /// Texture3D only asset reference
    /// </summary>
    [Serializable]
    public class AssetReferenceTexture3D : AssetReferenceT<Texture3D>
    {
        /// <summary>
        /// Constructs a new reference to a Texture3D.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceTexture3D(string guid) : base(guid) {}
    }

    /// <summary>
    /// Sprite only asset reference.
    /// </summary>
    [Serializable]
    public class AssetReferenceSprite : AssetReferenceT<Sprite>
    {
        /// <summary>
        /// Constructs a new reference to a AssetReferenceSprite.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceSprite(string guid) : base(guid) {}

        /// <inheritdoc/>
        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SpriteAtlas))
                return true;

            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            bool isTexture = typeof(Texture2D).IsAssignableFrom(type);
            if (isTexture)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                return (importer != null) && (importer.spriteImportMode != SpriteImportMode.None);
            }
#endif
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Typeless override of parent editorAsset. Used by the editor to represent the main asset referenced.
        /// </summary>
        public new Object editorAsset
        {
            get
            {
                if (CachedAsset != null || string.IsNullOrEmpty(AssetGUID))
                    return CachedAsset;

                var prop = typeof(AssetReference).GetProperty("editorAsset");
                return prop.GetValue(this, null) as Object;
            }
        }
#endif
    }

    /// <summary>
    /// Assetreference that only allows atlassed sprites.
    /// </summary>
    [Serializable]
    public class AssetReferenceAtlasedSprite : AssetReferenceT<Sprite>
    {
        /// <summary>
        /// Constructs a new reference to a AssetReferenceAtlasedSprite.
        /// </summary>
        /// <param name="guid">The object guid.</param>
        public AssetReferenceAtlasedSprite(string guid) : base(guid) {}

        /// <inheritdoc/>
        public override bool ValidateAsset(Object obj)
        {
            return obj is SpriteAtlas;
        }

        /// <inheritdoc/>
        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(SpriteAtlas);
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// SpriteAtlas Type-specific override of parent editorAsset. Used by the editor to represent the main asset referenced.
        /// </summary>
        public new SpriteAtlas editorAsset
        {
            get
            {
                if (CachedAsset != null || string.IsNullOrEmpty(AssetGUID))
                    return CachedAsset as SpriteAtlas;

                var assetPath = AssetDatabase.GUIDToAssetPath(AssetGUID);
                var main = AssetDatabase.LoadMainAssetAtPath(assetPath) as SpriteAtlas;
                if (main != null)
                    CachedAsset = main;
                return main;
            }
        }
#endif
    }

    /// <summary>
    /// Reference to an addressable asset.  This can be used in script to provide fields that can be easily set in the editor and loaded dynamically at runtime.
    /// To determine if the reference is set, use RuntimeKeyIsValid().
    /// </summary>
    [Serializable]
    public class AssetReference : IKeyEvaluator
    {
        [FormerlySerializedAs("m_assetGUID")]
        [SerializeField]
        string m_AssetGUID = "";
        [SerializeField]
        string m_SubObjectName;
        [SerializeField]
        string m_SubObjectType = null;

        AsyncOperationHandle m_Operation;
        /// <summary>
        /// The AsyncOperationHandle currently being used by the AssetReference.
        /// For example, if you call AssetReference.LoadAssetAsync, this property will return a handle to that operation.
        /// </summary>
        public AsyncOperationHandle OperationHandle
        {
            get
            {
                return m_Operation;
            }
            internal set
            {
                m_Operation = value;
#if UNITY_EDITOR
                if (m_Operation.Status != AsyncOperationStatus.Failed)
                    m_ActiveAssetReferences.Add(this);
#endif
            }
        }

        /// <summary>
        /// The actual key used to request the asset at runtime. RuntimeKeyIsValid() can be used to determine if this reference was set.
        /// </summary>
        public virtual object RuntimeKey
        {
            get
            {
                if (m_AssetGUID == null)
                    m_AssetGUID = string.Empty;
                if (!string.IsNullOrEmpty(m_SubObjectName))
                    return string.Format("{0}[{1}]", m_AssetGUID, m_SubObjectName);
                return m_AssetGUID;
            }
        }

        /// <summary>
        /// Stores the guid of the asset.
        /// </summary>
        public virtual string AssetGUID { get { return m_AssetGUID; } }

        /// <summary>
        /// Stores the name of the sub object.
        /// </summary>
        public virtual string SubObjectName { get { return m_SubObjectName; } set { m_SubObjectName = value; } }
        internal virtual Type SubOjbectType
        {
            get
            {
                if (!string.IsNullOrEmpty(m_SubObjectName) && m_SubObjectType != null)
                    return Type.GetType(m_SubObjectType);
                return null;
            }
        }
        /// <summary>
        /// Returns the state of the internal operation.
        /// </summary>
        /// <returns>True if the operation is valid.</returns>
        public bool IsValid()
        {
            return m_Operation.IsValid();
        }

        /// <summary>
        /// Get the loading status of the internal operation.
        /// </summary>
        public bool IsDone
        {
            get
            {
                return m_Operation.IsDone;
            }
        }
        
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void RegisterForPlaymodeChange()
        {
            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        static HashSet<AssetReference> m_ActiveAssetReferences = new HashSet<AssetReference>();

        static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange state)
        {
            if (EditorSettings.enterPlayModeOptionsEnabled && Addressables.reinitializeAddressables)
            {
                foreach (AssetReference reference in m_ActiveAssetReferences)
                {
                    reference.ReleaseHandleWhenPlaymodeStateChanged(state);
                }
            }
        }
        
        void ReleaseHandleWhenPlaymodeStateChanged(PlayModeStateChange state)
        {
            if (m_Operation.IsValid())
                m_Operation.Release();
        }
#endif

        /// <summary>
        /// Construct a new AssetReference object.
        /// </summary>
        public AssetReference()
        {
        }

#if UNITY_EDITOR
        ~AssetReference()
        {
            m_ActiveAssetReferences.Remove(this);
        }
#endif
        
        /// <summary>
        /// Construct a new AssetReference object.
        /// </summary>
        /// <param name="guid">The guid of the asset.</param>
        public AssetReference(string guid)
        {
            m_AssetGUID = guid;
        }
        
        //Special constructor only used when constructing in a derived class
        internal AssetReference(string guid, Type type)
        {
            m_AssetGUID = guid;
#if UNITY_EDITOR
            m_DerivedClassType = type;
#endif
        }

        /// <summary>
        /// The loaded asset.  This value is only set after the AsyncOperationHandle returned from LoadAssetAsync completes.
        /// It will not be set if only InstantiateAsync is called.  It will be set to null if release is called.
        /// </summary>
        public virtual Object Asset
        {
            get
            {
                if (!m_Operation.IsValid())
                    return null;

                return m_Operation.Result as Object;
            }
        }

#if UNITY_EDITOR
        Object m_CachedAsset;
        string m_CachedGUID = "";

        /// <summary>
        /// Cached Editor Asset.
        /// </summary>
        protected Object CachedAsset
        {
            get
            {
                if (m_CachedGUID != m_AssetGUID)
                {
                    m_CachedAsset = null;
                    m_CachedGUID = "";
                }
                return m_CachedAsset;
            }
            set
            {
                m_CachedAsset = value;
                m_CachedGUID = m_AssetGUID;
            }
        }
#endif
        /// <summary>
        /// String representation of asset reference.
        /// </summary>
        /// <returns>The asset guid as a string.</returns>
        public override string ToString()
        {
#if UNITY_EDITOR
            return "[" + m_AssetGUID + "]" + CachedAsset;
#else
            return "[" + m_AssetGUID + "]";
#endif
        }

        static AsyncOperationHandle<T> CreateFailedOperation<T>()
        {
            //this needs to be set in order for ResourceManager.ExceptionHandler to get hooked up to AddressablesImpl.LogException.
            Addressables.InitializeAsync();
            return Addressables.ResourceManager.CreateCompletedOperation(default(T), new Exception("Attempting to load an asset reference that has no asset assigned to it.").Message);
        }

        /// <summary>
        /// Load the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use <see cref="Addressables.LoadAssetAsync{TObject}(object)"/> and pass your AssetReference in as the key.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <returns>The load operation.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadAssetAsync(*)", true)]
        [Obsolete]
        public AsyncOperationHandle<TObject> LoadAsset<TObject>()
        {
            return LoadAssetAsync<TObject>();
        }

        /// <summary>
        /// Loads the reference as a scene.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use Addressables.LoadSceneAsync() and pass your AssetReference in as the key.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <returns>The operation handle for the scene load.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadSceneAsync(*)", true)]
        [Obsolete]
        public AsyncOperationHandle<SceneInstance> LoadScene()
        {
            return LoadSceneAsync();
        }

        /// <summary>
        /// InstantiateAsync the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use Addressables.InstantiateAsync() and pass your AssetReference in as the key.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <param name="position">Position of the instantiated object.</param>
        /// <param name="rotation">Rotation of the instantiated object.</param>
        /// <param name="parent">The parent of the instantiated object.</param>
        /// <returns>Returns the instantiation operation.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public AsyncOperationHandle<GameObject> Instantiate(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return InstantiateAsync(position, rotation, parent);
        }

        /// <summary>
        /// InstantiateAsync the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use Addressables.InstantiateAsync() and pass your AssetReference in as the key.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <param name="parent">The parent of the instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <returns>Returns the instantiation operation.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public AsyncOperationHandle<GameObject> Instantiate(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return InstantiateAsync(parent, instantiateInWorldSpace);
        }

        /// <summary>
        /// Load the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use <see cref="Addressables.LoadAssetAsync{TObject}(object)"/> and pass your AssetReference in as the key.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <returns>The load operation if there is not a valid cached operation, otherwise return default operation.</returns>
        public virtual AsyncOperationHandle<TObject> LoadAssetAsync<TObject>()
        {
            AsyncOperationHandle<TObject> result = default(AsyncOperationHandle<TObject>);
            if (m_Operation.IsValid())
                Debug.LogError("Attempting to load AssetReference that has already been loaded. Handle is exposed through getter OperationHandle");
            else
            {
                result = Addressables.LoadAssetAsync<TObject>(RuntimeKey);
                OperationHandle = result;
            }
            return result;
        }

        /// <summary>
        /// Loads the reference as a scene.
        /// This cannot be used a second time until the first load is unloaded. If you wish to call load multiple times
        /// on an AssetReference, use Addressables.LoadSceneAsync() and pass your AssetReference in as the key.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request if there is not a valid cached operation, otherwise return default operation</returns>
        public virtual AsyncOperationHandle<SceneInstance> LoadSceneAsync(LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            AsyncOperationHandle<SceneInstance> result = default(AsyncOperationHandle<SceneInstance>);
            if (m_Operation.IsValid())
                Debug.LogError("Attempting to load AssetReference Scene that has already been loaded. Handle is exposed through getter OperationHandle");
            else
            {
                result = Addressables.LoadSceneAsync(RuntimeKey, loadMode, activateOnLoad, priority);
                OperationHandle = result;
            }
            return result;
        }

        /// <summary>
        /// Unloads the reference as a scene.
        /// </summary>
        /// <returns>The operation handle for the scene load.</returns>
        public virtual AsyncOperationHandle<SceneInstance> UnLoadScene()
        {
            return Addressables.UnloadSceneAsync(m_Operation, true);
        }

        /// <summary>
        /// InstantiateAsync the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use Addressables.InstantiateAsync() and pass your AssetReference in as the key.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <param name="position">Position of the instantiated object.</param>
        /// <param name="rotation">Rotation of the instantiated object.</param>
        /// <param name="parent">The parent of the instantiated object.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<GameObject> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent, true);
        }

        /// <summary>
        /// InstantiateAsync the referenced asset as type TObject.
        /// This cannot be used a second time until the first load is released. If you wish to call load multiple times
        /// on an AssetReference, use Addressables.InstantiateAsync() and pass your AssetReference in as the key.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <param name="parent">The parent of the instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<GameObject> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
        {
            return Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace, true);
        }

        /// <inheritdoc/>
        public virtual bool RuntimeKeyIsValid()
        {
            Guid result;
            string guid = RuntimeKey.ToString();
            int subObjectIndex = guid.IndexOf("[");
            if (subObjectIndex != -1) //This means we're dealing with a sub-object and need to convert the runtime key.
                guid = guid.Substring(0, subObjectIndex);
            return Guid.TryParse(guid, out result);
        }

        /// <summary>
        /// Release the internal operation handle.
        /// </summary>
        public virtual void ReleaseAsset()
        {
            if (!m_Operation.IsValid())
            {
                Debug.LogWarning("Cannot release a null or unloaded asset.");
                return;
            }
            Addressables.Release(m_Operation);
            m_Operation = default(AsyncOperationHandle);
        }

        /// <summary>
        /// Release an instantiated object.
        /// </summary>
        /// <param name="obj">The object to release.</param>
        public virtual void ReleaseInstance(GameObject obj)
        {
            Addressables.ReleaseInstance(obj);
        }

        /// <summary>
        /// Validates that the referenced asset allowable for this asset reference.
        /// </summary>
        /// <param name="obj">The Object to validate.</param>
        /// <returns>Whether the referenced asset is valid.</returns>
        public virtual bool ValidateAsset(Object obj)
        {
            return true;
        }

        /// <summary>
        /// Validates that the referenced asset allowable for this asset reference.
        /// </summary>
        /// <param name="path">The path to the asset in question.</param>
        /// <returns>Whether the referenced asset is valid.</returns>
        public virtual bool ValidateAsset(string path)
        {
            return true;
        }

#if UNITY_EDITOR

        [SerializeField]
        #pragma warning disable CS0414
        bool m_EditorAssetChanged; 
        protected internal Type m_DerivedClassType;
#pragma warning restore CS0414
        
        /// <summary>
        /// Used by the editor to represent the main asset referenced.
        /// </summary>
        public virtual Object editorAsset
        {
            get
            {
                if (CachedAsset != null || string.IsNullOrEmpty(m_AssetGUID))
                    return CachedAsset;
                
                var asset = FetchEditorAsset();
                
                if (m_DerivedClassType == null)
                    return CachedAsset = asset;
                
                if (asset == null)
                    Debug.LogWarning("Assigned editorAsset does not match type " + m_DerivedClassType + ". EditorAsset will be null.");
                return CachedAsset = asset;
            }
        }
        
        internal Object FetchEditorAsset()
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, m_DerivedClassType ?? AssetDatabase.GetMainAssetTypeAtPath(assetPath));
            return asset;
        }
        
        /// <summary>
        /// Sets the main asset on the AssetReference.  Only valid in the editor, this sets both the editorAsset attribute,
        ///   and the internal asset GUID, which drives the RuntimeKey attribute. If the reference uses a sub object,
        ///   then it will load the editor asset during edit mode and load the sub object during runtime. For example, if
        ///   the AssetReference is set to a sprite within a sprite atlas, the editorAsset is the atlas (loaded during edit mode)
        ///   and the sub object is the sprite (loaded during runtime). If called by AssetReferenceT, will set the editorAsset
        ///   to the requested object if the object is of type T, and null otherwise.
        /// <param name="value">Object to reference</param>
        /// </summary>
        public virtual bool SetEditorAsset(Object value)
        {
            if (value == null)
            {
                CachedAsset = null;
                m_AssetGUID = string.Empty;
                m_SubObjectName = null;
                m_EditorAssetChanged = true;
                return true;
            }

            if (CachedAsset != value)
            {
                m_SubObjectName = null;
                var path = AssetDatabase.GetAssetOrScenePath(value);
                if (string.IsNullOrEmpty(path))
                {
                    Addressables.LogWarningFormat("Invalid object for AssetReference {0}.", value);
                    return false;
                }
                if (!ValidateAsset(path))
                {
                    Addressables.LogWarningFormat("Invalid asset for AssetReference path = '{0}'.", path);
                    return false;
                }
                else
                {
                    m_AssetGUID = AssetDatabase.AssetPathToGUID(path);
                    Object mainAsset;
                    if (m_DerivedClassType != null)
                        mainAsset = LocateEditorAssetForTypedAssetReference(value, path);
                    else
                    {
                        mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                        if (value != mainAsset)
                            SetEditorSubObject(value);
                    }
                    CachedAsset = mainAsset;
                }
            }

            m_EditorAssetChanged = true;
            return true;
        }

        internal Object LocateEditorAssetForTypedAssetReference(Object value, string path)
        {
            Object mainAsset;
            if (value.GetType() != m_DerivedClassType)
            {
                mainAsset = null;
            }
            else
            {
                mainAsset = AssetDatabase.LoadAssetAtPath(path, m_DerivedClassType);
                if (mainAsset != value)
                {
                    mainAsset = null;
                    var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
                    foreach (var asset in subAssets)
                    {
                        if (asset.GetType() == m_DerivedClassType && value == asset)
                        {
                            mainAsset = asset;
                            break;
                        }
                    }
                }
            }
            if (mainAsset == null)
                Debug.LogWarning( "Assigned editorAsset does not match type " + m_DerivedClassType + ". EditorAsset will be null.");

            return mainAsset;
        }
            

        /// <summary>
        /// Sets the sub object for this asset reference.
        /// </summary>
        /// <param name="value">The sub object.</param>
        /// <returns>True if set correctly.</returns>
        public virtual bool SetEditorSubObject(Object value)
        {
            if (value == null)
            {
                m_SubObjectName = null;
                m_SubObjectType = null;
                m_EditorAssetChanged = true;
                return true;
            }

            if (editorAsset == null)
                return false;
            if (editorAsset.GetType() == typeof(SpriteAtlas))
            {
                var spriteName = value.name;
                if (spriteName.EndsWith("(Clone)"))
                    spriteName = spriteName.Replace("(Clone)", "");
                if ((editorAsset as SpriteAtlas).GetSprite(spriteName) == null)
                {
                    Debug.LogWarningFormat("Unable to find sprite {0} in atlas {1}.", spriteName, editorAsset.name);
                    return false;
                }
                m_SubObjectName = spriteName;
                m_SubObjectType = typeof(Sprite).AssemblyQualifiedName;
                m_EditorAssetChanged = true;
                return true;
            }

            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GUIDToAssetPath(m_AssetGUID));
            foreach (var s in subAssets)
            {
                if (s.name == value.name && s.GetType() == value.GetType())
                {
                    m_SubObjectName = s.name;
                    m_SubObjectType = s.GetType().AssemblyQualifiedName;
                    m_EditorAssetChanged = true;
                    return true;
                }
            }
            return false;
        }

#endif
    }
}
