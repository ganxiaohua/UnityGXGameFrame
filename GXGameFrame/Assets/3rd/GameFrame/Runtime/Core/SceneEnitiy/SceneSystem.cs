using System;

namespace GameFrame
{
    public static class SceneSystem
    {
        public class SceneStartSystem : StartSystem<SceneEntity>
        {
            protected override void Start(SceneEntity self)
            {
                self.Scene.Start(self);
            }
        }

        public class SceneUpdateSystem : UpdateSystem<SceneEntity>
        {
            protected override void Update(SceneEntity self, float elapseSeconds, float realElapseSeconds)
            {
                self.Scene.Update(elapseSeconds, realElapseSeconds);
            }
        }

        public class SceneClearSystem : ClearSystem<SceneEntity>
        {
            protected override void Clear(SceneEntity self)
            {
                ReferencePool.Release(self.Scene);
            }
        }
    }
}