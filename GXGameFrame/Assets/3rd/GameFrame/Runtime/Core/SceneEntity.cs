using UnityEngine;

namespace GameFrame
{
    public interface SceneEntityType
    {
    }

    public class SceneEntity : Entity
    {
        public SceneEntity()
        {
        }

        public void Init<T>(Entity parent) where T : SceneEntityType
        {
            ComponentParent = parent;
            EnitityHouse.Instance.AddSceneEntity<T>(this);
        }

        public override void Clear()
        {
            EnitityHouse.Instance.RemoveSceneEntity(this);
            base.Clear();
        }
    }
}