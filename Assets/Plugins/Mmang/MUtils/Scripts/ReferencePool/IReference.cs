namespace Mmang.Util
{
    public interface IReference
    {
        /// <summary>
        /// 被引用池回收时调用
        /// </summary>
        public void Clear();
    }
} 
