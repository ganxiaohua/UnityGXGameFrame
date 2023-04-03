using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace GameFrame
{
    public class UIManager : Singleton<UIManager>
    {
        private GameFrameworkLinkedList<UINode> m_UILinkedList;
        private Queue<UINode> m_WaitOpenUIList;
        private Queue<UINode> m_WaitCloseUIList;

        /// <summary>
        /// 是否需要等待上一级窗口关闭
        /// </summary>
        private bool m_DontWaitPreWinClose
        {
            get
            {
                UINode node = GetCurUINode();
                if (node == null || (node.Window != null && node.NextActionState == WindowState.Exist))
                {
                    return true;
                }

                return false;
            }
        }

        public UIManager()
        {
            m_UILinkedList = new();
            m_WaitOpenUIList = new();
            m_WaitCloseUIList = new();
        }

        public void OpenUI<T>() where T : Entity
        {
            //如果打开的UI就是最上层的UI
            Type type = typeof(T);
            UINode curUINode = GetCurUINode();

            if (curUINode == null || (type == curUINode.WindowType))
            {
                return;
            }
            //如果需要打开的UI在列表中
            UINode findUINode = FindUINode(type);
            if (findUINode != null)
            {
                RemoveNode(findUINode);
                AddLastNode(findUINode);
                UIHideOrDisposeWithNextUIType(curUINode);
                findUINode.Show();
                return;
            }
            //打开新窗口
            Open<T>();
        }

        private void Open<T>() where T : Entity
        {
            UINode uinode = UINode.CreateNode<T>();
            m_UILinkedList.AddLast(uinode);
            //TODO:加载资源
          
        }

        private void AddWaitDestroyWindowList(UINode uiNode)
        {
            RemoveNode(uiNode);
            m_WaitCloseUIList.Enqueue(uiNode);
            // uiNode.Window
            //TODO:播放隐藏动画
        }

        private bool IsAction()
        {
            UINode curUINode = GetCurUINode();
            if (m_WaitCloseUIList.Count > 0 || (curUINode != null && curUINode.IsLoading))
            {
                return true;
            }

            return false;
        }

        private UINode GetCurUINode()
        {
            if (m_UILinkedList.Last == null)
            {
                return null;
            }

            return m_UILinkedList.Last.Value;
        }

        private UINode FindUINode(Type type)
        {
            var enumerator = m_UILinkedList.GetEnumerator();
            foreach (UINode node in m_UILinkedList)
            {
                if (node.WindowType == type)
                {
                    return node;
                }
            }

            return null;
        }

        private void RemoveNode(UINode uinode)
        {
            m_UILinkedList.Remove(uinode);
        }

        private void AddLastNode(UINode uinode)
        {
            m_UILinkedList.AddLast(uinode);
        }

        /// <summary>
        /// 关闭或者隐藏UI
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
                UINode.RecycleNode(uiNode);
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