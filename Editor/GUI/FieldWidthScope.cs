using System;
using UnityEditor;

namespace GameFrame.Editor
{
    public struct FieldWidthScope : IDisposable
    {
        private bool disposed;
        private float width;

        public FieldWidthScope(float width)
        {
            this.disposed = false;
            this.width = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = width;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                EditorGUIUtility.fieldWidth = width;
            }
        }
    }
}