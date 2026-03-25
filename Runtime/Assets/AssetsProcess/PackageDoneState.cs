namespace GameFrame.Runtime
{
    public class PackageDoneState : FsmState
    {
        public override void OnEnter(FsmController fsmController)
        {
            base.OnEnter(fsmController);
            EventData.Instance.FireAssetEvent(AssetEventType.Succ);
        }
    }
}