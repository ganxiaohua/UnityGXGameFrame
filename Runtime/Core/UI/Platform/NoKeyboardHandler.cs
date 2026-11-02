#if !GAMEFRAME_USE_UNITYUI
namespace GameFrame.Runtime
{
    public class NoKeyboardHandler : IKeyboardHandler
    {
        public void OnUpdateKeyboardEvent()
        {
        }
    }
}
#endif
