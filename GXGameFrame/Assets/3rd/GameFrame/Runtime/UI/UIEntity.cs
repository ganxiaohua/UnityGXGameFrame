using System;
using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public abstract class UIEntity : Entity, IPreShowSystem, IShowSystem, IHideSystem, IUpdateSystem
    {
        protected abstract string PackName { get; }

        protected abstract string WindowName { get; }

        protected abstract Type ViewType { get; }

        public UINode UINode { get; set; }
        
        private UIViewBase uiView;
        
        public virtual async UniTask Initialize()
        {
            var despen = AddComponent<DependentUI, string, string>(PackName, WindowName);
            uiView = (UIViewBase) Activator.CreateInstance(ViewType);
            var succ = await despen.WaitLoad();
            if (succ) uiView.Link(this, despen.Window, true);
        }
        
        public virtual void PreShow(bool isFirstShow)
        {
        }

        
        public virtual void Show()
        {
            uiView.OnShow();
        }

        public virtual void Hide()
        {
            uiView.OnHide();
        }

        
        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
            uiView.OnUpdate(elapseSeconds, realElapseSeconds);
        }
        
        public override void Dispose()
        {
            uiView.Clear();
        }
        
    }
}