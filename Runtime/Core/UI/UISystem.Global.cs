using FairyGUI;

namespace GameFrame.Runtime
{
    public sealed partial class UISystem
    {
        public void SetGlobalEnable(bool enableRender, bool enableInput)
        {
            GRoot.inst.visible = enableRender;
            GRoot.inst.touchable = enableInput;
        }
    }
}
