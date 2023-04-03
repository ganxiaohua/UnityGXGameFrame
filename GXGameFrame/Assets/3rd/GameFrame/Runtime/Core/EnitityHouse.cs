using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace GameFrame
{
    using DDictionaryETC = DDictionary<Type, Type, SystemObject>;

    public class EnitityHouse : Singleton<EnitityHouse>
    {
        enum UpdateType : byte
        {
            Update = 0,
            LateUpdate,
            FixUpdate,
        }

        /// <summary>
        /// 不会包含EcsEntity
        /// </summary>
        private Dictionary<Type, List<IEntity>> m_TypeWithEntitys = new();

        private DoubleMap<Type, SceneEntity> m_EverySceneEntity = new();

        private DDictionaryETC m_EntitySystems = new();

        private UpdateSystems m_UpdateSystems = new UpdateSystems();

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
            if (!m_TypeWithEntitys.TryGetValue(typeof(T), out var entityList))
            {
                throw new Exception($"TypeWithEntitys not have {typeof(T)}");
            }

            return entityList as List<T>;
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


        public void AddSceneEntity<T>(SceneEntity entity) where T : IScene
        {
            Type type = typeof(T);
            if (m_EverySceneEntity.ContainsKey(type))
            {
                throw new Exception($"EverySceneEntity have enitiy:{entity.ID}");
            }

            m_EverySceneEntity.Add(type, entity);
        }

        public SceneEntity GetScene<T>() where T : IScene
        {
            Type type = typeof(T);
            if (!m_EverySceneEntity.TryGetValueKv(type, out SceneEntity sceneEntity))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{type}");
            }

            return sceneEntity;
        }

        /// <summary>
        /// 更具scene的类型删除scene
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        public void RemoveSceneEntity<T>() where T : IScene
        {
            Type type = typeof(T);
            if (!m_EverySceneEntity.TryGetValueKv(type, out SceneEntity sceneEntity))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{type}");
            }

            m_EverySceneEntity.RemoveByKey(type);
        }

        /// <summary>
        /// 更具本身删除scene
        /// </summary>
        /// <param name="sceneEntity"></param>
        /// <exception cref="Exception"></exception>
        public void RemoveSceneEntity(SceneEntity sceneEntity)
        {
            if (!m_EverySceneEntity.TryGetValueVk(sceneEntity, out Type type))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{type}");
            }

            m_EverySceneEntity.RemoveByValue(sceneEntity);
        }

        public Type GetParentType(Type t)
        {
            if (t.IsAssignableFrom(typeof(IStartSystem)))
            {
                t = typeof(IStartSystem);
            }
            else if (t.IsAssignableFrom(typeof(IUpdateSystem)))
            {
                t = typeof(IUpdateSystem);
            }
            else if (t.IsAssignableFrom(typeof(IClearSystem)))
            {
                t = typeof(IClearSystem);
            }
            else if (t.IsAssignableFrom(typeof(IShowSystem)))
            {
                t = typeof(IShowSystem);
            }
            else if (t.IsAssignableFrom(typeof(IHideSystem)))
            {
                t = typeof(IHideSystem);
            }
            return t;
        }

        public void AddSystem<T>(IEntity entity) where T : ISystem
        {
            Type type =GetParentType(typeof(T));
            Type entityType = entity.GetType();
            SystemObject sysObject = m_EntitySystems.GetVValue(entityType, type);
            if (sysObject == null)
            {
                sysObject = ReferencePool.Acquire<SystemObject>();
                sysObject.AddSystem<T>();
                m_EntitySystems.Add(entityType, type, sysObject);
            }
            sysObject.System.SystemStart(entity);
            sysObject.System.SystemShow(entity);
            if (sysObject.System.IsUpdateSystem())
            {
                m_UpdateSystems.AddUpdateSystem(entity, sysObject);
            }
        }

        /// <summary>
        /// 删除一个updatesystem
        /// </summary>
        /// <param name="entity"></param>
        private void RemoveUpdateSystem(IEntity entity)
        {
            m_UpdateSystems.RemoveUpdateSystem(entity);
        }


        /// <summary>
        /// 运行showsystem
        /// </summary>
        /// <param name="entity"></param>
        public void RunShowSystem(IEntity entity)
        {
            SystemObject sysObject = m_EntitySystems.GetVValue(entity.GetType(), typeof(IShowSystem));
            if (sysObject == null)
            {
                Debugger.LogWarning("not have entity showSystem");
                return;
            }
            sysObject.System.SystemShow(entity);
            SystemObject updateScene = m_EntitySystems.GetVValue(entity.GetType(), typeof(IUpdateSystem));
            m_UpdateSystems.AddUpdateSystem(entity, updateScene);
        }

        /// <summary>
        /// 运行隐藏system
        /// </summary>
        /// <param name="entity"></param>
        public void RunHideSystem(IEntity entity)
        {
            SystemObject sysObject = m_EntitySystems.GetVValue(entity.GetType(), typeof(IHideSystem));
            if (sysObject != null)
            {
                Debugger.LogWarning("not have entity hideSystem");
                return;
            }
            RemoveUpdateSystem(entity);
            sysObject.System.SystemHide(entity);
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
                if (systemObject.System.IsUpdateSystem())
                {
                    RemoveUpdateSystem(entity);
                }
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

            allSystemDic.SystemDestroy(entity);
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
            m_UpdateSystems.UpdateSystemEnitiys.SystemUpdate(elapseSeconds, realElapseSeconds);
        }

        public void Disable()
        {
        }
    }
}