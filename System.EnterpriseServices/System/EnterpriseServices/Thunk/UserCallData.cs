namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Messaging;

    internal class UserCallData
    {
        public object except;
        public bool fIsAutoDone;
        public MemberInfo mb;
        public IMessage msg;
        public object otp;
        public unsafe IUnknown* pDestCtx;

        public unsafe UserCallData(object otp, IMessage msg, IntPtr ctx, [MarshalAs(UnmanagedType.U1)] bool fIsAutoDone, MemberInfo mb)
        {
            this.otp = otp;
            this.msg = msg;
            this.pDestCtx = (IUnknown*) ctx.ToInt32();
            this.fIsAutoDone = fIsAutoDone;
            this.mb = mb;
            this.except = null;
        }

        public static UserCallData Get(IntPtr pinned)
        {
            GCHandle handle = (GCHandle) pinned;
            return (UserCallData) handle.Target;
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

