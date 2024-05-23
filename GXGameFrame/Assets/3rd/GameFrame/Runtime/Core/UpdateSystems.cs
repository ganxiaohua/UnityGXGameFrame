using System.Collections.Generic;

namespace GameFrame
{
    public class UpdateSystems
    {
        private QueryList<SystemEntity>[] m_UpdateSystemEntitys;
        
        public QueryList<SystemEntity>[] UpdateSystemEntitys => m_UpdateSystemEntitys;

        private DDictionary<IEntity, ISystem, SystemEntity> m_Index = new();

        public UpdateSystems()
        {
            m_UpdateSystemEntitys = new[] {new QueryList<SystemEntity>(256), new QueryList<SystemEntity>(256), new QueryList<SystemEntity>(256)};
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
                if (m_Index.ContainsTk(entity, ecsSystemObject.System))
                {
                    Debugger.LogWarning("entity has add in the Index");
                    return;
                }

                SystemEntity systemEntity = ReferencePool.Acquire<SystemEntity>();
                systemEntity.Create(ecsSystemObject, entity);

                m_UpdateSystemEntitys[(int)updateType].Add(systemEntity);

                m_Index.Add(entity, ecsSystemObject.System, systemEntity);
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
                if (m_Index.ContainsT(entity))
                {
                    return true;
                }
            }
            else
            {
                if (m_Index.ContainsTk(entity, system))
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
                Dictionary<ISystem, SystemEntity> systemdic = m_Index.GetValue(enitity);
                if (systemdic != null)
                {
                    foreach (var systemObject in systemdic)
                    {
                        UpdateType updateType = systemObject.Key.IsUpdateSystem();
                        m_UpdateSystemEntitys[(int)updateType].Remove(systemObject.Value);
                    }

                    m_Index.RemoveTkey(enitity);
                }
            }
            else
            {
                SystemEntity systemobejct = m_Index.GetVValue(enitity, system);
                if (systemobejct != null)
                {
                    UpdateType updateType = system.IsUpdateSystem();
                    if (updateType != UpdateType.Node)
                    {
                        m_UpdateSystemEntitys[(int) updateType].Remove(systemobejct);
                        m_Index.RemoveKkey(enitity, system);
                    }
                }
            }
        }
    }
}