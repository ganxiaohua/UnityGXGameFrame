using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;

namespace GameFrame
{
    public class UIManager : Singleton<UIManager>
    {
        private GameFrameworkLinkedList<UINode> m_UILinkedList;
        private Stack<UINode> m_OpenUIList;
        private Queue<UINode> m_WaitOpenUIList;
        private Dictionary<Type, UINode> m_WaitCloseUIList;

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
            m_WaitOpenUIList = new();
            m_UILinkedList = new();
            m_OpenUIList = new();
            m_WaitCloseUIList = new();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (m_OpenUIList.Count > 0 && IsAction() == false)
            {
                foreach (UINode uiNode in m_OpenUIList)
                {
                    uiNode.Show();
                }

                m_OpenUIList.Clear();
            }
        }

        
        /// <summary>
        /// 打开预制列表
        /// </summary>
        /// <param name="uiTypelist"></param>
        public void OpenPrefabList(List<Type> uiTypelist)
        {
            
        }

        public void OpenUI(Type type)
        {
            //如果打开的UI就是最上层的UI
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
                m_OpenUIList.Push(findUINode);
                UIHideOrDisposeWithNextUIType(curUINode);
                return;
            }
            //打开新窗口
            Open(type);
        }

        private void Open(Type type)
        {
            UINode curUINode = GetCurUINode();
            UINode uinode = UINode.CreateNode(type);
            m_UILinkedList.AddLast(uinode);
            //TODO:先加载依赖资源,在加载UI资源

            //加入等待打开的UI列表
            m_OpenUIList.Push(uinode);
            UIHideOrDisposeWithNextUIType(curUINode);
        }

        private void AddWaitDestroyWindowList(UINode uiNode)
        {
            RemoveNode(uiNode);
            m_WaitCloseUIList.Add(uiNode.WindowType, uiNode);
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
        private UINode GetCurUINode()
        {
            if (m_UILinkedList.Last == null)
            {
                return null;
            }

            return m_UILinkedList.Last.Value;
        }

        /// <summary>
        /// 找到你想要的ui节点
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
                UINode.DestroyNode(uiNode);
            }
            else if (uiNode.NextActionState == WindowState.Hide)
            {
                uiNode.Hide();
            }
            else if (uiNode.NextActionState == WindowState.Exist)
            {
            }
        }


        private void DestroyUI()
        {
        }
    }
}