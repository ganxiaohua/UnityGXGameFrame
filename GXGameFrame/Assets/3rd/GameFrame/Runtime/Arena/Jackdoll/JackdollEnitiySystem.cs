using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using GameFrame;
namespace GameFrame
{
    public static class JackdollEnitiySystem
    {
        [SystemBind]
        public class JackdollEnitiyStartSystem : StartSystem<JackdollEnitiy, Type,Arena>
        {
            protected override void Start(JackdollEnitiy self, Type jcakdoll,Arena arena)
            {
                self.Jcakdoll = (Jackdoll) ReferencePool.Acquire(jcakdoll);
                self.Jcakdoll.Init(arena);
                self.Dolls = new List<IDoll>(16);
            }
        }

        [SystemBind]
        public class JackdollEnitiyUpdateSystem : UpdateSystem<JackdollEnitiy>
        {
            protected override void Update(JackdollEnitiy self, float elapseSeconds, float realElapseSeconds)
            {
                self.Jcakdoll.Update(self.Dolls, elapseSeconds);
            }
        }

        [SystemBind]
        public class JackdollEnitiyClearSystem : ClearSystem<JackdollEnitiy>
        {
            protected override void Clear(JackdollEnitiy self)
            {
                ReferencePool.Release(self.Jcakdoll);
                self.Dolls.Clear();
            }
        }


        public static void DollJoin(this JackdollEnitiy self, IDoll doll)
        {
            self.Jcakdoll.DollJoin(doll);
            self.Dolls.Add(doll);
        }

        public static void DollLeave(this JackdollEnitiy self, IDoll doll)
        {
            self.Jcakdoll.DollLeave(doll);
            self.Dolls.Remove(doll);
        }
    }
}