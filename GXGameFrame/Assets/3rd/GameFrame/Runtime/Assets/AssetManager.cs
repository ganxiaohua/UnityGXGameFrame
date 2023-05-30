using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using GameFrame;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class AssetManager : Singleton<AssetManager>
{
    class Assets
    {
        private const long DelayDelete = 10000;
        private AsyncOperationHandle AsyncOperationHandle;
        private int Count;
        private string Path;
        private Type Type;
        private List<object> CallBackList; //这个资源现在拥有的callback数量
        public Assets(AsyncOperationHandle asyncOperationHandle, string path, Type type, object action)
        {
            AsyncOperationHandle = asyncOperationHandle;
            CallBackList = new List<object>();
            Add(path, type, action);
        }

        public void Add(string path, Type type, object action)
        {
            if (Path == null && Type == null)
            {
                Path = path;
                Type = type;
            }
            else if (Path == path && Type != type)
            {
                Debugger.LogError($"{Path}一致,但是Type不一致");
                return;
            }

            if (action != null)
                CallBackList.Add(action);
            ++Count;
        }

        public bool CheckCallBack(object action)
        {
            if (action == null)
                return false;
            if (CallBackList.Contains(action))
            {
                return true;
            }

            return false;
        }

        public bool Release(object action)
        {
            if (action != null)
            {
                if (CallBackList.Contains(action))
                {
                    CallBackList.Remove(action);
                }
                else
                {
                    Debugger.LogWarning($"需要卸载的{Path}并不在资源列表的回调之中。");
                }
            }

            if (--Count == 0)
            {
                WaitReleaseAsset.CreateWaitReleaseAsset(AsyncOperationHandle, DelayDelete);
                CallBackList.Clear();
                return true;
            }
            else
            {
                Addressables.Release(AsyncOperationHandle);
                return false;
            }
        }
    }


    private readonly Dictionary<string, Assets> CacheAssetDic = new Dictionary<string, Assets>(16);
    public UILoader UILoader = new UILoader();
    public T Load<T>(string path)
    {
        var handle = Addressables.LoadAssetAsync<T>(path);
        handle.WaitForCompletion();
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Load Error:{handle.Status}");
            return default(T);
        }

        if (!CacheAssetDic.TryGetValue(path, out Assets asset))
        {
            CacheAssetDic.Add(path, new Assets(handle, path, typeof(T), null));
        }
        else
        {
            asset.Add(path, typeof(T), null);
        }
        return handle.Result;
    }

    public void LoadAsync<T>(string path, Action<object> completed = null)
    {
        var handle = Addressables.LoadAssetAsync<T>(path);
        if (!CacheAssetDic.TryGetValue(path, out Assets asset))
        {
            CacheAssetDic.Add(path, new Assets(handle, path, typeof(T), completed));
        }
        else
        {
            asset.Add(path, typeof(T), completed);
        }

        handle.Completed += (obj) =>
        {
            if (CacheAssetDic.TryGetValue(path, out Assets assets) && assets.CheckCallBack(completed))
            {
                completed?.Invoke(obj.Result);
            }
        };
    }

    public async UniTask<T> LoadAsyncTask<T>(string path)
    {
        var handle = Addressables.LoadAssetAsync<T>(path);
        if (!CacheAssetDic.TryGetValue(path, out Assets asset))
        {
            CacheAssetDic.Add(path, new Assets(handle, path, typeof(T), null));
        }
        else
        {
            asset.Add(path, typeof(T), null);
        }
        var obj = await handle.Task;
        return obj;
    }
    

    /// <summary>
    /// 异步加载 确保和Unload成对出现
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public void LoadAsync(string path, Type type, Action<object> completed = null)
    {
        if (type == typeof(GameObject))
        {
            Debugger.LogError("创建类型不准是 GameObject");
            return;
        }

        AsyncOperationHandle handle = FiltrateTypeLoad(type, path);

        if (!CacheAssetDic.TryGetValue(path, out Assets asset))
        {
            CacheAssetDic.Add(path, new Assets(handle, path, type, completed));
        }
        else
        {
            asset.Add(path, type, completed);
        }

        handle.Completed += (handle) =>
        {
            //防止在加载过程中卸载，由于延迟卸载特性而触发回调。
            if (CacheAssetDic.TryGetValue(path, out Assets assets) && assets.CheckCallBack(completed))
            {
                completed?.Invoke(handle.Result);
            }
        };
    }


    /// <summary>
    /// 异步加载 返回object和路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public void LoadAsyncWithPath(string path, Type type, Action<object, string> completed = null)
    {
        if (type == typeof(GameObject))
        {
            Debugger.LogError("创建类型不准是 GameObject");
            return;
        }

        AsyncOperationHandle handle = FiltrateTypeLoad(type, path);

        if (!CacheAssetDic.TryGetValue(path, out Assets asset))
        {
            CacheAssetDic.Add(path, new Assets(handle, path, type, completed));
        }
        else
        {
            asset.Add(path, type, completed);
        }

        handle.Completed += (handle) =>
        {
            //防止在加载过程中卸载，由于延迟卸载特性而触发回调。
            if (CacheAssetDic.TryGetValue(path, out Assets assets) && assets.CheckCallBack(completed))
            {
                completed?.Invoke(handle.Result, path);
            }
        };
    }


    /// <summary>
    /// 异步加载 确保和Unload成对出现
    /// </summary>
    /// <param name="path"></param>
    /// <param name="type"></param>
    /// <param name="completed"></param>
    /// <returns></returns>
    public void LoadAsyncWithLable(string label, Type type, Action<object> oneComleted, Action<object, string> completed = null)
    {
        if (type == typeof(GameObject))
        {
            Debugger.LogError("创建类型不准是 GameObject");
            return;
        }

        AsyncOperationHandle handle = FiltrateTypeLoads(type, label, oneComleted);

        if (!CacheAssetDic.TryGetValue(label, out Assets asset))
        {
            CacheAssetDic.Add(label, new Assets(handle, label, type, completed));
        }
        else
        {
            asset.Add(label, type, completed);
        }

        handle.Completed += (handle) =>
        {
            //防止在加载过程中卸载，由于延迟卸载特性而触发回调。
            if (CacheAssetDic.TryGetValue(label, out Assets assets) && assets.CheckCallBack(completed))
            {
                completed?.Invoke(handle.Result, label);
            }
        };
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="path"></param>
    /// <param name="update"></param>
    /// <param name="completed"></param>
    public async UniTask<Scene> LoadSceneAsync(string path)
    {
        if (CacheAssetDic.ContainsKey(path))
        {
            Debugger.LogError($"场景{path}已经存在");
            
        }

        var handle = Addressables.LoadSceneAsync(path,LoadSceneMode.Additive);

        if (!CacheAssetDic.TryGetValue(path, out Assets asset))
        {
            CacheAssetDic.Add(path, new Assets(handle, path, typeof(SceneManager),null));
        }
        else
        {
            asset.Add(path, typeof(SceneManager), null);
        }
        SceneInstance sceneinstance = await handle.Task;
        return sceneinstance.Scene;
    }

    /// <summary>
    /// 确保和LoadAsync或者Load成对出现
    /// </summary>
    /// <param name="path"></param>
    public void UnLoad(string path, object action = null)
    {
        if (CacheAssetDic.TryGetValue(path, out var asset))
        {
            if (asset.Release(action))
            {
                CacheAssetDic.Remove(path);
            }
        }
        else
        {
            throw new Exception($"no have {path}");
        }
    }

    protected void OnDestroy()
    {
        CacheAssetDic.Clear();
    }

    public string GetVersion()
    {
        return AddressablesHelper.AssetVersion.ToString();
    }

    public AsyncOperationHandle FiltrateTypeLoad(Type type, string path)
    {
        AsyncOperationHandle handle;
        if (type == typeof(Sprite))
        {
            handle = Addressables.LoadAssetAsync<Sprite>(path);
        }
        else if (type == typeof(Object))
        {
            handle = Addressables.LoadAssetAsync<Object>(path);
        }
        else if (type == typeof(Material))
        {
            handle = Addressables.LoadAssetAsync<Material>(path);
        }
        else
        {
            handle = Addressables.LoadAssetAsync<object>(path);
        }

        return handle;
    }

    public AsyncOperationHandle FiltrateTypeLoads(Type type, string path, Action<object> callback)
    {
        AsyncOperationHandle handle;
        if (type == typeof(Sprite))
        {
            handle = Addressables.LoadAssetsAsync<Sprite>(path, callback);
        }
        else if (type == typeof(Object))
        {
            handle = Addressables.LoadAssetsAsync<Object>(path, callback);
        }
        else if (type == typeof(Material))
        {
            handle = Addressables.LoadAssetsAsync<Material>(path, callback);
        }
        else
        {
            handle = Addressables.LoadAssetsAsync<object>(path, callback);
        }

        return handle;
    }

    /// <summary>
    /// 清理历史资源
    /// </summary>
    public void ClearHistoryFiles()
    {
    }
}