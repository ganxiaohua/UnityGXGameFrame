﻿using System;

namespace GameFrame.Runtime
{
    public interface IEntity : IDisposable, IContinuousID
    {
        public enum EntityState : byte
        {
            None = 0,
            IsRunning,
            IsClear
        }

        public IEntity Parent { get; }

        public string Name { get; }

        public EntityState State { get; }

        public bool IsAction { get; }

        public void OnDirty(IEntity parent, int id);
    }
}