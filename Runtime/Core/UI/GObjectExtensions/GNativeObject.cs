using FairyGUI;

namespace GameFrame.Runtime
{
    public sealed class GNativeObject : GObject
    {
        public GoWrapper Wrapper => (GoWrapper) displayObject;

        protected override void CreateDisplayObject()
        {
            displayObject = new GoWrapper();
            displayObject.gOwner = this;
        }
    }
}