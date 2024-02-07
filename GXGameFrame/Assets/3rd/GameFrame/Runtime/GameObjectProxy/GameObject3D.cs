using UnityEngine;

namespace Common.Runtime
{
    public class GameObject3D : GameObjectProxy
    {
        private Vector3 cachePosition = Vector3.zero;
        public Vector3 position
        { 
            get
            {
                if (transform) return transform.localPosition;
                return cachePosition;
            }
            set
            {
                if (transform) transform.localPosition = value;
                cachePosition = value;
            }
        }

        private Quaternion cacheRotation = Quaternion.identity;
        public Quaternion rotation
        {
            get
            {
                if (transform) return transform.localRotation;
                return cacheRotation;
            }
            set
            {
                if (transform) transform.localRotation = value;
                cacheRotation = value;
            }
        }

        private Vector3 cacheScale = Vector3.one;
        public Vector3 scale
        {
            get
            {
                if (transform) return transform.localScale;
                return cacheScale;
            }
            set
            {
                if (transform) transform.localScale = value;
                cacheScale = value;
            }
        }
        
        protected override void OnBeforeUnbind()
        {
            base.OnBeforeUnbind();
            if (transform == null)
                return;
            cachePosition = transform.localPosition;
            cacheRotation = transform.localRotation;
            cacheScale = transform.localScale;
        }

        protected override void OnAfterBind()
        {
            base.OnAfterBind();
            // transform.SetLocalPositionAndRotation(cachePosition, cacheRotation);
            transform.localScale = cacheScale;
        }
    }
}
