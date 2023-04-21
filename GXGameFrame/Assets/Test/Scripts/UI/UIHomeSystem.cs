using System;
using System.Collections.Generic;
using GameFrame;

namespace GXGame
{
    public static class UIWindowSystem
    {
        [SystemBind]
        public class UIHomeStartSystem : StartSystem<UIHome>
        {
            protected override void Start(UIHome self)
            {
                self.UIHomeView = ReferencePool.Acquire<UIHomeView>();
                self.UIHomeView.Link(self, null);
                List<string> temp = new List<string>();
                temp.Add("Home/Home");
                self.AddComponent<DependentResources, List<string>>(temp);
            }
        }

        [SystemBind]
        public class UIHomeShowSystem : ShowSystem<UIHome>
        {
            protected override void Show(UIHome self)
            {
                self.UIHomeView.Show();
            }
        }

        [SystemBind]
        public class UIHomeHideSystem : HideSystem<UIHome>
        {
            protected override void Hide(UIHome self)
            {
                self.UIHomeView.Hide();
            }
        }

        [SystemBind]
        public class UIHomeUpdateSystem : UpdateSystem<UIHome>
        {
            protected override void Update(UIHome self, float elapseSeconds, float realElapseSeconds)
            {
            }
        }

        [SystemBind]
        public class UIHomeClearSystem : ClearSystem<UIHome>
        {
            protected override void Clear(UIHome self)
            {
                ReferencePool.Release(self.UIHomeView);
            }
        }

        public static void Cilck(this UIHome self)
        {

        }

    }
}