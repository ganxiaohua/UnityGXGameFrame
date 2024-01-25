using System.Collections.Generic;
using FairyGUI;

namespace GameFrame
{
    public class UIViewBase : IUIView
    {
        protected IEntity UIBase;
        private PlayCompleteCallback m_PlayCompleteCallbackIn;
        private PlayCompleteCallback m_PlayCompleteCallbackOut;
        protected GComponent Root;

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
        
        public virtual void OnInit()
        {
            Root.visible = false;
            m_PlayCompleteCallbackIn = AnimatoinInComplete;
            m_PlayCompleteCallbackOut = AnimatoinOutComplete;
            AnimationPlayingCount = new();
            AnimationPlayDic = new();
        }

        public virtual void OnShow()
        {
            Root.visible = true;
            DoShowAnimation();
        }

        public virtual void OnHide()
        {
            
        }

        protected virtual void DoShowAnimation()
        {
            if (!PlayAnimation(InAnimationName, m_PlayCompleteCallbackIn))
            {
                UIManager.Instance.UIOpened(UIBase.GetType());
            }
        }

        public virtual void DoHideAnimation()
        {
            if (!PlayAnimation(OutAnimationName, m_PlayCompleteCallbackOut))
            {
                UIManager.Instance.UIClose(UIBase.GetType());
                OnHide();
            }
        }

        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public virtual void Clear()
        {
            AnimationPlayDic.Clear();
            AnimationPlayingCount.Clear();
            m_PlayCompleteCallbackIn = null;
            m_PlayCompleteCallbackOut = null;
            AnimationPlayingCount = null;
        }

        public void Link(Entity uiBase, GObject root,bool isMainPanel)
        {
            UIBase = uiBase;
            this.Root = (GComponent) root;
            if (isMainPanel)
            {
                Root.SetSize(GRoot.inst.width, GRoot.inst.height);
                Root.AddRelation(GRoot.inst, RelationType.Size);
                GRoot.inst.AddChild(Root);
            }
            OnInit();
        }

        protected bool PlayAnimation(string animationName, PlayCompleteCallback compeleFunc)
        {
            if (!AnimationPlayDic.TryGetValue(animationName, out Transition[] Transitions))
            {
                Transitions = this.Root.GetTransitionsInChildren(animationName);
                AnimationPlayDic.Add(animationName, Transitions);
            }

            AnimationPlayingCount[animationName] = Transitions.Length;

            foreach (var animatoin in Transitions)
            {
                animatoin.Play(compeleFunc);
            }

            return Transitions.Length > 0;
        }

        protected void AnimatoinInComplete()
        {
            if (!AnimationPlayingCount.ContainsKey(InAnimationName))
            {
                return;
            }

            if (--AnimationPlayingCount[InAnimationName] == 0)
            {
                UIManager.Instance.UIOpened(UIBase.GetType());
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
                OnHide();
                UIManager.Instance.UIClose(UIBase.GetType());
            }
        }
        
    }
}