using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FairyGUI;

namespace GameFrame
{
    public class UINode : IDisposable
    {
        public enum StateType
        {
            None,
            WaitOpen,
            Loading,
            LoadEnd,
            Open,
            WaitDestroy,
            Destroy,
            Wait,
            WaitEnd,
            Hide,
        }

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
        /// 当前窗口状态
        /// </summary>
        public StateType NodeState;

        public UINode Parent;

        public List<UINode> Childs = new List<UINode>();

        /// <summary>
        /// 加载的窗体
        /// </summary>
        public UIEntity Window;

        public static UINode CreateEmptyNode(Type windowType)
        {
            UINode uiNode = ReferencePool.Acquire<UINode>();
            uiNode.Window = null;
            uiNode.WindowType = windowType;
            return uiNode;
        }

        public static UINode CreateNode(Type windowType, object data)
        {
            UIComponent UIComponent = GXGameFrame.Instance.RootEntity.GetComponent<UIComponent>();
            UINode uiNode = ReferencePool.Acquire<UINode>();
            uiNode.Name = windowType.Name;
            uiNode.WindowType = windowType;
            uiNode.NextActionState = WindowState.Hide;
            uiNode.NodeState = StateType.WaitOpen;
            uiNode.Window = (UIEntity) UIComponent.GetComponent(windowType);
            if (uiNode.Window == null)
            {
                uiNode.Window = (UIEntity) UIComponent.AddComponent(windowType);
                uiNode.Window.OnInitialize().Forget();
            }

            if (data != null)
                uiNode.Window.AddComponent<UIObjectData>().Data = data;

            return uiNode;
        }

        public static void DestroyNode(UINode uinode)
        {
            uinode.Hide();
            foreach (var child in uinode.Childs)
            {
                DestroyNode(child);
            }

            uinode.Window.TryRemoveComponent<UIObjectData>();
            ReferencePool.Release(uinode);
        }

        public void PreShow(bool isFirstOpen)
        {
            if (Window == null)
                return;
            EntityHouse.Instance.RunPreShowSystem(Window, isFirstOpen);
            foreach (var child in Childs)
            {
                child.PreShow(isFirstOpen);
            }
        }

        public void Show()
        {
            if (Window == null)
                return;
            EntityHouse.Instance.RunShowSystem(Window);
            foreach (var child in Childs)
            {
                child.Show();
            }
        }

        public void Hide()
        {
            if (Window == null)
                return;
            EntityHouse.Instance.RunHideSystem(Window);
            foreach (var child in Childs)
            {
                child.Hide();
            }

            NodeState = StateType.Hide;
        }

        public async UniTask<bool> LoadMustDependentOver(GComponent root = null)
        {
            NodeState = StateType.Loading;
            DependentUI dependent = Window.GetComponent<DependentUI>();
            bool over = false;
            Window.UINode = this;
            if (dependent != null)
            {
                over = await dependent.WaitLoad();
            }

            if (root != null)
            {
                root.AddChild(dependent.Window);
            }

            NodeState = StateType.LoadEnd;
            return over;
        }

        public async UniTask<bool> UIWait()
        {
            NodeState = StateType.Wait;
            WaitComponent waitComponent = Window.GetComponent<WaitComponent>();
            bool over = true;
            if (waitComponent != null)
            {
                over = await waitComponent.Wait();
            }

            NodeState = StateType.WaitEnd;
            return over;
        }

        public void Dispose()
        {
            NextActionState = WindowState.None;
            Name = String.Empty;
            WindowType = null;
            NodeState = StateType.None;
            Childs.Clear();
            Parent = null;
        }
    }
}