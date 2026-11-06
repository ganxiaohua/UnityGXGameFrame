using System.Collections.Generic;

namespace GameFrame.Runtime
{
    /// <summary>
    /// 不精确倒计时的主线程驱动入口，各组独立分配轮询间隔和每帧检查预算。
    /// </summary>
    public sealed class InexactCountdownSystem : Singleton<InexactCountdownSystem>
    {
        private readonly LinkedList<InexactCountdownGroup> groups = new();
        private LinkedListNode<InexactCountdownGroup> nextGroup;
        private long groupSequence;

        public int GroupCount => groups.Count;

        public InexactCountdownGroup CreateGroup(float intervalSeconds = 5f, int maxChecksPerFrame = 30)
        {
            var group = new InexactCountdownGroup(this, ++groupSequence, intervalSeconds, maxChecksPerFrame);
            group.SystemNode = groups.AddLast(group);
            return group;
        }

        internal void RemoveGroup(InexactCountdownGroup group)
        {
            var node = group.SystemNode;
            if (node == null)
                return;

            if (nextGroup == node)
                nextGroup = node.Next;
            groups.Remove(node);
            group.SystemNode = null;
        }

        /// <summary>
        /// 由 GXGameFrame 每帧调用一次并传入真实时间，回调中不调用此方法。
        /// 更新过程中创建的新组从下一帧开始参与。
        /// </summary>
        public void Update(float currentTime)
        {
            long frameLastSequence = groupSequence;
            nextGroup = groups.First;
            while (nextGroup != null && nextGroup.Value.SystemSequence <= frameLastSequence)
            {
                var group = nextGroup.Value;
                nextGroup = nextGroup.Next;
                group.Update(currentTime);
            }
            nextGroup = null;
        }

        /// <summary>
        /// 销毁全部组并释放对象引用，不触发到期回调。
        /// </summary>
        public void Clear()
        {
            while (groups.First != null)
                groups.First.Value.Dispose();
        }
    }
}
