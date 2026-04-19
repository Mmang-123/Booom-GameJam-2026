

namespace Mmang.Game
{
    public static class GlobalProperties
    {
        public static void AddProperty(PropertyBase property)
        {
            var config = GlobalConfigAssets.GetConfigInstance<GlobalPropertiesConfig>();
            config.AddProperty(property);
        }

        public static void AddValueProperty<T>(string propertyName, T initValue = default)
        {
            var config = GlobalConfigAssets.GetConfigInstance<GlobalPropertiesConfig>();
            if (config.ContainsProperty(propertyName))
            {
                return;
            }

            ValueProperty<T> valueProperty = new(propertyName, initValue);
            config.AddProperty(valueProperty);
        }

        public static void SetValue<T>(string name, T newValue)
        {
            var config = GlobalConfigAssets.GetConfigInstance<GlobalPropertiesConfig>();
            config.SetValue(name, newValue);
        }

        public static T GetValue<T>(string name)
        {
            var config = GlobalConfigAssets.GetConfigInstance<GlobalPropertiesConfig>();
            return config.GetValue<T>(name);
        }

        public static object GetObjectValue(string name)
        {
            var config = GlobalConfigAssets.GetConfigInstance<GlobalPropertiesConfig>();
            return config.GetValue(name);
        }
    }

    public static class GlobalPropertiesExtensions
    {
        public static bool IsValid(PropertyBase property)
        {
            return property != null && !string.IsNullOrEmpty(property.Name);
        }
    }
}