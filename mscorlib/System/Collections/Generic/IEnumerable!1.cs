namespace System.Collections.Generic
{
    using System.Collections;
    using System.Runtime.CompilerServices;

    [TypeDependency("System.SZArrayHelper")]
    public interface IEnumerable<out T> : IEnumerable
    {
        IEnumerator<T> GetEnumerator();
    }
}

