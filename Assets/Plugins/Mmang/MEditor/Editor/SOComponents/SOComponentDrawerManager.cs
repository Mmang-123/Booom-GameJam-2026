
namespace Mmang.Editors
{
    public static class SOComponentDrawerManager
    {
        private static SOComponentDrawer m_DefaultDrawer = null;

        public static SOComponentDrawer GetDrawer(System.Type componentType)
        {
            // TODO: 这里目前只返回默认drawer，待支持拓展
            m_DefaultDrawer ??= new();

            return m_DefaultDrawer;
        }
    }
}