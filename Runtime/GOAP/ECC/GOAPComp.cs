namespace GameFrame.Runtime.Components
{
    public struct GOAPComp : EffComponent
    {
        private int valueIndex;

        public void Init(GOAPAgent data)
        {
            valueIndex = ObjectDatas<GOAPAgent>.Instance.AddData(data);
        }

        public GOAPAgent GetData()
        {
            return ObjectDatas<GOAPAgent>.Instance.GetData(valueIndex);
        }

        public static GOAPAgent Create()
        {
            var input = ReferencePool.Acquire<GOAPAgent>();
            return input;
        }

        public void Dispose()
        {
            var data = GetData();
            if (data == null)
                return;
            ReferencePool.Release(data);
            ObjectDatas<GOAPAgent>.Instance.RemoveData(valueIndex);
        }
    }
}