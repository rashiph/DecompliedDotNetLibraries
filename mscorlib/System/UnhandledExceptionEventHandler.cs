namespace System
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public delegate void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e);
}

