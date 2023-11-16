using System;
using System.Collections.Generic;
using GameFrame;
namespace GameFrame
{
    public class Jackdoll : IReference
    {
        private Arena arena;

        public virtual void Init(Arena ar)
        {
            arena = ar;
        }

        public virtual void DollJoin(IDoll doll)
        {
        }

        public virtual void Update(List<IDoll> dolls, float elapseSeconds)
        {
        }

        public virtual void DollLeave(IDoll doll)
        {
        }

        protected virtual void ChangeJackdoll<T>(IDoll doll) where T : Jackdoll
        {
            arena.ChangeJackdoll<T>(doll);
        }

        public void Clear()
        {
        }
    }
}