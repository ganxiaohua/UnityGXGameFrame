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
        GXGmeFrameMain.Instance.Start();
        var MainScene = SceneEntityFactory.CreateScene<GameInitScene>();

         SceneEntityFactory.CreateScene<BattlegroundScene>(MainScene);
    }

    // Update is called once per frame
    void Update()
    {
        GXGmeFrameMain.Instance.Update();
        if (Input.GetKeyDown(KeyCode.A))
        {
            EnitityHouse.Instance.GetSceneEntity<BattlegroundScene>().AddComponent<Entity1>();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            EnitityHouse.Instance.GetSceneEntity<BattlegroundScene>().RemoveComponent<Entity1>(); 
        }
    }
}