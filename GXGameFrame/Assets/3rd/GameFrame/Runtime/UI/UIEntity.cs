using Cysharp.Threading.Tasks;

namespace GameFrame
{
    public abstract class UIEntity : Entity, IStartSystem, IPreShowSystem, IShowSystem, IHideSystem, IUpdateSystem, IClearSystem
    {
        public abstract string PackName { get; }
        public abstract string WindowName { get; }

        protected DependentUI Despen;

        public UINode UINode { get; set; }

        public void Start()
        {
            Initialize().Forget();
        }
        protected abstract UniTaskVoid Initialize();
        
        public virtual void PreShow(bool isFirstShow)
        {
          
        }
        
        public virtual void Show()
        {
        }

        public virtual void Hide()
        {
        }

        public virtual void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Clear()
        {
            
        }
        
    }
}