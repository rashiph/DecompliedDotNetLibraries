namespace System.Net
{
    using System;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    internal class NetworkAddressChangePolled : IDisposable
    {
        private bool disposed;
        private SafeCloseSocketAndEvent ipv4Socket;
        private SafeCloseSocketAndEvent ipv6Socket;

        internal NetworkAddressChangePolled()
        {
            int num;
            Socket.InitializeSockets();
            if (Socket.OSSupportsIPv4)
            {
                num = -1;
                this.ipv4Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP, true, false);
                UnsafeNclNativeMethods.OSSOCK.ioctlsocket(this.ipv4Socket, -2147195266, ref num);
            }
            if (Socket.OSSupportsIPv6)
            {
                num = -1;
                this.ipv6Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.IP, true, false);
                UnsafeNclNativeMethods.OSSOCK.ioctlsocket(this.ipv6Socket, -2147195266, ref num);
            }
            this.Setup(StartIPOptions.Both);
        }

        internal bool CheckAndReset()
        {
            if (!this.disposed)
            {
                lock (this)
                {
                    if (!this.disposed)
                    {
                        StartIPOptions none = StartIPOptions.None;
                        if ((this.ipv4Socket != null) && this.ipv4Socket.GetEventHandle().WaitOne(0, false))
                        {
                            none |= StartIPOptions.StartIPv4;
                        }
                        if ((this.ipv6Socket != null) && this.ipv6Socket.GetEventHandle().WaitOne(0, false))
                        {
                            none |= StartIPOptions.StartIPv6;
                        }
                        if (none != StartIPOptions.None)
                        {
                            this.Setup(none);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                lock (this)
                {
                    if (!this.disposed)
                    {
                        if (this.ipv6Socket != null)
                        {
                            this.ipv6Socket.Close();
                            this.ipv6Socket = null;
                        }
                        if (this.ipv4Socket != null)
                        {
                            this.ipv4Socket.Close();
                            this.ipv6Socket = null;
                        }
                        this.disposed = true;
                    }
                }
            }
        }

        private void Setup(StartIPOptions startIPOptions)
        {
            int num;
            if (Socket.OSSupportsIPv4 && ((startIPOptions & StartIPOptions.StartIPv4) != StartIPOptions.None))
            {
                if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(this.ipv4Socket.DangerousGetHandle(), 0x28000017, null, 0, null, 0, out num, SafeNativeOverlapped.Zero, IntPtr.Zero) != SocketError.Success)
                {
                    NetworkInformationException exception = new NetworkInformationException();
                    if (exception.ErrorCode != 0x2733L)
                    {
                        this.Dispose();
                        return;
                    }
                }
                if (UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(this.ipv4Socket, this.ipv4Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange) != SocketError.Success)
                {
                    this.Dispose();
                    return;
                }
            }
            if (Socket.OSSupportsIPv6 && ((startIPOptions & StartIPOptions.StartIPv6) != StartIPOptions.None))
            {
                if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(this.ipv6Socket.DangerousGetHandle(), 0x28000017, null, 0, null, 0, out num, SafeNativeOverlapped.Zero, IntPtr.Zero) != SocketError.Success)
                {
                    NetworkInformationException exception2 = new NetworkInformationException();
                    if (exception2.ErrorCode != 0x2733L)
                    {
                        this.Dispose();
                        return;
                    }
                }
                if (UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(this.ipv6Socket, this.ipv6Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange) != SocketError.Success)
                {
                    this.Dispose();
                }
            }
        }
    }
}

