
using System.Collections.Generic;

namespace Mmang
{
    public interface IMappable { }
    public interface IMappable<T>
    {
        public T MapKey { get; }
    }

    public class MappableList<TKey, T> : List<T> where T : IMappable<TKey>
    {
        
    }
}