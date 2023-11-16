using System;
using System.Collections.Generic;
using GameFrame;
namespace GameFrame
{
    public class ArenaComponent : Entity, IStart,IUpdate,IClear
    {
        public Arena Arena;
        public Dictionary<Type, JackdollComponent> JackdollDic;
        public Dictionary<IDoll, JackdollComponent> DollForJackdollDic;
        public List<IDoll> Dolls;
    } 
}
