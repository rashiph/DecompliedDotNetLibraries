namespace System.Runtime.InteropServices
{
    using System;

    [Obsolete("Use System.Runtime.InteropServices.ComTypes.IEnumerator instead. http://go.microsoft.com/fwlink/?linkid=14202", false), Guid("496B0ABF-CDEE-11d3-88E8-00902754C43A")]
    internal interface UCOMIEnumerator
    {
        bool MoveNext();
        object Current { get; }
        void Reset();
    }
}

