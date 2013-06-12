namespace System.Collections.Generic
{
    using System;
    using System.Collections;

    public interface IEnumerator<out T> : IDisposable, IEnumerator
    {
        T Current { get; }
    }
}

