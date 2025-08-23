#if UNITY_EDITOR
using Sirenix.OdinInspector;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        [ShowInInspector] private ObjectPoolManager ObjectPoolManagerEditor = ObjectPoolManager.Instance;
        
        [ShowInInspector] private UISystem UISystem = UISystem.Instance;
        
        [ShowInInspector] private TimerSystem TimerSystem = TimerSystem.Instance;
    }
}
#endif