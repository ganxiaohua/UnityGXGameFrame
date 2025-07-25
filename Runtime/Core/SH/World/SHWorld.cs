namespace GameFrame.Runtime.SH
{
    public class SHWorld : World, IFixedUpdateSystem
    {
        private Capabilitys capabilitys;

        public void BindCapability<T>(int playerId, CapabilitysUpdateMode mode) where T : CapabilityBase
        {
            Assert.IsNotNull(Children[playerId],$"not have {playerId} ecsentity");
            capabilitys.Add<T>(playerId, mode);
            //TODO:加入Componet;
            // Children[playerId].AddComponent(CapabiltyComponent);
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

        public void FixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            capabilitys.OnFixedUpdate(elapseSeconds);
        }
    }
}