﻿namespace GameFrame
{
    public static class SceneFactory
    {
        /// <summary>
        /// 切换自己逻辑的scene场景
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChangePlayerScene<T>(FsmState state) where T : class, IEntity, IScene, new()
        {
            IScene scene = EntityHouse.Instance.AddSceneEntity<T>(SceneType.PlayerScene, state);
            return (T) scene;
        }

        /// <summary>
        /// 获得当前player的scene
        /// </summary>
        /// <returns></returns>
        public static Entity GetPlayerScene()
        {
            IScene scene = EntityHouse.Instance.GetScene(SceneType.PlayerScene);
            return (Entity) scene;
        }

        /// <summary>
        /// 创建其他玩家的Scene
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChangeOtherPlayerScene<T>(FsmState state) where T : class, IEntity, IScene, new()
        {
            IScene scene = EntityHouse.Instance.AddSceneEntity<T>(SceneType.OtherPlayerScene, state);
            return (T) scene;
        }

        /// <summary>
        /// 获得其他玩家的Scene
        /// </summary>
        /// <returns></returns>
        public static Entity GetOtherPlayer()
        {
            IScene scene = EntityHouse.Instance.GetScene(SceneType.OtherPlayerScene);
            return (Entity) scene;
        }

        /// <summary>
        /// 创建回放的Scene
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ChangePlayBackScene<T>(FsmState state) where T : class, IEntity, IScene, new()
        {
            IScene scene = EntityHouse.Instance.AddSceneEntity<T>(SceneType.PlayBackScene, state);
            return (T) scene;
        }

        /// <summary>
        /// 获得回放的Scene
        /// </summary>
        /// <returns></returns>
        public static Entity GetPlayBack()
        {
            IScene scene = EntityHouse.Instance.GetScene(SceneType.PlayBackScene);
            return (Entity) scene;
        }
    }
}