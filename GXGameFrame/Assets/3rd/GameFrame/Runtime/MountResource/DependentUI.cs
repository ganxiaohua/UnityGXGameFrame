using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame
{
    public class DependentUI : Entity, IStartSystem<string,string>, IClearSystem
    {
        public DefaultAssetReference DefaultAssetReference;
        public GObject Window;
        private UniTaskCompletionSource m_WaitLoadTask;

        public void Start(string packageName,string windowName)
        {
            DefaultAssetReference = new DefaultAssetReference();
            m_WaitLoadTask = new UniTaskCompletionSource();
            Init(packageName, windowName).Forget();
        }

        private async UniTaskVoid Init(string packageName, string windowName)
        {
            await UILoader.Instance.AddPackage(packageName, DefaultAssetReference);
            Window = UIPackage.CreateObject(packageName, windowName);
            var succ = await UILoader.Instance.LoadOver(packageName);
            if (succ)
            {
                m_WaitLoadTask.TrySetResult();
                return;
            }
            m_WaitLoadTask.TrySetCanceled();
        }
        
        public  async UniTask<bool> WaitLoad()
        {
            if (m_WaitLoadTask == null)
            {
                return false;
            }

            await m_WaitLoadTask.Task.SuppressCancellationThrow();
            if (m_WaitLoadTask.GetStatus(0) == UniTaskStatus.Succeeded)
            {
                return true;
            }
            return false;
        }
        
        public override void Clear()
        {
            base.Clear();
            m_WaitLoadTask?.TrySetCanceled();
        }
    }
}