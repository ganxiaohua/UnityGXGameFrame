using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace GameFrame
{
    using DDictionaryETC = DDictionary<IEntity, Type, SystemObject>;

    public class EnitityHouse : Singleton<EnitityHouse>
    {
        enum UpdateType : byte
        {
            Update = 0,
            LateUpdate,
            FixUpdate,
        }
        

        // private Dictionary<Type, List<IEntity>> m_TypeWithEntitys = new();

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
        
        // public void AddEntityWithType(IEntity entity)
        // {
        //     Type type = entity.GetType();
        //     if (!m_TypeWithEntitys.TryGetValue(type,out List<IEntity> entities))
        //     {
        //         entities = new List<IEntity>();
        //         m_TypeWithEntitys.Add(type,entities);
        //     }
        //
        //     if (entities.Contains(entity))
        //     {
        //         throw new Exception($"m_TypeWithEntitys  have enitiy:{entity.ID}");
        //     }
        //     entities.Add(entity);
        // }

        // public void RemoveEntity(IEntity entity)
        // {
        //     if (!m_EveryEntity.Contains(entity))
        //     {
        //         throw new Exception($"EveryEntity not have enitiy:{entity.ID}");
        //     }
        //
        //     m_EveryEntity.Remove(entity);
        //     m_EntitySystems.GetValue(entity)?.SystemDestroy(entity);
        //     m_EntitySystems.RemoveTkey(entity);
        //     RemoveUpdateSystem(entity);
        // }

        //TODO:这里有新能瓶颈,需要优化一下
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

        public void RemoveSceneEntity<T>() where T : IScene
        {
            Type type = typeof(T);
            if (!m_EverySceneEntity.TryGetValueKv(type, out SceneEntity sceneEntity))
            {
                throw new Exception($"EverySceneEntity not have enitiy:{type}");
            }

            m_EverySceneEntity.RemoveByKey(type);
        }

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
            if (m_EntitySystems.ContainsTk(entity, type))
            {
                throw new Exception($"EntitySystems have system:{type}");
            }

            SystemObject sysObject = ReferencePool.Acquire<SystemObject>();
            sysObject.AddSystem<T>();
            m_EntitySystems.Add(entity, type, sysObject);
            if (sysObject.System.SystemInit(entity))
            {
            }
            else if (sysObject.System.IsUpdateSystem())
            {
                SystemEnitiy systenitiy = ReferencePool.Acquire<SystemEnitiy>();
                systenitiy.Create(sysObject, entity);
                UpdateSystemEnitiys[(byte) UpdateType.Update].Add(systenitiy);
            }
        }

        public void RemoveSystem<T>(Entity entity)
        {
            Type type = typeof(T);
            SystemObject systemObject = m_EntitySystems.RemoveKkey(entity, type);
            if (systemObject != null)
            {
                ReferencePool.Release(systemObject);
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