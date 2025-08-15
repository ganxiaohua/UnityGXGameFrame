using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class EntityHouse : Singleton<EntityHouse>
    {
        /// <summary>
        /// 不会包含EcsEntity的实体集合
        /// </summary>
        private Dictionary<Type, List<IEntity>> typeWithEntityDic = new();

        private Dictionary<SceneType, IScene> sceneEntityDic = new(4);

        private UpdateManager updateManager = new();


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

            updateManager.RemoveUpdateSystem(entity);
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
        public IScene AddSceneEntity<T>(SceneType sceneType, Entity state) where T : class, IEntity, IScene, new()
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

            updateManager.AddUpdateSystem(entity, system);
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
            updateManager.AddUpdateSystem(entity, system);
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
            updateManager.RemoveUpdateSystem(entity);
            foreach (IEntity entity1 in entity.Children)
            {
                RunHideSystem((Entity) entity1);
            }

            foreach (KeyValuePair<Type, IEntity> entity1 in entity.Components)
            {
                RunHideSystem((Entity) entity1.Value);
            }
        }


        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            updateManager.UpdateSystemEntityArr[UpdateRunType.Update].SystemUpdate(elapseSeconds, realElapseSeconds);
        }

        public void LateUpdate(float elapseSeconds, float realElapseSeconds)
        {
            updateManager.UpdateSystemEntityArr[UpdateRunType.LateUpdate].SystemLateUpdate(elapseSeconds, realElapseSeconds);
        }

        public void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            updateManager.UpdateSystemEntityArr[UpdateRunType.FixedUpdate].SystemFixedUpdate(elapseSeconds, realElapseSeconds);
        }


        public void Disable()
        {
        }
    }
}