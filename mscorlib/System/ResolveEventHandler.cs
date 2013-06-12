namespace System
{
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public delegate Assembly ResolveEventHandler(object sender, ResolveEventArgs args);
}

