namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class LdapMod
    {
        public int type;
        public IntPtr attribute = IntPtr.Zero;
        public IntPtr values = IntPtr.Zero;
        ~LdapMod()
        {
            if (this.attribute != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.attribute);
            }
            if (this.values != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.values);
            }
        }
    }
}

