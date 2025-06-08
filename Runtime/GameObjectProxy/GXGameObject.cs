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
                if (GoBase!=null) return GoBase.Tra.localPosition;
                return cacheLocalPosition;
            }
            set
            {
                isLocalPos = true;
                if (GoBase!=null) GoBase.Tra.localPosition = value;
                cacheLocalPosition = value;
            }
        }

        private Vector3 cachePosition = Vector3.zero;

        public Vector3 position
        {
            get
            {
                if (GoBase!=null)  return GoBase.Tra.position;
                return cachePosition;
            }
            set
            {
                isLocalPos = false;
                if (GoBase!=null)  GoBase.Tra.position = value;
                cachePosition = value;
            }
        }

        private Quaternion cachelocalRotation = Quaternion.identity;

        public Quaternion localRotation
        {
            get
            {
                if (GoBase!=null)  return GoBase.Tra.localRotation;
                return cachelocalRotation;
            }
            set
            {
                isLocalRot = true;
                if (GoBase!=null)  GoBase.Tra.localRotation = value;
                cachelocalRotation = value;
            }
        }


        private Quaternion cacheRotation = Quaternion.identity;

        public Quaternion rotation
        {
            get
            {
                if (GoBase!=null)  return GoBase.Tra.rotation;
                return cacheRotation;
            }
            set
            {
                isLocalRot = false;
                if (GoBase!=null)  GoBase.Tra.rotation = value;
                cacheRotation = value;
            }
        }

        private Vector3 cacheScale = Vector3.one;

        public Vector3 scale
        {
            get
            {
                if (GoBase!=null)  return GoBase.Tra.localScale;
                return cacheScale;
            }
            set
            {
                if (GoBase!=null)  GoBase.Tra.localScale = value;
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