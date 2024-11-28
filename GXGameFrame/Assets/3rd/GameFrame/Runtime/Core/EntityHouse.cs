using System;
using System.Collections.Generic;

namespace GameFrame
{
    using DDictionaryETC = DDictionary<IEntity, Type, ISystemObject>;

    public class EntityHouse : Singleton<EntityHouse>
    {
        /// <summary>
        /// 不会包含EcsEntity的实体集合
        /// </summary>
        private Dictionary<Type, List<IEntity>> typeWithEntityDic = new();

        private Dictionary<SceneType, IScene> sceneEntityDic = new(4);

        private DDictionaryETC entitySystemsDDic = new();

        private UpdateSystems updateSystems = new();


        /// <summary>
        /// 增加实体 所有的实体都会加入到这里
        /// </summary>
        /// <param name="entity"></param>
        /// <exception cref="Exception"></exception>
        public void AddEntity(IEntity entity)
        {
            Type type = entity.GetType();
            if (!typeWithEntityDic.TryGetValue(type, out var entityList))
            {
                entityList = new List<IEntity>();
                typeWithEntityDic.Add(type, entityList);
            }

            if (entityList.Contains(entity))
            {
                throw new Exception($"TypeWithEntitys have entity:{entity.ID}");
            }

            EventData.Instance.AddEventEntity(entity);
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
            if (!typeWithEntityDic.TryGetValue(type, out var entityList))
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
            if (!typeWithEntityDic.TryGetValue(type, out var entityList) || !entityList.Contains(entity))
            {
                throw new Exception($"TypeWithEntitys not have entity:{entity.ID}");
            }
            RemoveAllSystem(entity);
            EventData.Instance.RemoveEventEntity(entity);
            entityList.Remove(entity);
        }

        /// <summary>
        /// 获得此类型的实体的数量
        /// </summary>
        public int GetEntityCount(Type entityType)
        {
            if (typeWithEntityDic.TryGetValue(entityType, out var entityList))
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
        public IScene AddSceneEntity<T>(SceneType sceneType, FsmState state) where T : class, IEntity, IScene, new()
        {
            if (sceneEntityDic.TryGetValue(sceneType, out IScene IScene))
            {
                Type type = typeof(T);
                if (IScene.GetType() == type)
                {
                    throw new Exception($"EverySceneEntity  have entity:{type.Name}");
                }

                RemoveSceneEntity(sceneType);
            }

            T scene = state.AddComponent<T>();
            sceneEntityDic.Add(sceneType, scene);
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
            if (!sceneEntityDic.TryGetValue(sceneType, out IScene IScene))
            {
                throw new Exception($"EverySceneEntity not have entity:{sceneType}");
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
            if (!sceneEntityDic.TryGetValue(sceneType, out IScene scene))
            {
                throw new Exception($"EverySceneEntity not have entity:{sceneType}");
            }

            GXGameFrame.Instance.RootEntity.RemoveComponent(scene.GetType());
            sceneEntityDic.Remove(sceneType);
        }

        public void AddSystem<T>(World entity) where T : ISystem
        {
            AddSystem(entity, typeof(T));
        }

        /// <summary>
        /// 创建获得得到一个SystemObject
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="system"> 系统类</param>
        /// <returns></returns>
        private ISystemObject CreateSystem(IEntity entity, Type system)
        {
            ISystemObject sysObject = entitySystemsDDic.GetVValue(entity, system);
            if (sysObject == null)
            {
                sysObject = ReferencePool.Acquire<IndependentSystem>();
                ((IndependentSystem) sysObject).AddSystem(system);
                entitySystemsDDic.Add(entity, system, sysObject);
            }

            return sysObject;
        }

        /// <summary>
        /// ecs 加入系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        public void AddSystem(World entity, Type type)
        {
            ISystemObject sysObject = CreateSystem(entity, type);
            sysObject.System.SystemInitialize(entity);
            updateSystems.AddUpdateSystem(entity, sysObject);
        }

        /// <summary>
        /// 自身又是实体又是Updtae系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        public void AddUpdateSystem(IEntity entity)
        {
            if (entity is not ISystem system)
            {
                return;
            }


            void Add(Type type)
            {
                var sysObject = ReferencePool.Acquire<EntitySystem>();
                sysObject.AddSystem((ISystem) entity);
                entitySystemsDDic.Add(entity, type, sysObject);
                updateSystems.AddUpdateSystem(entity, sysObject);
            }

            if (system is IUpdateSystem)
            {
                Add(typeof(IUpdateSystem));
            }

            if (system is ILateUpdateSystem)
            {
                Add(typeof(ILateUpdateSystem));
            }

            if (system is IFixedUpdateSystem)
            {
                Add(typeof(IFixedUpdateSystem));
            }
        }

        /// <summary>
        /// 删除一个updatesystem
        /// </summary>
        /// <param name="entity"></param>
        private void RemoveUpdateSystem(IEntity entity, ISystem system = null)
        {
            updateSystems.RemoveUpdateSystem(entity, system);
        }

        /// <summary>
        /// 运行PreShowsystem
        /// </summary>
        /// <param name="entity"></param>
        public void RunPreShowSystem(ISystem entity, bool IsFirstShow)
        {
            entity?.SystemPreShow(IsFirstShow);
        }

        /// <summary>
        /// 运行showsystem
        /// </summary>
        /// <param name="entity"></param>
        public void RunShowSystem(Entity entity)
        {
            if (entity is not ISystem)
                return;
            ISystem system = (ISystem) entity;
            system?.SystemShow();
            EventData.Instance.AddEventEntity(entity);
            if (!updateSystems.InUpdateMap(entity))
            {
                ISystemObject updateSystem = entitySystemsDDic.GetVValue(entity, typeof(IUpdateSystem));
                if (updateSystem != null)
                {
                    updateSystems.AddUpdateSystem(entity, updateSystem);
                }

                updateSystem = entitySystemsDDic.GetVValue(entity, typeof(ILateUpdateSystem));
                if (updateSystem != null)
                {
                    updateSystems.AddUpdateSystem(entity, updateSystem);
                }

                updateSystem = entitySystemsDDic.GetVValue(entity, typeof(IFixedUpdateSystem));
                if (updateSystem != null)
                {
                    updateSystems.AddUpdateSystem(entity, updateSystem);
                }
            }

            foreach (IEntity entity1 in entity.Children)
            {
                RunShowSystem((Entity) entity1);
            }

            foreach (KeyValuePair<Type, IEntity> valuePair in entity.Components)
            {
                RunShowSystem((Entity) valuePair.Value);
            }
        }

        /// <summary>
        /// 运行隐藏system
        /// </summary>
        /// <param name="entity"></param>
        public void RunHideSystem(Entity entity)
        {
            if (entity is not ISystem)
                return;
            ISystem system = (ISystem) entity;
            system?.SystemHide();
            EventData.Instance.RemoveEventEntity(entity);
            updateSystems.RemoveUpdateSystem(entity);
            foreach (IEntity entity1 in entity.Children)
            {
                RunHideSystem((Entity) entity1);
            }

            foreach (KeyValuePair<Type, IEntity> entity1 in entity.Components)
            {
                RunHideSystem((Entity) entity1.Value);
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
            ISystemObject ecsSystemObject = entitySystemsDDic.GetVValue(entity, type);
            if (ecsSystemObject != null)
            {
                RemoveUpdateSystem(entity, ecsSystemObject.System);
                ReferencePool.Release(ecsSystemObject);
            }
        }

        /// <summary>
        /// 有实体进行销毁
        /// </summary>
        /// <param name="entity"></param>
        private void RemoveAllSystem(IEntity entity)
        {
            Type entityType = entity.GetType();
            var allSystemDic = entitySystemsDDic.GetValue(entity);
            if (allSystemDic == null)
            {
                //这个实体上不存在系统
                return;
            }

            RemoveUpdateSystem(entity);
            if (GetEntityCount(entityType) == 0)
            {
                var item = entitySystemsDDic.RemoveTkey(entity);
                foreach (KeyValuePair<Type, ISystemObject> typeSystem in item)
                {
                    ReferencePool.Release(typeSystem.Value);
                }
            }
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            updateSystems.UpdateSystemEntityArr[(int) UpdateType.Update].SystemUpdate(elapseSeconds, realElapseSeconds);
        }

        public void LateUpdate(float elapseSeconds, float realElapseSeconds)
        {
            updateSystems.UpdateSystemEntityArr[(int) UpdateType.LateUpdate].SystemLateUpdate(elapseSeconds, realElapseSeconds);
        }

        public void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            updateSystems.UpdateSystemEntityArr[(int) UpdateType.FixedUpdate].SystemFixedUpdate(elapseSeconds, realElapseSeconds);
        }


        public void Disable()
        {
        }
    }
}