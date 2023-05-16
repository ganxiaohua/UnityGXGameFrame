using System;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

namespace GameFrame
{
    public class GameObjectPoolComponent : Entity, IStart,IClear
    {
        public Dictionary<string, ObjectPool<GameObjectObjectBase>> AllGameObjectPools;

        public int DefaultSize;

        public int ExpireTime;
    }
}
