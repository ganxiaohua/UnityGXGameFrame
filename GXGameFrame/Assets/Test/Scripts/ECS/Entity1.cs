using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame;
using GameFrame.Timer;
using UnityEngine;
using Entity = GameFrame.Entity;

#region 演示传统对象形式

public class Entity1 : Entity, IStart, IClear, IUpdate
{
    public Eniti1View view;

    private Vector2 c;

    public Vector2 C
    {
        set
        {
            c = value;
            view?.Position(c);
        }
        get => c;
    }
}

public static class Entity1System
{
    [SystemBind]
    public class Entity1StartSystem : StartSystem<Entity1, int>
    {
        protected override void Start(Entity1 self, int p1)
        {
            Debug.Log("参数传递:" + p1);
            Eniti1View eniti1View = ReferencePool.Acquire<Eniti1View>();
            eniti1View.Link(self, "Cube");
            self.view = eniti1View;
        }
    }
    [SystemBind]
    public class Entity1UpdateSystem : UpdateSystem<Entity1>
    {
        protected override void Update(Entity1 self, float elapseSeconds, float realElapseSeconds)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                self.C += Vector2.one * Time.deltaTime;
            }
        }
    }

    
    [SystemBind]
    public class Entity1ClearSystem : ClearSystem<Entity1>
    {
        protected override void Clear(Entity1 self)
        {
            Debug.Log("Entity1 destroy");
            ReferencePool.Release(self.view);
        }
    }
}


public class Eniti1View : IView
{
    private Entity Entity;
    private GameObject Obj;
    private Transform Trans;

    public void Link(Entity entity, string path)
    {
        Entity = entity;
        Load(path);
    }

    public async UniTask Load(string path)
    {
        var go = await AssetManager.Instance.LoadAsyncTask<GameObject>(path);
        Obj = GameObject.Instantiate(go);
        Trans = Obj.transform;
    }

    public void Position(Vector2 vector2)
    {
        Trans.position = vector2;
    }


    public void Clear()
    {
        GameObject.Destroy(Obj);
        Entity = null;
    }
}

// --------------------------------------------------------------------
public class CreateComponent : Entity, IStart, IClear
{
    public int cank = 0;
    public int cank2 = 0;
}

public static class CreateComponentSystem
{
    [SystemBind]
    public class CreateComponentStartSystem : StartSystem<CreateComponent, int>
    {
        protected override void Start(CreateComponent self, int p1)
        {
            self.cank = p1;
            Debug.Log("CreateComponent Start" + p1);
        }
    }

    [SystemBind]
    public class CreateComponentClearSystem : ClearSystem<CreateComponent>
    {
        protected override void Clear(CreateComponent self)
        {
            Debug.Log("CreateComponent clear");
        }
    }
}


// ----------------------事件系统测试-------------------------------------
public class EventTest : IMessenger<int, int>
{
    public void Send(int p1, int p2)
    {
        List<CreateComponent> list = EnitityHouse.Instance.GetEntity<CreateComponent>();
        CreateComponent createComponent = EnitityHouse.Instance.GetScene<GXGame.BattleGroudScene>().GetComponent<CreateComponent>();
        if (createComponent == null)
        {
            return;
        }

        Debug.Log($"接收到事件*EventTest {p1}{p2}");
    }

    public void Clear()
    {
    }
}

public class EventTestAsyn : IMessengerAsyn<string>
{
    public async UniTask Send(string p1)
    {
        Debug.Log(p1);
        await TimerComponent.Instance.OnceTimerAsync(1000, null);
    }

    public void Clear()
    {
    }
}

#endregion