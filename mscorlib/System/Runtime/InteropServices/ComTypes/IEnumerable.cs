namespace System.Runtime.InteropServices.ComTypes
{
    using System.Collections;
    using System.Runtime.InteropServices;

    [Guid("496B0ABE-CDEE-11d3-88E8-00902754C43A")]
    internal interface IEnumerable
    {
        [DispId(-4)]
        System.Collections.IEnumerator GetEnumerator();
    }
}

