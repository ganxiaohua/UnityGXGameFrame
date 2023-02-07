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
    public  void Initialize(Context entity)
    {
        entity.AddChild<Monster>();
        entity.AddChild<Monster>();
        entity.AddChild<Monster>();
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
    protected override HashSet<int> GetTrigger(Context context)
    {
        //TODO进行筛选
        return null;
    }

    protected override void Update(List<ECSEntity> entities)
    {
       
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
        AddComponent<Pos>();
        AddComponent<Rotate>();
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



#endregion