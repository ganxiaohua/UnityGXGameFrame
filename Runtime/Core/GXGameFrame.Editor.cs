#if UNITY_EDITOR
using Sirenix.OdinInspector;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        [ShowInInspector] private ObjectPoolManager ObjectPoolManagerEditor = ObjectPoolManager.Instance;
    }
}
#endif