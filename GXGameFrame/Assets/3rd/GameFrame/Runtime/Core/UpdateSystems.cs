using System.Collections.Generic;
using System.Linq;

namespace GameFrame
{
    public class UpdateSystems
    {
        private List<SystemEnitiy> m_UpdateSystemEnitiys = new();

        public List<SystemEnitiy> UpdateSystemEnitiys => m_UpdateSystemEnitiys;

        private DDictionary<IEntity, ISystem, SystemEnitiy> m_Index = new();

        /// <summary>
        /// 加入update系统
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="systemObject"></param>
        public void AddUpdateSystem(IEntity entity, SystemObject systemObject)
        {
            if (m_Index.ContainsTk(entity, systemObject.System))
            {
                Debugger.LogWarning("entity has add in the Index");
                return;
            }

            SystemEnitiy systenitiy = ReferencePool.Acquire<SystemEnitiy>();
            systenitiy.Create(systemObject, entity);
            m_UpdateSystemEnitiys.Add(systenitiy);
            m_Index.Add(entity, systemObject.System, systenitiy);
        }

        /// <summary>
        /// 是有拥有update系统,如果system为空那就是这个实体上有没有存在至少一个update系统如果不为空则为是否存在指定update
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
                Dictionary<ISystem, SystemEnitiy> systemdic = m_Index.GetValue(enitity);
                if (systemdic != null)
                {
                    foreach (var systemObject in systemdic)
                    {
                        m_UpdateSystemEnitiys.Remove(systemObject.Value);
                    }

                    m_Index.RemoveTkey(enitity);
                }
            }
            else
            {
                SystemEnitiy systemobejct = m_Index.GetVValue(enitity, system);
                if (systemobejct != null)
                {
                    m_UpdateSystemEnitiys.Remove(systemobejct);
                    m_Index.RemoveKkey(enitity,system);
                }
            }
            
        }
    }
}