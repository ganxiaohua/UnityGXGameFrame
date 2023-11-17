using System;
using System.Collections.Generic;
using GameFrame;
namespace GameFrame
{
    public class ArenaEnitiy : Entity, IStart,IUpdate,IClear
    {
        public Arena Arena;
        public Dictionary<Type, JackdollEnitiy> JackdollDic;
        public Dictionary<IDoll, JackdollEnitiy> DollForJackdollDic;
        public List<IDoll> Dolls;
    } 
}
