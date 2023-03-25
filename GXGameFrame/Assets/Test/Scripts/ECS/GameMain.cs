using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;


class BattlegroundScene : IScene
{
    public void Start(SceneEntity sceneEntity)
    {
    }

    public void Update(float elapseSeconds, float realElapseSeconds)
    {
    }

    public void Clear()
    {
        Debug.Log("BattlegroundScene clear");
    }
}

public class GameMain : MonoBehaviour
{
    // Start is called before the first frame updat

    void Start()
    {
        GXGameFrame.Instance.Start();
        SceneEntityFactory.CreateScene<BattlegroundScene>(GXGameFrame.Instance.MainScene);
    }

    // Update is called once per frame
    private int Entity1id;

    void Update()
    {
        GXGameFrame.Instance.Update();
        if (Input.GetKeyDown(KeyCode.A))
        {
            Entity1id = EnitityHouse.Instance.GetScene<BattlegroundScene>().AddChild<Entity1>().ID;
            EnitityHouse.Instance.GetScene<BattlegroundScene>().AddComponent<CreateComponent, int>(1);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneEntityFactory.RemoveScene<BattlegroundScene>();
            // EventTest text = EventManager.Instance.CreateEvent<EventTest>();
            // text.Init(10, 15);
            // EventManager.Instance.Send(text);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            EnitityHouse.Instance.GetScene<BattlegroundScene>().RemoveChild(Entity1id);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            EnitityHouse.Instance.GetScene<BattlegroundScene>().AddComponent<Bttleground>();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            EnitityHouse.Instance.GetScene<BattlegroundScene>().RemoveComponent<Bttleground>();
        }
    }
}