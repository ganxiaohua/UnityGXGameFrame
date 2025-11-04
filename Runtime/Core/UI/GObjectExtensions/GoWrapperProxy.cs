using FairyGUI;
using UnityEngine;

namespace GameFrame.Runtime
{
    public class GoWrapperProxy : GameObjectProxy
    {
        public GoWrapper Wrapper { get; private set; }

        public Vector2 xy
        {
            get => Wrapper.xy;
            set => Wrapper.xy = value;
        }

        public float z
        {
            get => Wrapper.z;
            set => Wrapper.z = value;
        }

        public new Vector2 scale
        {
            get => Wrapper.scale;
            set => Wrapper.scale = value;
        }

        public float rotation
        {
            get => Wrapper.rotation;
            set => Wrapper.rotation = value;
        }

        public new bool Visible
        {
            get => Wrapper.visible;
            set => Wrapper.visible = value;
        }

        public int SortingOrder
        {
            get => Wrapper.gOwner.sortingOrder;
            set => Wrapper.gOwner.sortingOrder = value;
        }


        private static GameObject CreateGoWrapper(GGraph graph)
        {
            var wrapper = new GoWrapper();
            graph.SetNativeObject(wrapper);
            return wrapper.gameObject;
        }


        public override void Initialize(object initData)
        {
            base.Initialize(initData);
            Wrapper = (GoWrapper) initData;

            AutoLayers = false;

#if UNITY_EDITOR
            gameObject.name = $"GoWrapperProxy:Native";
#endif
        }


        public override void Dispose()
        {
            base.Dispose();

            Wrapper.Dispose();
            Wrapper = null;
        }

        protected override void OnBeforeUnbind()
        {
            Wrapper.SetWrapTarget(null, false);
        }

        protected override void OnAfterBind(GameObject go)
        {
            Wrapper.SetWrapTarget(go, false);
        }

        protected override void OnRequestRecycle()
        {
            base.OnRequestRecycle();
        }

        public void SetParent(GComponent parent)
        {
            if (parent == null)
                Wrapper.gOwner.RemoveFromParent();
            else
                parent.AddChild(Wrapper.gOwner);
        }

        public void SetParentAt(GComponent parent, int siblingIndex)
        {
            parent.AddChildAt(Wrapper.gOwner, siblingIndex);
        }
    }
}