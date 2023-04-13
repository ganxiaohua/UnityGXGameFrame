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
                self.UIBase = ReferencePool.Acquire<UIViewBase>();
                self.UIBase.Link(self,null);
            }
        }

        public class UIWindowUpdateSystem : UpdateSystem<UIWindow>
        {
            protected override void Update(UIWindow self, float elapseSeconds, float realElapseSeconds)
            {
                
            }
        }

        public class UIWindowClearSystem : ClearSystem<UIWindow>
        {
            protected override void Clear(UIWindow self)
            {
                ReferencePool.Release(self.UIBase);
            }
        }

        public class UIShowSystem : ShowSystem<UIWindow>
        {
            protected override void Show(UIWindow self)
            {
                self.UIBase.Show();
            }
        }

        public class UIHideSystem : HideSystem<UIWindow>
        {
            protected override void Hide(UIWindow self)
            {
                self.UIBase.Hide();
            }
        }
    }
}