namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class LdapVlvInfo
    {
        private int version = 1;
        private int beforeCount;
        private int afterCount;
        private int offset;
        private int count;
        private IntPtr attrvalue = IntPtr.Zero;
        private IntPtr context = IntPtr.Zero;
        private IntPtr extraData = IntPtr.Zero;
        public LdapVlvInfo(int version, int before, int after, int offset, int count, IntPtr attribute, IntPtr context)
        {
            this.version = version;
            this.beforeCount = before;
            this.afterCount = after;
            this.offset = offset;
            this.count = count;
            this.attrvalue = attribute;
            this.context = context;
        }
    }
}

