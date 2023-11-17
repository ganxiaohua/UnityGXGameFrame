using System;
using GameFrame;
namespace GameFrame
{
    public abstract class Arena : IReference
    {
        private ArenaEnitiy arenaEnitiy;
        public virtual void Init(ArenaEnitiy arena)
        {
            arenaEnitiy = arena;
        }

        public virtual void Update(float elapseSeconds)
        {
        }

        internal void ChangeJackdoll<T>(IDoll doll)where T : Jackdoll
        {
            arenaEnitiy.ChangeJackdoll<T>(doll);
        }

        public void Clear()
        {
        }
    }
}