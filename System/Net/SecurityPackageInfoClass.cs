namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal class SecurityPackageInfoClass
    {
        internal int Capabilities;
        internal string Comment;
        internal int MaxToken;
        internal string Name;
        internal short RPCID;
        internal short Version;

        internal SecurityPackageInfoClass(SafeHandle safeHandle, int index)
        {
            if (!safeHandle.IsInvalid)
            {
                IntPtr ptr = IntPtrHelper.Add(safeHandle.DangerousGetHandle(), SecurityPackageInfo.Size * index);
                this.Capabilities = Marshal.ReadInt32(ptr, (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "Capabilities"));
                this.Version = Marshal.ReadInt16(ptr, (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "Version"));
                this.RPCID = Marshal.ReadInt16(ptr, (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "RPCID"));
                this.MaxToken = Marshal.ReadInt32(ptr, (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "MaxToken"));
                IntPtr ptr2 = Marshal.ReadIntPtr(ptr, (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "Name"));
                if (ptr2 != IntPtr.Zero)
                {
                    if (ComNetOS.IsWin9x)
                    {
                        this.Name = Marshal.PtrToStringAnsi(ptr2);
                    }
                    else
                    {
                        this.Name = Marshal.PtrToStringUni(ptr2);
                    }
                }
                ptr2 = Marshal.ReadIntPtr(ptr, (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "Comment"));
                if (ptr2 != IntPtr.Zero)
                {
                    if (ComNetOS.IsWin9x)
                    {
                        this.Comment = Marshal.PtrToStringAnsi(ptr2);
                    }
                    else
                    {
                        this.Comment = Marshal.PtrToStringUni(ptr2);
                    }
                }
            }
        }

        public override string ToString()
        {
            return ("Capabilities:" + string.Format(CultureInfo.InvariantCulture, "0x{0:x}", new object[] { this.Capabilities }) + " Version:" + this.Version.ToString(NumberFormatInfo.InvariantInfo) + " RPCID:" + this.RPCID.ToString(NumberFormatInfo.InvariantInfo) + " MaxToken:" + this.MaxToken.ToString(NumberFormatInfo.InvariantInfo) + " Name:" + ((this.Name == null) ? "(null)" : this.Name) + " Comment:" + ((this.Comment == null) ? "(null)" : this.Comment));
        }
    }
}

