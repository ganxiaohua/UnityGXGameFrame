using System;
using UnityEngine;

namespace GameFrame
{
    public class SceneEntity : Entity, IStart, IUpdate,IClear
    {
        public override void Initialize()
        {
            Type type = ((ParameterP1<Type>) Parameter).Param1;
            Scene = (IScene) ReferencePool.Acquire(type);
            base.Initialize();
            this.AddSystem<SceneSystem.SceneStartSystem>();
            this.AddSystem<SceneSystem.SceneUpdateSystem>();
            this.AddSystem<SceneSystem.SceneClearSystem>();
        }

        public IScene Scene;
    }
}