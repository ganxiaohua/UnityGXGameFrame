namespace GameFrame.Runtime
{
    public interface IContinuousID
    {
        /// <summary>
        /// 需要从0开始且是连续的的ID类型
        /// </summary>
        public int ID { get; }
    }
}