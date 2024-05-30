using UnityEngine;

namespace Common.Runtime
{
    public class GXGameObject : GameObjectProxy
    {
        private bool isLocalPos;

        private bool isLocalRot;

        private Vector3 cacheLocalPosition = Vector3.zero;

        public Vector3 localPosition
        {
            get
            {
                if (transform) return transform.localPosition;
                return cacheLocalPosition;
            }
            set
            {
                isLocalPos = true;
                if (transform) transform.localPosition = value;
                cacheLocalPosition = value;
            }
        }

        private Vector3 cachePosition = Vector3.zero;

        public Vector3 position
        {
            get
            {
                if (transform) return transform.position;
                return cachePosition;
            }
            set
            {
                isLocalPos = false;
                if (transform) transform.position = value;
                cachePosition = value;
            }
        }

        private Quaternion cachelocalRotation = Quaternion.identity;

        public Quaternion localRotation
        {
            get
            {
                if (transform) return transform.localRotation;
                return cachelocalRotation;
            }
            set
            {
                isLocalRot = true;
                if (transform) transform.localRotation = value;
                cachelocalRotation = value;
            }
        }


        private Quaternion cacheRotation = Quaternion.identity;

        public Quaternion rotation
        {
            get
            {
                if (transform) return transform.rotation;
                return cacheRotation;
            }
            set
            {
                isLocalRot = false;
                if (transform) transform.rotation = value;
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
        }

        protected override void OnAfterBind()
        {
            base.OnAfterBind();
            scale = cacheScale;
            if (isLocalPos)
                localPosition = cacheLocalPosition;
            else
            {
                position = cachePosition;
            }

            if (isLocalRot)
                localRotation = cachelocalRotation;
            else
            {
                rotation = cacheRotation;
            }
        }
    }
}