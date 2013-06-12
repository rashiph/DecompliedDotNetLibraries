namespace System.Collections.Generic
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate bool TreeWalkPredicate<T>(SortedSet<T>.Node node);
}

