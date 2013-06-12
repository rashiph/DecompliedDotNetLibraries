namespace System.Web.Hosting
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate int ExecuteFunctionDelegate(IntPtr managedHttpContext, IntPtr nativeRequestContext, IntPtr moduleData, int flags);
}

