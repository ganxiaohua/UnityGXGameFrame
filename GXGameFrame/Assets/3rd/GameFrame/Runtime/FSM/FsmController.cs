using System;
using System.Collections.Generic;

namespace GameFrame
{
    public abstract partial class FsmController : Entity, IInitializeSystem, IUpdateSystem
    {
        public FsmState CurState { get; private set; }

        private Dictionary<Type, FsmState> states = new Dictionary<Type, FsmState>();

        public virtual void Initialize()
        {
      
        }

        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            CurState?.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        public override void Dispose()
        {
            foreach (var item in states)
            {
                if (item.Value == CurState)
                {
                    item.Value.OnExit();
                }
            }

            states.Clear();
            CurState = null;
            ClearBlcakboard();
            base.Dispose();
        }

        protected T AddState<T>() where T : FsmState
        {
            var state = AddChild<T>();
            states.Add(typeof(T), state);
            return state;
        }

        protected T AddState<T, TP1>(TP1 p1) where T : FsmState
        {
            var state = AddChild<T, TP1>(p1);
            states.Add(typeof(T), state);
            return state;
        }

        protected T AddState<T, TP1, TP2>(TP1 p1, TP2 p2) where T : FsmState
        {
            var state = AddChild<T, TP1, TP2>(p1, p2);
            states.Add(typeof(T), state);
            return state;
        }

        protected void RemoveState(FsmState state)
        {
            Type type = state.GetType();
            bool b = states.Remove(type);
            Assert.IsTrue(b, $"不包含这个stateP{type.Name}");
            if (CurState == state)
            {
                state.OnExit();
            }

            RemoveChild(state);
        }
        
        public virtual void ChangeState<T>() where T : FsmState
        {
            bool b = states.TryGetValue(typeof(T), out var state);
            Assert.IsTrue(b, $"不包含这个stateP{typeof(T)}");
            CurState?.OnExit();
            CurState = state;
            CurState.OnEnter(this);
        }
    }
}