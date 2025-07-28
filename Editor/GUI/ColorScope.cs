using System;
using UnityEngine;

namespace GameFrame.Runtime.Editor
{
    public struct ColorScope : IDisposable
    {
        private bool disposed;
        private Color color;

        public ColorScope(Color color)
        {
            this.disposed = false;
            this.color = GUI.color;
            GUI.color = color;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                GUI.color = color;
            }
        }
    }
}