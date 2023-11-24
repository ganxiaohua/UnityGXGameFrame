using UnityEngine;

namespace Common.Runtime
{
    public class GameObjectRoot3D
    {
        public readonly GameObject3D avatar;
        
        public readonly Transform root;
        
        public Vector3 position
        { 
            get
            {
                return root.localPosition;
            }
            set
            {
                root.localPosition = value;
            }
        }

        public Quaternion rotation
        {
            get
            {
                return root.localRotation;
            }
            set
            {
                root.localRotation = value;
            }
        }

        public Vector3 scale
        {
            get
            {
                return root.localScale;
            }
            set
            {
                root.localScale = value;
            }
        }

        public GameObjectRoot3D()
        {
            avatar = new GameObject3D();
            avatar.onAfterBind += () =>
            {
                avatar.transform.SetParent(root);
                // avatar.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                avatar.transform.localScale = Vector3.one;
            };

            root = new GameObject().transform;
        }
    }
}
