using System;
using System.Collections.Generic;

namespace GameFrame
{
    public class Context : Entity
    {
        private Dictionary<ECSEntity, Group> m_ECSEnitiyGroup;
        private Dictionary<Matcher, Group> m_Groups;
        private List<KeyValuePair<Matcher,Group>> m_RemoveGroup;
        

        public override void Initialize()
        {
            m_ECSEnitiyGroup = new();
            m_Groups = new();
            m_RemoveGroup = new();
        }
        
        public new T AddChild<T>() where T : ECSEntity
        {
            T ecsEntity = base.AddChild<T>();
            ChangeAddRomoveChildOrCompone(ecsEntity);
            return ecsEntity;
        }

        public new void RemoveChild(ECSEntity ecsEntity)
        {
            base.RemoveChild(ecsEntity);
            ChangeAddRomoveChildOrCompone(ecsEntity);
        }

        public void ChangeAddRomoveChildOrCompone(ECSEntity ecsEntity)
        {
            foreach (var group in m_Groups.Values)
            {
                Group gourp = group;
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
                    grop.HandleEntitySilently(item as ECSEntity);
                }
                m_Groups.Add(matcher, grop);
            }

            return grop;
        }
        /// <summary>
        /// 清除
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            foreach (var group in m_Groups)
            {
                //Matcher 匹配器其他的Context可能也会用到不需要删除.
                // Matcher.RemoveMatcher(group.Key);
                Group.RemoveGroup(group.Value);
            }
            m_Groups.Clear();
        }
    }
}