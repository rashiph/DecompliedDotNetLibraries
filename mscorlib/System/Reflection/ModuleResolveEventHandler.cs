namespace System.Reflection
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public delegate Module ModuleResolveEventHandler(object sender, ResolveEventArgs e);
}

