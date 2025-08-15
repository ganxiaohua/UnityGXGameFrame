using FairyGUI;

namespace GameFrame.Runtime
{
    public static class GObjectExtensions
    {
        public static GObject GetChild(this GGroup group, string name)
        {
            return group.parent.GetChildInGroup(group, name);
        }
    }
}