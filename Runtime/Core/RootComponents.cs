namespace GameFrame.Runtime
{
    /// <summary>
    /// only
    /// </summary>
    public class UIRootComponents : Entity, IInitializeSystem
    {
        public static UIRootComponents sInstance;

        public void OnInitialize()
        {
            sInstance = this;
        }

        public override void Dispose()
        {
            sInstance = null;
        }
    }

    /// <summary>
    /// only
    /// </summary>
    public class FsmComponents : Entity, IInitializeSystem
    {
        public static FsmComponents sInstance;

        public void OnInitialize()
        {
            sInstance = this;
        }

        public override void Dispose()
        {
            sInstance = null;
        }
    }
}