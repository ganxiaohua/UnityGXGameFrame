namespace GameFrame.Runtime
{
    /// <summary>
    /// 在下一个节点的UI本窗口打开的
    /// </summary>
    public enum WindowState
    {
        None,
        /// <summary>
        ///隐藏窗口
        /// </summary>
        Hide,           
        /// <summary>
        ///让窗口保持
        /// </summary>
        Exist,          
        /// <summary>
        /// 删除窗口
        /// </summary>
        Destroy,        
    }
}