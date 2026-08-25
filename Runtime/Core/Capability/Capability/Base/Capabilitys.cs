namespace GameFrame.Runtime
{
    public partial class Capabilitys
    {
        private JumpIndexArray<CapabilityBase>[] capabilitiesUpdateList;

        private JumpIndexArray<CapabilityBase>[] capabilitiesFixUpdateList;

        private JumpIndexArray<CapabilityBase>[] capabilitiesLateUpdateList;
        private int estimatedNumberPlayer;

        private ECCWorld eccWorld;

        public void Init(ECCWorld eccWorld, int updateCapabilityCount, int fixedUpdateCapabilityCount, int lateUpdateCapabilityCount,
            int estimatedNumberPlayer)
        {
            this.eccWorld = eccWorld;
            this.estimatedNumberPlayer = estimatedNumberPlayer;
            capabilitiesUpdateList = new JumpIndexArray<CapabilityBase>[updateCapabilityCount];
            capabilitiesFixUpdateList = new JumpIndexArray<CapabilityBase>[fixedUpdateCapabilityCount];
            capabilitiesLateUpdateList = new JumpIndexArray<CapabilityBase>[lateUpdateCapabilityCount];
        }

        public void OnUpdate(float delatTime, float realElapseSeconds)
        {
            ConvenientCapabilitys(capabilitiesUpdateList, delatTime, realElapseSeconds);
        }

        public void OnFixedUpdate(float delatTime, float realElapseSeconds)
        {
            ConvenientCapabilitys(capabilitiesFixUpdateList, delatTime, realElapseSeconds);
        }

        public void OnLateUpdate(float delatTime, float realElapseSeconds)
        {
            ConvenientCapabilitys(capabilitiesLateUpdateList, delatTime, realElapseSeconds);
        }


        private void ConvenientCapabilitys(JumpIndexArray<CapabilityBase>[] arrays, float delatTime, float realElapseSeconds)
        {
            int count = arrays.Length;
            if (count == 0)
                return;
            for (int i = 0; i < count; i++)
            {
                var capabilityArray = arrays[i];
                if (capabilityArray == null)
                    continue;
#if UNITY_EDITOR
                using (new Profiler(GetCapabilityName(capabilityArray)))
#endif
                    UpdateCapability(capabilityArray, delatTime, realElapseSeconds);
            }
        }

#if UNITY_EDITOR
        private static string GetCapabilityName(JumpIndexArray<CapabilityBase> capabilityArray)
        {
            foreach (var capability in capabilityArray)
            {
                if (capability != null)
                    return capability.GetType().Name;
            }

            return nameof(CapabilityBase);
        }
#endif

        private void UpdateCapability(JumpIndexArray<CapabilityBase> capabilityBaseArrayEx, float delatTime, float realElapseSeconds)
        {
            foreach (var capability in capabilityBaseArrayEx)
            {
                var owner = capability.Owner;
                if (capability.TagList != null)
                {
                    var capabilityComp = owner.GetComponent<CapabilityComponent>();
                    if (capabilityComp.IsBlock(capability.TagList))
                        continue;
                }

                if (capability.TryComponentChanges)
                {
                    if (!capability.IsActive)
                    {
                        bool succ = capability.ShouldActivate();
                        if (succ)
                        {
                            capability.OnActivated();
                        }
                    }
                    else
                    {
                        bool succ = capability.ShouldDeactivate();
                        if (succ)
                        {
                            capability.OnDeactivated();
                        }
                    }
                }

                if (capability.IsActive)
                    capability.TickActive(delatTime, realElapseSeconds);
            }
        }

        public void Dispose()
        {
            ClearCapabilities(capabilitiesUpdateList);
            ClearCapabilities(capabilitiesFixUpdateList);
            ClearCapabilities(capabilitiesLateUpdateList);
            capabilitiesUpdateList = null;
            capabilitiesFixUpdateList = null;
            capabilitiesLateUpdateList = null;
        }

        private void ClearCapabilities(JumpIndexArray<CapabilityBase>[] arrays)
        {
            if (arrays == null)
                return;

            foreach (var array in arrays)
            {
                if (array == null)
                    continue;
                foreach (var item in array)
                {
                    ReferencePool.Release(item);
                }

                array.Dispose();
            }
        }
    }
}