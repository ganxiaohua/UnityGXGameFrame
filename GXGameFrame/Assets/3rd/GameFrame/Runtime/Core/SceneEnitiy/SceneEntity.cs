using System;
using UnityEngine;

namespace GameFrame
{
    public class SceneEntity : Entity, IStart, IUpdate,IClear
    {
        public IScene Scene;
    }
}
