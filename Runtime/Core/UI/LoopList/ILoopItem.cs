using System;
using FairyGUI;

namespace GameFrame.Runtime
{
    public interface ILoopItem : IDisposable
    {
        GComponent Com { get; }

        ILoopList Owner { get; }

        bool Visible { get; }

        bool Selected { get; }

        int GlobalIndex { get; }

        /// <summary>
        /// single selection if mono is true
        /// </summary>
        void Select(bool mono = true);

        void Deselect();
    }
}