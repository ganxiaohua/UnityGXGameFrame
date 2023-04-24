using System;
using System.Collections.Generic;
using FairyGUI;

namespace GameFrame
{
    public class UIViewBase : Window, IUIView
    {
        protected IEntity UIBase;
        private PlayCompleteCallback m_PlayCompleteCallbackIn;
        private PlayCompleteCallback m_PlayCompleteCallbackOut;
        /// <summary>
        /// 正在播放动画的数量当value的数值为0时代表播放结束
        /// </summary>
        protected Dictionary<string, int> AnimationPlayingCount;
        
        /// <summary>
        /// 当前动画的存储
        /// </summary>
        protected Dictionary<string, Transition[]> AnimationPlayDic;
        private const string InAnimationName = "in";
        private const string OutAnimationName = "out";

        protected override void OnInit()
        {
            m_PlayCompleteCallbackIn = AnimatoinInComplete;
            m_PlayCompleteCallbackOut = AnimatoinOutComplete;
            AnimationPlayingCount = new();
            AnimationPlayDic = new();
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

        public override void Dispose()
        {
            m_PlayCompleteCallbackIn = null;
            m_PlayCompleteCallbackOut = null;
            AnimationPlayingCount = null;
            base.Dispose();
            displayObject = null;
        }

        protected override void DoShowAnimation()
        {

            if (!PlayAnimation(InAnimationName, m_PlayCompleteCallbackIn))
            {
                EventManager.Instance.Send<UIOpenEvent,Type>(UIBase.GetType());
                base.DoShowAnimation();
            }
        }

        protected override void DoHideAnimation()
        {
            if (!PlayAnimation(OutAnimationName, m_PlayCompleteCallbackOut))
            {
                EventManager.Instance.Send<UICloseEvent,Type>(UIBase.GetType());
                base.DoHideAnimation();
            }
        }
        
        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public virtual void Clear()
        {
            AnimationPlayDic.Clear();
            AnimationPlayingCount.Clear();
            Dispose();
        }

        public void Link(Entity uiBase)
        {
            UIBase = uiBase;
        }

        protected bool PlayAnimation(string animationName,PlayCompleteCallback compeleFunc)
        {

            if (!AnimationPlayDic.TryGetValue(animationName,out Transition[] Transitions))
            {
                Transitions = contentPane.GetTransitionsInChildren(animationName);
                AnimationPlayDic.Add(animationName,Transitions);
            }
            AnimationPlayingCount[animationName] = Transitions.Length;

            foreach (var animatoin in Transitions)
            {
                animatoin.Play(compeleFunc);
            }

            return Transitions.Length > 0 ? true : false;
        }
        protected void AnimatoinInComplete()
        {
            if (!AnimationPlayingCount.ContainsKey(InAnimationName))
            {
                return;
            }

            if (--AnimationPlayingCount[InAnimationName] == 0)
            {
                OnShown();
                EventManager.Instance.Send<UIOpenEvent,Type>(UIBase.GetType());
            }
        }
        
        protected void AnimatoinOutComplete()
        {
            if (!AnimationPlayingCount.ContainsKey(OutAnimationName))
            {
                return;
            }
            if (--AnimationPlayingCount[OutAnimationName] == 0)
            {
                HideImmediately();
                EventManager.Instance.Send<UICloseEvent,Type>(UIBase.GetType());
            }
        }
    }
}