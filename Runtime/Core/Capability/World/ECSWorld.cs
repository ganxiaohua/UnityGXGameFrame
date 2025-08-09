using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class ECSWorld : World
    {
        private int serialId;
        public Dictionary<Type, IEntity> EcsSystems { get; private set; } = new();

        private IEntity Create(Type type)
        {
            IEntity entity = (IEntity) ReferencePool.Acquire(type);
            entity.OnDirty(this, serialId++);
            EcsSystems.Add(type, entity);

            EntityHouse.Instance.AddEntity(entity);
            return entity;
        }

        private void Remove<T>() where T : IEntity
        {
            Type type = typeof(T);
            Remove(type);
        }

        private void Remove(Type type)
        {
            if (!EcsSystems.TryGetValue(type, out IEntity entity))
            {
                throw new Exception($"entity not already  component: {type.FullName}");
            }

            EcsSystems.Remove(type);
            Remove(entity);
        }


        /// <summary>
        /// 删除组件
        /// </summary>
        /// <param name="entity"></param>
        private void Remove(IEntity entity)
        {
            EntityHouse.Instance.RemoveEntity(entity);
            ReferencePool.Release(entity);
        }


        /// <summary>
        /// 创建一个组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private IEntity CreateSystem<T>() where T : IEntity
        {
            Type type = typeof(T);
            return CreateSystem(type);
        }

        private IEntity CreateSystem(Type type)
        {
            if (this.EcsSystems != null && this.EcsSystems.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            IEntity component = Create(type);
            return component;
        }

        /// <summary>
        /// 加入entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T AddSystem<T>() where T : class, IEntity
        {
            IEntity system = CreateSystem<T>();
            if (system is IInitializeSystem<World> initySystem)
            {
                initySystem.SystemInitialize(this);
            }
            EntityHouse.Instance.AddUpdateSystem(system);
            return (T)system;
        }
        
        
        public bool HasSystem<T>() where T : class, IEntity
        {
            return GetSystem<T>() != null;
        }

        public T GetSystem<T>() where T : class, IEntity
        {
            Type type = typeof(T);
            return (T) GetSystem(type);
        }

        public IEntity GetSystem(Type type)
        {
            IEntity value = null;
            if (this.EcsSystems != null && !this.EcsSystems.TryGetValue(type, out value))
            {
                return null;
            }

            return value;
        }

        public void TryRemoveSystem<T>() where T : class, IEntity
        {
            if (HasSystem<T>())
            {
                RemoveSystem<T>();
            }
        }


        /// <summary>
        /// 删除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveSystem<T>() where T : class, IEntity
        {
            Remove<T>();
        }

        public void RemoveSystem(Type type)
        {
            Remove(type);
        }

        /// <summary>
        /// 清理所有的组件
        /// </summary>
        public void ClearAllSystem()
        {
            foreach (var item in EcsSystems)
            {
                ReferencePool.Release(item.Value);
            }

            EcsSystems.Clear();
        }


        /// <summary>
        /// 清除
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            ClearAllSystem();
        }
    }
}