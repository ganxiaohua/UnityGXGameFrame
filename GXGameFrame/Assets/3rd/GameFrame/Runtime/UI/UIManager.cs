using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GameFrame
{
    public class UIManager : Singleton<UIManager>
    {
        private GameFrameworkLinkedList<UINode> m_UILinkedList;
        private Stack<UINode> m_OpenUIList;
        private Queue<Type> m_WaitOpenUIList;
        private Dictionary<Type, UINode> m_WaitCloseUIList;

        public UIManager()
        {
            m_WaitOpenUIList = new();
            m_UILinkedList = new();
            m_OpenUIList = new();
            m_WaitCloseUIList = new();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            //有等待加入的UI在队列里的时候有限这个,然后将m_OpenUIList隐藏
            if (m_WaitOpenUIList.Count > 0 && IsAction() == false)
            {
                OpenUIS(m_WaitOpenUIList.ToArray());
                m_WaitOpenUIList.Clear();
                m_OpenUIList.Clear();
            }
            else if (m_OpenUIList.Count > 0 && IsAction() == false)
            {
                foreach (UINode uiNode in m_OpenUIList)
                {
                    uiNode.Show();
                }
                m_OpenUIList.Clear();
            }
        }

        /// <summary>
        /// 打开一系列UI窗口一般用于战斗结束 需要回退到某个特殊UI
        /// </summary>
        /// <param name="types"></param>
        public void OpenUIS(Type[] types)
        {
            if (IsAction())
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (!m_WaitOpenUIList.Contains(types[i]))
                    {
                        m_WaitOpenUIList.Enqueue(types[i]);
                    }
                }
                return;
            }

            int count = types.Length;
            for (int i = 0; i < count - 1; i++)
            {
                Type type = types[i];
                UINode findNode = FindUINode(type);
                if (findNode != null)
                {
                    RemoveNode(findNode);
                    AddLastNode(findNode);
                    continue;
                }

                UINode uinode = UINode.CreateEmptyNode(type);
                m_UILinkedList.AddLast(uinode);
            }
            OpenUI(types[count - 1]);
        }

        /// <summary>
        /// 打开一个UI
        /// </summary>
        /// <param name="type"></param>
        public void OpenUI(Type type)
        {
            if (IsAction())
            {
                m_WaitOpenUIList.Enqueue(type);
                return;
            }

            //如果打开的UI就是最上层的UI
            UINode curUINode = GetCurUINode();

            //如果最上层节点就是想要打开的节点则什么都不做
            if (curUINode != null && curUINode.WindowType == type)
            {
                return;
            }

            //如果需要打开的UI在列表中则将其拉扯到最上面进行打开
            UINode findUINode = FindUINode(type);
            if (findUINode != null)
            {
                RemoveNode(findUINode);
                AddLastNode(findUINode);
                m_OpenUIList.Push(findUINode);
                UIHideOrDisposeWithNextUIType(curUINode);
                return;
            }

            //打开新窗口
            Open(type);
        }

        /// <summary>
        /// 打开一个UI
        /// </summary>
        /// <param name="type"></param>
        private async UniTask Open(Type type)
        {
            UINode curUINode = GetCurUINode();
            UINode uinode = UINode.CreateNode(type);
            m_UILinkedList.AddLast(uinode);
            await uinode.LoadDependentOver();
            Debug.Log("资源加载结束");
            //加入等待打开的UI列表
            m_OpenUIList.Push(uinode);
            if (curUINode != null)
            {
                UIHideOrDisposeWithNextUIType(curUINode);
            }
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

            NodeShowTypeChick(beforeNode);
        }

        /// <summary>
        /// 从当前界面返回到第一层界面
        /// </summary>
        public void Back2Frist()
        {
            if (m_UILinkedList.Count <= 1)
            {
                return;
            }

            //倒数第二个
            LinkedListNode<UINode> Penult = m_UILinkedList.Last.Previous;
            while (m_UILinkedList.First != m_UILinkedList.Last)
            {
                DestroyNode(Penult.Value);
                m_UILinkedList.Remove(Penult);
                Penult = Penult.Previous;
            }

            var curUINode = GetFirstUINode();
            NodeShowTypeChick(curUINode);
        }

        //如果window则打开window. 如果不存在代表这只是一个占用节点,删除此节点,重新走OpenUI逻辑.
        private void NodeShowTypeChick(UINode uiNode)
        {
            if (uiNode.Window != null)
            {
                AddWaitDestroyWindowList(GetCurUINode());
                m_OpenUIList.Push(uiNode);
            }
            else
            {
                FIXNextUIType(GetCurUINode(), WindowState.Destroy);
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
            if (m_WaitCloseUIList.Count > 0 || (curUINode != null && curUINode.IsLoading))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 删除一个节点,加入等待关闭列表
        /// </summary>
        /// <param name="uiNode"></param>
        private void AddWaitDestroyWindowList(UINode uiNode)
        {
            RemoveNode(uiNode);
            m_WaitCloseUIList.Add(uiNode.WindowType, uiNode);
            DestroyNode(uiNode);
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
        /// 获得到UI关闭的信息
        /// </summary>
        public void GetUIClose(Type type)
        {
            if (m_WaitCloseUIList.ContainsKey(type))
            {
                m_WaitCloseUIList.Remove(type);
            }
        }

        /// <summary>
        /// 获得到当前节点
        /// </summary>
        /// <returns></returns>
        private UINode GetFirstUINode()
        {
            if (m_UILinkedList.First == null)
            {
                return null;
            }

            return m_UILinkedList.First.Value;
        }

        /// <summary>
        /// 获得到当前节点(最后一个节点)
        /// </summary>
        /// <returns></returns>
        private UINode GetCurUINode()
        {
            if (m_UILinkedList.Last == null)
            {
                return null;
            }

            return m_UILinkedList.Last.Value;
        }

        /// <summary>
        /// 获得最后一个节点的上一个节点
        /// </summary>
        /// <returns></returns>
        private UINode GetCurBeforeNode()
        {
            if (m_UILinkedList.Last.Previous == null)
            {
                return null;
            }

            return m_UILinkedList.Last.Previous.Value;
        }

        /// <summary>
        /// 找到你想要的ui节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private UINode FindUINode(Type type)
        {
            // var enumerator = m_UILinkedList.GetEnumerator();
            foreach (UINode node in m_UILinkedList)
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
            m_UILinkedList.Remove(uinode);
        }

        /// <summary>
        /// 将一个节点放在最后面
        /// </summary>
        /// <param name="uinode"></param>
        private void AddLastNode(UINode uinode)
        {
            m_UILinkedList.AddLast(uinode);
        }

        /// <summary>
        /// 修改当有上一届窗口打开的时候,自身该如何处理
        /// </summary>
        /// <param name="uiNode"></param>
        public void FIXNextUIType(UINode uiNode, WindowState windowstate)
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
                RemoveNode(uiNode);
                DestroyNode(uiNode);
            }
            else if (uiNode.NextActionState == WindowState.Hide)
            {
                uiNode.Hide();
            }
            else if (uiNode.NextActionState == WindowState.Exist)
            {
            }
        }
    }
}