namespace System
{
    using System.Runtime.CompilerServices;
    using System.Security;

    internal static class ConfigServer
    {
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void RunParser(IConfigHandler factory, string fileName);
    }
}

