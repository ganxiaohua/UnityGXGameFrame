using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public sealed partial class TimerSystem : Singleton<TimerSystem>
    {
        private readonly List<TimerGroup> groups = new();
        private int nextGroupIndex = -1;
        private long groupSequence;
        private TimerGroup uiGroup;

        public int GroupCount => groups.Count;

        /// <summary>
        /// UI 共用的内置组；组被释放后，下次访问时重新创建。
        /// </summary>
        public TimerGroup UIGroup => uiGroup ??= CreateGroup("UI");

        public TimerGroup CreateGroup(string name = null)
        {
            var group = new TimerGroup(this, ++groupSequence, name);
            group.SystemIndex = groups.Count;
            groups.Add(group);
            return group;
        }

        internal void RemoveGroup(TimerGroup group)
        {
            var index = group.SystemIndex;
            if (index < 0)
                return;

            if (nextGroupIndex >= 0 && index < nextGroupIndex)
            {
                // 用已扫描区尾项补位，把未扫描的尾项留在回退后的游标处。
                int lastVisitedIndex = --nextGroupIndex;
                if (index != lastVisitedIndex)
                {
                    var visitedGroup = groups[lastVisitedIndex];
                    groups[index] = visitedGroup;
                    visitedGroup.SystemIndex = index;
                }
                index = lastVisitedIndex;
            }

            int lastIndex = groups.Count - 1;
            if (index != lastIndex)
            {
                var lastGroup = groups[lastIndex];
                groups[index] = lastGroup;
                lastGroup.SystemIndex = index;
            }
            groups.RemoveAt(lastIndex);
            group.SystemIndex = -1;
            if (uiGroup == group)
                uiGroup = null;
        }

        /// <summary>
        /// 每帧传入真实时间，各组更新顺序不固定；回调中不调用此方法。
        /// 本帧新建的组从下一帧开始参与，跨组不保证全局到期顺序。
        /// 回调异常直接向外传播，下次更新重新初始化扫描游标。
        /// </summary>
        public void Update(float currentTime)
        {
            long frameLastSequence = groupSequence;
            nextGroupIndex = 0;
            while (nextGroupIndex < groups.Count)
            {
                var group = groups[nextGroupIndex++];
                // 尾部交换后新组可能位于旧组之前，只跳过新组，不能结束遍历。
                if (group.SystemSequence > frameLastSequence)
                    continue;
                group.Update(currentTime);
            }
            nextGroupIndex = -1;
        }

        /// <summary>
        /// 释放全部组并取消计时器，不触发回调；UIGroup 可按需重新创建。
        /// </summary>
        public void Clear()
        {
            while (groups.Count > 0)
                groups[groups.Count - 1].Dispose();
        }
    }
}
