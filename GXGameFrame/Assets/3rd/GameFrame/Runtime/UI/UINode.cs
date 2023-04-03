using System;
using FairyGUI;
using UnityEditor.Experimental.GraphView;

namespace GameFrame
{
    public class UINode : IReference
    {
        /// <summary>
        /// 窗体的名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 打开窗口的类型
        /// </summary>
        public Type WindowType;

        /// <summary>
        /// 当有新的UI操作的时候,当前窗口保持何种状态
        /// </summary>
        public WindowState NextActionState;

        /// <summary>
        /// 是否正在加载资源
        /// </summary>
        public bool IsLoading;

        /// <summary>
        /// 加载的窗体
        /// </summary>
        public Entity Window;

        public static UINode CreateNode<T>() where T : Entity
        {
            Type windowType = typeof(T);
            UINode uiNode = ReferencePool.Acquire<UINode>();
            uiNode.Name = windowType.Name;
            uiNode.WindowType = windowType;
            uiNode.NextActionState = WindowState.Hide;
            uiNode.IsLoading = true;
            uiNode.Window = GXGameFrame.Instance.MainScene.GetComponent<UIComponent>().AddComponent<T>();
            return uiNode;
        }

        public static void RecycleNode(UINode uinode)
        {
            ReferencePool.Release(uinode);
        }
        
        public void Show()
        {
            EnitityHouse.Instance.RunShowSystem(Window);
        }
        
        public void Hide()
        {
          EnitityHouse.Instance.RunHideSystem(Window);
        }
        
        public void Clear()
        {
            GXGameFrame.Instance.MainScene.GetComponent<UIComponent>().RemoveComponent(this.WindowType);
        }
    }
}