using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame;
using GameFrame.Timer;
using GXGame;
using UnityEngine;



public class GameMain : MonoBehaviour
{
    void Start()
    {
        GXGameFrame.Instance.Start();
        SceneEntityFactory.CreateScene<BattleGroudScene>(GXGameFrame.Instance.MainScene);
        UIManager.Instance.OpenUI(typeof(UIHomeMainPanel));
    }

    // Update is called once per frame
    private int Entity1id;

    void Update()
    {
        GXGameFrame.Instance.Update();
        if (Input.GetKeyDown(KeyCode.A))
        {
            Entity1 entity1 = EnitityHouse.Instance.GetScene<BattleGroudScene>().AddChild<Entity1,int>(5);
            Entity1id = entity1.ID;
            CreateComponent createComponent =  EnitityHouse.Instance.GetScene<BattleGroudScene>().AddComponent<CreateComponent, int>(1);
            for (int i = 0; i < 2; i++)
            {
                createComponent.AddChild<Entity1,int>(5);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            AsynTest();
            EventManager.Instance.Send<EventTest, int, int>(1, 2);
            SceneEntityFactory.RemoveScene<BattleGroudScene>();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            EnitityHouse.Instance.GetScene<BattleGroudScene>().RemoveChild(Entity1id);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            EnitityHouse.Instance.GetScene<BattleGroudScene>().AddComponent<Bttleground>();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            EnitityHouse.Instance.GetScene<BattleGroudScene>().RemoveComponent<Bttleground>();
        }
    }

    public async UniTask AsynTest()
    {
        await EventManager.Instance.SendAsyn<EventTestAsyn,string>("你好");
        Debug.Log("時間等待結束");
    }
}