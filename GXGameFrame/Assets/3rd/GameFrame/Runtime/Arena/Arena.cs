using System;
using GameFrame;
namespace GameFrame
{
    public abstract class Arena : IReference
    {
        private ArenaComponent arenaComponent;
        public virtual void Init(ArenaComponent arena)
        {
            arenaComponent = arena;
        }

        public virtual void Update(float elapseSeconds)
        {
        }

        internal void ChangeJackdoll<T>(IDoll doll)where T : Jackdoll
        {
            arenaComponent.ChangeJackdoll<T>(doll);
        }

        public void Clear()
        {
        }
    }
}