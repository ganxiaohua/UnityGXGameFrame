using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using GameFrame;
namespace GameFrame
{
    public static class JackdollComponentSystem
    {
        [SystemBind]
        public class JackdollComponentStartSystem : StartSystem<JackdollComponent, Type,Arena>
        {
            protected override void Start(JackdollComponent self, Type jcakdoll,Arena arena)
            {
                self.Jcakdoll = (Jackdoll) ReferencePool.Acquire(jcakdoll);
                self.Jcakdoll.Init(arena);
                self.Dolls = new List<IDoll>(16);
            }
        }

        [SystemBind]
        public class JackdollComponentUpdateSystem : UpdateSystem<JackdollComponent>
        {
            protected override void Update(JackdollComponent self, float elapseSeconds, float realElapseSeconds)
            {
                self.Jcakdoll.Update(self.Dolls, elapseSeconds);
            }
        }

        [SystemBind]
        public class JackdollComponentClearSystem : ClearSystem<JackdollComponent>
        {
            protected override void Clear(JackdollComponent self)
            {
                ReferencePool.Release(self.Jcakdoll);
                self.Dolls.Clear();
            }
        }


        public static void DollJoin(this JackdollComponent self, IDoll doll)
        {
            self.Jcakdoll.DollJoin(doll);
            self.Dolls.Add(doll);
        }

        public static void DollLeave(this JackdollComponent self, IDoll doll)
        {
            self.Jcakdoll.DollLeave(doll);
            self.Dolls.Remove(doll);
        }
    }
}