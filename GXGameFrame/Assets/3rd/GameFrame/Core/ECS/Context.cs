using System.Collections.Generic;

namespace GameFrame
{
    public class Context:Entity
    {
        private Dictionary<Matcher, Group> m_Groups;
        
        protected override Entity Create<T>(bool isComponent)
        {
            Context entity = base.Create<T>(isComponent) as Context;
            entity.m_Groups = new();
            return entity;
        }

        public Group GetGroup(Matcher matcher)
        {
            if (!m_Groups.TryGetValue(matcher, out Group grop ))
            {
                grop =  Group.CreateGroup();
                m_Groups.Add(matcher,grop);
            }
            return grop;
        }
    }
}