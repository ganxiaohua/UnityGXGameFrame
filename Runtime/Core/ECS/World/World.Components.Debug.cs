#if UNITY_EDITOR
namespace GameFrame.Runtime
{
    public partial class World
    {
        private long CompSize;

        private void InitCompSize()
        {
            CompSize = 0;
        }

        private void CalculateSize(long size)
        {
            CompSize += size;
        }

        public void OutputSize()
        {
            Debugger.Log("组件总共内存占用：" + CompSize / 1024.0f / 1024.0f + "mb");
        }
    }
}
#endif