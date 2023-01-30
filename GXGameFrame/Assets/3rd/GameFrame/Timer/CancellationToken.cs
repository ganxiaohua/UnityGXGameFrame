using System;
using System.Collections.Generic;
using System.Threading;

namespace GameFrame
{
    public class CancellationToken : IReference
    {
        private HashSet<Action> actions = new HashSet<Action>();

        public void Add(Action callback)
        {
            // 如果action是null，绝对不能添加,要抛异常，说明有协程泄漏
            this.actions.Add(callback);
        }

        public void Remove(Action callback)
        {
            this.actions?.Remove(callback);
        }

        public bool IsCancel()
        {
            return this.actions == null;
        }


        public void Invoke()
        {
            HashSet<Action> runActions = this.actions;
            try
            {
                foreach (Action action in runActions)
                {
                    action.Invoke();
                }
            }
            catch (Exception e)
            {
                Debugger.Log(e.Message);
            }
        }

        public void Clear()
        {
            if (this.actions == null)
            {
                return;
            }
            actions.Clear();
        }
    }
}