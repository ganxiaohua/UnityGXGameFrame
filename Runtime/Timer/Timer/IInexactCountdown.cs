namespace GameFrame.Runtime
{
    /// <summary>
    /// 由分组轮询检查的倒计时对象，按实例引用注册。
    /// </summary>
    public interface IInexactCountdown
    {
        /// <summary>
        /// 与 Time.realtimeSinceStartup 使用相同时间轴的绝对到期时间；续期只需修改此值。
        /// </summary>
        float ExpireTime { get; }

        /// <summary>
        /// 到期对象已从组中移除后调用；周期任务可更新到期时间并重新加入组。
        /// </summary>
        void OnCountdownExpired();
    }
}
