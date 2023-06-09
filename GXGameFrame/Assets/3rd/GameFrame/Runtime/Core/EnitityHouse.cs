using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Unity.VisualScripting;

namespace GameFrame
{
    using DDictionaryETC = DDictionary<Type, Type, SystemObject>;

    public class EnitityHouse : Singleton<EnitityHouse>
    {
        /// <summary>
        /// 不会包含EcsEntity的实体集合
        /// </summary>
        private Dictionary<Type, List<IEntity>> m_TypeWithEntitys = new();

        private Dictionary<SceneType, IScene> m_EverySceneEntity = new(4);

        private DDictionaryETC m_EntitySystems = new();

        private UpdateSystems m_UpdateSystems = new UpdateSystems();

        private List<IEntity> m_HideEnitiys = new(16);

        public List<IEntity> HideEnitiys => m_HideEnitiys;

        public void Init()
        {
        }

        /// <summary>
        /// 增加实体 所有的实体都会加入到这里
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="Exception"></exception>
        public void AddEntity(IEntity entity)
        {
            Type type = entity.GetType();
            if (!m_TypeWithEntitys.TryGetValue(type, out var entityList))
            {
                entityList = new List<IEntity>();
                m_TypeWithEntitys.Add(type, entityList);
            }

            if (entityList.Contains(entity))
            {
                throw new Exception($"TypeWithEntitys have enitiy:{entity.ID}");
            }

            entityList.Add(entity);
        }

        public List<T> GetEntity<T>() where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var entityList = GetEntity(type);
            return entityList as List<T>;
        }

        public List<IEntity> GetEntity(Type type)
        {
            if (!m_TypeWithEntitys.TryGetValue(type, out var entityList))
            {
                throw new Exception($"TypeWithEntitys not have {type}");
            }

            return entityList;
        }


        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveEntity(IEntity entity)
        {
            Type type = entity.GetType();
            if (!m_TypeWithEntitys.TryGetValue(type, out var entityList) || !entityList.Contains(entity))
            {
                throw new Exception($"TypeWithEntitys not have enitiy:{entity.ID}");
            }

            Type entityType = entity.GetType();
            var allSystemDic = m_EntitySystems.GetValue(entityType);
            if (allSystemDic != null)
            {
                allSystemDic.SystemClear(entity);
            }

            RemoveAllSystem(entity);
            entityList.Remove(entity);
        }

        /// <summary>
        /// 获得此类型的实体的数量
        /// </summary>
        public int GetEntityCount(Type entityType)
        {
            if (m_TypeWithEntitys.TryGetValue(entityType, out var entityList))
            {
                return entityList.Count;
            }

            return 0;
        }


        /// <summary>
        /// 加入scene
        /// </summary>
        /// <param name="sceneType"></param>
        /// <typeparam name="T"></typeparam>
        public IScene AddSceneEntity<T>(SceneType sceneType) where T : class, IEntity, IScene, new()
        {
            if (m_EverySceneEntity.TryGetValue(sceneType, out IScene IScene))
            {
                Type type = typeof(T);
                if (IScene.GetType() == type)
                {
                    throw new Exception($"EverySceneEntity  have enitiy:{type.Name}");
                }

                RemoveSceneEntity(sceneType);
            }

            T scene = GXGameFrame.Instance.MainScene.AddComponent<T>();
            m_EverySceneEntity.Add(sceneType, scene);
            return scene;
        }

        /// <summary>
        /// 获得scene
        /// </summary>
        /// <param name="sceneType"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IScene GetScene(SceneType sceneType)
        {
            if (!m_EverySceneEntity.TryGetValue(sceneType, out IScene IScene))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{sceneType}");
            }

            return IScene;
        }


        /// <summary>
        /// 清除scene
        /// </summary>
        /// <param name="sceneEntity"></param>
        /// <exception cref="Exception"></exception>
        public void RemoveSceneEntity(SceneType sceneType)
        {
            if (!m_EverySceneEntity.TryGetValue(sceneType, out IScene scene))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{sceneType}");
            }

            GXGameFrame.Instance.MainScene.RemoveComponent(scene.GetType());
            m_EverySceneEntity.Remove(sceneType);
        }

        public void AddSystem<T>(IEntity entity) where T : ISystem
        {
            AddSystem(entity, typeof(T));
        }

        /// <summary>
        /// 创建获得得到一个SystemObject
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="system"> 系统类</param>
        /// <returns></returns>
        private SystemObject CreateSystem(IEntity entity, Type system)
        {
            Type entityType = entity.GetType();
            SystemObject sysObject = m_EntitySystems.GetVValue(entityType, system);
            if (sysObject == null)
            {
                sysObject = ReferencePool.Acquire<SystemObject>();
                Type entitySystem = AutoBindSystem.Instance.GetEnitiySystem(entityType, system);
                if (entitySystem == null)
                {
                    entitySystem = system;
                }

                sysObject.AddSystem(entitySystem);
                m_EntitySystems.Add(entityType, system, sysObject);
            }

            return sysObject;
        }

        public void AddSystem(IEntity entity, Type type)
        {
            Type entityType = entity.GetType();
            Type systemType = AutoBindSystem.Instance.GetIsystem(entityType, type);
            if (systemType == null)
            {
                systemType = type;
            }

            SystemObject sysObject = CreateSystem(entity, systemType);
            sysObject.System.SystemStart(entity);
            m_UpdateSystems.AddUpdateSystem(entity, sysObject);
        }

        public void AddStartSystem<P1>(IEntity entity, P1 p1)
        {
            Type entityType = entity.GetType();
            Type systemType = typeof(IStartSystem<P1>);
            if (AutoBindSystem.Instance.GetEnitiySystem(entityType, systemType) == default(Type))
            {
                throw new Exception($"not have IStartSystem<P1>");
            }

            SystemObject sysObject = CreateSystem(entity, systemType);
            sysObject.System.SystemStart(entity, p1);
        }

        public void AddStartSystem<P1, P2>(IEntity entity, P1 p1, P2 p2)
        {
            Type entityType = entity.GetType();
            Type systemType = typeof(IStartSystem<P1, P2>);
            if (AutoBindSystem.Instance.GetEnitiySystem(entityType, systemType) == default(Type))
            {
                throw new Exception($"not have IStartSystem<P1>");
            }

            SystemObject sysObject = CreateSystem(entity, systemType);
            sysObject.System.SystemStart(entity, p1, p2);
        }

        // public void AddShowSystem<P1>(IEntity entity, Type type, P1 p1)
        // {
        //     Type systemType = AutoBindSystem.Instance.GetIsystem(typeof(Entity), type);
        //     SystemObject sysObject = CreateSystem(entity, systemType);
        //     sysObject.System.SystemShow(entity, p1);
        // }
        //
        // public void AddShowSystem<P1, P2>(IEntity entity, Type type, P1 p1, P2 p2)
        // {
        //     Type systemType = AutoBindSystem.Instance.GetIsystem(typeof(Entity), type);
        //     SystemObject sysObject = CreateSystem(entity, systemType);
        //     sysObject.System.SystemShow(entity, p1, p2);
        // }

        /// <summary>
        /// 删除一个updatesystem
        /// </summary>
        /// <param name="entity"></param>
        private void RemoveUpdateSystem(IEntity entity, ISystem system = null)
        {
            m_UpdateSystems.RemoveUpdateSystem(entity, system);
        }

        /// <summary>
        /// 运行PreShowsystem
        /// </summary>
        /// <param name="entity"></param>
        public void RunPreShowSystem(IEntity entity, bool IsFirstShow)
        {
            SystemObject sysObject = m_EntitySystems.GetVValue(entity.GetType(), typeof(IPreShowSystem));
            sysObject?.System.SystemPreShow(entity, IsFirstShow);
        }

        /// <summary>
        /// 运行showsystem
        /// </summary>
        /// <param name="entity"></param>
        public void RunShowSystem(Entity entity)
        {
            if (m_HideEnitiys.Contains(entity))
            {
                m_HideEnitiys.Remove(entity);
            }

            RunShow(entity);
        }

        public void RunShow(Entity entity)
        {
            SystemObject sysObject = m_EntitySystems.GetVValue(entity.GetType(), typeof(IShowSystem));
            sysObject?.System.SystemShow(entity);

            if (!m_UpdateSystems.HasUpdateSystem(entity))
            {
                SystemObject updateSystem = m_EntitySystems.GetVValue(entity.GetType(), typeof(IUpdateSystem));
                if (updateSystem != null)
                {
                    m_UpdateSystems.AddUpdateSystem(entity, updateSystem);
                }
            }

            foreach (KeyValuePair<int, IEntity> enitiy in entity.Children)
            {
                RunShow((Entity) enitiy.Value);
            }

            foreach (KeyValuePair<Type, IEntity> enitiy in entity.Components)
            {
                RunShow((Entity) enitiy.Value);
            }
        }

        /// <summary>
        /// 运行隐藏system
        /// </summary>
        /// <param name="entity"></param>
        public void RunHideSystem(Entity entity)
        {
            SystemObject sysObject = m_EntitySystems.GetVValue(entity.GetType(), typeof(IHideSystem));
            if (sysObject == null)
            {
                Debugger.LogWarning($"{entity.GetType()} not have hidesystem");
                return;
            }

            if (m_HideEnitiys.Contains(entity))
            {
                Debugger.LogWarning($"{entity.GetType()}  already hide");
                return;
            }

            m_HideEnitiys.Add(entity);
            RunHide(entity);
        }

        public void RunHide(Entity entity)
        {
            SystemObject sysObject = m_EntitySystems.GetVValue(entity.GetType(), typeof(IHideSystem));
            if (sysObject != null)
            {
                RemoveUpdateSystem(entity);
                sysObject.System.SystemHide(entity);
            }

            foreach (KeyValuePair<int, IEntity> enitiy in entity.Children)
            {
                RunHide((Entity) enitiy.Value);
            }

            foreach (KeyValuePair<Type, IEntity> enitiy in entity.Components)
            {
                RunHide((Entity) enitiy.Value);
            }
        }

        /// <summary>
        /// 删除实体身上的某一个系统
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveSystem<T>(IEntity entity)
        {
            Type type = typeof(T);
            Type entityType = entity.GetType();
            SystemObject systemObject = m_EntitySystems.GetVValue(entityType, type);
            if (systemObject != null)
            {
                RemoveUpdateSystem(entity, systemObject.System);
            }
        }

        /// <summary>
        /// 有实体进行销毁
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveAllSystem(IEntity entity)
        {
            Type entityType = entity.GetType();
            var allSystemDic = m_EntitySystems.GetValue(entityType);
            if (allSystemDic == null)
            {
                //这个实体上不存在系统
                return;
            }
            RemoveUpdateSystem(entity);
            if (GetEntityCount(entityType) == 0)
            {
                var item = m_EntitySystems.RemoveTkey(entityType);
                foreach (KeyValuePair<Type, SystemObject> typeSystem in item)
                {
                    ReferencePool.Release(typeSystem.Value);
                }
            }
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_UpdateSystems.UpdateSystemEnitiys[(int)UpdateType.Update].SystemUpdate(elapseSeconds, realElapseSeconds);
        }

        public void LateUpdate(float elapseSeconds, float realElapseSeconds)
        {
            m_UpdateSystems.UpdateSystemEnitiys[(int)UpdateType.LateUpdate].SystemLateUpdate(elapseSeconds, realElapseSeconds);
        }
        
        public void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            m_UpdateSystems.UpdateSystemEnitiys[(int)UpdateType.FixedUpdate].SystemFixedUpdate(elapseSeconds, realElapseSeconds);
        }
        

        public void Disable()
        {
        }
    }
}