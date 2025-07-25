﻿using System;
using System.Collections.Generic;

namespace GameFrame
{
    public partial class World
    {
        public int ChildsCount { get; private set; }

        public GXArray<ECSEntity> Children { get; private set; }

        private Stack<int> heritageId = new();

        private void InitializeChilds()
        {
            Children = new GXArray<ECSEntity>();
        }

        public void EstimateChildsCount(int count)
        {
            ChildsCount = count;
            Children.Init(count);
        }

        public virtual T AddChild<T>() where T : ECSEntity
        {
            return (T)CreateChild(typeof(T));
        }

        public virtual ECSEntity AddChild()
        {
            return CreateChild(typeof(ECSEntity));
        }

        private ECSEntity CreateChild(Type type)
        {
            int id = 0;
            if (!heritageId.TryPop(out id))
            {
                id = ecsSerialId++;
            }

            var entity = Children.Add(id,type);
            entity.SetContext(this);
            entity.OnDirty(this, id);
            return entity;
        }

        public void RemoveChild(IEntity ecsEntity)
        {
            bool b = Children.Remove(ecsEntity.ID);
            if (!b)
                return;
            heritageId.Push(ecsEntity.ID);
        }

        public ECSEntity GetChild(int id)
        {
            return Children[id];
        }

        private void ClearAllChild()
        {
            Children.Dispose();
        }

        private void DisposeChilds()
        {
            ClearAllChild();
            ChildsCount = 0;
            heritageId.Clear();
        }
    }
}