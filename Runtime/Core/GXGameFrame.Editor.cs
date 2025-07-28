#if  UNITY_EDITOR
using Common.Runtime;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace GameFrame.Runtime
{
    public partial class GXGameFrame
    {
        [ShowInInspector]
        private ObjectPoolManager ObjectPoolManagerEditor = ObjectPoolManager.Instance;
        
        GXGameObject go = new GXGameObject();
        [Button]
        public void AddObject()
        {
            go.BindFromAssetAsync("Player/Prefab/Player",transform).Forget();
        }
        
        [Button]
        public void removeObject()
        {
            go.Unbind();
        }
    }
}
#endif