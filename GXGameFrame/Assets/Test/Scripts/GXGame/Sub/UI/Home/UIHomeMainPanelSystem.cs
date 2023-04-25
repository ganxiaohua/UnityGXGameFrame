
using System;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

namespace GXGame
{
    public static class UIHomeMainPanelSystem
    {
        
        [SystemBind]
        public class UIHomeMainPanelStartSystem : StartSystem<UIHomeMainPanel>
        {
            protected override void Start(UIHomeMainPanel self)
            {

                self.UIHomeMainPanelView = new UIHomeMainPanelView();
                self.UIHomeMainPanelView.Link(self);
                List<string> temp = new List<string>();
                temp.Add("Home/Home");
                self.AddComponent<DependentResources, List<string>>(temp);
            }
        }

        [SystemBind]
        public class UIHomeMainPanelShowSystem : ShowSystem<UIHomeMainPanel>
        {
            protected override void Show(UIHomeMainPanel self)
            {
                self.UIHomeMainPanelView.Show();
            }
        }

        [SystemBind]
        public class UIHomeMainPanelHideSystem : HideSystem<UIHomeMainPanel>
        {
            protected override void Hide(UIHomeMainPanel self)
            {
                self.UIHomeMainPanelView.Hide();
            }
        }

        [SystemBind]
        public class UIHomeMainPanelUpdateSystem : UpdateSystem<UIHomeMainPanel>
        {
            protected override void Update(UIHomeMainPanel self,float elapseSeconds, float realElapseSeconds)
            {
                
            }
        }

        public static void OpenCard(this UIHomeMainPanel self)
        {
            UIManager.Instance.OpenUI(typeof(UICardListWindow));
        }

    }
}
