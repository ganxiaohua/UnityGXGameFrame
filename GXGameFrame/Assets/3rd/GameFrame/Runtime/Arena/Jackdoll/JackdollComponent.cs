using System;
using System.Collections.Generic;
using GameFrame;
namespace GameFrame
{
    public class JackdollComponent : Entity, IStart,IUpdate,IClear
    {
        public Jackdoll Jcakdoll;
        public List<IDoll> Dolls;
    }
}
