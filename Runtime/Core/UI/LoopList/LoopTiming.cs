namespace GameFrame.Runtime
{
    public enum LoopTiming
    {
        /// <summary>
        /// Caused by user scroll operation
        /// </summary>
        UserScroll,

        /// <summary>
        /// Caused by <see cref="ILoopList.Count">ILoopList.Count</see> call
        /// </summary>
        HardUpdate,

        /// <summary>
        /// Caused by <see cref="ILoopList.Refresh">ILoopList.Refresh</see> call
        /// </summary>
        SoftUpdate,
    }
}