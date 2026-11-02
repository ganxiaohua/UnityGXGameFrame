using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Runtime.UnityUI
{
    public abstract class TransitionPanel : Panel
    {
        public virtual float ShowDuration => 0;
        public virtual float HideDuration => 0;
        private CancellationTokenSource animationCancellation;

        protected override void DoShowAnimation()
        {
            base.DoShowAnimation();
            StartFade(1, ShowDuration, OnShowTransitionComplete);
        }

        protected override void DoRestoreAnimation()
        {
            base.DoShowAnimation();
            StartFade(1, ShowDuration, OnRestoreTransitionComplete);
        }

        protected override void DoHideAnimation() => StartFade(0, HideDuration, OnHideTransitionComplete);

        private void StartFade(float target, float duration, Action complete)
        {
            var wasAnimating = animationCancellation != null;
            CancelAnimation();
            if (!wasAnimating && target > 0) CanvasGroup.alpha = 0;
            if (duration <= 0)
            {
                CanvasGroup.alpha = target;
                complete();
                return;
            }
            var cancellation = new CancellationTokenSource();
            animationCancellation = cancellation;
            FadeAsync(target, duration, complete, cancellation).Forget();
        }

        private async UniTask FadeAsync(float target, float duration, Action complete, CancellationTokenSource cancellation)
        {
            var token = cancellation.Token;
            var start = CanvasGroup.alpha;
            var elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    await UniTask.Yield(token);
                    elapsed += Time.unscaledDeltaTime;
                    CanvasGroup.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                }
                animationCancellation = null;
                complete();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            finally
            {
                if (animationCancellation == cancellation) animationCancellation = null;
                cancellation.Dispose();
            }
        }

        private void CancelAnimation()
        {
            // The asynchronous owner disposes its CTS after the cancellation is observed.
            animationCancellation?.Cancel();
            animationCancellation = null;
        }

        protected virtual void OnShowTransitionComplete() { }
        protected virtual void OnRestoreTransitionComplete() { }
        protected virtual void OnHideTransitionComplete() => base.DoHideAnimation();

        public override void Dispose()
        {
            CancelAnimation();
            base.Dispose();
        }
    }
}
