using System.Collections.Generic;

namespace GameFrame
{
    public class Context : Entity
    {
        private Dictionary<Matcher, Group> m_Groups;

        protected override void ThisInit()
        {
            m_Groups= new();
        }

        public Group GetGroup(Matcher matcher)
        {
            if (!m_Groups.TryGetValue(matcher, out Group grop))
            {
                grop = Group.CreateGroup(matcher);
                foreach (var item in Children)
                {
                    grop.HandleEntitySilently(item.Value);
                }
                m_Groups.Add(matcher, grop);
            }
            return grop;
        }
        
    }
}