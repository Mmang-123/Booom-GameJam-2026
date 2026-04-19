using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.Game
{

    public class ValueProperty<T> : Property<T>
    {
        [SerializeField] protected T m_Value;
        public override T Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        public ValueProperty() { }
        public ValueProperty(string name, T initValue = default)
        {
            Name = name;
            m_Value = initValue;
        }

        public override T CloneValue()
        {
            return m_Value;
        }
    }

    public class ReadOnlyValueProperty<T> : ReadOnlyProperty<T>
    {
        [SerializeField] protected T m_Value;
        public override T Value
        {
            get => m_Value;
        }

        public ReadOnlyValueProperty() { }
        public ReadOnlyValueProperty(string name, T initValue = default)
        {
            Name = name;
            m_Value = initValue;
        }

        public override void CloneTo(ReadOnlyProperty<T> other)
        {
            if (other is ReadOnlyValueProperty<T> otherValueProperty)
            {
                otherValueProperty.m_Value = m_Value;
            }
        }
    }

    public abstract class NotifyingValueProperty<T> : ValueProperty<T>, IOnPropertyChangedCallback<T> where T : System.IEquatable<T>
    {
        public event System.Action<IOnPropertyChangedCallback<T>.ValueChangedData> ValueChanged;

        public override T Value
        {
            get => base.Value;
            set
            {
                var oldValue = Value;
                m_Value = value;

                if (oldValue == null ? value != null : !oldValue.Equals(value))
                {
                    IOnPropertyChangedCallback<T>.ValueChangedData data = new()
                    {
                        OldValue = oldValue,
                        NewValue = value
                    };
                    ValueChanged?.Invoke(data);
                }
            }
        }
    }

    public class ValueListProperty<T> : ValueProperty<List<T>>
    {
        public ValueListProperty()
        {
            Value = new();
        }

        public override List<T> CloneValue()
        {
            var newList = new List<T>(Value.Count);
            newList.AddRange(Value);
            return newList;
        }
    }

    // Common
    [MProperty(typeof(float))]
    public class FloatProperty : ValueProperty<float> { }
    [MProperty(typeof(double))]
    public class DoubleProperty : ValueProperty<double> { }
    [MProperty(typeof(bool))]
    public class BoolProperty : ValueProperty<bool> { }
    [MProperty(typeof(int))]
    public class IntProperty : ValueProperty<int> { }
    [MProperty(typeof(uint))]
    public class UIntProperty : ValueProperty<uint> { }
    [MProperty(typeof(Vector2))]
    public class Vector2Property : ValueProperty<Vector2> { }
    [MProperty(typeof(Vector3))]
    public class Vector3Property : ValueProperty<Vector3> { }
    [MProperty(typeof(Vector4))]
    public class Vector4Property : ValueProperty<Vector4> { }
    [MProperty(typeof(Color))]
    public class ColorProperty : ValueProperty<Color> { }

    // ReadOnly
    [MProperty(typeof(float))]
    public class ReadOnlyFloatProperty : ReadOnlyValueProperty<float> { }
    [MProperty(typeof(double))]
    public class ReadOnlyDoubleProperty : ReadOnlyValueProperty<double> { }
    [MProperty(typeof(bool))]
    public class ReadOnlyBoolProperty : ReadOnlyValueProperty<bool> { }
    [MProperty(typeof(int))]
    public class ReadOnlyIntProperty : ReadOnlyValueProperty<int> { }
    [MProperty(typeof(uint))]
    public class ReadOnlyUIntProperty : ReadOnlyValueProperty<uint> { }
    [MProperty(typeof(Vector2))]
    public class ReadOnlyVector2Property : ReadOnlyValueProperty<Vector2> { }
    [MProperty(typeof(Vector3))]
    public class ReadOnlyVector3Property : ReadOnlyValueProperty<Vector3> { }
    [MProperty(typeof(Vector4))]
    public class ReadOnlyVector4Property : ReadOnlyValueProperty<Vector4> { }
    [MProperty(typeof(Color))]
    public class ReadOnlyColorProperty : ReadOnlyValueProperty<Color> { }

    // Notifying
    [MProperty(typeof(float))]
    public class NotifyingFloatProperty : NotifyingValueProperty<float> { }
    [MProperty(typeof(double))]
    public class NotifyingDoubleProperty : NotifyingValueProperty<double> { }
    [MProperty(typeof(bool))]
    public class NotifyingBoolProperty : NotifyingValueProperty<bool> { }
    [MProperty(typeof(int))]
    public class NotifyingIntProperty : NotifyingValueProperty<int> { }
    [MProperty(typeof(uint))]
    public class NotifyingUIntProperty : NotifyingValueProperty<uint> { }
    [MProperty(typeof(Vector2))]
    public class NotifyingVector2Property : NotifyingValueProperty<Vector2> { }
    [MProperty(typeof(Vector3))]
    public class NotifyingVector3Property : NotifyingValueProperty<Vector3> { }
    [MProperty(typeof(Vector4))]
    public class NotifyingVector4Property : NotifyingValueProperty<Vector4> { }
    [MProperty(typeof(Color))]
    public class NotifyingColorProperty : NotifyingValueProperty<Color> { }

    // 随机
    [MProperty(typeof(float))]
    public class FloatRangeProperty : ReadOnlyProperty<float>
    {
        [SerializeField] private Vector2 m_ValueRange;
        public override float Value
        {
            get => Random.Range(m_ValueRange.x, m_ValueRange.y);
        }

        public override void CloneTo(ReadOnlyProperty<float> other)
        {
            if (other is FloatRangeProperty floatRangeProperty)
            {
                floatRangeProperty.m_ValueRange = m_ValueRange;
            }
        }
    }

    [MProperty(typeof(int))]
    public class IntRangeProperty : ReadOnlyProperty<int>
    {
        [SerializeField] private Vector2Int m_ValueRange;
        public override int Value
        {
            get => Random.Range(m_ValueRange.x, m_ValueRange.y);
        }

        public override void CloneTo(ReadOnlyProperty<int> other)
        {
            if (other is IntRangeProperty intRangeProperty)
            {
                intRangeProperty.m_ValueRange = m_ValueRange;
            }   
        }
    }
}