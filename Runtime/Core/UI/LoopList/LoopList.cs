using System;
using FairyGUI;

namespace GameFrame.Runtime
{
    public sealed partial class LoopList<T, T_Data> : ILoopList where T : LoopItem<T_Data>, new()
    {
        public Panel Owner { get; private set; }

        public bool Visible
        {
            get => glist.visible;
            set => glist.visible = value;
        }

        public int Count
        {
            get => glist.numItems;
            set
            {
                Timing = LoopTiming.HardUpdate;
                glist.numItems = value;
                Timing = LoopTiming.UserScroll;
            }
        }

        public LoopTiming Timing { get; private set; }

        public Action<ILoopItem> OnSelectItem { get; set; }

        public int LastSelectItemGlobalIndex { get; private set; }

        private GList glist;

        private T[] items;
        private int itemSize;

        private BitList selects;

        private Func<int, T_Data> dataGetter;

        /// <summary>
        /// T_Data data = dataGetter(globalIndex);
        /// </summary>
        public LoopList(GList glist, Panel owner, Func<int, T_Data> dataGetter)
        {
            this.Owner = owner;
            this.glist = glist;
            this.glist.itemRenderer = OnItemShow;
            this.items = new T[16];
            this.itemSize = 0;
            this.selects = new BitList();
            this.dataGetter = dataGetter;
            if (glist.scrollPane != null)
                this.glist.SetVirtual();
            this.LastSelectItemGlobalIndex = -1;
#if UNITY_EDITOR
            DebugInspector.Register(this, glist, typeof(T));
#endif
        }

        public void Dispose()
        {
            LastSelectItemGlobalIndex = -1;
            OnSelectItem = null;
            for (var i = 0; i < itemSize; i++)
            {
                items[i].Dispose();
            }

            items = null;
            itemSize = 0;
            selects.Clear(true);
            glist.itemRenderer = null;
            glist = null;
            dataGetter = null;
            Owner = null;
        }

        public void ScrollToView(int globalIndex, bool smooth = true, bool setFirst = false)
        {
            glist.ScrollToView(globalIndex, smooth, setFirst);
        }

        public void Select(int globalIndex, bool mono = true)
        {
            foreach (var item in this)
            {
                if (item.GlobalIndex == globalIndex)
                {
                    item.Select(mono);
                    return;
                }
            }
        }

        public void SelectClear()
        {
            foreach (var item in this)
            {
                item.Deselect();
            }

            selects.Clear();
            LastSelectItemGlobalIndex = -1;
        }

        public void OnSelectChangedInternal(ILoopItem item)
        {
            selects[item.GlobalIndex] = item.Selected;
            if (item.Selected)
            {
                LastSelectItemGlobalIndex = item.GlobalIndex;
                OnSelectItem?.Invoke(item);
            }
        }

        public bool IsSelected(int globalIndex)
        {
            return selects[globalIndex];
        }

        private void OnItemShow(int globalIndex, GObject itemObj)
        {
            T item = (T) itemObj.data;
            if (item == null)
            {
                item = new T();
                itemObj.data = item;

                if (itemSize == items.Length)
                {
                    var newItems = new T[itemSize * 2];
                    Array.Copy(items, newItems, itemSize);
                    items = newItems;
                }

                items[itemSize++] = item;

                item.OnAwake((GComponent) itemObj, this);
            }

            T_Data data = dataGetter != null ? dataGetter(globalIndex) : default(T_Data);
            item.OnShow(globalIndex, data, selects[globalIndex]);
        }

        public void Refresh(int count = -1)
        {
            Timing = LoopTiming.SoftUpdate;
            if (count < 0)
            {
                glist.numItems = glist.numItems;
            }
            else
            {
                glist.numItems = count;
            }

            Timing = LoopTiming.UserScroll;
        }


        public ILoopItem Find(int globalIndex)
        {
            foreach (var item in this)
            {
                if (item.GlobalIndex == globalIndex)
                    return item;
            }

            return null;
        }
    }
}