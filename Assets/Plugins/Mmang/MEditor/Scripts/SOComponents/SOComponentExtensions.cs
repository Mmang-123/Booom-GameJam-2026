
namespace Mmang
{
    public static class SOComponentExtensions
    {
        public static SOComponent GetComponent(this ISOComponentContainer container, System.Type type)
        {
            foreach (var component in container.SOComponents)
            {
                if (type.IsAssignableFrom(component.GetType()))
                {
                    return component;
                }
            }
            return null;
        }

        public static T GetComponent<T>(this ISOComponentContainer container) where T : SOComponent
        {
            foreach (var component in container.SOComponents)
            {
                if (component is T tComponent)
                {
                    return tComponent;
                }
            }
            return null;
        }

        public static bool TryGetComponent(this ISOComponentContainer container, System.Type type, out SOComponent outComponent)
        {
            foreach (var component in container.SOComponents)
            {
                if (type.IsAssignableFrom(component.GetType()))
                {
                    outComponent = component;
                    return true;
                }
            }

            outComponent = null;
            return false;
        }

        public static bool TryGetComponent<T>(this ISOComponentContainer container, out T outComponent) where T : SOComponent
        {
            foreach (var component in container.SOComponents)
            {
                if (component is T tComponent)
                {
                    outComponent = tComponent;
                    return true;
                }
            }

            outComponent = null;
            return false;
        }
    }
}