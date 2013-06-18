namespace System.IdentityModel
{
    using System;
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
                    this.Name = Marshal.PtrToStringUni(ptr2);
                }
                ptr2 = Marshal.ReadIntPtr(ptr, (int) Marshal.OffsetOf(typeof(SecurityPackageInfo), "Comment"));
                if (ptr2 != IntPtr.Zero)
                {
                    this.Comment = Marshal.PtrToStringUni(ptr2);
                }
            }
        }
    }
}

