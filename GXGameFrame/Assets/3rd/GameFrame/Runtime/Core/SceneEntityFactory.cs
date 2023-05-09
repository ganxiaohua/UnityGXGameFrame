using System;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace GameFrame
{
    public static class SceneEntityFactory
    {
        public static T CreateScene<T>(Entity parent) where T : class, IEntity, IScene, new()
        {
            if (parent == null)
            {
                throw new Exception("SceneEntityFactory parent is null");
            }

            T scene = parent.AddComponent<T>();
            EnitityHouse.Instance.AddSceneEntity(scene);
            return scene;
        }

        public static void RemoveScene<T>() where T : IScene
        {
            var scene = EnitityHouse.Instance.GetScene<T>();
            RemoveScene(scene);
        }

        public static void RemoveScene(IScene scene)
        {
            Entity entity = (Entity) scene;
            EnitityHouse.Instance.RemoveSceneEntity(scene);
            ((Entity) (entity.Parent)).RemoveComponent(scene.GetType());
        }
    }
}