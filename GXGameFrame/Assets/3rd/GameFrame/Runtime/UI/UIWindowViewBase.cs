using FairyGUI;

namespace GameFrame
{
    public abstract class UIViewBase : Window, IView
    {
        protected Entity UIBase;
        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnInit()
        {
            base.OnInit();
        }

        protected override void OnShown()
        {
            base.OnShown();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }

        protected override void DoShowAnimation()
        {
            base.DoShowAnimation();
        }

        protected override void DoHideAnimation()
        {
            base.DoHideAnimation();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void Clear()
        {
            Dispose();
        }

        public void Link(Entity uiBase, string path)
        {
            UIBase = uiBase;
        }
    }
}