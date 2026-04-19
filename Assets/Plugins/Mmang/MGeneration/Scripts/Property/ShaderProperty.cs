using UnityEngine;

namespace Mmang.Generations
{
    public abstract class ShaderProperty
    {
        [SerializeField] protected string m_Name;
        public string Name => m_Name;
    }

    public abstract class ShaderProperty<T> : ShaderProperty
    {
        [SerializeField] protected T m_Value;
        public T Value { get => m_Value; set => m_Value = value; }
    }

    [System.Serializable]
    public class ShaderFloatProperty : ShaderProperty<float>
    {
        public ShaderFloatProperty() { }
        public ShaderFloatProperty(string name, float value)
        {
            m_Name = name;
            m_Value = value;
        }    
    }

    [System.Serializable]
    public class ShaderRangeFloatProperty : ShaderProperty
    {
        [SerializeField] protected Vector2 m_ValueRange;
        public float RandomValue => Random.Range(m_ValueRange.x, m_ValueRange.y);

        public Vector2 Range => m_ValueRange;

        public ShaderRangeFloatProperty(string name, Vector2 range)
        {
            m_Name = name;
            m_ValueRange = range;
        }
    }
}