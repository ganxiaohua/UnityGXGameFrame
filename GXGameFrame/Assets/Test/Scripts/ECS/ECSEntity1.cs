using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;
using Timer = System.Threading.Timer;

#region 演示Ecs形式

//TODO:代码自动生成
public class Bttleground : Context
{
    public override void InitializeSystem()
    {
        this.AddSystem<CreateMonsterSystem>();
        this.AddSystem<MoveSystem>();
        this.AddSystem<ViewSystem>();
        this.AddSystem<InputSystem>();
        this.AddSystem<DestroySystem>();
    }
}



public class CreateMonsterSystem : IECSInitSystem
{
    public void Initialize(Context entity)
    {
        entity.AddChild<Monster>().SetPos(Vector2.one).SetRotate(new Vector2(30, 0));
    }

    public void Clear()
    {
    }
}

public class ViewSystem : ReactiveSystem
{
    
    public override void Initialize(Context entity)
    {
        base.Initialize(entity);
    }

    protected override Collector GetTrigger(Context context) => Collector.CreateCollector(context, typeof(Asset));

    protected override bool Filter(ECSEntity entity)
    {
        if (entity.GetView() == null)
            return true;
        return false;
    }

    protected override void Update(List<ECSEntity> entities)
    {
        foreach (var entity in entities)
        {
            LoadAsset(entity);
        }
    }

    public async UniTask LoadAsset(ECSEntity ecsentity)
    {
        ecsentity.AddView();
        GameObjectView gv = ReferencePool.Acquire<GameObjectView>();
        gv.Link(ecsentity, ecsentity.GetAsset());
        ecsentity.SetView(gv);
    }

    public override void Clear()
    {
        
    }
}

public class MoveSystem : ReactiveSystem
{
    protected override Collector GetTrigger(Context context) => Collector.CreateCollector(context, typeof(Pos), typeof(InputVec));

    protected override bool Filter(ECSEntity entity)
    {
        return true;
    }

    protected override void Update(List<ECSEntity> entities)
    {
        foreach (var entity in entities)
        {
            var pos = entity.GetPos();
            entity.SetPos(pos.vec+entity.GetInputVec().vec);
            if (pos.vec.x > 5)
            {
                entity.AddDestroy();
            }
        }
    }

    public override void Clear()
    {
    }
}

public class DestroySystem : ReactiveSystem
{
    private Context context;

    public override void Initialize(Context entity)
    {
        context = entity;
        base.Initialize(entity);
    }

    protected override Collector GetTrigger(Context context) => Collector.CreateCollector(context, typeof(Destroy));

    protected override bool Filter(ECSEntity entity)
    {
        return true;
    }

    protected override void Update(List<ECSEntity> entities)
    {
        foreach (var item in entities)
        {
            context.RemoveChild(item.ID);
        }
    }

    public override void Clear()
    {
        context = null;
    }
}

public class InputSystem : ReactiveSystem
{
    protected override Collector GetTrigger(Context context) => Collector.CreateCollector(context, typeof(InputVec));

    protected override bool Filter(ECSEntity entity)
    {
        return true;
    }

    protected override void Update(List<ECSEntity> entities)
    {
        if (Input.GetKey(KeyCode.W))
        {
            foreach (var entitie in entities)
            {
                var vect = entitie.GetInputVec();
                entitie.SetInputVec(Vector2.one * Time.deltaTime * 2);
            }
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
        this.AddPos();
        this.AddRotate();
        this.AddAsset("Cube");
        this.AddInputVec();
    }
}

// [evet]
public class Pos : Entity
{
    public Vector2 vec;
}


public class InputVec : Entity
{
    public Vector2 vec;
}

// [evet]
public class Rotate : Entity
{
    public Vector2 vec;
}


public class Asset : Entity
{
    public string Path;
}

//---------------------------------------------------------------------
public class GameObjectView : IEceView
{
    private ECSEntity Entity;
    private GameObject Obj;
    private Transform Trans;

    public void Link(ECSEntity ecsEntity, string path)
    {
        Entity = ecsEntity;
        Load(path);
    }

    public async UniTask Load(string path)
    {
        var go = await AssetManager.Instance.LoadAsyncTask<GameObject>(path);
        Obj = GameObject.Instantiate(go);
        Trans = Obj.transform;
        Position(Entity.GetPos(), Entity);
        Rotate(Entity.GetRotate(), Entity);
        Event.PosEntityComponentNumericalChange += Position;
        Event.RotateEntityComponentNumericalChange += Rotate;
    }

    private void Position(Pos pos, ECSEntity ecsEntity)
    {
        if (Entity != ecsEntity)
            return;
        Trans.position = pos.vec;
    }

    private void Rotate(Rotate pos, ECSEntity ecsEntity)
    {
        if (Entity != ecsEntity)
            return;
        Trans.rotation = Quaternion.Euler(pos.vec);
    }

    public void Clear()
    {
        //对象池操作可以是
        Event.PosEntityComponentNumericalChange -= Position;
        Event.RotateEntityComponentNumericalChange -= Rotate;
        GameObject.Destroy(Obj);
        Entity = null;
    }
}


/// <summary>
/// 自动生成
/// </summary>
public static class Event
{
    public static EntityComponentNumericalChange<Pos> PosEntityComponentNumericalChange;
    public static EntityComponentNumericalChange<Rotate> RotateEntityComponentNumericalChange;
}

public static class ManInnt
{
    public static Asset AddAsset(this ECSEntity ecsEntity, string path)
    {
        Asset p = ecsEntity.AddComponent<Asset>();
        p.Path = path;
        return p;
    }

    public static string GetAsset(this ECSEntity ecsEntity)
    {
        Asset p = ecsEntity.GetComponent<Asset>();
        return p.Path;
    }


    public static void AddPos(this ECSEntity ecsEntity, Vector2 vector2)
    {
        Pos p = ecsEntity.AddComponent<Pos>();
        p.vec = vector2;
    }

    public static void AddPos(this ECSEntity ecsEntity)
    {
        Pos p = ecsEntity.AddComponent<Pos>();
    }

    public static Pos GetPos(this ECSEntity ecsEntity)
    {
        Pos p = ecsEntity.GetComponent<Pos>();
        return p;
    }


    public static ECSEntity SetPos(this ECSEntity ecsEntity, Vector2 vector2)
    {
        var pos = ecsEntity.GetPos();
        pos.vec = vector2;
        if (Event.PosEntityComponentNumericalChange != null)
        {
            Event.PosEntityComponentNumericalChange(pos, ecsEntity);
        }
        return ecsEntity;
    }

    public static void AddRotate(this ECSEntity ecsEntity, Vector2 vector2)
    {
        Rotate p = ecsEntity.AddComponent<Rotate>();
        p.vec = vector2;
    }

    public static void AddRotate(this ECSEntity ecsEntity)
    {
        Rotate p = ecsEntity.AddComponent<Rotate>();
    }

    public static Rotate SetRotate(this ECSEntity ecsEntity, Vector2 vector2)
    {
        Rotate p = ecsEntity.GetRotate();
        p.vec = vector2;
        if (Event.RotateEntityComponentNumericalChange != null)
        {
            Event.RotateEntityComponentNumericalChange(p, ecsEntity);
        }

        return p;
    }

    public static Rotate GetRotate(this ECSEntity ecsEntity)
    {
        Rotate p = ecsEntity.GetComponent<Rotate>();
        return p;
    }


    public static void AddInputVec(this ECSEntity ecsEntity)
    {
        InputVec p = ecsEntity.AddComponent<InputVec>();
    }


    public static InputVec GetInputVec(this ECSEntity ecsEntity)
    {
        InputVec p = ecsEntity.GetComponent<InputVec>();
        return p;
    }

    public static InputVec SetInputVec(this ECSEntity ecsEntity, Vector2 vect)
    {
        InputVec p = ecsEntity.GetComponent<InputVec>();
        p.vec = vect;
        return p;
    }
}

#endregion