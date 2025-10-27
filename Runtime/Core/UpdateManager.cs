using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class UpdateManager
    {
        private MarkDieList<ISystem>[] updateSystemEntityArr;

        public MarkDieList<ISystem>[] UpdateSystemEntityArr => updateSystemEntityArr;

        private Dictionary<IEntity, List<ISystem>> entityUpdateMap = new();

        public UpdateManager()
        {
            updateSystemEntityArr = new[] {new MarkDieList<ISystem>(256), new MarkDieList<ISystem>(256), new MarkDieList<ISystem>(256)};
        }

        /// <summary>
        /// 加入update系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ecsSystemObject"></param>
        public void AddUpdateSystem(IEntity entity, ISystem system)
        {
            int updateType = system.GetUpdateSystemType();
            if (updateType != UpdateRunType.Node && !InUpdateMap(entity, system))
            {
                if (!entityUpdateMap.TryGetValue(entity, out var updateSystem))
                {
                    updateSystem = new List<ISystem>();
                    entityUpdateMap.Add(entity, updateSystem);
                }

                if (!updateSystem.Contains(system))
                {
                    updateSystem.Add(system);
                    if ((updateType & (1 << UpdateRunType.Update)) != 0)
                        updateSystemEntityArr[UpdateRunType.Update].Add(system);
                    if ((updateType & (1 << UpdateRunType.FixedUpdate)) != 0)
                        updateSystemEntityArr[UpdateRunType.FixedUpdate].Add(system);
                    if ((updateType & (1 << UpdateRunType.LateUpdate)) != 0)
                        updateSystemEntityArr[UpdateRunType.LateUpdate].Add(system);
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
        public void MarkRemoveUpdateSystem(IEntity enitity)
        {
            if (!entityUpdateMap.TryGetValue(enitity, out List<ISystem> systems)) return;

            for (int i = systems.Count - 1; i >= 0; i--)
            {
                var sys = systems[i];
                var updateType = sys.GetUpdateSystemType();
                if (updateType == UpdateRunType.Node)
                    return;
                systems.Remove(sys);
                if ((updateType & (1 << UpdateRunType.Update)) != 0)
                    updateSystemEntityArr[UpdateRunType.Update].MarkRemove(sys);
                if ((updateType & (1 << UpdateRunType.FixedUpdate)) != 0)
                    updateSystemEntityArr[UpdateRunType.FixedUpdate].MarkRemove(sys);
                if ((updateType & (1 << UpdateRunType.LateUpdate)) != 0)
                    updateSystemEntityArr[UpdateRunType.LateUpdate].MarkRemove(sys);
            }
        }
    }
}