using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace GameFrame
{
    using DDictionaryETC = DDictionary<IEntity, Type, SystemObject>;

    public class EnitityHouse : Singleton<EnitityHouse>
    {
        enum UpdateType 
        {
            Update = 0,
            LateUpdate,
            FixUpdate,
        }

        public class SystemEnitiy : IReference
        {
            public SystemObject SystemObject { get; set; }
            public IEntity Entity { get; set; }

            public void Create(SystemObject systemobject, IEntity entity)
            {
                SystemObject = systemobject;
                Entity = entity;
            }

            public void Clear()
            {
                SystemObject = null;
                Entity = null;
            }
        }

        private HashSet<IEntity> m_EveryEntity = new();

        private DoubleMap<Type, SceneEntity> m_EverySceneEntity = new();

        private Dictionary<Type, SystemObject> m_SystemObjects = new();

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


        public void AddEntity(IEntity entity)
        {
            if (m_EveryEntity.Contains(entity))
            {
                throw new Exception($"have enitiy:{entity.ID}");
            }

            m_EveryEntity.Add(entity);
        }

        public void RemoveEntity(IEntity entity)
        {
            if (!m_EveryEntity.Contains(entity))
            {
                throw new Exception($"EveryEntity not have enitiy:{entity.ID}");
            }

            m_EveryEntity.Remove(entity);
            m_EntitySystems.GetValue(entity)?.SystemDestroy(entity);
            m_EntitySystems.RemoveTkey(entity);
            RemoveUpdateSystem(entity);
        }

        private void RemoveUpdateSystem(IEntity entity)
        {
            for (int z = 0; z < UpdateSystemEnitiys.Length; z++)
            {
                List<SystemEnitiy> systemEnitiys = UpdateSystemEnitiys[z];
                for (int i =systemEnitiys.Count - 1; i >= 0; i--)
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

        public void AddSceneEntity<T>(SceneEntity entity) where T : SceneEntityType
        {
            Type type = typeof(T);
            if (m_EverySceneEntity.ContainsKey(type))
            {
                throw new Exception($"EverySceneEntity have enitiy:{entity.ID}");
            }

            m_EverySceneEntity.Add(type, entity);
            AddEntity(entity);
        }

        public SceneEntity GetScene<T>() where T : SceneEntityType
        {
            Type type = typeof(T);
            if (!m_EverySceneEntity.TryGetValueKv(type, out SceneEntity sceneEntity))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{type}");
            }

            return sceneEntity;
        }

        public void RemoveSceneEntity<T>() where T : SceneEntityType
        {
            Type type = typeof(T);
            if (!m_EverySceneEntity.TryGetValueKv(type, out SceneEntity sceneEntity))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{type}");
            }

            m_EverySceneEntity.RemoveByKey(type);
            RemoveEntity(sceneEntity);
        }

        public void RemoveSceneEntity(SceneEntity sceneEntity)
        {
            if (!m_EverySceneEntity.TryGetValueVk(sceneEntity, out Type type))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{type}");
            }

            m_EverySceneEntity.RemoveByValue(sceneEntity);
            RemoveEntity(sceneEntity);
        }

        public void AddSystem<T>(IEntity entity) where T : ISystem
        {
            Type type = typeof(T);
            m_SystemObjects.TryGetValue(type, out SystemObject systemObject);
            if (systemObject == null)
            {
                systemObject = ReferencePool.Acquire<SystemObject>();
                systemObject.AddSystem<T>();
                m_SystemObjects.Add(type, systemObject);
            }

            if (m_EntitySystems.ContainsTk(entity, type))
            {
                throw new Exception($"EntitySystems have system:{type}");
            }

            m_EntitySystems.Add(entity, type, systemObject);
            systemObject.System.SystemInit(entity);
            //update系统特殊处理.
            if (systemObject.System.IsUpdateSystem())
            {
                SystemEnitiy systenitiy = ReferencePool.Acquire<SystemEnitiy>();
                systenitiy.Create(systemObject, entity);
                UpdateSystemEnitiys[(int) UpdateType.Update].Add(systenitiy);
            }
        }

        public void RemoveSystem<T>(Entity entity)
        {
            Type type = typeof(T);
            m_SystemObjects.TryGetValue(type, out SystemObject systemObject);
            m_EntitySystems.RemoveKkey(entity, type);
        }

        public void Update()
        {
            UpdateSystemEnitiys[(int) UpdateType.Update].SystemUpdate();
        }

        public void Disable()
        {
        }
    }
}