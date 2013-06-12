namespace System.Web.Hosting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal delegate int RoleFunctionDelegate(IntPtr pManagedPrincipal, IntPtr pszRole, int cchRole, bool disposing, out bool isInRole);
}

