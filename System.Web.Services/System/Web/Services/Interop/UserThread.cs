namespace System.Web.Services.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class UserThread
    {
        internal int pSidBuffer = 0;
        internal int dwSidLen = 0;
        internal int dwTid = 0;
        public override bool Equals(object obj)
        {
            if (!(obj is UserThread))
            {
                return false;
            }
            UserThread thread = (UserThread) obj;
            return (((thread.dwTid == this.dwTid) && (thread.pSidBuffer == this.pSidBuffer)) && (thread.dwSidLen == this.dwSidLen));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal UserThread()
        {
        }
    }
}

