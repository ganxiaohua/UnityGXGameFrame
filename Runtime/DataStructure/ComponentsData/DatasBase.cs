using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public interface DatasBase : IInitializeSystem
    {
        public int AddArrayDatas(int size);
        public void RemoveDatas(int index);
    }
}