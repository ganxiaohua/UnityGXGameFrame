using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace GameFrame
{
    public static class SceneFactory
    {
        /// <summary>
        /// 切换自己逻辑的scene场景
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChangePlayerScene<T>() where T : class, IEntity, IScene, new()
        {
            IScene scene =  EnitityHouse.Instance.AddSceneEntity<T>(SceneType.PlayerScene);
            return (T)scene;
        }
        
        /// <summary>
        /// 获得当前player的scene
        /// </summary>
        /// <returns></returns>
        public static Entity GetPlayerScene()
        {
            IScene scene = EnitityHouse.Instance.GetScene(SceneType.PlayerScene);
            return (Entity)scene;
        }
        
        /// <summary>
        /// 创建其他玩家的Scene
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChangeOtherPlayerScene<T>() where T : class, IEntity, IScene, new()
        {
            IScene scene =  EnitityHouse.Instance.AddSceneEntity<T>(SceneType.OtherPlayerScene);
            return (T)scene;
        }
        
        /// <summary>
        /// 获得其他玩家的Scene
        /// </summary>
        /// <returns></returns>
        public static Entity GetOtherPlayer()
        {
            IScene scene = EnitityHouse.Instance.GetScene(SceneType.OtherPlayerScene);
            return (Entity)scene;
        }
        
        /// <summary>
        /// 创建回放的Scene
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChangePlayBackScene<T>() where T : class, IEntity, IScene, new()
        {
            IScene scene =  EnitityHouse.Instance.AddSceneEntity<T>(SceneType.PlayBackScene);
            return (T)scene;
        }
        
        /// <summary>
        /// 获得回放的Scene
        /// </summary>
        /// <returns></returns>
        public static Entity GetPlayBack()
        {
            IScene scene = EnitityHouse.Instance.GetScene(SceneType.PlayBackScene);
            return (Entity)scene;
        }

        
    }
}