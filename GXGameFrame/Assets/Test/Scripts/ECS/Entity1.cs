using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Entitas;
using GameFrame;
using UnityEngine;
using Entity = GameFrame.Entity;

#region 演示传统对象形式

public class Entity1 : Entity, IInit, IDestroy, IUpdate
{
    public override void Initialize()
    {
        this.AddSystem<Entity1System.Entity1InitSystem>();
        this.AddSystem<Entity1System.Entity1DestroySystem>();
        this.AddSystem<Entity1System.Entity1UpdateSystem>();
    }
    
    public Eniti1View view;

    public int a;

    public string b;

    private Vector2 c;
    public Vector2 C
    {
        set
        {
            c = value;
            view.Position(c);
        }
        get => c;
    }
}

public static class Entity1System
{
    public class Entity1InitSystem : InitSystem<Entity1>
    {
        protected override void Init(Entity1 self)
        {
            Eniti1View eniti1View = ReferencePool.Acquire<Eniti1View>();
            eniti1View.Link(self, "Cube");
            self.view = eniti1View;
        }
    }

    public class Entity1UpdateSystem : UpdateSystem<Entity1>
    {
        protected override void Update(Entity1 self)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                self.C +=  Vector2.one * Time.deltaTime;
            }
        }
    }

    public class Entity1DestroySystem : DestroySystem<Entity1>
    {
        protected override void Destroy(Entity1 self)
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

#endregion