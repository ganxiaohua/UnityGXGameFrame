
using System;
using System.Collections.Generic;
using GameFrame;
namespace GXGame
{
    public static class BattleGroudSceneSystem
    {
        
        [SystemBind]
        public class BattleGroudSceneStartSystem : StartSystem<BattleGroudScene>
        {
            protected override void Start(BattleGroudScene self)
            {
                
            }
        }

        [SystemBind]
        public class BattleGroudSceneShowSystem : ShowSystem<BattleGroudScene>
        {
            protected override void Show(BattleGroudScene self)
            {
                
            }
        }

        [SystemBind]
        public class BattleGroudSceneUpdateSystem : UpdateSystem<BattleGroudScene>
        {
            protected override void Update(BattleGroudScene self,float elapseSeconds, float realElapseSeconds)
            {
                
            }
        }

    }
}
