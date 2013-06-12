namespace System.Collections.Generic
{
    using System.Collections;

    internal class IEnumerableContract<T> : IEnumerableContract, IEnumerable<T>, IEnumerable
    {
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return null;
        }
    }
}

