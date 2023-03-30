namespace GameFrame
{
    public static class UIShowComponentSystem
    {
        public class UIShowComponentStartSystem : StartSystem<UIShowComponent>
        {
            protected override void Start(UIShowComponent self)
            {
                self.UIBase.Show();
            }
        }
        
        public class UIShowComponentClearSystem : ClearSystem<UIShowComponent>
        {
            protected override void Clear(UIShowComponent self)
            {
                self.UIBase.Hide();
            }
        }
    }
}