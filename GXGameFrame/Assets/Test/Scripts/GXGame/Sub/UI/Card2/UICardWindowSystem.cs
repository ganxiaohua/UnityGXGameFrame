
using System;
using System.Collections.Generic;
using GameFrame;
namespace GXGame
{
    public static class UICardWindowSystem
    {
        
        [SystemBind]
        public class UICardWindowStartSystem : StartSystem<UICardWindow>
        {
            protected override void Start(UICardWindow self)
            {
                                
                self.UICardWindowView = new UICardWindowView();
                self.UICardWindowView.Link(self);
                List<string> temp = new List<string>();
                temp.Add("Card/Card");
                self.AddComponent<DependentResources, List<string>>(temp);
            }
        }

        [SystemBind]
        public class UICardWindowShowSystem : ShowSystem<UICardWindow>
        {
            protected override void Show(UICardWindow self)
            {
                self.UICardWindowView.Show();
            }
        }

        [SystemBind]
        public class UICardWindowHideSystem : HideSystem<UICardWindow>
        {
            protected override void Hide(UICardWindow self)
            {
                self.UICardWindowView.Hide();
            }
        }

        [SystemBind]
        public class UICardWindowUpdateSystem : UpdateSystem<UICardWindow>
        {
            protected override void Update(UICardWindow self,float elapseSeconds, float realElapseSeconds)
            {
                
            }
        }

        [SystemBind]
        public class UICardWindowClearSystem : ClearSystem<UICardWindow>
        {
            protected override void Clear(UICardWindow self)
            {
                self.UICardWindowView.Clear();
            }
        }
        


    }
}
