namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class EmptyReadOnlyCollection<T>
    {
        public static ReadOnlyCollection<T> Instance;

        static EmptyReadOnlyCollection()
        {
            EmptyReadOnlyCollection<T>.Instance = new ReadOnlyCollection<T>(new List<T>());
        }
    }
}

