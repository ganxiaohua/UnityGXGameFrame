namespace SH.GameFrame
{
    public class MoveCapability : CapabilityBase
    {
        public override void Init()
        {
        }

        public override bool ShouldActivate()
        {
            return true;
        }

        public override bool ShouldDeactivate()
        {
            return false;
        }

        public override void TickActive(float delatTime)
        {

        }

        public override void Dispose()
        {
            
        }
    }
}