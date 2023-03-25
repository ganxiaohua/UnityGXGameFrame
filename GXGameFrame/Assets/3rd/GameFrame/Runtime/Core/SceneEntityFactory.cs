using System;
using UnityEngine;

namespace GameFrame
{
    public static class SceneEntityFactory
    {
        public static SceneEntity CreateScene<T>(SceneEntity parent) where T : class, IScene, new()
        {
            if (parent == null)
            {
                throw new Exception("SceneEntityFactory parent is null");
            }
            
            SceneEntity sceneEntity = parent.AddChild<SceneEntity,Type>(typeof(T));
            EnitityHouse.Instance.AddSceneEntity<T>(sceneEntity);
            return sceneEntity;
        }

        public static void RemoveScene<T>() where T : IScene
        {
            SceneEntity sceneEntity = EnitityHouse.Instance.GetScene<T>();
            RemoveScene(sceneEntity);
        }

        public static void RemoveScene(SceneEntity sceneEntity)
        {
            EnitityHouse.Instance.RemoveSceneEntity(sceneEntity);
            ((SceneEntity) sceneEntity.Parent).RemoveChild(sceneEntity.ID);
        }
    }
}