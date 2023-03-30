using System;
using Unity.VisualScripting;

namespace GameFrame
{
    public static class UIWindowSystem
    {
        public class UIWindowStartSystem : StartSystem<UIWindow>
        {
            protected override void Start(UIWindow self)
            {
                Type type = ((ParameterP1<Type>) self.Parameter).Param1;
                self.UIBase = (UIViewBase) ReferencePool.Acquire(type);
                self.UIBase.Init();
            }
        }
        
        public class UIWindowUpdateSystem : UpdateSystem<UIWindow>
        {
            protected override void Update(UIWindow self, float elapseSeconds, float realElapseSeconds)
            {
                self.UIBase.Update(elapseSeconds,realElapseSeconds);
            }
        }

        public class UIWindowClearSystem : ClearSystem<UIWindow>
        {
            protected override void Clear(UIWindow self)
            {
                ReferencePool.Release(self.UIBase);
            }
        }
        
    }
}