namespace System.Runtime.InteropServices
{
    using System;
    using System.Collections;

    [Obsolete("Use System.Runtime.InteropServices.ComTypes.IEnumerable instead. http://go.microsoft.com/fwlink/?linkid=14202", false), Guid("496B0ABE-CDEE-11d3-88E8-00902754C43A")]
    internal interface UCOMIEnumerable
    {
        [DispId(-4)]
        IEnumerator GetEnumerator();
    }
}

