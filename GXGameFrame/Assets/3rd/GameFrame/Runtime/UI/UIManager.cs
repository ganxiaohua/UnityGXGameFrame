using System.Collections.Generic;

namespace GameFrame
{
    public class UIManager : Singleton<UIManager>
    {
        private GameFrameworkLinkedList<UINode> m_UILinkedList;

        public UIManager()
        {
        }

        public void OpenUI<T>() where T : UIViewBase
        {
            //TODO:如果打开的窗口是最上层的窗口

            //TODO:如果打开的窗口在队列之中那就将至拉到最上层

            //TODO:如果没有这个东西那进行打开程序
        }

        private void Open<T>() where T : UIViewBase
        {
            //TODO:先加载资源,创建实例化,等这些完成之后播放本层的动画
            
        }
    }
}