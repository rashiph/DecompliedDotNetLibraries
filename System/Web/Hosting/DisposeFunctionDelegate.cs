namespace System.Web.Hosting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal delegate void DisposeFunctionDelegate([In] IntPtr managedHttpContext);
}

