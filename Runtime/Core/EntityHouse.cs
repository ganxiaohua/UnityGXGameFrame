using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    using DDictionaryETC = DDictionary<IEntity, Type, ISystem>;

    public class EntityHouse : Singleton<EntityHouse>
    {
        /// <summary>
        /// 不会包含EcsEntity的实体集合
        /// </summary>
        private Dictionary<Type, List<IEntity>> typeWithEntityDic = new();

        private Dictionary<SceneType, IScene> sceneEntityDic = new(4);

        private DDictionaryETC entitySysTypeSysDic = new();

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

        public List<IEntity> GetEntity<T>() where T : class, IEntity, new()
        {
            Type type = typeof(T);
            var entityList = GetEntity(type);
            return entityList;
        }

        public List<IEntity> GetEntity(Type type)
        {
            return typeWithEntityDic.GetValueOrDefault(type);
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
                return null;
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

        public void AddSystem<T>(World entity, object obj) where T : ISystem
        {
            AddSystem(entity, typeof(T), obj);
        }


        /// <summary>
        /// 创建获得得到一个SystemObject
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="system"> 系统类</param>
        /// <returns></returns>
        private ISystem CreateSystem(IEntity entity, Type systemType)
        {
            var sys = entitySysTypeSysDic.GetVValue(entity, systemType);
            Assert.IsNull(sys, $"{systemType.Name}已经加入了过了");
            sys = (ISystem) ReferencePool.Acquire(systemType);
            entitySysTypeSysDic.Add(entity, systemType, sys);
            return sys;
        }

        private ISystem CreateSystem(IEntity entity, Type systemType, object obj)
        {
            var sys = entitySysTypeSysDic.GetVValue(entity, systemType);
            Assert.IsNull(sys, $"{systemType.Name}已经加入了过了");
            sys = (ISystem) ReferencePool.Acquire(systemType);
            entitySysTypeSysDic.Add(entity, systemType, sys);
            ((ISystemCarryover) sys).Carryover = obj;
            return sys;
        }

        /// <summary>
        /// ecs 加入系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        public void AddSystem(World entity, Type type)
        {
            var sys = CreateSystem(entity, type);
            sys.SystemInitialize(entity);
            updateSystems.AddUpdateSystem(entity, sys);
        }

        /// <summary>
        /// ecs 加入系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        public void AddSystem(World entity, Type type, object obj)
        {
            var sys = CreateSystem(entity, type, obj);
            sys.SystemInitialize(entity);
            updateSystems.AddUpdateSystem(entity, sys);
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
                entitySysTypeSysDic.Add(entity, type, system);
                updateSystems.AddUpdateSystem(entity, system);
            }

            switch (system)
            {
                case IUpdateSystem:
                    Add(typeof(IUpdateSystem));
                    break;
                case ILateUpdateSystem:
                    Add(typeof(ILateUpdateSystem));
                    break;
                case IFixedUpdateSystem:
                    Add(typeof(IFixedUpdateSystem));
                    break;
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
                var sys = entitySysTypeSysDic.GetVValue(entity, typeof(IUpdateSystem));
                if (sys != null)
                {
                    updateSystems.AddUpdateSystem(entity, sys);
                }

                sys = entitySysTypeSysDic.GetVValue(entity, typeof(ILateUpdateSystem));
                if (sys != null)
                {
                    updateSystems.AddUpdateSystem(entity, sys);
                }

                sys = entitySysTypeSysDic.GetVValue(entity, typeof(IFixedUpdateSystem));
                if (sys != null)
                {
                    updateSystems.AddUpdateSystem(entity, sys);
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
            var sys = entitySysTypeSysDic.GetVValue(entity, type);
            if (sys != null)
            {
                RemoveUpdateSystem(entity, sys);
                ReferencePool.Release(sys);
            }
        }

        /// <summary>
        /// 有实体进行销毁
        /// </summary>
        /// <param name="entity"></param>
        private void RemoveAllSystem(IEntity entity)
        {
            Type entityType = entity.GetType();
            var allSystemDic = entitySysTypeSysDic.GetValue(entity);
            if (allSystemDic == null)
            {
                //这个实体上不存在系统
                return;
            }

            RemoveUpdateSystem(entity);
            if (GetEntityCount(entityType) == 0)
            {
                var item = entitySysTypeSysDic.RemoveTkey(entity);
                foreach (KeyValuePair<Type, ISystem> typeSystem in item)
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
            foreach (var ts in entitySysTypeSysDic)
            {
                if (ts is not IEntity)
                    ReferencePool.Release(ts);
            }
        }
    }
}