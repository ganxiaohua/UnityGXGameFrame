using System;

namespace GameFrame.Runtime.UnityUI
{
    [Flags]
    public enum PanelFlag
    {
        None = 0,
        Persistent = 0x01,
        Builtin = 0x02,
        Stashable = 0x04,
        NonTopmost = 0x08,
    }
}
