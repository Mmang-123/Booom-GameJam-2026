using System.Collections.Generic;
using UnityEngine;

namespace Mmang
{
    public abstract class SOComponentContainer : ScriptableObject, ISOComponentContainer
    {
        public abstract IReadOnlyList<SOComponent> SOComponents { get; }
        public abstract string SOComponentsFieldName { get; }
    }
}