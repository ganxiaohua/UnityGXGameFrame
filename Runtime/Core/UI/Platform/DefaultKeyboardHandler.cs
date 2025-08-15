using GameFrame.Runtime;

namespace Eden.Core.Runtime.UI.Platform
{
    public sealed class DefaultKeyboardHandler :
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            DefaultWindowsKeyboardHandler
#else
        NoKeyboardHandler
#endif
    {
    }
}