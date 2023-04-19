using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace GameFrame
{
    public static class UIWindowSystem
    {
        [SystemBind]
        public class UIWindowStartSystem : StartSystem<UIWindow>
        {
            protected override void Start(UIWindow self)
            {
                self.UIBase = ReferencePool.Acquire<UIViewBase>();
                self.UIBase.Link(self,null);
                List<string> temp = new List<string>();
                temp.Add("Home/Home");
                self.AddComponent<DependentResources,List<string>>(temp);
            }
        }

        [SystemBind]
        public class UIWindowUpdateSystem : UpdateSystem<UIWindow>
        {
            protected override void Update(UIWindow self, float elapseSeconds, float realElapseSeconds)
            {
                
            }
        }

        [SystemBind]
        public class UIWindowClearSystem : ClearSystem<UIWindow>
        {
            protected override void Clear(UIWindow self)
            {
                ReferencePool.Release(self.UIBase);
            }
        }

        [SystemBind]
        public class UIShowSystem : ShowSystem<UIWindow>
        {
            protected override void Show(UIWindow self)
            {
                self.UIBase.Show();
            }
        }

        [SystemBind]
        public class UIHideSystem : HideSystem<UIWindow>
        {
            protected override void Hide(UIWindow self)
            {
                self.UIBase.Hide();
            }
        }
    }
}