using System;

namespace GameFrame
{
    public static class SceneEntitySystem
    {
        [SystemBind]
        public class SceneEntityStartSystem : StartSystem<SceneEntity, Type>
        {
            protected override void Start(SceneEntity self, Type p1)
            {
                self.Scene = (IScene) ReferencePool.Acquire(p1);
            }
        }

        [SystemBind]
        public class SceneEntityUpdateSystem : UpdateSystem<SceneEntity>
        {
            protected override void Update(SceneEntity self, float elapseSeconds, float realElapseSeconds)
            {
                self.Scene.Update(elapseSeconds, realElapseSeconds);
            }
        }

        [SystemBind]
        public class SceneEntityClearSystem : ClearSystem<SceneEntity>
        {
            protected override void Clear(SceneEntity self)
            {
                ReferencePool.Release(self.Scene);
            }
        }
    }
}