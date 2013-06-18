namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComImport, Guid("CAFC823D-B441-11D1-B82B-0000F8757E2A")]
    internal interface ISecurityCallersColl
    {
        int Count { [DispId(0x60020000)] get; }
        [DispId(0)]
        ISecurityIdentityColl GetItem(int lIndex);
        [DispId(-4)]
        void GetEnumerator(out IEnumerator pEnum);
    }
}

