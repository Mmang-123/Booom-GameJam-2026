namespace Mmang.Util
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T m_Instance;
        public static T Instance
        {
            get
            {
                m_Instance ??= new T();
                return m_Instance;
            }
        }

        public Singleton()
        {
            m_Instance = this as T;
        }
    }

}