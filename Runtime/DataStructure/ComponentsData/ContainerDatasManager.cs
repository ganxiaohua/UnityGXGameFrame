using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class ContainerDatasManager : Singleton<ContainerDatasManager>
    {
        private HashSet<IDisposable> objects = new HashSet<IDisposable>();

        public void Add(IDisposable obj)
        {
            Assert.IsNotNull(obj, "obj is null");
            objects.Add(obj);
        }

        public void DestroyAllObject()
        {
            foreach (var obj in objects)
            {
                obj.Dispose();
            }

            objects.Clear();
        }
    }
}