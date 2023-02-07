using System.Collections;
using System.Collections.Generic;
using Entitas;
using GameFrame;
using UnityEngine;
using Entity = GameFrame.Entity;

#region 演示传统对象形式

public class Entity1 : Entity, IInit,IDestroy,IUpdate
{
    public override void InitializeSystem()
    {
        this.AddSystem<Entity1System.Entity1InitSystem>();
        this.AddSystem<Entity1System.Entity1DestroySystem>();
        this.AddSystem<Entity1System.Entity1UpdateSystem>();
    }

    public int a;

    public string b;

    public Vector2 c;
}

public static class Entity1System
{
    public class Entity1InitSystem : InitSystem<Entity1>
    {
        protected override void Init(Entity1 self)
        {
            self.Entity1Func1();
            Debug.Log("Entity1 init"+self.a);
        }
    }
    
    public class Entity1UpdateSystem :UpdateSystem<Entity1>
    {
        protected override void Update(Entity1 self)
        {
           Debug.Log("Entity1 Update");
        }
    }
    
    public class Entity1DestroySystem : DestroySystem<Entity1>
    {

        protected override void Destroy(Entity1 self)
        {
            Debug.Log("Entity1 destroy");
            self.a = 0;
            self.b = default(string);
            self.c = default(Vector2);
        }
    }

    public static void Entity1Func1(this Entity1 self)
    {
        self.a += 10;
    }
}

#endregion
