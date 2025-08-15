using System.Threading;
using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame.Runtime
{
    public abstract class TransitionPanel : Panel
    {
        public Transition TransitionIn { get; private set; }

        public Transition TransitionOut { get; private set; }

        private PlayCompleteCallback showTransitionCallback;
        private PlayCompleteCallback restoreTransitionCallback;
        private PlayCompleteCallback hideTransitionCallback;

        public override async UniTask OnInitializeAsync(GComponent root, CancellationToken cancelToken = default)
        {
            await base.OnInitializeAsync(root, cancelToken);

            TransitionIn = root.GetTransition("in");
            TransitionOut = root.GetTransition("out");

            showTransitionCallback = OnShowTransitionComplete;
            restoreTransitionCallback = OnRestoreTransitionComplete;
            hideTransitionCallback = OnHideTransitionComplete;
        }

        protected override void DoShowAnimation()
        {
            base.DoShowAnimation();

            TransitionOut?.Stop();
            TransitionIn?.Play(showTransitionCallback);
        }

        protected virtual void OnShowTransitionComplete()
        {
        }

        protected override void DoRestoreAnimation()
        {
            base.DoRestoreAnimation();

            TransitionOut?.Stop();
            TransitionIn?.Play(restoreTransitionCallback);
        }

        protected virtual void OnRestoreTransitionComplete()
        {
        }

        protected override void DoHideAnimation()
        {
            TransitionIn?.Stop();
            if (TransitionOut != null)
                TransitionOut.Play(hideTransitionCallback);
            else
                base.DoHideAnimation();
        }

        protected virtual void OnHideTransitionComplete()
        {
            base.DoHideAnimation();
        }
    }
}