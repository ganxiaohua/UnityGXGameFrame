#if UNITY_EDITOR
namespace GameFrame.Runtime
{
    public partial class World
    {
        private long compSize;

        private void InitCompSize()
        {
            compSize = 0;
        }

        private void CalculateSize(long size)
        {
            compSize += size;
        }

        public void OutputSize()
        {
            Debugger.Log("All Components Size:" + compSize / 1024.0f / 1024.0f + "mb");
        }
    }
}
#endif