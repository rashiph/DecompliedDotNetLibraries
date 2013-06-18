namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;

    internal class SecurityBuffer
    {
        public int offset;
        public int size;
        public byte[] token;
        public BufferType type;
        public SafeHandle unmanagedToken;

        public SecurityBuffer(ChannelBinding channelBinding)
        {
            this.size = channelBinding.Size;
            this.type = BufferType.ChannelBindings;
            this.unmanagedToken = channelBinding;
        }

        public SecurityBuffer(byte[] data, BufferType tokentype)
        {
            this.size = (data == null) ? 0 : data.Length;
            this.type = tokentype;
            this.token = data;
        }

        public SecurityBuffer(int size, BufferType tokentype)
        {
            this.size = size;
            this.type = tokentype;
            this.token = (size == 0) ? null : DiagnosticUtility.Utility.AllocateByteArray(size);
        }

        public SecurityBuffer(byte[] data, int offset, int size, BufferType tokentype)
        {
            this.offset = offset;
            this.size = (data == null) ? 0 : size;
            this.type = tokentype;
            this.token = data;
        }
    }
}

