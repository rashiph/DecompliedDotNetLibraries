namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class ReceiveMessageOverlappedAsyncResult : BaseOverlappedAsyncResult
    {
        private byte[] m_ControlBuffer;
        internal SocketFlags m_flags;
        internal IPPacketInformation m_IPPacketInformation;
        private unsafe UnsafeNclNativeMethods.OSSOCK.WSAMsg* m_Message;
        internal byte[] m_MessageBuffer;
        internal System.Net.SocketAddress m_SocketAddress;
        private unsafe WSABuffer* m_WSABuffer;
        private byte[] m_WSABufferArray;
        private static readonly int s_ControlDataIPv6Size = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6));
        private static readonly int s_ControlDataSize = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.ControlData));
        private static readonly int s_WSABufferSize = Marshal.SizeOf(typeof(WSABuffer));
        private static readonly int s_WSAMsgSize = Marshal.SizeOf(typeof(UnsafeNclNativeMethods.OSSOCK.WSAMsg));
        internal System.Net.SocketAddress SocketAddressOriginal;

        internal ReceiveMessageOverlappedAsyncResult(Socket socket, object asyncState, AsyncCallback asyncCallback) : base(socket, asyncState, asyncCallback)
        {
        }

        protected override unsafe void ForceReleaseUnmanagedStructures()
        {
            this.m_flags = this.m_Message.flags;
            base.ForceReleaseUnmanagedStructures();
        }

        internal IntPtr GetSocketAddressSizePtr()
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(this.m_SocketAddress.m_Buffer, this.m_SocketAddress.GetAddressSizeOffset());
        }

        private unsafe void InitIPPacketInformation()
        {
            IPAddress address = null;
            if (this.m_ControlBuffer.Length == s_ControlDataSize)
            {
                UnsafeNclNativeMethods.OSSOCK.ControlData data = (UnsafeNclNativeMethods.OSSOCK.ControlData) Marshal.PtrToStructure(this.m_Message.controlBuffer.Pointer, typeof(UnsafeNclNativeMethods.OSSOCK.ControlData));
                if (data.length != UIntPtr.Zero)
                {
                    address = new IPAddress((long) data.address);
                }
                this.m_IPPacketInformation = new IPPacketInformation((address != null) ? address : IPAddress.None, (int) data.index);
            }
            else if (this.m_ControlBuffer.Length == s_ControlDataIPv6Size)
            {
                UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6 pv = (UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6) Marshal.PtrToStructure(this.m_Message.controlBuffer.Pointer, typeof(UnsafeNclNativeMethods.OSSOCK.ControlDataIPv6));
                if (pv.length != UIntPtr.Zero)
                {
                    address = new IPAddress(pv.address);
                }
                this.m_IPPacketInformation = new IPPacketInformation((address != null) ? address : IPAddress.IPv6None, (int) pv.index);
            }
            else
            {
                this.m_IPPacketInformation = new IPPacketInformation();
            }
        }

        private unsafe void LogBuffer(int size)
        {
            Logging.Dump(Logging.Sockets, base.AsyncObject, "PostCompletion", this.m_WSABuffer.Pointer, Math.Min(this.m_WSABuffer.Length, size));
        }

        internal override object PostCompletion(int numBytes)
        {
            this.InitIPPacketInformation();
            if ((base.ErrorCode == 0) && Logging.On)
            {
                this.LogBuffer(numBytes);
            }
            return numBytes;
        }

        internal unsafe void SetUnmanagedStructures(byte[] buffer, int offset, int size, System.Net.SocketAddress socketAddress, SocketFlags socketFlags)
        {
            bool flag = ((Socket) base.AsyncObject).AddressFamily == AddressFamily.InterNetwork;
            bool flag2 = ((Socket) base.AsyncObject).AddressFamily == AddressFamily.InterNetworkV6;
            this.m_MessageBuffer = new byte[s_WSAMsgSize];
            this.m_WSABufferArray = new byte[s_WSABufferSize];
            if (flag)
            {
                this.m_ControlBuffer = new byte[s_ControlDataSize];
            }
            else if (flag2)
            {
                this.m_ControlBuffer = new byte[s_ControlDataIPv6Size];
            }
            object[] objectsToPin = new object[(this.m_ControlBuffer != null) ? 5 : 4];
            objectsToPin[0] = buffer;
            objectsToPin[1] = this.m_MessageBuffer;
            objectsToPin[2] = this.m_WSABufferArray;
            this.m_SocketAddress = socketAddress;
            this.m_SocketAddress.CopyAddressSizeIntoBuffer();
            objectsToPin[3] = this.m_SocketAddress.m_Buffer;
            if (this.m_ControlBuffer != null)
            {
                objectsToPin[4] = this.m_ControlBuffer;
            }
            base.SetUnmanagedStructures(objectsToPin);
            this.m_WSABuffer = (WSABuffer*) Marshal.UnsafeAddrOfPinnedArrayElement(this.m_WSABufferArray, 0);
            this.m_WSABuffer.Length = size;
            this.m_WSABuffer.Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, offset);
            this.m_Message = (UnsafeNclNativeMethods.OSSOCK.WSAMsg*) Marshal.UnsafeAddrOfPinnedArrayElement(this.m_MessageBuffer, 0);
            this.m_Message.socketAddress = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_SocketAddress.m_Buffer, 0);
            this.m_Message.addressLength = (uint) this.m_SocketAddress.Size;
            this.m_Message.buffers = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_WSABufferArray, 0);
            this.m_Message.count = 1;
            if (this.m_ControlBuffer != null)
            {
                this.m_Message.controlBuffer.Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(this.m_ControlBuffer, 0);
                this.m_Message.controlBuffer.Length = this.m_ControlBuffer.Length;
            }
            this.m_Message.flags = socketFlags;
        }

        internal void SetUnmanagedStructures(byte[] buffer, int offset, int size, System.Net.SocketAddress socketAddress, SocketFlags socketFlags, ref OverlappedCache overlappedCache)
        {
            base.SetupCache(ref overlappedCache);
            this.SetUnmanagedStructures(buffer, offset, size, socketAddress, socketFlags);
        }

        internal void SyncReleaseUnmanagedStructures()
        {
            this.InitIPPacketInformation();
            this.ForceReleaseUnmanagedStructures();
        }

        internal System.Net.SocketAddress SocketAddress
        {
            get
            {
                return this.m_SocketAddress;
            }
        }
    }
}

