using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public partial class ECCWorld
    {
        private Dictionary<Type, IEntity> systems = new Dictionary<Type, IEntity>();
        private List<IUpdateSystem> updateSystems = new List<IUpdateSystem>();
        private List<IFixedUpdateSystem> fixedUpdateSystems = new List<IFixedUpdateSystem>();

        public void AddSystem<T>() where T : ISystem, IEntity
        {
            Type type = typeof(T);
            if (systems.ContainsKey(type))
            {
                throw new Exception($"already has Systems: {type.FullName}");
            }

            IEntity entity = (IEntity) ReferencePool.Acquire(type);
            systems.Add(type, entity);
            entity.OnDirty(this, 0);
            EntityHouse.Instance.AddEntity(entity);
            if (entity is IInitializeSystem system)
            {
                system.SystemInitialize();
            }

            if (entity is IUpdateSystem updateSystem)
            {
                updateSystems.Add(updateSystem);
            }

            if (entity is IFixedUpdateSystem fixedUpdateManagers)
            {
                fixedUpdateSystems.Add(fixedUpdateManagers);
            }
        }

        public void OnUpdateSystem(float elapseSeconds, float realElapseSeconds)
        {
            int count = updateSystems.Count;
            for (int i = 0; i < count; i++)
            {
                updateSystems[i].OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        public void OnFixedUpdateSystem(float elapseSeconds, float realElapseSeconds)
        {
            int count = fixedUpdateSystems.Count;
            for (int i = 0; i < count; i++)
            {
                fixedUpdateSystems[i].OnFixedUpdate(elapseSeconds, realElapseSeconds);
            }
        }


        private void DisposeSystem()
        {
            foreach (KeyValuePair<Type, IEntity> system in systems)
            {
                EntityHouse.Instance.RemoveEntity(system.Value);
            }

            updateSystems.Clear();
            fixedUpdateSystems.Clear();
        }
    }
}