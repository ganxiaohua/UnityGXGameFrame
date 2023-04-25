
using System;
using System.Collections.Generic;
using GameFrame;
namespace GXGame
{
    public static class UICardListWindowSystem
    {
        
        [SystemBind]
        public class UICardListWindowStartSystem : StartSystem<UICardListWindow>
        {
            protected override void Start(UICardListWindow self)
            {
                self.UICardListWindowView = new UICardListWindowView();
                self.UICardListWindowView.Link(self);
                List<string> temp = new List<string>();
                temp.Add("Card/Card");
                self.AddComponent<DependentResources, List<string>>(temp);
            }
        }

        [SystemBind]
        public class UICardListWindowShowSystem : ShowSystem<UICardListWindow>
        {
            protected override void Show(UICardListWindow self)
            {
                self.UICardListWindowView.Show();
            }
        }

        [SystemBind]
        public class UICardListWindowHideSystem : HideSystem<UICardListWindow>
        {
            protected override void Hide(UICardListWindow self)
            {
                self.UICardListWindowView.Hide();
            }
        }

        [SystemBind]
        public class UICardListWindowUpdateSystem : UpdateSystem<UICardListWindow>
        {
            protected override void Update(UICardListWindow self,float elapseSeconds, float realElapseSeconds)
            {
                
            }
        }

        [SystemBind]
        public class UICardListWindowClearSystem : ClearSystem<UICardListWindow>
        {
            protected override void Clear(UICardListWindow self)
            {
                self.UICardListWindowView.Clear();
                self.UICardListWindowView = null;
            }
        }
        
        public static void Back(this UICardListWindow self)
        {
            UIManager.Instance.Back();
        }

    }
}
