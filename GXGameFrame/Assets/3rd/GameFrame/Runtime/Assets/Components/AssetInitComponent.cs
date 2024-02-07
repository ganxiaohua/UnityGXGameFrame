using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public class AssetInitComponent : Entity, IStartSystem, IUpdateSystem
    {
        public CheckUpdate Check;
        public UniTaskCompletionSource Task;

        public void Start()
        {
            Init().Forget();
        }

        private async UniTaskVoid Init()
        {
            Task = new UniTaskCompletionSource();
            await CheckUpdate();
            Task.TrySetResult();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Clear()
        {
            base.Clear();
        }

        public async UniTask CheckUpdate()
        {
            await AddressablesHelper.InitializeAsync();
            //如果是在editor模式之下,且开启了资源分离(将资源拆到其他的工程里面去)  或者是 非Editor模式
#if (UNITY_EDITOR && RESSEQ) || !UNITY_EDITOR
            Check = new CheckUpdate();
            await Check.CheckVersions();
#endif
        }

        public async UniTask WaitLoad()
        {
            if (Task == null)
            {
                return;
            }
            await Task.Task;
            Task = null;
        }
    }
}