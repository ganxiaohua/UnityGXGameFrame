using FairyGUI;

namespace GameFrame
{
    public abstract class UIViewBase : Window, IView
    {
        protected Entity m_UIBase;

        protected abstract string m_WindName { get; set; }

        public string WindName
        {
            get { return m_WindName; }
        }

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
            m_UIBase = uiBase;
        }
    }
}