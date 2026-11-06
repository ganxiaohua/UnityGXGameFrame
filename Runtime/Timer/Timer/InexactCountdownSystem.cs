using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 不精确倒计时的主线程驱动入口，各组独立设置检查间隔和每批检查数量。
    /// </summary>
    public sealed class InexactCountdownSystem : Singleton<InexactCountdownSystem>
    {
        private readonly List<InexactCountdownGroup> groups = new();

        public int GroupCount => groups.Count;

        public InexactCountdownGroup CreateGroup(float intervalSeconds = 5f, int maxChecksPerFrame = 30)
        {
            var group = new InexactCountdownGroup(this, intervalSeconds, maxChecksPerFrame);
            group.SystemIndex = groups.Count;
            groups.Add(group);
            return group;
        }

        internal void RemoveGroup(InexactCountdownGroup group)
        {
            var index = group.SystemIndex;
            if (index < 0)
                return;

            int lastIndex = groups.Count - 1;
            if (index != lastIndex)
            {
                var lastGroup = groups[lastIndex];
                groups[index] = lastGroup;
                lastGroup.SystemIndex = index;
            }
            groups.RemoveAt(lastIndex);
            group.SystemIndex = -1;
        }

        /// <summary>
        /// 由 GXGameFrame 每帧调用一次并传入真实时间，各组更新顺序不固定。
        /// 回调中不调用此方法；更新过程中创建的新组从下一帧开始参与。
        /// 回调中只允许删除当前组；删除其他组或清空系统应在遍历之外进行。
        /// </summary>
        public void Update(float currentTime)
        {
            for (int index = groups.Count - 1; index >= 0; index--)
                groups[index].Update(currentTime);
        }

        /// <summary>
        /// 销毁全部组并释放对象引用，不触发到期回调。
        /// </summary>
        public void Clear()
        {
            while (groups.Count > 0)
                groups[groups.Count - 1].Dispose();
        }
    }
}
