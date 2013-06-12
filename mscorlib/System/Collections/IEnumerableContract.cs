namespace System.Collections
{
    using System;

    internal class IEnumerableContract : IEnumerable
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }
    }
}

