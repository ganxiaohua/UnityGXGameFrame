using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace GameFrame
{
    public class UINode : IReference
    {
        public enum StateType
        {
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


        /// <summary>
        /// 加载的窗体
        /// </summary>
        public Entity Window;

        public static UINode CreateEmptyNode(Type windowType)
        {
            UINode uiNode = ReferencePool.Acquire<UINode>();
            uiNode.Window = null;
            uiNode.WindowType = windowType;
            return uiNode;
        }

        public static UINode CreateNode(Type windowType)
        {
            UIComponent UIComponent = GXGameFrame.Instance.MainScene.GetComponent<UIComponent>();
            UINode uiNode = ReferencePool.Acquire<UINode>();
            uiNode.Name = windowType.Name;
            uiNode.WindowType = windowType;
            uiNode.NextActionState = WindowState.Hide;
            uiNode.NodeState = StateType.WaitOpen;
            uiNode.Window = (Entity) UIComponent.GetComponent(windowType);
            if (uiNode.Window == null)
                uiNode.Window = (Entity) UIComponent.AddComponent(windowType);
            return uiNode;
        }

        public static void DestroyNode(UINode uinode)
        {
            uinode.Hide();
            ReferencePool.Release(uinode);
        }

        public void PreShow(bool isFirstOpen)
        {
            if (Window == null)
                return;
            EnitityHouse.Instance.RunPreShowSystem(Window, isFirstOpen);
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
            NodeState = StateType.Hide;
        }

        public async UniTask<bool> LoadMustDependentOver()
        {
            NodeState = StateType.Loading;
            DependentUIResources dependentResources = Window.GetComponent<DependentUIResources>();
            bool over = true;
            if (dependentResources != null)
            {
                over = await dependentResources.WaitLoad();
                
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

        public void Clear()
        {
        }
    }
}