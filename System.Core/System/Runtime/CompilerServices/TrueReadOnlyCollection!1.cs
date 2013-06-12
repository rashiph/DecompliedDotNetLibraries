namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections.ObjectModel;

    internal sealed class TrueReadOnlyCollection<T> : ReadOnlyCollection<T>
    {
        internal TrueReadOnlyCollection(T[] list) : base(list)
        {
        }
    }
}

