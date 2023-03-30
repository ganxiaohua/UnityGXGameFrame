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

        public UIManager()
        {
            m_UILinkedList = new();
            m_WaitOpenUIList = new();
            m_WaitCloseUIList = new();
        }

        public void OpenUI<T>() where T : UIViewBase
        {
            Type type = typeof(T);
            UINode findUINode = GetCurUINode();

            if (findUINode == null || (type == findUINode.WindowType))
            {
                return;
            }
            //TODO:如果打开的窗口在队列之中那就将至拉到最上层
            findUINode = (FindUINode(type));
            if (findUINode != null)
            {
                RemoveNode(findUINode);
                AddLastNode(findUINode);
            }

            //TODO:如果没有这个东西那进行打开程序
        }

        private void Open<T>() where T : UIViewBase
        {
            //TODO:先加载资源,创建实例化,等这些完成之后播放本层的动画
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

            if (uiNode.WindowState == WindowState.Destroy)
            {
                
            }
        }
    }
}