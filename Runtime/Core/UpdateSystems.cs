using System.Collections.Generic;

namespace GameFrame
{
    public class UpdateSystems
    {
        private StrongList<ISystem>[] updateSystemEntityArr;
        
        public StrongList<ISystem>[] UpdateSystemEntityArr => updateSystemEntityArr;

        private Dictionary<IEntity, StrongList<ISystem>> entityUpdateMap = new();

        public UpdateSystems()
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
            UpdateType updateType = system.GetUpdateSystemType();
            if (updateType != UpdateType.Node && !InUpdateMap(entity, system))
            {
                if (!entityUpdateMap.TryGetValue(entity, out var updateSystem))
                {
                    updateSystem = new StrongList<ISystem>();
                    entityUpdateMap.Add(entity, updateSystem);
                }

                int index = (int) updateType;
                if (!updateSystem.Contains(system))
                {
                    updateSystem.Add(system);
                    updateSystemEntityArr[index].Add(system);
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
        public void RemoveUpdateSystem(IEntity enitity, ISystem system = null)
        {
            if (entityUpdateMap.TryGetValue(enitity, out var systems))
            {
                if (system != null)
                {
                    var updateType = system.GetUpdateSystemType();
                    if (updateType == UpdateType.Node)
                        return;
                    int index = (int) updateType;
                    systems.Remove(system);
                    updateSystemEntityArr[index].Remove(system);
                }
                else
                {
                    foreach (var sys in systems)
                    {
                        var updateType = sys.GetUpdateSystemType();
                        int index = (int) updateType;
                        updateSystemEntityArr[index].Remove(sys);
                        systems.Remove(sys);
                    }
                }
            }
        }
    }
}