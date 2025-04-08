﻿using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;

namespace GameFrame
{
    public class SceneBase : Entity, IInitializeSystem, IScene
    {
        private Dictionary<string, SceneHandle> sceneHandleDic = new Dictionary<string, SceneHandle>();
        
        protected virtual string SingleSceneName { get; set; }

        public virtual void OnInitialize()
        {
            LoadMainScene().Forget();
        }

        private async UniTask LoadMainScene()
        {
            bool succ = await LoadScene(SingleSceneName, LoadSceneMode.Single);
            if (succ)
                OnReady();
        }

        protected virtual void OnReady()
        {
        }

        protected async UniTask<bool> LoadScene(string name, LoadSceneMode sceneMode)
        {
            int versions = Versions;
            var hand = await AssetManager.Instance.LoadSceneAsync(name, sceneMode);
            if (hand != null && versions != Versions)
            {
                hand.UnSuspend();
                return false;
            }
            else if (hand != null)
            {
                sceneHandleDic.Add(name, hand);
                return true;
            }

            return false;
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