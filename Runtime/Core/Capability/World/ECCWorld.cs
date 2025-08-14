using System.Collections.Generic;
using UnityEngine;

namespace GameFrame.Runtime
{
    public abstract class ECCWorld : World, IFixedUpdateSystem
    {
        private int maxCapabilityCount;

        private int maxCapabilityTag;

        private Capabilitys capabilitys;


        protected void InitCapabilitys(int maxCapabilityCount, int maxTag, int estimatePlayer)
        {
            maxCapabilityTag = maxTag;
            capabilitys = new Capabilitys();
            capabilitys.Init(this, maxCapabilityCount, estimatePlayer);
        }

        public override EffEntity AddChild()
        {
            var child = base.AddChild();
            BindCapability<DestroyCapability>(child);
            var capabiltyComponet = child.AddComponent<CapabiltyComponent>();
            capabiltyComponet.Init(maxCapabilityTag);
            return child;
        }

        public override void RemoveChild(EffEntity effEntity)
        {
            capabilitys.RemoveEffEntitysAllCapability(effEntity);
            base.RemoveChild(effEntity);
        }

        public void GetCapability(EffEntity eff, List<CapabilityBase> update, List<CapabilityBase> fixedUpdate)
        {
            capabilitys.GetCapabilityBaseWithPlayer(eff, update, fixedUpdate);
        }

        public void BindCapability<T>(EffEntity effEntity) where T : CapabilityBase
        {
            Assert.IsNotNull(effEntity, $"not have {effEntity.Name} ecsentity");
            capabilitys.Add<T>(effEntity);
        }

        public void UnBindCapability<T>(EffEntity player) where T : CapabilityBase
        {
            int id = CapabilityID<T, IUpdateSystem>.TID;
            UnBindCapability(player, id);
        }

        public void UnBindCapability(EffEntity player, int capabilitiyId)
        {
            capabilitys.Remove(player, capabilitiyId);
        }

        public bool IsBindCapability(EffEntity player, List<int> tagInts)
        {
            var capabiltyComponent = (CapabiltyComponent) player.GetComponent(ComponentsID<CapabiltyComponent>.TID);
            return capabiltyComponent.IsBlock(tagInts);
        }

        public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            capabilitys.OnUpdate(DeltaTime, realElapseSeconds);
        }

        public void OnFixedUpdate(float elapseSeconds, float realElapseSeconds)
        {
            capabilitys.OnFixedUpdate(elapseSeconds * Multiple, realElapseSeconds);
        }

        public override void Dispose()
        {
            capabilitys.Dispose();
            base.Dispose();
        }
    }
}