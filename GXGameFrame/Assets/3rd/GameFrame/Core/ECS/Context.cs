using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Context : Entity
    {
        private Dictionary<Matcher, Group> m_Groups;
        private List<KeyValuePair<Matcher,Group>> m_RemoveGroup;

        protected override void ThisInit()
        {
            m_Groups = new();
            m_RemoveGroup = new();
        }

        public T AddChild<T>() where T : ECSEntity
        {
            T ecsEntity = base.AddChild<T>();
            ChangeAddRomoveChildOrCompone(ecsEntity);
            return ecsEntity;
        }

        public void RemoveChild(int id)
        {
            ECSEntity ecsentity = Children[id] as ECSEntity;
            base.RemoveChild(id);
            ChangeAddRomoveChildOrCompone(ecsentity);
        }

        public void ChangeAddRomoveChildOrCompone(ECSEntity ecsEntity)
        {
            foreach (var groupkv in m_Groups)
            {
                Group gourp = groupkv.Value;
                int count = gourp.HandleEntitySilently(ecsEntity);
                if (count == 0)
                {
                    m_RemoveGroup.Add(groupkv);
                }
            }
            foreach (var groupkv in m_RemoveGroup)
            {
                Matcher.RemoveMatcher(groupkv.Key);
                Group.RemoveGroup(groupkv.Value);
                m_Groups.Remove(groupkv.Key);
            }
            m_RemoveGroup.Clear();
        }

        public Group GetGroup(Matcher matcher)
        {
            if (!m_Groups.TryGetValue(matcher, out Group grop))
            {
                grop = Group.CreateGroup(matcher);
                foreach (var item in Children)
                {
                    grop.HandleEntitySilently(item.Value as ECSEntity);
                }
                m_Groups.Add(matcher, grop);
            }

            return grop;
        }
    }
}