using System.Collections.Generic;

namespace Mmang
{
    public interface ISOComponentContainer
    {
        public IReadOnlyList<SOComponent> SOComponents { get; }
        public string SOComponentsFieldName { get; }
    }

    
}