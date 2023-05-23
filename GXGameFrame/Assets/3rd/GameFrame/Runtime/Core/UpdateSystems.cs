using System.Collections.Generic;
using System.Linq;

namespace GameFrame
{
    public class UpdateSystems
    {
        private List<SystemEnitiy> m_UpdateSystemEnitiys = new ();

        public List<SystemEnitiy> UpdateSystemEnitiys => m_UpdateSystemEnitiys;

        private Dictionary<IEntity, SystemEnitiy> m_Index = new ();

        public void AddUpdateSystem(IEntity entity,SystemObject systemObject)
        {
            if (m_Index.ContainsKey(entity))
            {
                Debugger.LogWarning("entity has add in the Index");
                return;
            }
            SystemEnitiy systenitiy = ReferencePool.Acquire<SystemEnitiy>();
            systenitiy.Create(systemObject, entity);
            m_UpdateSystemEnitiys.Add(systenitiy);
            m_Index.Add(entity,systenitiy);
        }

        public bool HasUpdateSystem(IEntity entity)
        {
            if (m_Index.ContainsKey(entity))
            {
                return true;
            }
            return false;
        }

        public void RemoveUpdateSystem(IEntity enitity)
        {
            if (m_Index.TryGetValue(enitity, out SystemEnitiy systemenitiy))
            {
                m_Index.Remove(enitity);
                m_UpdateSystemEnitiys.Remove(systemenitiy);
            }
        }
    }
}