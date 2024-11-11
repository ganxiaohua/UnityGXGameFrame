using System.Collections.Generic;
using FairyGUI;

namespace GameFrame
{
    public class UIViewBase : IUIView
    {
        private const string InAnimationName = "in";
        private const string OutAnimationName = "out";

        /// <summary>
        ///     当前动画的存储
        /// </summary>
        protected Dictionary<string, Transition[]> AnimationPlayDic;

        /// <summary>
        ///     正在播放动画的数量当value的数值为0时代表播放结束
        /// </summary>
        protected Dictionary<string, int> AnimationPlayingCount;

        private PlayCompleteCallback m_PlayCompleteCallbackIn;
        private PlayCompleteCallback m_PlayCompleteCallbackOut;
        protected GComponent Root;
        protected IEntity UIBase;

        public void Link(Entity uiBase, GObject root, bool isMainPanel)
        {
            UIBase = uiBase;
            Root = (GComponent) root;
            if (isMainPanel)
            {
                Root.SetSize(GRoot.inst.width, GRoot.inst.height);
                Root.AddRelation(GRoot.inst, RelationType.Size);
                GRoot.inst.AddChild(Root);
            }

            OnInit();
        }

        public virtual void OnInit()
        {
            Root.visible = false;
            m_PlayCompleteCallbackIn = AnimatoinInComplete;
            m_PlayCompleteCallbackOut = AnimatoinOutComplete;
            AnimationPlayingCount = new Dictionary<string, int>();
            AnimationPlayDic = new Dictionary<string, Transition[]>();
        }

        public virtual void OnShow()
        {
            Root.visible = true;
            DoShowAnimation();
        }

        public virtual void OnHide()
        {
            DoHideAnimation();
        }


        protected virtual void OnHideLater()
        {
            Root.visible = false;
        }

        protected virtual void DoShowAnimation()
        {
            if (!PlayAnimation(InAnimationName, m_PlayCompleteCallbackIn)) UIManager.Instance.UIOpened(UIBase.GetType());
        }

        protected virtual void DoHideAnimation()
        {
            if (!PlayAnimation(OutAnimationName, m_PlayCompleteCallbackOut))
            {
                UIManager.Instance.UIClose(UIBase.GetType());
                OnHideLater();
            }
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public virtual void Clear()
        {
            AnimationPlayDic?.Clear();
            AnimationPlayingCount?.Clear();
            m_PlayCompleteCallbackIn = null;
            m_PlayCompleteCallbackOut = null;
            AnimationPlayingCount = null;
        }

        protected bool PlayAnimation(string animationName, PlayCompleteCallback compeleFunc)
        {
            if (!AnimationPlayDic.TryGetValue(animationName, out var Transitions))
            {
                Transitions = Root.GetTransitionsInChildren(animationName);
                AnimationPlayDic.Add(animationName, Transitions);
            }

            AnimationPlayingCount[animationName] = Transitions.Length;

            foreach (var animatoin in Transitions) animatoin.Play(compeleFunc);

            return Transitions.Length > 0;
        }

        protected void AnimatoinInComplete()
        {
            if (!AnimationPlayingCount.ContainsKey(InAnimationName)) return;

            if (--AnimationPlayingCount[InAnimationName] == 0) UIManager.Instance.UIOpened(UIBase.GetType());
        }

        protected void AnimatoinOutComplete()
        {
            if (!AnimationPlayingCount.ContainsKey(OutAnimationName)) return;

            if (--AnimationPlayingCount[OutAnimationName] == 0)
            {
                OnHideLater();
                UIManager.Instance.UIClose(UIBase.GetType());
            }
        }
    }
}