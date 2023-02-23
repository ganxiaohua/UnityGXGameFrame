using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    // Start is called before the first frame update
    struct GameInitScene : SceneEntityType
    {
    }

    struct BattlegroundScene : SceneEntityType
    {
    }
    
    void Start()
    {
        GXGameFrameMain.Instance.Start();
        var MainScene = SceneEntityFactory.CreateScene<GameInitScene>();

         SceneEntityFactory.CreateScene<BattlegroundScene>(MainScene);
    }

    // Update is called once per frame
    private int Entity1id;
    void Update()
    {
        GXGameFrameMain.Instance.Update();
        if (Input.GetKeyDown(KeyCode.A))
        {
            Entity1id = EnitityHouse.Instance.GetScene<BattlegroundScene>().AddChild<Entity1>().ID;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            EnitityHouse.Instance.GetScene<BattlegroundScene>().RemoveChild(Entity1id); 
        }
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EnitityHouse.Instance.GetScene<BattlegroundScene>().AddComponent<Bttleground>();
        }
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            EnitityHouse.Instance.GetScene<BattlegroundScene>().RemoveComponent<Bttleground>();
        }
    }
}