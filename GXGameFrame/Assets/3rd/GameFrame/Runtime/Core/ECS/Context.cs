using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Context : Entity
    {
        private Dictionary<Matcher, Group> m_Groups;
        private List<KeyValuePair<Matcher,Group>> m_RemoveGroup;

        public override void Initialize()
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
                gourp.HandleEntitySilently(ecsEntity);
            }
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