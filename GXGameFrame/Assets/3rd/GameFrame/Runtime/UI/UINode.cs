using System;
using FairyGUI;

namespace GameFrame
{
    public class UINode
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
        /// 窗口的状态
        /// </summary>
        public WindowState WindowState;
        
        /// <summary>
        /// 是否正在加载资源
        /// </summary>
        public bool IsLoading;

        /// <summary>
        /// 加载的窗体
        /// </summary>
        public UIWindow Window;
    }
}
