using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame;
using UnityEngine;
using Entity = GameFrame.Entity;

#region 演示传统对象形式

public class Entity1 : Entity, IStart, IClear, IUpdate
{
    public override void Initialize()
    {
        this.AddSystem<Entity1System.Entity1StartSystem>();
        this.AddSystem<Entity1System.Entity1DestroySystem>();
        this.AddSystem<Entity1System.Entity1UpdateSystem>();
    }

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
    public class Entity1StartSystem : StartSystem<Entity1>
    {
        protected override void Start(Entity1 self)
        {
            Eniti1View eniti1View = ReferencePool.Acquire<Eniti1View>();
            eniti1View.Link(self, "Cube");
            self.view = eniti1View;
        }
        
    }

    public class Entity1UpdateSystem : UpdateSystem<Entity1>
    {
        protected override void Update(Entity1 self,float elapseSeconds, float realElapseSeconds)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                self.C += Vector2.one * Time.deltaTime;
            }
        }
    }

    public class Entity1DestroySystem : ClearSystem<Entity1>
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
public class CreateComponent : Entity, IStart,IClear
{
    public override void Initialize()
    {
        var x = (ParameterP1<int>) Parameter;
        cank = x.Param1;
        this.AddSystem<ComponentSystem.CreateComponentStartSystem>();
        this.AddSystem<ComponentSystem.CreateComponentClearSystem>();
    }
    
    public int cank = 0;
    public int cank2 = 0;
}

public static class ComponentSystem
{
    public class CreateComponentStartSystem : StartSystem<CreateComponent>
    {
        protected override void Start(CreateComponent self)
        {
            Debug.Log("CreateComponent Start");
        }
    }
    
    public class CreateComponentClearSystem : ClearSystem<CreateComponent>
    {
        protected override void Clear(CreateComponent self)
        {
            Debug.Log("CreateComponent clear");
        }
    }
}


// ----------------------事件系统测试-------------------------------------
public class EventTest : IMessenger
{
    private int A;
    private int B;

    public int CA
    {
        get { return A; }
    }

    public int CB
    {
        get { return B; }
    }

    private EnitityEvnetDo enitityEvnetDo;

    public void Init(int a, int b)
    {
        A = a;
        B = b;
    }

    public void Send()
    {
        enitityEvnetDo = ReferencePool.Acquire<EnitityEvnetDo>();
        enitityEvnetDo.Do(this);
    }

    public void Clear()
    {
        ReferencePool.Release(enitityEvnetDo);
    }
}

public class EnitityEvnetDo : IAddressee
{
    public void Do(IMessenger messenger)
    {
        EventTest eventTest = messenger as EventTest;
        CreateComponent createComponent = EnitityHouse.Instance.GetScene<BattlegroundScene>().GetComponent<CreateComponent>();
        if (createComponent == null)
        {
            return;
        }

        createComponent.cank = eventTest.CA;
        Debug.Log("接受到了消息");
    }

    public void Clear()
    {
    }
}

#endregion