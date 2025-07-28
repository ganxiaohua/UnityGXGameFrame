using System;
using UnityEditor;

namespace GameFrame.Editor
{
    public struct LabelWidthScope : IDisposable
    {
        private bool disposed;
        private float width;

        public LabelWidthScope(float width)
        {
            this.disposed = false;
            this.width = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                EditorGUIUtility.labelWidth = width;
            }
        }
    }
}