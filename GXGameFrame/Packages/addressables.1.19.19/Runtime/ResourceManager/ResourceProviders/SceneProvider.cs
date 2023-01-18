using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;

namespace UnityEngine.ResourceManagement.ResourceProviders
{
    /// <summary>
    /// Implementation if ISceneProvider
    /// </summary>
    public class SceneProvider : ISceneProvider2
    {
        class SceneOp : AsyncOperationBase<SceneInstance>, IUpdateReceiver
        {
            bool m_ActivateOnLoad;
            SceneInstance m_Inst;
            IResourceLocation m_Location;
            LoadSceneMode m_LoadMode;
            int m_Priority;
            private AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;
            ResourceManager m_ResourceManager;
            public SceneOp(ResourceManager rm)
            {
                m_ResourceManager = rm;
            }

            internal override DownloadStatus GetDownloadStatus(HashSet<object> visited)
            {
                return m_DepOp.IsValid() ? m_DepOp.InternalGetDownloadStatus(visited) : new DownloadStatus() { IsDone = IsDone };
            }

            public void Init(IResourceLocation location, LoadSceneMode loadMode, bool activateOnLoad, int priority, AsyncOperationHandle<IList<AsyncOperationHandle>> depOp)
            {
                m_DepOp = depOp;
                if (m_DepOp.IsValid())
                    m_DepOp.Acquire();

                m_Location = location;
                m_LoadMode = loadMode;
                m_ActivateOnLoad = activateOnLoad;
                m_Priority = priority;
            }

            ///<inheritdoc />
            protected override bool InvokeWaitForCompletion()
            {
                if (m_DepOp.IsValid() && !m_DepOp.IsDone)
                    m_DepOp.WaitForCompletion();

                m_RM?.Update(Time.unscaledDeltaTime);
                if (!HasExecuted)
                    InvokeExecute();
                
                var timer = new Stopwatch();
                timer.Start();
                
                while (!IsDone)
                {
                    ((IUpdateReceiver)this).Update(Time.unscaledDeltaTime);
                    //We need the operation to complete but it'll take a frame to activate the scene (post 0.9 progress).
                    if (m_Inst.m_Operation.progress == 0 && timer.ElapsedMilliseconds > 5000)
                        throw new Exception("Infinite loop detected within LoadSceneAsync.WaitForCompletion. For more information see the notes under the Scenes section of the \"Synchronous Addressables\" page of the Addressables documentation.");

                    if (m_Inst.m_Operation.allowSceneActivation && Mathf.Approximately(m_Inst.m_Operation.progress, .9f))
                    {
                        Result = m_Inst;
                        return true;
                    }
                }
                
                return IsDone;
            }

            /// <inheritdoc />
            public override void GetDependencies(List<AsyncOperationHandle> deps)
            {
                if (m_DepOp.IsValid())
                    deps.Add(m_DepOp);
            }

            protected override string DebugName { get { return string.Format("Scene({0})", m_Location == null ? "Invalid" : ShortenPath(m_ResourceManager.TransformInternalId(m_Location), false)); } }

            protected override void Execute()
            {
                var loadingFromBundle = false;
                if (m_DepOp.IsValid())
                {
                    foreach (var d in m_DepOp.Result)
                    {
                        var abResource = d.Result as IAssetBundleResource;
                        if (abResource != null && abResource.GetAssetBundle() != null)
                            loadingFromBundle = true;
                    }
                }
                
                if (!m_DepOp.IsValid() || m_DepOp.OperationException == null)
                {
                    m_Inst = InternalLoadScene(m_Location, loadingFromBundle, m_LoadMode, m_ActivateOnLoad, m_Priority);
                    ((IUpdateReceiver)this).Update(0.0f);
                    if (!IsDone)
                        m_ResourceManager.AddUpdateReceiver(this);
                }
                else
                {
                    m_ResourceManager.RemoveUpdateReciever(this);
                    Complete(m_Inst, false, m_DepOp.OperationException);
                }
                HasExecuted = true;
            }

            internal SceneInstance InternalLoadScene(IResourceLocation location, bool loadingFromBundle, LoadSceneMode loadMode, bool activateOnLoad, int priority)
            {
                var internalId = m_ResourceManager.TransformInternalId(location);
                var op = InternalLoad(internalId, loadingFromBundle, loadMode);
                op.allowSceneActivation = activateOnLoad;
                op.priority = priority;
                return new SceneInstance() { m_Operation = op, Scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1) };
            }

            AsyncOperation InternalLoad(string path, bool loadingFromBundle, LoadSceneMode mode)
            {
#if !UNITY_EDITOR
                return SceneManager.LoadSceneAsync(path, new LoadSceneParameters() { loadSceneMode = mode });
#else
                if (loadingFromBundle)
                    return SceneManager.LoadSceneAsync(path, new LoadSceneParameters() { loadSceneMode = mode });
                else
                {
                    if (!path.ToLower().StartsWith("assets/") && !path.ToLower().StartsWith("packages/"))
                        path = "Assets/" + path;
                    if (path.LastIndexOf(".unity") == -1)
                        path += ".unity";

                    return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters() { loadSceneMode = mode });
                }
#endif
            }

            protected override void Destroy()
            {
                //the scene will be unloaded via the UnloadSceneOp as it waits correctlyf or the unload to complete before releasing the load op
                if (m_DepOp.IsValid())
                    m_DepOp.Release();
                base.Destroy();
            }

            protected override float Progress
            {
                get
                {
                    float depOpWeight = 0.9f;
                    float loadOpWeight = 0.1f;
                    float progress = 0f;

                    //We will always have an instance operation but this will be null until the dependant operation is completed.
                    if (m_Inst.m_Operation != null)
                        progress += m_Inst.m_Operation.progress * loadOpWeight;

                    if (!m_DepOp.IsDone)
                        progress += m_DepOp.PercentComplete * depOpWeight;
                    else
                        progress += depOpWeight;

                    return progress;
                }
            }

            void IUpdateReceiver.Update(float unscaledDeltaTime)
            {
                if (m_Inst.m_Operation != null)
                {
                    if (m_Inst.m_Operation.isDone || (!m_Inst.m_Operation.allowSceneActivation && Mathf.Approximately(m_Inst.m_Operation.progress, .9f)))
                    {
                        m_ResourceManager.RemoveUpdateReciever(this);
                        Complete(m_Inst, true, null);
                    }
                }
            }
        }

        class UnloadSceneOp : AsyncOperationBase<SceneInstance>
        {
            SceneInstance m_Instance;
            AsyncOperationHandle<SceneInstance> m_sceneLoadHandle;
            UnloadSceneOptions m_UnloadOptions;
            public void Init(AsyncOperationHandle<SceneInstance> sceneLoadHandle, UnloadSceneOptions options)
            {
                if (sceneLoadHandle.ReferenceCount > 0)
                {
                    m_sceneLoadHandle = sceneLoadHandle;
                    m_Instance = m_sceneLoadHandle.Result;
                }
                m_UnloadOptions = options;
            }

            protected override void Execute()
            {
                if (m_sceneLoadHandle.IsValid() && m_Instance.Scene.isLoaded)
                {
                    var unloadOp = SceneManager.UnloadSceneAsync(m_Instance.Scene, m_UnloadOptions);
                    if (unloadOp == null)
                        UnloadSceneCompletedNoRelease(null);
                    else
                        unloadOp.completed += UnloadSceneCompletedNoRelease;
                }
                else
                    UnloadSceneCompleted(null);

                HasExecuted = true;
            }

            ///<inheritdoc />
            protected override bool InvokeWaitForCompletion()
            {
                m_RM?.Update(Time.unscaledDeltaTime);
                if (!HasExecuted)
                    InvokeExecute();
                Debug.LogWarning("Cannot unload a Scene with WaitForCompletion. Scenes must be unloaded asynchronously.");
                return true;
            }

            private void UnloadSceneCompleted(AsyncOperation obj)
            {
                Complete(m_Instance, true, "");
                if (m_sceneLoadHandle.IsValid())
                    m_sceneLoadHandle.Release();
            }
            
            private void UnloadSceneCompletedNoRelease(AsyncOperation obj)
            {
                Complete(m_Instance, true, "");
            }

            protected override float Progress
            {
                get { return m_sceneLoadHandle.PercentComplete; }
            }
        }

        /// <inheritdoc/>
        public AsyncOperationHandle<SceneInstance> ProvideScene(ResourceManager resourceManager, IResourceLocation location, LoadSceneMode loadMode, bool activateOnLoad, int priority)
        {
            AsyncOperationHandle<IList<AsyncOperationHandle>> depOp = default(AsyncOperationHandle<IList<AsyncOperationHandle>>);
            if (location.HasDependencies)
                depOp = resourceManager.ProvideResourceGroupCached(location.Dependencies, location.DependencyHashCode, typeof(IAssetBundleResource), null);

            SceneOp op = new SceneOp(resourceManager);
            op.Init(location, loadMode, activateOnLoad, priority, depOp);

            var handle = resourceManager.StartOperation<SceneInstance>(op, depOp);

            if (depOp.IsValid())
                depOp.Release();

            return handle;
        }

        /// <inheritdoc/>
        public AsyncOperationHandle<SceneInstance> ReleaseScene(ResourceManager resourceManager, AsyncOperationHandle<SceneInstance> sceneLoadHandle)
        {
            return ((ISceneProvider2)(this)).ReleaseScene(resourceManager, sceneLoadHandle, UnloadSceneOptions.None);
        }

        /// <inheritdoc/>
        AsyncOperationHandle<SceneInstance> ISceneProvider2.ReleaseScene(ResourceManager resourceManager, AsyncOperationHandle<SceneInstance> sceneLoadHandle, UnloadSceneOptions unloadOptions)
        {
            var unloadOp = new UnloadSceneOp();
            unloadOp.Init(sceneLoadHandle, unloadOptions);
            return resourceManager.StartOperation(unloadOp, sceneLoadHandle);
        }
    }
}
