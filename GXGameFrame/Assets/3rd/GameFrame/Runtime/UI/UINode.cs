using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        public static List<Type> RecycleWindow = new();

        public static int MaxRecycleCount = 4;

        public static UINode CreateEmptyNode(Type windowType)
        {
            UINode uiNode = ReferencePool.Acquire<UINode>();
            uiNode.Window = null;
            uiNode.WindowType = windowType;
            return uiNode;
        }

        public static UINode CreateNode(Type windowType)
        {
            UINode uiNode = ReferencePool.Acquire<UINode>();
            uiNode.Name = windowType.Name;
            uiNode.WindowType = windowType;
            uiNode.NextActionState = WindowState.Hide;
            uiNode.IsLoading = true;
            if (RecycleWindow.Contains(windowType))
            {
                uiNode.Window = (Entity) GXGameFrame.Instance.MainScene.GetComponent<UIComponent>().GetComponent(windowType);
            }
            else
            {
                uiNode.Window = (Entity) GXGameFrame.Instance.MainScene.GetComponent<UIComponent>().AddComponent(windowType);
            }

            return uiNode;
        }

        public static void DestroyNode(UINode uinode)
        {
            uinode.Hide();
            ReferencePool.Release(uinode);
        }

        public void Show()
        {
            if (Window == null)
                return;
            EnitityHouse.Instance.RunShowSystem(Window);
        }

        public void Hide()
        {
            if (Window == null)
                return;
            EnitityHouse.Instance.RunHideSystem(Window);
        }

        public async UniTask LoadDependentOver()
        {
            DependentResources dependentResources = Window.GetComponent<DependentResources>();
            if (dependentResources != null)
            {
                await dependentResources.WaitLoad();
            }

            IsLoading = false;
        }

        public void Clear()
        {
            int recycleCount = RecycleWindow.Count;
            if (recycleCount >= 4)
            {
                for (int i = MaxRecycleCount / 2 - 1; i >= 0; i--)
                {
                    GXGameFrame.Instance.MainScene.GetComponent<UIComponent>().RemoveComponent(RecycleWindow[i]);
                    RecycleWindow.RemoveAt(i);
                }
            }

            RecycleWindow.Add(WindowType);
        }
    }
}