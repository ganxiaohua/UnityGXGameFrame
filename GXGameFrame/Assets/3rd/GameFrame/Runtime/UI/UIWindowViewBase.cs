using System;
using System.Collections.Generic;
using FairyGUI;

namespace GameFrame
{
    public class UIViewBase : Window, IView
    {
        protected IEntity UIBase;
        private PlayCompleteCallback m_PlayCompleteCallbackIn;
        private PlayCompleteCallback m_PlayCompleteCallbackOut;
        protected Dictionary<string, int> AnimationPlayDic;
        private const string InAnimationName = "in";
        private const string OutAnimationName = "out";

        protected override void OnInit()
        {
            m_PlayCompleteCallbackIn = AnimatoinInComplete;
            m_PlayCompleteCallbackOut = AnimatoinOutComplete;
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
            AnimationPlayDic = null;
            base.Dispose();
        }

        protected override void DoShowAnimation()
        {
            base.DoShowAnimation();
            PlayAnimation(InAnimationName, m_PlayCompleteCallbackIn);
        }

        protected override void DoHideAnimation()
        {
            base.DoHideAnimation();
            PlayAnimation(OutAnimationName, m_PlayCompleteCallbackOut);
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

        protected void PlayAnimation(string animationName,PlayCompleteCallback compeleFunc)
        {
            var animatoins = contentPane.GetTransitionsInChildren(animationName);
            if (animatoins.Length > 0)
            {
                AnimationPlayDic.Add(animationName,animatoins.Length);
            }

            foreach (var animatoin in animatoins)
            {
                animatoin.Play(compeleFunc);
            }
        }
        protected void AnimatoinInComplete()
        {
            if (!AnimationPlayDic.ContainsKey(InAnimationName))
            {
                return;
            }

            if (--AnimationPlayDic[InAnimationName] == 0)
            {
                
            }
        }
        
        protected void AnimatoinOutComplete()
        {
            if (!AnimationPlayDic.ContainsKey(OutAnimationName))
            {
                return;
            }
            
            if (--AnimationPlayDic[OutAnimationName] == 0)
            {
                EventManager.Instance.Send<UICloseEvent,Type>(UIBase.GetType());
            }
        }
    }
}