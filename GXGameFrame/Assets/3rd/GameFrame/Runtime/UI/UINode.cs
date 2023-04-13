using System;
using FairyGUI;
using UnityEditor.Experimental.GraphView;

namespace GameFrame
{
    public class UINode : ObjectBase
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

        private static ObjectPool<UINode> sObjectPool;

        public static UINode CreateNode(Type windowType) 
        {
            if (sObjectPool == null)
                sObjectPool = ObjectPoolManager.Instance.CreateObjectPool<UINode>("UI", 16, windowType);
            UINode uiNode = sObjectPool.Spawn();
            uiNode.Name = windowType.Name;
            uiNode.WindowType = windowType;
            uiNode.NextActionState = WindowState.Hide;
            uiNode.IsLoading = true;
            uiNode.Window = (Entity)GXGameFrame.Instance.MainScene.GetComponent<UIComponent>().AddComponent(windowType);
            return uiNode;
        }

        public static void DestroyNode(UINode uinode)
        {
            uinode.Hide();
            sObjectPool.UnSpawn(uinode);
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