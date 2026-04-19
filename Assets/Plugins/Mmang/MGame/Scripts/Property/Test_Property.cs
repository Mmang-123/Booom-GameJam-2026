
using Mmang.Game;
using UnityEngine;

namespace Mmang.Test
{
    public class Test_Property : MonoBehaviour
    {
        [SerializeField] private PropertyBase m_Property;

        [SerializeField, PropertyContainer(typeof(float))]
        private PropertyContainer m_Container = new();

        [SerializeField] private GlobalPropertyReference<float> m_GlobalFloat = new("Test.FloatProperty");
        [SerializeField] private GlobalPropertyReference<float> m_GlobalFloat2 = new("Test.FloatProperty2");

        [ContextMenu("AddGlobalProperty")]
        private void AddGlobalProperty()
        {
            GlobalProperties.AddValueProperty<float>("Test.FloatProperty", 10f);
        }
    }
}