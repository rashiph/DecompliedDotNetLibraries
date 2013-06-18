namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComImport, Guid("2A005C0D-A5DE-11CF-9E66-00AA00A3F464")]
    internal interface ISharedPropertyGroupManager
    {
        ISharedPropertyGroup CreatePropertyGroup([In, MarshalAs(UnmanagedType.BStr)] string name, [In, Out, MarshalAs(UnmanagedType.I4)] ref PropertyLockMode dwIsoMode, [In, Out, MarshalAs(UnmanagedType.I4)] ref PropertyReleaseMode dwRelMode, out bool fExist);
        ISharedPropertyGroup Group(string name);
        void GetEnumerator(out IEnumerator pEnum);
    }
}

