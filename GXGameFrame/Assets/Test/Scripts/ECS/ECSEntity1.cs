using System;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

#region 演示Ecs形式

public class Bttleground : Context
{
    public override void InitializeSystem()
    {
        this.AddSystem<CreateMonsterSystem>();
        this.AddSystem<MoveSystem>();
    }
}

public class CreateMonsterSystem : IECSInitSystem
{
    public void Initialize(Context entity)
    {
        entity.AddChild<Monster>();
        entity.AddChild<Monster>();
        entity.AddChild<Monster>();
        entity.AddChild<Wood>();
    }

    public void Clear()
    {
    }
}

public class MoveSystem : ReactiveSystem
{
    public override void Initialize(Context entity)
    {
        base.Initialize(entity);
    }

    protected override Collector GetTrigger(Context context) => Collector.CreateCollector(context, typeof(Pos), typeof(Rotate));
    protected override bool Filter(ECSEntity entity)
    {
        return true;
    }

    protected override void Update(List<ECSEntity> entities)
    {
        foreach (var entity in entities)
        { 
            Debug.Log("xxxxxxxxxxxx");
            var pos = entity.GetPos();
        }
    }

    public override void Clear()
    {
    }
}

// -------------------------------------------------
public class Monster : ECSEntity
{
    public override void InitComponent()
    {
        this.AddPos(Vector2.down);
        this.AddRotate(Vector2.left);
    }
}

public class Wood: ECSEntity
{
    public override void InitComponent()
    {
        this.AddPos(Vector2.down);
    }
}

public class Pos : Entity
{
    public Vector3 vec;
}

public class Rotate : Entity
{
    public Vector3 vec;
}

/// <summary>
/// 自动生成
/// </summary>
public static class ManInnt
{
    public static void AddPos(this ECSEntity ecsEntity, Vector2 vector2)
    {
        Pos p = ecsEntity.AddComponent<Pos>();
        p.vec = vector2;
    }
    
    public static Pos GetPos(this ECSEntity ecsEntity)
    {
        Pos p = ecsEntity.GetComponent<Pos>();
        return p;
    }
    
    public static void AddRotate(this ECSEntity ecsEntity, Vector2 vector2)
    {
        Rotate p = ecsEntity.AddComponent<Rotate>();
        p.vec = vector2;
    }
    
    public static Rotate GetRotate(this ECSEntity ecsEntity)
    {
        Rotate p = ecsEntity.GetComponent<Rotate>();
        return p;
    }
}

#endregion