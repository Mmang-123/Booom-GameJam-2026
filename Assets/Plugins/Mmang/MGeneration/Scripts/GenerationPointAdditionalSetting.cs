using System;
using UnityEngine;

namespace Mmang.Generations
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AdditionalSettingAttribute : Attribute
    {
        public Type settingType;
        public AdditionalSettingAttribute(Type settingType)
        {
            this.settingType = settingType;
        }
    }

    [Serializable]
    public abstract class GenerationPointAdditionalSetting
    {

    }
}