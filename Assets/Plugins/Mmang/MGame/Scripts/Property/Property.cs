using System.Collections.Generic;
using Mmang.Util;
using UnityEngine;

namespace Mmang.Game
{

    public interface IGettableProperty
    {
        public object GetPropertyObjectValue();
    }
    public interface ISettableProperty { }

    public interface IGettableProperty<T> : IGettableProperty
    {
        public T GetPropertyValue();
    }

    public interface ISettableProperty<T> : ISettableProperty
    {
        public void SetPropertyValue(T newValue);
    }

    public interface IOnPropertyChangedCallback { }
    public interface IOnPropertyChangedCallback<T> : IOnPropertyChangedCallback
    {
        public struct ValueChangedData
        {
            public T OldValue;
            public T NewValue;
        }
        public event System.Action<ValueChangedData> ValueChanged;
    }

    [System.Serializable]
    public abstract class PropertyBase : IReference
    {
        [SerializeField] private string m_Name;
        public string Name { get => m_Name; set => m_Name = value; }
        public virtual System.Type PropertyType { get; }

        public virtual void Clear()
        {
            m_Name = string.Empty;
        }

        public abstract PropertyBase Clone();
    }

    [System.Serializable]
    public abstract class Property : PropertyBase, IGettableProperty, ISettableProperty
    {
        public abstract object GetPropertyObjectValue();
    }
    [System.Serializable]
    public abstract class Property<T> : Property, IGettableProperty<T>, ISettableProperty<T>
    {
        public override System.Type PropertyType => typeof(T);
        public abstract T Value { get; set; }
        public T GetPropertyValue() => Value;
        public void SetPropertyValue(T newValue) => Value = newValue;


        public abstract T CloneValue();

        public override PropertyBase Clone()
            => CloneT();

        public Property<T> CloneT()
        {
            var newInstance = ReferencePool.Acquire(typeof(Property<T>)) as Property<T>;
            newInstance.Name = Name;
            CloneTo(newInstance);
            return newInstance;
        }

        public virtual void CloneTo(Property<T> other)
        {
            other.Value = CloneValue();
        }

        public override object GetPropertyObjectValue()
        {
            return Value;
        }
    }

    [System.Serializable]
    public abstract class ReadOnlyProperty : PropertyBase, IGettableProperty
    {
        public abstract object GetPropertyObjectValue();
    }
    [System.Serializable]
    public abstract class ReadOnlyProperty<T> : ReadOnlyProperty, IGettableProperty<T>
    {
        public override System.Type PropertyType => typeof(T);
        public abstract T Value { get; }
        public T GetPropertyValue() => Value;


        public override PropertyBase Clone()
            => CloneT();

        public ReadOnlyProperty<T> CloneT()
        {
            var newInstance = ReferencePool.Acquire(typeof(ReadOnlyProperty<T>)) as ReadOnlyProperty<T>;
            newInstance.Name = Name;
            CloneTo(newInstance);
            return newInstance;
        }

        public abstract void CloneTo(ReadOnlyProperty<T> other);

        public override object GetPropertyObjectValue()
        {
            return Value;
        }
    }

}