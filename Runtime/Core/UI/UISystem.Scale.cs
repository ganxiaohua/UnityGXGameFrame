using FairyGUI;

namespace GameFrame.Runtime
{
    public sealed partial class UISystem
    {
        private void UpdateScale()
        {
            var scaleFactor = UIContentScaler.scaleFactor;
            Stage.devicePixelRatio = 1.0f / scaleFactor;
        }
    }
}