using Mmang.Util;

namespace Mmang.Game
{

    [System.Serializable]
    public struct ModifiableValueStruct
    {
        public float RawValue;
        public float Modifier;
        public float PreValue;
        public float PostValue;
    }


    // final = (pre + raw) * modifier + post
    public class ModifiableValue : IReference
    {
        public float RawValue { get; private set; }
        public float Modifier { get; private set; }
        public float PreValue { get; private set; }
        public float PostValue { get; private set; }

        private float m_ForceFinalValue;
        public bool IsForceSet { get; private set; }
        public float FinalValue => IsForceSet
            ? m_ForceFinalValue
            : (PreValue + RawValue) * Modifier + PostValue;

        public void Clear()
        {
            IsForceSet = false;
            m_ForceFinalValue = 0f;
            RawValue = PreValue = PostValue = 0f;
            Modifier = 1;
        }

        /// <summary>
        /// 直接设置最终数值, 进行此操作后, 最终数值将不再由公式得出
        /// </summary>
        public void ForceSetFinalValue(float finalValue)
        {
            IsForceSet = true;
            m_ForceFinalValue = finalValue;
        }

        public static ModifiableValue Create(in ModifiableValueStruct valueStruct)
        {
            var instance = ReferencePool.Acquire<ModifiableValue>();
            instance.RawValue = valueStruct.RawValue;
            instance.Modifier = valueStruct.Modifier;
            instance.PreValue = valueStruct.PreValue;
            instance.PostValue = valueStruct.PostValue;
            return instance;
        }

        public static ModifiableValue Create(float rawValue = 0f, float modifier = 1f, float preValue = 0f, float postValue = 0f)
        {
            var instance = ReferencePool.Acquire<ModifiableValue>();
            instance.RawValue = rawValue;
            instance.Modifier = modifier;
            instance.PreValue = preValue;
            instance.PostValue = postValue;
            return instance;
        }

    }
}