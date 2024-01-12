using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class JackdollEnitiy : Entity, IStartSystem<Type, Arena>, IUpdateSystem, IClearSystem
    {
        public Jackdoll Jcakdoll;
        public List<IDoll> Dolls;

        public void Start(Type jcakdoll, Arena arena)
        {
            Jcakdoll = (Jackdoll) ReferencePool.Acquire(jcakdoll);
            Jcakdoll.Init(arena);
            Dolls = new List<IDoll>(16);
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            Jcakdoll.Update(Dolls, elapseSeconds);
        }

        public override void Clear()
        {
            base.Clear();
            Dolls.Clear();
            ReferencePool.Release(Jcakdoll);
        }

        public void DollJoin(IDoll doll)
        {
            Jcakdoll.DollJoin(doll);
            Dolls.Add(doll);
        }

        public void DollLeave(IDoll doll)
        {
            Jcakdoll.DollLeave(doll);
            Dolls.Remove(doll);
        }
    }
}