using System.Collections.Generic;

namespace GameFrame
{
    public class UpdateSystems
    {
        private StrongList<SystemEntity>[] updateSystemEntityArr;
        
        public StrongList<SystemEntity>[] UpdateSystemEntityArr => updateSystemEntityArr;

        private DDictionary<IEntity, ISystem, SystemEntity> IndexDDc = new();

        public UpdateSystems()
        {
            updateSystemEntityArr = new[] {new StrongList<SystemEntity>(256), new StrongList<SystemEntity>(256), new StrongList<SystemEntity>(256)};
        }

        /// <summary>
        /// 加入update系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ecsSystemObject"></param>
        public void AddUpdateSystem(IEntity entity, ISystemObject ecsSystemObject)
        {
            UpdateType updateType = ecsSystemObject.System.IsUpdateSystem();
            if (updateType != UpdateType.Node && !HasUpdateSystem(entity, ecsSystemObject.System))
            {
                if (IndexDDc.ContainsTk(entity, ecsSystemObject.System))
                {
                    Debugger.LogWarning("entity has add in the Index");
                    return;
                }

                SystemEntity systemEntity = ReferencePool.Acquire<SystemEntity>();
                systemEntity.Create(ecsSystemObject, entity);

                updateSystemEntityArr[(int)updateType].Add(systemEntity);

                IndexDDc.Add(entity, ecsSystemObject.System, systemEntity);
            }
        }

        /// <summary>
        /// 是否拥有update系统,如果system为空那就是这个实体上有没有存在至少一个update系统如果不为空则为是否存在指定update
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="system"></param>
        /// <returns></returns>
        public bool HasUpdateSystem(IEntity entity, ISystem system = null)
        {
            if (system == null)
            {
                if (IndexDDc.ContainsT(entity))
                {
                    return true;
                }
            }
            else
            {
                if (IndexDDc.ContainsTk(entity, system))
                {
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
            if (system == null)
            {
                Dictionary<ISystem, SystemEntity> systemdic = IndexDDc.GetValue(enitity);
                if (systemdic != null)
                {
                    foreach (var systemObject in systemdic)
                    {
                        UpdateType updateType = systemObject.Key.IsUpdateSystem();
                        updateSystemEntityArr[(int)updateType].Remove(systemObject.Value);
                    }

                    IndexDDc.RemoveTkey(enitity);
                }
            }
            else
            {
                SystemEntity systemobejct = IndexDDc.GetVValue(enitity, system);
                if (systemobejct != null)
                {
                    UpdateType updateType = system.IsUpdateSystem();
                    if (updateType != UpdateType.Node)
                    {
                        updateSystemEntityArr[(int) updateType].Remove(systemobejct);
                        IndexDDc.RemoveKkey(enitity, system);
                    }
                }
            }
        }
    }
}