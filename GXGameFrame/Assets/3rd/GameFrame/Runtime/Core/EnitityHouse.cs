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


        private Dictionary<Type, List<IEntity>> m_TypeWithEntitys = new();

        private DoubleMap<Type, SceneEntity> m_EverySceneEntity = new();

        private DDictionaryETC m_EntitySystems = new();

        private List<SystemEnitiy>[] UpdateSystemEnitiys;

        public void Init()
        {
            UpdateSystemEnitiys = new List<SystemEnitiy>[3];
            for (int i = 0; i < UpdateSystemEnitiys.Length; i++)
            {
                UpdateSystemEnitiys[i] = new List<SystemEnitiy>();
            }
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


        /// <summary>
        /// 删除一个updatesystem
        /// </summary>
        /// <param name="entity"></param>
        private void RemoveUpdateSystem(IEntity entity)
        {
            for (int z = 0; z < UpdateSystemEnitiys.Length; z++)
            {
                List<SystemEnitiy> systemEnitiys = UpdateSystemEnitiys[z];
                for (int i = systemEnitiys.Count - 1; i >= 0; i--)
                {
                    SystemEnitiy systemenitiy = systemEnitiys[i];
                    if (systemenitiy.Entity == entity)
                    {
                        ReferencePool.Release(systemenitiy);
                        systemEnitiys.RemoveAt(i);
                    }
                }
            }
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

        public void AddSystem<T>(IEntity entity) where T : ISystem
        {
            Type type = typeof(T);
            Type entityType = entity.GetType();
            SystemObject sysObject = m_EntitySystems.GetVValue(entityType, type);
            if (sysObject == null)
            {
                sysObject = ReferencePool.Acquire<SystemObject>();
                sysObject.AddSystem<T>();
                m_EntitySystems.Add(entityType, type, sysObject);
            }

            sysObject.System.SystemInit(entity);
            if (sysObject.System.IsUpdateSystem())
            {
                SystemEnitiy systenitiy = ReferencePool.Acquire<SystemEnitiy>();
                systenitiy.Create(sysObject, entity);
                UpdateSystemEnitiys[(byte) UpdateType.Update].Add(systenitiy);
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
            UpdateSystemEnitiys[(byte) UpdateType.Update].SystemUpdate(elapseSeconds, realElapseSeconds);
        }

        public void Disable()
        {
        }
    }
}