using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrame;
using UnityEngine;

#region 演示Ecs形式

public class Bttleground : Context
{
    public override void Initialize()
    {
        base.Initialize();
        ObjectPoolManager.Instance.GetObjectPool<GameObjectObjectBase>("Gameobject");
        this.AddSystem<CreateMonsterSystem>();
        this.AddSystem<MoveSystem>();
        this.AddSystem<ViewSystem>();
        this.AddSystem<InputSystem>();
        this.AddSystem<DestroySystem>();
    }

    public override void Clear()
    {
        base.Clear();
        ObjectPoolManager.Instance.DeleteObjectPool<GameObjectObjectBase>("Gameobject");
    }
}


public class CreateMonsterSystem : IECSStartSystem
{
    public void Start(Context entity)
    {
        var xx = entity.AddChild<Monster>();
        xx.SetPos(Vector2.one).SetRotate(new Vector2(30, 0));
    }

    public void Clear()
    {
    }
}

public class ViewSystem : ReactiveSystem
{
    public override void Start(Context entity)
    {
        base.Start(entity);
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
        gv.Link(ecsentity, ecsentity.GetAsset().Path);
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
            entity.SetPos(pos.vec + entity.GetInputVec().vec);
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

    public override void Start(Context entity)
    {
        context = entity;
        base.Start(entity);
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
        foreach (var entitie in entities)
        {
            if (Input.GetKey(KeyCode.W))
            {
                entitie.SetInputVec(Vector2.one * Time.deltaTime * 2);
            }
            else
            {
                entitie.SetInputVec(Vector2.zero);
                ;
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
    public override void Initialize()
    {
        this.AddPos();
        this.AddRotate();
        this.AddAsset("Cube");
        this.AddInputVec();
    }
}

[ViewBind]
public class Pos : IECSComponent
{
    public Vector2 vec;

    public void Clear()
    {
    }
}

[ViewBind]
public class Rotate : IECSComponent
{
    public Vector2 vec;

    public void Clear()
    {
    }
}

public class InputVec : IECSComponent
{
    public Vector2 vec;

    public void Clear()
    {
    }
}


public class Asset : IECSComponent
{
    public string Path;

    public void Clear()
    {
    }
}

//---------------------------------------------------------------------

public class GameObjectObjectBase : ObjectBase
{
    private GameObject Obj;
    private Transform Trans;
    private Vector3 CurPos;
    private Quaternion CurRot;
    private string LoadPath;

    public Vector3 Pos
    {
        get
        {
            if (Trans != null)
            {
                return Trans.position;
            }

            return Vector3.zero;
        }
        set
        {
            CurPos = value;
            if (Trans != null)
            {
                Trans.position = value;
            }
        }
    }

    public Quaternion Rot
    {
        get
        {
            if (Trans != null)
            {
                return Trans.rotation;
            }

            return Quaternion.identity;
        }
        set
        {
            CurRot = value;
            if (Trans != null)
            {
                Trans.rotation = value;
            }
        }
    }


    internal override void Initialize(object initObject)
    {
        base.Initialize(initObject);
        Load(m_InitData as string);
        Debug.Log("初始化");
    }

    private async UniTask Load(string path)
    {
        LoadPath = path;
        var go = await AssetManager.Instance.LoadAsyncTask<GameObject>(path);
        if (go == null)
        {
            return;
        }

        Obj = GameObject.Instantiate(go);
        Trans = Obj.transform;
        Pos = CurPos;
        Rot = CurRot;
    }

    /// <summary>
    /// 获取对象时的事件。
    /// </summary>
    internal override void OnSpawn()
    {
        if (Obj != null)
        {
            Obj.SetActive(true);
        }

        Debug.Log("创建");
    }

    /// <summary>
    /// 回收对象时的事件。
    /// </summary>
    internal override void OnUnspawn()
    {
        CurPos = Vector3.zero;
        CurRot = Quaternion.identity;
        if (Obj != null)
        {
            Obj.SetActive(false);
        }

        Debug.Log("回收");
    }

    /// <summary>
    /// 清理对象基类。
    /// </summary>
    public override void Clear()
    {
        CurPos = Vector3.zero;
        CurRot = Quaternion.identity;
        AssetManager.Instance.UnLoad(LoadPath);
        GameObject.Destroy(Obj);
        Debug.Log("清理");
    }
}

public class GameObjectView : IEceView
{
    private ECSEntity m_BindEntity;
    private string m_LoadPath;
    private ObjectPool<GameObjectObjectBase> m_ObjectPool;
    private GameObjectObjectBase m_GameObject;
    private EntityComponentNumericalChange<Pos> m_PosDelegate;
    private EntityComponentNumericalChange<Rotate> m_RotDelegate;

    public void Link(ECSEntity ecsEntity, string path)
    {
        m_BindEntity = ecsEntity;
        Init(path);
    }


    public void Init(string path)
    {
        if (m_ObjectPool == null)
        {
            m_ObjectPool = ObjectPoolManager.Instance.CreateObjectPool<GameObjectObjectBase>("Gameobject", 10, path);
        }

        m_GameObject = m_ObjectPool.Spawn();
        m_LoadPath = path;
        Position(m_BindEntity.GetPos(), m_BindEntity);
        Rotate(m_BindEntity.GetRotate(), m_BindEntity);
        m_PosDelegate = Position;
        m_RotDelegate = Rotate;
        ViewBindEventClass.PosEntityComponentNumericalChange -= m_PosDelegate;
        ViewBindEventClass.PosEntityComponentNumericalChange += m_PosDelegate;
        ViewBindEventClass.RotateEntityComponentNumericalChange -= m_RotDelegate;
        ViewBindEventClass.RotateEntityComponentNumericalChange += m_RotDelegate;
    }

    private void Position(Pos pos, ECSEntity ecsEntity)
    {
        if (m_BindEntity.ID != ecsEntity.ID)
            return;
        m_GameObject.Pos = pos.vec;
    }

    private void Rotate(Rotate pos, ECSEntity ecsEntity)
    {
        if (m_BindEntity.ID != ecsEntity.ID)
            return;
        m_GameObject.Rot = Quaternion.Euler(pos.vec);
    }

    public void Clear()
    {
        ViewBindEventClass.PosEntityComponentNumericalChange -= m_PosDelegate;
        ViewBindEventClass.RotateEntityComponentNumericalChange -= m_RotDelegate;
        m_ObjectPool.UnSpawn(m_GameObject);
        m_PosDelegate = null;
        m_RotDelegate = null;
        m_ObjectPool = null;
        m_BindEntity = null;
    }
}

#endregion