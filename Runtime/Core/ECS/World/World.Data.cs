using System;
using System.Collections.Generic;
using GameFrame.Runtime.WorldData;

namespace GameFrame.Runtime
{
    public partial class World
    {
        private Dictionary<Type, IWorldData> WorldDatas;

        public void InitializeWorld()
        {
            WorldDatas = new Dictionary<Type, IWorldData>();
        }

        public T AddWorldData<T>() where T : IWorldData
        {
            if (WorldDatas.ContainsKey(typeof(T)))
                throw new InvalidOperationException("This world data has been added" + typeof(T).Name);
            T worldData = (T) ReferencePool.Acquire(typeof(T));
            worldData.Initialize();
            WorldDatas.Add(typeof(T), worldData);
            return worldData;
        }

        public bool RemoveWorldData(IWorldData worldData)
        {
            if (WorldDatas.Remove(worldData.GetType()))
            {
                ReferencePool.Release(worldData);
                return true;
            }

            return false;
        }

        public T GetWorldDatas<T>() where T : IWorldData
        {
            return (T) WorldDatas.GetValueOrDefault(typeof(T));
        }


        void DisposeWorld()
        {
            foreach (var pair in WorldDatas)
            {
                ReferencePool.Release(pair.Value);
            }

            WorldDatas = null;
        }
    }
}