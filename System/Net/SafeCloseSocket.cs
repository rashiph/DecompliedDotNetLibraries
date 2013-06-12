namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal class SafeCloseSocket : SafeHandleMinusOneIsInvalid
    {
        private InnerSafeCloseSocket m_InnerSocket;
        private volatile bool m_Released;

        protected SafeCloseSocket() : base(true)
        {
        }

        internal static SafeCloseSocket Accept(SafeCloseSocket socketHandle, byte[] socketAddress, ref int socketAddressSize)
        {
            return CreateSocket(InnerSafeCloseSocket.Accept(socketHandle, socketAddress, ref socketAddressSize));
        }

        internal void CloseAsIs()
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                InnerSafeCloseSocket socket = (this.m_InnerSocket == null) ? null : Interlocked.Exchange<InnerSafeCloseSocket>(ref this.m_InnerSocket, null);
                base.Close();
                if (socket != null)
                {
                    while (!this.m_Released)
                    {
                        Thread.SpinWait(1);
                    }
                    socket.BlockingRelease();
                }
            }
        }

        private static SafeCloseSocket CreateSocket(InnerSafeCloseSocket socket)
        {
            SafeCloseSocket target = new SafeCloseSocket();
            CreateSocket(socket, target);
            return target;
        }

        protected static void CreateSocket(InnerSafeCloseSocket socket, SafeCloseSocket target)
        {
            if ((socket != null) && socket.IsInvalid)
            {
                target.SetHandleAsInvalid();
            }
            else
            {
                bool success = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    socket.DangerousAddRef(ref success);
                }
                catch
                {
                    if (success)
                    {
                        socket.DangerousRelease();
                        success = false;
                    }
                }
                finally
                {
                    if (success)
                    {
                        target.SetInnerSocket(socket);
                        socket.Close();
                    }
                    else
                    {
                        target.SetHandleAsInvalid();
                    }
                }
            }
        }

        internal static unsafe SafeCloseSocket CreateWSASocket(byte* pinnedBuffer)
        {
            return CreateSocket(InnerSafeCloseSocket.CreateWSASocket(pinnedBuffer));
        }

        internal static SafeCloseSocket CreateWSASocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return CreateSocket(InnerSafeCloseSocket.CreateWSASocket(addressFamily, socketType, protocolType));
        }

        protected override bool ReleaseHandle()
        {
            this.m_Released = true;
            InnerSafeCloseSocket socket = (this.m_InnerSocket == null) ? null : Interlocked.Exchange<InnerSafeCloseSocket>(ref this.m_InnerSocket, null);
            if (socket != null)
            {
                socket.DangerousRelease();
            }
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void SetInnerSocket(InnerSafeCloseSocket socket)
        {
            this.m_InnerSocket = socket;
            base.SetHandle(socket.DangerousGetHandle());
        }

        public override bool IsInvalid
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                if (!base.IsClosed)
                {
                    return base.IsInvalid;
                }
                return true;
            }
        }

        internal class InnerSafeCloseSocket : SafeHandleMinusOneIsInvalid
        {
            private bool m_Blockable;
            private static readonly byte[] tempBuffer = new byte[1];

            protected InnerSafeCloseSocket() : base(true)
            {
            }

            internal static SafeCloseSocket.InnerSafeCloseSocket Accept(SafeCloseSocket socketHandle, byte[] socketAddress, ref int socketAddressSize)
            {
                SafeCloseSocket.InnerSafeCloseSocket socket = UnsafeNclNativeMethods.SafeNetHandles.accept(socketHandle.DangerousGetHandle(), socketAddress, ref socketAddressSize);
                if (socket.IsInvalid)
                {
                    socket.SetHandleAsInvalid();
                }
                return socket;
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal void BlockingRelease()
            {
                this.m_Blockable = true;
                base.DangerousRelease();
            }

            internal static unsafe SafeCloseSocket.InnerSafeCloseSocket CreateWSASocket(byte* pinnedBuffer)
            {
                SafeCloseSocket.InnerSafeCloseSocket socket = UnsafeNclNativeMethods.OSSOCK.WSASocket(AddressFamily.Unknown, SocketType.Unknown, ProtocolType.Unknown, pinnedBuffer, 0, SocketConstructorFlags.WSA_FLAG_OVERLAPPED);
                if (socket.IsInvalid)
                {
                    socket.SetHandleAsInvalid();
                }
                return socket;
            }

            internal static SafeCloseSocket.InnerSafeCloseSocket CreateWSASocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
            {
                SafeCloseSocket.InnerSafeCloseSocket socket = UnsafeNclNativeMethods.OSSOCK.WSASocket(addressFamily, socketType, protocolType, IntPtr.Zero, 0, SocketConstructorFlags.WSA_FLAG_OVERLAPPED);
                if (socket.IsInvalid)
                {
                    socket.SetHandleAsInvalid();
                }
                return socket;
            }

            protected override bool ReleaseHandle()
            {
                SocketError error;
                Linger linger;
                if (this.m_Blockable)
                {
                    error = UnsafeNclNativeMethods.SafeNetHandles.closesocket(base.handle);
                    if (error == SocketError.SocketError)
                    {
                        error = (SocketError) Marshal.GetLastWin32Error();
                    }
                    if (error != SocketError.WouldBlock)
                    {
                        return (error == SocketError.Success);
                    }
                    int argp = 0;
                    error = UnsafeNclNativeMethods.SafeNetHandles.ioctlsocket(base.handle, -2147195266, ref argp);
                    switch (error)
                    {
                        case SocketError.SocketError:
                            error = (SocketError) Marshal.GetLastWin32Error();
                            break;

                        case SocketError.InvalidArgument:
                            error = UnsafeNclNativeMethods.SafeNetHandles.WSAEventSelect(base.handle, IntPtr.Zero, AsyncEventBits.FdNone);
                            error = UnsafeNclNativeMethods.SafeNetHandles.ioctlsocket(base.handle, -2147195266, ref argp);
                            break;
                    }
                    if (error == SocketError.Success)
                    {
                        error = UnsafeNclNativeMethods.SafeNetHandles.closesocket(base.handle);
                        if (error == SocketError.SocketError)
                        {
                            error = (SocketError) Marshal.GetLastWin32Error();
                        }
                        if (error != SocketError.WouldBlock)
                        {
                            return (error == SocketError.Success);
                        }
                    }
                }
                linger.OnOff = 1;
                linger.Time = 0;
                error = UnsafeNclNativeMethods.SafeNetHandles.setsockopt(base.handle, SocketOptionLevel.Socket, SocketOptionName.Linger, ref linger, 4);
                if (error == SocketError.SocketError)
                {
                    error = (SocketError) Marshal.GetLastWin32Error();
                }
                if (((error != SocketError.Success) && (error != SocketError.InvalidArgument)) && (error != SocketError.ProtocolOption))
                {
                    return false;
                }
                return (UnsafeNclNativeMethods.SafeNetHandles.closesocket(base.handle) == SocketError.Success);
            }

            public override bool IsInvalid
            {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                get
                {
                    if (!base.IsClosed)
                    {
                        return base.IsInvalid;
                    }
                    return true;
                }
            }
        }
    }
}

