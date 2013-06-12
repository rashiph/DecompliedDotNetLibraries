namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("496B0ABF-CDEE-11d3-88E8-00902754C43A")]
    public interface IEnumerator
    {
        bool MoveNext();
        object Current { get; }
        void Reset();
    }
}

