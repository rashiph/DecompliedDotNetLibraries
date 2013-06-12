namespace System.Dynamic.Utils
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal static class EmptyReadOnlyCollection<T>
    {
        internal static ReadOnlyCollection<T> Instance;

        static EmptyReadOnlyCollection()
        {
            EmptyReadOnlyCollection<T>.Instance = new TrueReadOnlyCollection<T>(new T[0]);
        }
    }
}

