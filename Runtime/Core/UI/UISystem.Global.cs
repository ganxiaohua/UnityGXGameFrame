using FairyGUI;

namespace GameFrame.Runtime
{
    public sealed partial class UISystem
    {
        public void SetGlobalEnable(bool enableRender, bool enableInput)
        {
            StageCamera.main.enabled = enableRender;
        }
    }
}