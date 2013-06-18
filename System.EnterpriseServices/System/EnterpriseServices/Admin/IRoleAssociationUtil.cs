namespace System.EnterpriseServices.Admin
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("6EB22876-8A19-11D0-81B6-00A0C9231C29")]
    internal interface IRoleAssociationUtil
    {
        [DispId(1)]
        void AssociateRole([In, MarshalAs(UnmanagedType.BStr)] string bstrRoleID);
        [DispId(2)]
        void AssociateRoleByName([In, MarshalAs(UnmanagedType.BStr)] string bstrRoleName);
    }
}

