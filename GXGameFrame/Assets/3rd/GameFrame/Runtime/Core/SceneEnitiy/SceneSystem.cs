using System;

namespace GameFrame
{
    public static class SceneEntitySystem
    {
        [SystemBind]
        public class SceneEntityStartSystem : StartSystem<MainScene, Type>
        {
            protected override void Start(MainScene self, Type p1)
            {
                // self.Scene = (IScene) ReferencePool.Acquire(p1);
            }
        }

        [SystemBind]
        public class SceneEntityUpdateSystem : UpdateSystem<MainScene>
        {
            protected override void Update(MainScene self, float elapseSeconds, float realElapseSeconds)
            {
                // self.Scene.Update(elapseSeconds, realElapseSeconds);
            }
        }

        [SystemBind]
        public class SceneEntityClearSystem : ClearSystem<MainScene>
        {
            protected override void Clear(MainScene self)
            {
                // ReferencePool.Release(self.Scene);
            }
        }
    }
}