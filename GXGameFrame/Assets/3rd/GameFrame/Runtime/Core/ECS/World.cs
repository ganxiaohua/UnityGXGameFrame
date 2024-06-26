﻿using System.Collections.Generic;

namespace GameFrame
{
    public class World : Entity, IStartSystem, IUpdateSystem
    {
        private Dictionary<Matcher, Group> m_Groups = new();
        private List<Group>[] m_GroupsList;
        public float DeltaTime { get; private set; }
        public float Multiple { get; private set; }

        public virtual void Start()
        {
            SetMultiple(1);
            m_GroupsList = new List<Group>[GXComponents.ComponentTypes.Length];
        }

        protected virtual void SetMultiple(float mul)
        {
            Multiple = mul;
        }


        public ECSEntity AddChild()
        {
            ECSEntity ecsEntity = AddChild<ECSEntity>();
            ecsEntity.SetContext(this);
            return ecsEntity;
        }

        public void RemoveChild(ECSEntity ecsEntity)
        {
            base.RemoveChild(ecsEntity);
        }

        public void Reactive(List<int> indexs, ECSEntity ecsEntity)
        {
            foreach (var cid in indexs)
            {
                Reactive(cid, ecsEntity);
            }
        }

        public Group GetGroup(Matcher matcher)
        {
            if (m_Groups.TryGetValue(matcher, out Group grop)) return grop;
            grop = Group.CreateGroup(matcher);
            foreach (var item in Children)
            {
                grop.HandleEntitySilently((ECSEntity) item);
            }

            m_Groups.Add(matcher, grop);
            foreach (var cid in matcher.Indices)
            {
                m_GroupsList[cid] ??= new List<Group>(128);
                m_GroupsList[cid].Add(grop);
            }

            return grop;
        }

        public void Reactive(int comid, ECSEntity entity)
        {
            var groupList = m_GroupsList[comid];
            if (groupList != null)
            {
                foreach (var t in groupList)
                {
                    t.HandleEntity(entity);
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            foreach (var group in m_Groups)
            {
                Matcher.RemoveMatcher(group.Key);
                Group.RemoveGroup(group.Value);
            }

            m_GroupsList = null;
            m_Groups.Clear();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            DeltaTime = elapseSeconds * Multiple;
        }
    }
}