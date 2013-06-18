namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable]
    internal class UserMarshalData
    {
        public byte[] buffer;
        public IntPtr pUnk;

        public UserMarshalData(IntPtr pUnk)
        {
            this.pUnk = pUnk;
            this.buffer = null;
        }

        public static UserMarshalData Get(IntPtr pinned)
        {
            GCHandle handle = (GCHandle) pinned;
            return (UserMarshalData) handle.Target;
        }

        public IntPtr Pin()
        {
            return (IntPtr) GCHandle.Alloc(this, GCHandleType.Normal);
        }

        public void Unpin(IntPtr pinned)
        {
            ((GCHandle) pinned).Free();
        }
    }
}

