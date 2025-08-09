using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class UpdateManager
    {
        private StrongList<ISystem>[] updateSystemEntityArr;

        public StrongList<ISystem>[] UpdateSystemEntityArr => updateSystemEntityArr;

        private Dictionary<IEntity, StrongList<ISystem>> entityUpdateMap = new();

        public UpdateManager()
        {
            updateSystemEntityArr = new[] {new StrongList<ISystem>(256, true), new StrongList<ISystem>(256, true), new StrongList<ISystem>(256, true)};
        }

        /// <summary>
        /// 加入update系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ecsSystemObject"></param>
        public void AddUpdateSystem(IEntity entity, ISystem system)
        {
            int updateType = system.GetUpdateSystemType();
            if (updateType != (int) UpdateType.Node && !InUpdateMap(entity, system))
            {
                if (!entityUpdateMap.TryGetValue(entity, out var updateSystem))
                {
                    updateSystem = new StrongList<ISystem>();
                    entityUpdateMap.Add(entity, updateSystem);
                }

                if (!updateSystem.Contains(system))
                {
                    updateSystem.Add(system);
                    if ((updateType & (1 << (int) UpdateType.Update)) != 0)
                        updateSystemEntityArr[(int) UpdateType.Update].Add(system);
                    if ((updateType & (1 << (int) UpdateType.FixedUpdate)) != 0)
                        updateSystemEntityArr[(int) UpdateType.FixedUpdate].Add(system);
                    if ((updateType & (1 << (int) UpdateType.LateUpdate)) != 0)
                        updateSystemEntityArr[(int) UpdateType.LateUpdate].Add(system);
                }
            }
        }

        /// <summary>
        /// 是否拥有update系统,如果system为空那就是这个实体上有没有存在至少一个update系统如果不为空则为是否存在指定update
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="system"></param>
        /// <returns></returns>
        public bool InUpdateMap(IEntity entity, ISystem system = null)
        {
            if (system == null)
            {
                if (entityUpdateMap.ContainsKey(entity))
                {
                    return true;
                }
            }
            else
            {
                if (entityUpdateMap.TryGetValue(entity, out var systems))
                {
                    if (systems.Contains(system))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 删除update系统,如果system则删除这个物体上所有的update系统,如果不为空则删除指定update系统
        /// </summary>
        /// <param name="enitity"></param>
        /// <param name="system"></param>
        public void RemoveUpdateSystem(IEntity enitity)
        {
            if (!entityUpdateMap.TryGetValue(enitity, out var systems)) return;
            foreach (var sys in systems)
            {
                var updateType = sys.GetUpdateSystemType();
                if (updateType == (int) UpdateType.Node)
                    return;
                systems.Remove(sys);
                RemoveUpdate(updateType, sys);
            }
        }

        private void RemoveUpdate(int updateType, ISystem system)
        {
            if ((updateType & (1 << (int) UpdateType.Update)) != 0)
                updateSystemEntityArr[(int) UpdateType.Update].Remove(system);
            if ((updateType & (1 << (int) UpdateType.FixedUpdate)) != 0)
                updateSystemEntityArr[(int) UpdateType.FixedUpdate].Remove(system);
            if ((updateType & (1 << (int) UpdateType.LateUpdate)) != 0)
                updateSystemEntityArr[(int) UpdateType.LateUpdate].Remove(system);
        }
    }
}