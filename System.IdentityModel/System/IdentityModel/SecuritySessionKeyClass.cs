namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    internal class SecuritySessionKeyClass
    {
        private byte[] sessionKey;

        internal SecuritySessionKeyClass(SafeHandle safeHandle, int sessionKeyLength)
        {
            byte[] destination = new byte[sessionKeyLength];
            Marshal.Copy(safeHandle.DangerousGetHandle(), destination, 0, sessionKeyLength);
            this.sessionKey = destination;
        }

        internal byte[] SessionKey
        {
            get
            {
                return this.sessionKey;
            }
        }
    }
}

