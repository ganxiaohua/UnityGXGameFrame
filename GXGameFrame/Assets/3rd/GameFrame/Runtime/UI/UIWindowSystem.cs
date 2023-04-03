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
            }
        }

        public class UIShowSystem : ShowSystem<UIWindow>
        {
            protected override void Show(UIWindow self)
            {
            }
        }

        public class UIHideSystem : HideSystem<UIWindow>
        {
            protected override void Hide(UIWindow self)
            {
            }
        }
    }
}