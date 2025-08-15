using FairyGUI;
using UnityEngine;

namespace GameFrame.Runtime
{
    public abstract class LoopItem<T> : ILoopItem
    {
        public GComponent Com { get; private set; }

        public ILoopList Owner { get; private set; }

        public Panel OwnerPanel => Owner.Owner;

        public bool Visible => Com.displayObject.gameObject.activeSelf;

        public bool Selected { get; private set; }

        public int GlobalIndex { get; private set; }

        public T Data { get; private set; }

        public virtual void OnAwake(GComponent com, ILoopList owner)
        {
            Com = com;
            Owner = owner;
        }

        public virtual void OnShow(int globalIndex, T data, bool selected)
        {
            GlobalIndex = globalIndex;
            Data = data;
            Selected = selected;
        }

        public virtual void OnDestroy()
        {
        }

        public void Dispose()
        {
            if (Com.data != this)
            {
                Debug.LogError("Bad Dispose call time");
                return;
            }

            OnDestroy();
            Com.data = null;
            Com = null;
            Owner = null;
            Data = default;
            Selected = false;
        }

        public void Select(bool mono = true)
        {
            if (Selected) return;
            if (mono) Owner.SelectClear();
            Selected = true;
            OnSelectChanged();
            Owner.OnSelectChangedInternal(this);
        }

        public void Deselect()
        {
            if (!Selected) return;
            Selected = false;
            OnSelectChanged();
            Owner.OnSelectChangedInternal(this);
        }

        protected virtual void OnSelectChanged()
        {
        }
    }
}