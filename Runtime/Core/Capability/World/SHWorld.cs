namespace GameFrame.Runtime.SH
{
    
    public class SHWorld : World, IFixedUpdateSystem, IInitializeSystem<SHWorld.Input>
    {
        public struct Input
        {
            public int MaxComponentCount;
            public int MaxCapabilityCount;
            public int MaxCapabilityTagsCount;

            public Input(int mcc, int mcac, int mctc)
            {
                MaxComponentCount = mcc;
                MaxCapabilityCount = mcac;
                MaxCapabilityTagsCount = mctc;
            }
        }
        private int maxCapabilityTagsCount;

        private int maxCapabilityCount;
        private Capabilitys capabilitys;
        
        public void OnInitialize(Input input)
        {
            base.OnInitialize(input.MaxComponentCount);
            capabilitys = new Capabilitys();
            capabilitys.OnInitialize(this,input.MaxCapabilityCount);
            maxCapabilityTagsCount = input.MaxCapabilityTagsCount;
        }

        public override ECSEntity AddChild()
        {
            var child = base.AddChild();
            var capabiltyComponet= child.AddComponent<CapabiltyComponent>();
            capabiltyComponet.Init(maxCapabilityTagsCount);
            return child;
        }

        public void BindCapability<T>(int playerId, CapabilitysUpdateMode mode) where T : CapabilityBase
        {
            Assert.IsNotNull(Children[playerId], $"not have {playerId} ecsentity");
            capabilitys.Add<T>(playerId, mode);
        }

        public void UnBindCapability<T>(int playerId) where T : CapabilityBase
        {
            capabilitys.Remove<T>(playerId);
        }

        public void UnBindCapability(int playerId, int capabilitieId)
        {
            capabilitys.Remove(playerId, capabilitieId);
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            capabilitys.OnUpdate(elapseSeconds);
        }

        public void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            capabilitys.OnFixedUpdate(elapseSeconds);
        }
    }
}