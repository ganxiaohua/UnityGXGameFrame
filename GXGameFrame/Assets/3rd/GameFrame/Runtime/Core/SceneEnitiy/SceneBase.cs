using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;

namespace GameFrame
{
    public class SceneBase : Entity, IInitializeSystem, IScene
    {
        private Dictionary<string, SceneHandle> sceneHandleDic = new Dictionary<string, SceneHandle>();

        protected virtual string SingleSceneName { get; set; }

        public virtual void Initialize()
        {
            LoadMainScene().Forget();
        }

        private async UniTask LoadMainScene()
        {
            bool succ = await LoadScene(SingleSceneName, LoadSceneMode.Single);
            if (succ)
            {
                OnReady();
            }
            else
            {
                Debugger.LogError("加载主场景失败");
            }
        }

        protected virtual void OnReady()
        {
            
        }

        protected async UniTask<bool> LoadScene(string name, LoadSceneMode sceneMode = LoadSceneMode.Additive)
        {
            int id = ID;
            var hand = await AssetManager.Instance.LoadSceneAsync(name);
            if (id != ID)
            {
                hand.UnSuspend();
                return false;
            }

            sceneHandleDic.Add(name, hand);
            return true;
        }

        public override void Dispose()
        {
            foreach (var sceneHandle in sceneHandleDic)
            {
                sceneHandle.Value.UnSuspend();
            }

            sceneHandleDic.Clear();
            base.Dispose();
        }
    }
}