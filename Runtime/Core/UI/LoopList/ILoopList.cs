using System;

namespace GameFrame.Runtime
{
    public interface ILoopList : IDisposable
    {
        Panel Owner { get; }

        bool Visible { get; set; }

        /// <summary>
        /// Hard update
        /// </summary>
        int Count { get; set; }

        LoopTiming Timing { get; }

        Action<ILoopItem> OnSelectItem { get; set; }

        int LastSelectItemGlobalIndex { get; }

        void ScrollToView(int globalIndex, bool smooth = true, bool setFirst = false);

        void Select(int globalIndex, bool mono = true);

        void SelectClear();

        void OnSelectChangedInternal(ILoopItem item);

        bool IsSelected(int globalIndex);

        /// <summary>
        /// Soft update
        /// </summary>
        /// <param name="count">negative: keep, otherwise: replace</param>
        void Refresh(int count = -1);


        ILoopItem Find(int globalIndex);
    }
}