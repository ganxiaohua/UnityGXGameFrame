using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FairyGUI;
using Time = UnityEngine.Time;

namespace GameFrame
{
    public class UIManager : Singleton<UIManager>
    {
        /// <summary>
        /// 回收UI主题
        /// </summary>
        public class RecycleWindow : IDisposable
        {
            /// <summary>
            /// 回收UI类型
            /// </summary>
            public Type RecycleWindowType;

            /// <summary>
            /// 回收UI最大缓存时间
            /// </summary>
            public float DelayTime;

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// UI的队列
        /// </summary>
        private GameFrameworkLinkedList<UINode> mUILinkedLinkedList;

        /// <summary>
        /// 正在打开的窗口
        /// </summary>
        private Stack<UINode> mOpenUIList;

        /// <summary>
        /// 等待打开的窗口
        /// </summary>
        private HashSet<Type> mWaitOpenUIList;

        /// <summary>
        /// 等待关闭的窗口
        /// </summary>
        private Dictionary<Type, UINode> mWaitCloseUIDic;

        /// <summary>
        /// 缓存的窗口类型
        /// </summary>
        private Dictionary<Type, RecycleWindow> mRecycleWindowDic;

        /// <summary>
        /// 需要在RecycleWindowDic进行删除操作的临时存储
        /// </summary>
        private List<Type> mTempRecycleWindow;

        /// <summary>
        /// 当前回收UI检查时间
        /// </summary>
        private float mCurAutoReleaseTime;

        /// <summary>
        /// 回收UI检查间隔
        /// </summary>
        private float mAutoReleaseInterval;

        /// <summary>
        /// 回收UI保存时间,超过时间就销毁
        /// </summary>
        private int mReleaseTime;

        /// <summary>
        /// 最大暂存数
        /// </summary>
        public const int MaxRecycleCount = 6;

        public UIManager()
        {
            mWaitOpenUIList = new(16);
            mUILinkedLinkedList = new();
            mOpenUIList = new(16);
            mWaitCloseUIDic = new(16);
            mRecycleWindowDic = new(16);
            mTempRecycleWindow = new(16);
            mAutoReleaseInterval = 10;
            mReleaseTime = 10;
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            //有等待加入的UI在队列里的时候有限这个,然后将m_OpenUIList隐藏
            if (mWaitOpenUIList.Count > 0 && IsAction() == false)
            {
                OpenUIS(mWaitOpenUIList);
                mWaitOpenUIList.Clear();
            }
            else if (mOpenUIList.Count > 0 && mWaitCloseUIDic.Count == 0)
            {
                for (int i = 0; i < mOpenUIList.Count; i++)
                {
                    mOpenUIList.Pop().Show();
                }

                mOpenUIList.Clear();
            }

            TimeoutDeletion(elapseSeconds);
        }

        /// <summary>
        /// 打开一系列UI窗口一般用于战斗结束 需要回退到某个特殊UI
        /// 只会打开最后一个窗口,其他的都是以空节点的形态保存.
        /// </summary>
        /// <param name="types"></param>
        public void OpenUIS(HashSet<Type> types)
        {
            if (IsAction())
            {
                foreach (Type obj in types)
                    mWaitOpenUIList.Add(obj);
                return;
            }

            Type openPanel = null;
            foreach (var type in types)
            {
                UINode findNode = FindUINode(type);
                if (findNode != null)
                {
                    RemoveNode(findNode);
                    AddLastNode(findNode);
                    continue;
                }

                openPanel = type;
                UINode node = UINode.CreateEmptyNode(type);
                AddLastNode(node);
            }

            OpenUI(openPanel);
        }

        /// <summary>
        /// 打开一个UI
        /// </summary>
        /// <param name="type"></param>
        public void OpenUI(Type type, object data = null)
        {
            //如果打开的UI就是最上层的UI
            UINode curUINode = GetCurUINode();

            //如果最上层节点就是想要打开的节点则什么都不做
            if (curUINode != null && curUINode.WindowType == type)
            {
                //如果back的时候是个空节点,在back的时候会设置不可点击,所以如果打开的本身 需要重新打开可以点击.
                //一般而言不会有这样的操作,但是架不住别人胡乱使用.
                SetTouchable(true);
                return;
            }

            if (IsAction())
            {
                mWaitOpenUIList.Add(type);
                return;
            }

            SetTouchable(false);
            //如果需要打开的UI在列表中则将其拉扯到最上面进行打开
            UINode findUINode = FindUINode(type);
            if (findUINode != null)
            {
                RemoveNode(findUINode);
                AddLastNode(findUINode);
                mOpenUIList.Push(findUINode);
                UIHideOrDisposeWithNextUIType(curUINode);
                return;
            }

            //打开新窗口
            Open(type, data).Forget();
        }


        /// <summary>
        /// 打开一个UI
        /// </summary>
        /// <param name="type"></param>
        private async UniTaskVoid Open(Type type, object data)
        {
            RemoveRecycleWindowDic(type);
            UINode curUINode = GetCurUINode();
            UINode uiNode = UINode.CreateNode(type, data);
            AddLastNode(uiNode);
            bool loadover = await uiNode.LoadMustDependentOver();
            if (!loadover)
            {
                RemoveNode(uiNode);
                return;
            }

            uiNode.PreShow(true);
            bool wait = await uiNode.UIWait();
            if (!wait)
            {
                return;
            }

            //加入等待打开的UI列表
            mOpenUIList.Push(uiNode);
            if (curUINode != null)
            {
                UIHideOrDisposeWithNextUIType(curUINode);
            }
        }


        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parent"></param>
        public async UniTaskVoid AddChildUI(Type type, UINode parent, GComponent root, object obj = null)
        {
            Assert.IsTrue(parent != null, $"找不到父节点{parent.Name}");
            UINode uinode = UINode.CreateNode(type, obj);
            bool loadover = await uinode.LoadMustDependentOver(root);
            if (!loadover)
            {
                return;
            }

            uinode.PreShow(true);
            bool wait = await uinode.UIWait();
            if (!wait)
            {
                return;
            }

            uinode.Show();
            uinode.Parent = parent;
            parent.Childs.Add(uinode);
        }

        public void RemoveChildUI(UINode uiNode)
        {
            uiNode.Parent.Childs.Remove(uiNode);
            uiNode.Parent = null;
            AddWaitDestroyWindowList(uiNode);
        }

        /// <summary>
        /// 从当前界面返回到上一层界面
        /// </summary>
        public void Back()
        {
            UINode curUINode = GetCurUINode();
            UINode beforeNode = GetCurBeforeNode();
            if (curUINode == null || beforeNode == null)
            {
                return;
            }

            NodeShowTypeChick(beforeNode).Forget();
        }

        /// <summary>
        /// 从当前界面返回到第一层界面
        /// </summary>
        public void Back2Frist()
        {
            if (mUILinkedLinkedList.Count <= 1)
            {
                return;
            }

            //倒数第二个
            LinkedListNode<UINode> Penult = mUILinkedLinkedList.Last.Previous;
            while (mUILinkedLinkedList.First != mUILinkedLinkedList.Last)
            {
                DestroyNode(Penult.Value);
                RemoveNode(Penult.Value);
                Penult = Penult.Previous;
            }

            var curUINode = GetFirstUINode();
            NodeShowTypeChick(curUINode).Forget();
        }

        /// <summary>
        /// 如果window不为空就打开. 如果不存在代表这只是一个占用节点,删除此节点,重新走OpenUI逻辑.
        /// </summary>
        /// <param name="uiNode"></param>
        private async UniTaskVoid NodeShowTypeChick(UINode uiNode)
        {
            SetTouchable(false);
            if (uiNode.Window != null)
            {
                uiNode.PreShow(false);
                await uiNode.UIWait();
                AddWaitDestroyWindowList(GetCurUINode());
                mOpenUIList.Push(uiNode);
            }
            else
            {
                FixNextUIType(GetCurUINode(), WindowState.Destroy);
                RemoveNode(uiNode);
                DestroyNode(uiNode);
                OpenUI(uiNode.WindowType);
            }
        }


        /// <summary>
        /// 当前是否是活跃的状态,当时活跃的状态就将其他的操作延后处理
        /// </summary>
        /// <returns></returns>
        private bool IsAction()
        {
            UINode curUINode = GetCurUINode();
            if (curUINode != null && curUINode.NodeState != UINode.StateType.Open)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 设置是否可以触摸状态
        /// </summary>
        /// <param name="able"></param>
        /// <returns></returns>
        private void SetTouchable(bool able)
        {
            FairyGUI.GRoot.inst.touchable = able;
        }

        /// <summary>
        /// 删除一个节点,加入等待关闭列表
        /// </summary>
        /// <param name="uiNode"></param>
        private void AddWaitDestroyWindowList(UINode uiNode)
        {
            RecycleWindow recycleWindow = ReferencePool.Acquire<RecycleWindow>();
            recycleWindow.RecycleWindowType = uiNode.WindowType;
            recycleWindow.DelayTime = Time.realtimeSinceStartup + mReleaseTime;
            mRecycleWindowDic.Add(uiNode.WindowType, recycleWindow);
            RemoveNode(uiNode);
            AddWaitCloseWindowList(uiNode);
            DestroyNode(uiNode);
        }

        /// <summary>
        /// 关闭一个阶段，加入等待关闭列表
        /// </summary>
        /// <param name="uiNode"></param>
        private void AddWaitCloseWindowList(UINode uiNode)
        {
            uiNode.NodeState = UINode.StateType.WaitDestroy;
            mWaitCloseUIDic.Add(uiNode.WindowType, uiNode);
        }

        /// <summary>
        /// 删除一个节点
        /// </summary>
        /// <param name="uiNode"></param>
        private void DestroyNode(UINode uiNode)
        {
            UINode.DestroyNode(uiNode);
        }


        /// <summary>
        /// 界面彻底关闭
        /// </summary>
        public void UIClose(Type type)
        {
            if (mWaitCloseUIDic.ContainsKey(type))
            {
                mWaitCloseUIDic[type].NodeState = UINode.StateType.Destroy;
                mWaitCloseUIDic.Remove(type);
                RecycleWindowMaxClear();
            }
            else
            {
                Debugger.LogError($"{type.Name} not in WaitCloseUIDic");
            }

            if (!IsAction())
            {
                SetTouchable(true);
            }
        }

        /// <summary>
        /// 界面彻底打开
        /// </summary>
        public void UIOpened(Type type)
        {
            GetCurUINode().NodeState = UINode.StateType.Open;
            SetTouchable(true);
        }

        /// <summary>
        /// 获得到当前节点
        /// </summary>
        /// <returns></returns>
        private UINode GetFirstUINode()
        {
            if (mUILinkedLinkedList.First == null)
            {
                return null;
            }

            return mUILinkedLinkedList.First.Value;
        }

        /// <summary>
        /// 获得到当前节点(最后一个节点) 如果最后一个节点是占位的则往上搜寻
        /// </summary>
        /// <returns></returns>
        private UINode GetCurUINode()
        {
            return GetWindowNotNullNode(mUILinkedLinkedList.Last);
        }

        //找到上一个非空节点
        private UINode GetWindowNotNullNode(LinkedListNode<UINode> uinode)
        {
            if (uinode == null)
            {
                return null;
            }
            else if (uinode.Value.Window == null)
            {
                return GetWindowNotNullNode(uinode.Previous);
            }
            else
            {
                return uinode.Value;
            }
        }


        /// <summary>
        /// 获得最后一个节点的上一个节点
        /// </summary>
        /// <returns></returns>
        private UINode GetCurBeforeNode()
        {
            if (mUILinkedLinkedList.Last.Previous == null)
            {
                return null;
            }

            return mUILinkedLinkedList.Last.Previous.Value;
        }

        /// <summary>
        /// 找到你想要的ui节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private UINode FindUINode(Type type)
        {
            // var enumerator = m_UILinkedList.GetEnumerator();
            foreach (UINode node in mUILinkedLinkedList)
            {
                if (node.WindowType == type)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// 删除一个节点
        /// </summary>
        /// <param name="uinode"></param>
        private void RemoveNode(UINode uinode)
        {
            mUILinkedLinkedList.Remove(uinode);
        }

        /// <summary>
        /// 将一个节点放在最后面
        /// </summary>
        /// <param name="uinode"></param>
        private void AddLastNode(UINode uinode)
        {
            mUILinkedLinkedList.AddLast(uinode);
        }

        /// <summary>
        /// 修改当有上一届窗口打开的时候,自身该如何处理
        /// </summary>
        /// <param name="uiNode"></param>
        public void FixNextUIType(UINode uiNode, WindowState windowstate)
        {
            if (uiNode == null)
            {
                return;
            }

            uiNode.NextActionState = windowstate;
        }

        /// <summary>
        /// 根据自身属性,当有上一级UI打开的时候,自身该如何处理
        /// </summary>
        /// <param name="???"></param>
        private void UIHideOrDisposeWithNextUIType(UINode uiNode)
        {
            if (uiNode == null)
            {
                return;
            }

            if (uiNode.NextActionState == WindowState.Destroy)
            {
                AddWaitDestroyWindowList(uiNode);
            }
            else if (uiNode.NextActionState == WindowState.Hide)
            {
                AddWaitCloseWindowList(uiNode);
                uiNode.Hide();
            }
        }


        /// <summary>
        /// 当存储器里面的UI大于MaxRecycleCount的时候删除一半的UI
        /// </summary>
        private void RecycleWindowMaxClear()
        {
            int recycleCount = mRecycleWindowDic.Count;
            if (recycleCount >= MaxRecycleCount)
            {
                //删除一半
                int needClearNum = MaxRecycleCount / 2;
                var current = mRecycleWindowDic.GetEnumerator();
                while (needClearNum != 0 && current.MoveNext())
                {
                    mTempRecycleWindow.Add(current.Current.Key);
                    --needClearNum;
                }

                current.Dispose();
                RecycleWindowClear();
            }
        }

        /// <summary>
        /// 超过了储存的时间限制就删除他
        /// </summary>
        /// <param name="realElapseSeconds"></param>
        private void TimeoutDeletion(float elapseSeconds)
        {
            mCurAutoReleaseTime += elapseSeconds;
            if (mCurAutoReleaseTime > mAutoReleaseInterval)
            {
                mCurAutoReleaseTime = 0;
                foreach (var item in mRecycleWindowDic)
                {
                    if (Time.realtimeSinceStartup > item.Value.DelayTime)
                    {
                        mTempRecycleWindow.Add(item.Key);
                    }
                }

                RecycleWindowClear();
            }
        }


        /// <summary>
        /// 回收中的窗口清理
        /// </summary>
        private void RecycleWindowClear()
        {
            foreach (Type recycleWindow in mTempRecycleWindow)
            {
                RemoveRecycleWindowDic(recycleWindow);
                GXGameFrame.Instance.RootEntity.GetComponent<UIComponent>().RemoveComponent(recycleWindow);
            }

            mTempRecycleWindow.Clear();
        }

        /// <summary>
        /// 清理指定的在回收字典中的窗口
        /// </summary>
        /// <param name="type"></param>
        private void RemoveRecycleWindowDic(Type type)
        {
            if (mRecycleWindowDic.TryGetValue(type, out RecycleWindow recyclewindow))
            {
                ReferencePool.Release(recyclewindow);
                mRecycleWindowDic.Remove(type);
            }
        }

        public void Disable()
        {
        }
    }
}