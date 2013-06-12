namespace System.Net.NetworkInformation
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    public class Ping : Component
    {
        private ManualResetEvent asyncFinished;
        private AsyncOperation asyncOp;
        private bool cancelled;
        private byte[] defaultSendBuffer;
        private const int DefaultSendBufferSize = 0x20;
        private const int DefaultTimeout = 0x1388;
        private const int Disposed = 2;
        private bool disposeRequested;
        private const int Free = 0;
        private SafeCloseIcmpHandle handlePingV4;
        private SafeCloseIcmpHandle handlePingV6;
        private const int InProgress = 1;
        private bool ipv6;
        private object lockObject = new object();
        private const int MaxBufferSize = 0xffdc;
        private const int MaxUdpPacket = 0x100ff;
        private SendOrPostCallback onPingCompletedDelegate;
        internal ManualResetEvent pingEvent;
        private RegisteredWaitHandle registeredWait;
        private SafeLocalFree replyBuffer;
        private SafeLocalFree requestBuffer;
        private int sendSize;
        private int status;

        public event PingCompletedEventHandler PingCompleted;

        public Ping()
        {
            this.onPingCompletedDelegate = new SendOrPostCallback(this.PingCompletedWaitCallback);
        }

        private void CheckStart(bool async)
        {
            if (this.disposeRequested)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            switch (Interlocked.CompareExchange(ref this.status, 1, 0))
            {
                case 1:
                    throw new InvalidOperationException(SR.GetString("net_inasync"));

                case 2:
                    throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (async)
            {
                this.InAsyncCall = true;
            }
        }

        private void ContinueAsyncSend(object state)
        {
            AsyncStateObject obj2 = (AsyncStateObject) state;
            try
            {
                IPAddress address = Dns.GetHostAddresses(obj2.hostName)[0];
                new NetworkInformationPermission(NetworkInformationAccess.Ping).Demand();
                this.InternalSend(address, obj2.buffer, obj2.timeout, obj2.options, true);
            }
            catch (Exception exception)
            {
                PingException error = new PingException(SR.GetString("net_ping"), exception);
                PingCompletedEventArgs arg = new PingCompletedEventArgs(null, error, false, this.asyncOp.UserSuppliedState);
                this.Finish(true);
                this.asyncOp.PostOperationCompleted(this.onPingCompletedDelegate, arg);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.InternalDispose();
            }
            base.Dispose(disposing);
        }

        private void Finish(bool async)
        {
            this.status = 0;
            if (async)
            {
                this.InAsyncCall = false;
            }
            if (this.disposeRequested)
            {
                this.InternalDispose();
            }
        }

        private void FreeUnmanagedStructures()
        {
            if (this.requestBuffer != null)
            {
                this.requestBuffer.Close();
                this.requestBuffer = null;
            }
        }

        private void InternalDispose()
        {
            this.disposeRequested = true;
            if (Interlocked.CompareExchange(ref this.status, 2, 0) == 0)
            {
                if (this.handlePingV4 != null)
                {
                    this.handlePingV4.Close();
                    this.handlePingV4 = null;
                }
                if (this.handlePingV6 != null)
                {
                    this.handlePingV6.Close();
                    this.handlePingV6 = null;
                }
                this.UnregisterWaitHandle();
                if (this.pingEvent != null)
                {
                    this.pingEvent.Close();
                    this.pingEvent = null;
                }
                if (this.replyBuffer != null)
                {
                    this.replyBuffer.Close();
                    this.replyBuffer = null;
                }
                if (this.asyncFinished != null)
                {
                    this.asyncFinished.Close();
                    this.asyncFinished = null;
                }
            }
        }

        private PingReply InternalSend(IPAddress address, byte[] buffer, int timeout, PingOptions options, bool async)
        {
            int num;
            PingReply reply;
            this.ipv6 = address.AddressFamily == AddressFamily.InterNetworkV6;
            this.sendSize = buffer.Length;
            if (!this.ipv6 && (this.handlePingV4 == null))
            {
                this.handlePingV4 = UnsafeNetInfoNativeMethods.IcmpCreateFile();
                if (this.handlePingV4.IsInvalid)
                {
                    this.handlePingV4 = null;
                    throw new Win32Exception();
                }
            }
            else if (this.ipv6 && (this.handlePingV6 == null))
            {
                this.handlePingV6 = UnsafeNetInfoNativeMethods.Icmp6CreateFile();
                if (this.handlePingV6.IsInvalid)
                {
                    this.handlePingV6 = null;
                    throw new Win32Exception();
                }
            }
            IPOptions options2 = new IPOptions(options);
            if (this.replyBuffer == null)
            {
                this.replyBuffer = SafeLocalFree.LocalAlloc(0x100ff);
            }
            try
            {
                if (async)
                {
                    if (this.pingEvent == null)
                    {
                        this.pingEvent = new ManualResetEvent(false);
                    }
                    else
                    {
                        this.pingEvent.Reset();
                    }
                    this.registeredWait = ThreadPool.RegisterWaitForSingleObject(this.pingEvent, new WaitOrTimerCallback(Ping.PingCallback), this, -1, true);
                }
                this.SetUnmanagedStructures(buffer);
                if (!this.ipv6)
                {
                    if (async)
                    {
                        num = (int) UnsafeNetInfoNativeMethods.IcmpSendEcho2(this.handlePingV4, this.pingEvent.SafeWaitHandle, IntPtr.Zero, IntPtr.Zero, (uint) address.m_Address, this.requestBuffer, (ushort) buffer.Length, ref options2, this.replyBuffer, 0x100ff, (uint) timeout);
                    }
                    else
                    {
                        num = (int) UnsafeNetInfoNativeMethods.IcmpSendEcho2(this.handlePingV4, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint) address.m_Address, this.requestBuffer, (ushort) buffer.Length, ref options2, this.replyBuffer, 0x100ff, (uint) timeout);
                    }
                }
                else
                {
                    SocketAddress address2 = new IPEndPoint(address, 0).Serialize();
                    byte[] sourceSocketAddress = new byte[0x1c];
                    if (async)
                    {
                        num = (int) UnsafeNetInfoNativeMethods.Icmp6SendEcho2(this.handlePingV6, this.pingEvent.SafeWaitHandle, IntPtr.Zero, IntPtr.Zero, sourceSocketAddress, address2.m_Buffer, this.requestBuffer, (ushort) buffer.Length, ref options2, this.replyBuffer, 0x100ff, (uint) timeout);
                    }
                    else
                    {
                        num = (int) UnsafeNetInfoNativeMethods.Icmp6SendEcho2(this.handlePingV6, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, sourceSocketAddress, address2.m_Buffer, this.requestBuffer, (ushort) buffer.Length, ref options2, this.replyBuffer, 0x100ff, (uint) timeout);
                    }
                }
            }
            catch
            {
                this.UnregisterWaitHandle();
                throw;
            }
            if (num == 0)
            {
                num = Marshal.GetLastWin32Error();
                if (async && (num == 0x3e5L))
                {
                    return null;
                }
                this.FreeUnmanagedStructures();
                this.UnregisterWaitHandle();
                if ((async || (num < 0x2afa)) || (num > 0x2b25))
                {
                    throw new Win32Exception(num);
                }
                return new PingReply((IPStatus) num);
            }
            if (async)
            {
                return null;
            }
            this.FreeUnmanagedStructures();
            if (this.ipv6)
            {
                Icmp6EchoReply reply2 = (Icmp6EchoReply) Marshal.PtrToStructure(this.replyBuffer.DangerousGetHandle(), typeof(Icmp6EchoReply));
                reply = new PingReply(reply2, this.replyBuffer.DangerousGetHandle(), this.sendSize);
            }
            else
            {
                IcmpEchoReply reply3 = (IcmpEchoReply) Marshal.PtrToStructure(this.replyBuffer.DangerousGetHandle(), typeof(IcmpEchoReply));
                reply = new PingReply(reply3);
            }
            GC.KeepAlive(this.replyBuffer);
            return reply;
        }

        protected void OnPingCompleted(PingCompletedEventArgs e)
        {
            if (this.PingCompleted != null)
            {
                this.PingCompleted(this, e);
            }
        }

        private static void PingCallback(object state, bool signaled)
        {
            Ping ping = (Ping) state;
            PingCompletedEventArgs arg = null;
            bool cancelled = false;
            AsyncOperation asyncOp = null;
            SendOrPostCallback d = null;
            try
            {
                lock (ping.lockObject)
                {
                    cancelled = ping.cancelled;
                    asyncOp = ping.asyncOp;
                    d = ping.onPingCompletedDelegate;
                    if (!cancelled)
                    {
                        PingReply reply;
                        SafeLocalFree replyBuffer = ping.replyBuffer;
                        if (!ping.ipv6 && !ComNetOS.IsVista)
                        {
                            UnsafeNetInfoNativeMethods.IcmpParseReplies(replyBuffer.DangerousGetHandle(), 0x100ff);
                        }
                        if (ping.ipv6)
                        {
                            Icmp6EchoReply reply2 = (Icmp6EchoReply) Marshal.PtrToStructure(replyBuffer.DangerousGetHandle(), typeof(Icmp6EchoReply));
                            reply = new PingReply(reply2, replyBuffer.DangerousGetHandle(), ping.sendSize);
                        }
                        else
                        {
                            IcmpEchoReply reply3 = (IcmpEchoReply) Marshal.PtrToStructure(replyBuffer.DangerousGetHandle(), typeof(IcmpEchoReply));
                            reply = new PingReply(reply3);
                        }
                        arg = new PingCompletedEventArgs(reply, null, false, asyncOp.UserSuppliedState);
                    }
                    else
                    {
                        arg = new PingCompletedEventArgs(null, null, true, asyncOp.UserSuppliedState);
                    }
                }
            }
            catch (Exception exception)
            {
                PingException error = new PingException(SR.GetString("net_ping"), exception);
                arg = new PingCompletedEventArgs(null, error, false, asyncOp.UserSuppliedState);
            }
            finally
            {
                ping.FreeUnmanagedStructures();
                ping.UnregisterWaitHandle();
                ping.Finish(true);
            }
            asyncOp.PostOperationCompleted(d, arg);
        }

        private void PingCompletedWaitCallback(object operationState)
        {
            this.OnPingCompleted((PingCompletedEventArgs) operationState);
        }

        public PingReply Send(IPAddress address)
        {
            return this.Send(address, 0x1388, this.DefaultSendBuffer, null);
        }

        public PingReply Send(string hostNameOrAddress)
        {
            return this.Send(hostNameOrAddress, 0x1388, this.DefaultSendBuffer, null);
        }

        public PingReply Send(IPAddress address, int timeout)
        {
            return this.Send(address, timeout, this.DefaultSendBuffer, null);
        }

        public PingReply Send(string hostNameOrAddress, int timeout)
        {
            return this.Send(hostNameOrAddress, timeout, this.DefaultSendBuffer, null);
        }

        public PingReply Send(IPAddress address, int timeout, byte[] buffer)
        {
            return this.Send(address, timeout, buffer, null);
        }

        public PingReply Send(string hostNameOrAddress, int timeout, byte[] buffer)
        {
            return this.Send(hostNameOrAddress, timeout, buffer, null);
        }

        public PingReply Send(IPAddress address, int timeout, byte[] buffer, PingOptions options)
        {
            IPAddress address2;
            PingReply reply;
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (buffer.Length > 0xffdc)
            {
                throw new ArgumentException(SR.GetString("net_invalidPingBufferSize"), "buffer");
            }
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.TestIsIpSupported(address);
            if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            {
                throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
            }
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                address2 = new IPAddress(address.GetAddressBytes());
            }
            else
            {
                address2 = new IPAddress(address.GetAddressBytes(), address.ScopeId);
            }
            new NetworkInformationPermission(NetworkInformationAccess.Ping).Demand();
            this.CheckStart(false);
            try
            {
                reply = this.InternalSend(address2, buffer, timeout, options, false);
            }
            catch (Exception exception)
            {
                throw new PingException(SR.GetString("net_ping"), exception);
            }
            finally
            {
                this.Finish(false);
            }
            return reply;
        }

        public PingReply Send(string hostNameOrAddress, int timeout, byte[] buffer, PingOptions options)
        {
            IPAddress address;
            if (ValidationHelper.IsBlankString(hostNameOrAddress))
            {
                throw new ArgumentNullException("hostNameOrAddress");
            }
            if (!IPAddress.TryParse(hostNameOrAddress, out address))
            {
                try
                {
                    address = Dns.GetHostAddresses(hostNameOrAddress)[0];
                }
                catch (ArgumentException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    throw new PingException(SR.GetString("net_ping"), exception);
                }
            }
            return this.Send(address, timeout, buffer, options);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(IPAddress address, object userToken)
        {
            this.SendAsync(address, 0x1388, this.DefaultSendBuffer, userToken);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(string hostNameOrAddress, object userToken)
        {
            this.SendAsync(hostNameOrAddress, 0x1388, this.DefaultSendBuffer, userToken);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(IPAddress address, int timeout, object userToken)
        {
            this.SendAsync(address, timeout, this.DefaultSendBuffer, userToken);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(string hostNameOrAddress, int timeout, object userToken)
        {
            this.SendAsync(hostNameOrAddress, timeout, this.DefaultSendBuffer, userToken);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(IPAddress address, int timeout, byte[] buffer, object userToken)
        {
            this.SendAsync(address, timeout, buffer, null, userToken);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(string hostNameOrAddress, int timeout, byte[] buffer, object userToken)
        {
            this.SendAsync(hostNameOrAddress, timeout, buffer, null, userToken);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(IPAddress address, int timeout, byte[] buffer, PingOptions options, object userToken)
        {
            IPAddress address2;
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (buffer.Length > 0xffdc)
            {
                throw new ArgumentException(SR.GetString("net_invalidPingBufferSize"), "buffer");
            }
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            this.TestIsIpSupported(address);
            if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            {
                throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
            }
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                address2 = new IPAddress(address.GetAddressBytes());
            }
            else
            {
                address2 = new IPAddress(address.GetAddressBytes(), address.ScopeId);
            }
            new NetworkInformationPermission(NetworkInformationAccess.Ping).Demand();
            this.CheckStart(true);
            try
            {
                this.cancelled = false;
                this.asyncOp = AsyncOperationManager.CreateOperation(userToken);
                this.InternalSend(address2, buffer, timeout, options, true);
            }
            catch (Exception exception)
            {
                this.Finish(true);
                throw new PingException(SR.GetString("net_ping"), exception);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public void SendAsync(string hostNameOrAddress, int timeout, byte[] buffer, PingOptions options, object userToken)
        {
            IPAddress address;
            if (ValidationHelper.IsBlankString(hostNameOrAddress))
            {
                throw new ArgumentNullException("hostNameOrAddress");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (buffer.Length > 0xffdc)
            {
                throw new ArgumentException(SR.GetString("net_invalidPingBufferSize"), "buffer");
            }
            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            if (IPAddress.TryParse(hostNameOrAddress, out address))
            {
                this.SendAsync(address, timeout, buffer, options, userToken);
            }
            else
            {
                this.CheckStart(true);
                try
                {
                    this.cancelled = false;
                    this.asyncOp = AsyncOperationManager.CreateOperation(userToken);
                    AsyncStateObject state = new AsyncStateObject(hostNameOrAddress, buffer, timeout, options, userToken);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.ContinueAsyncSend), state);
                }
                catch (Exception exception)
                {
                    this.Finish(true);
                    throw new PingException(SR.GetString("net_ping"), exception);
                }
            }
        }

        public void SendAsyncCancel()
        {
            lock (this.lockObject)
            {
                if (!this.InAsyncCall)
                {
                    return;
                }
                this.cancelled = true;
            }
            this.asyncFinished.WaitOne();
        }

        private unsafe void SetUnmanagedStructures(byte[] buffer)
        {
            this.requestBuffer = SafeLocalFree.LocalAlloc(buffer.Length);
            byte* handle = (byte*) this.requestBuffer.DangerousGetHandle();
            for (int i = 0; i < buffer.Length; i++)
            {
                handle[i] = buffer[i];
            }
        }

        private void TestIsIpSupported(IPAddress ip)
        {
            if ((ip.AddressFamily == AddressFamily.InterNetwork) && !Socket.OSSupportsIPv4)
            {
                throw new NotSupportedException(SR.GetString("net_ipv4_not_installed"));
            }
            if ((ip.AddressFamily == AddressFamily.InterNetworkV6) && !Socket.OSSupportsIPv6)
            {
                throw new NotSupportedException(SR.GetString("net_ipv6_not_installed"));
            }
        }

        private void UnregisterWaitHandle()
        {
            lock (this.lockObject)
            {
                if (this.registeredWait != null)
                {
                    this.registeredWait.Unregister(null);
                    this.registeredWait = null;
                }
            }
        }

        private byte[] DefaultSendBuffer
        {
            get
            {
                if (this.defaultSendBuffer == null)
                {
                    this.defaultSendBuffer = new byte[0x20];
                    for (int i = 0; i < 0x20; i++)
                    {
                        this.defaultSendBuffer[i] = (byte) (0x61 + (i % 0x17));
                    }
                }
                return this.defaultSendBuffer;
            }
        }

        private bool InAsyncCall
        {
            get
            {
                if (this.asyncFinished == null)
                {
                    return false;
                }
                return !this.asyncFinished.WaitOne(0);
            }
            set
            {
                if (this.asyncFinished == null)
                {
                    this.asyncFinished = new ManualResetEvent(!value);
                }
                else if (value)
                {
                    this.asyncFinished.Reset();
                }
                else
                {
                    this.asyncFinished.Set();
                }
            }
        }

        internal class AsyncStateObject
        {
            internal byte[] buffer;
            internal string hostName;
            internal PingOptions options;
            internal int timeout;
            internal object userToken;

            internal AsyncStateObject(string hostName, byte[] buffer, int timeout, PingOptions options, object userToken)
            {
                this.hostName = hostName;
                this.buffer = buffer;
                this.timeout = timeout;
                this.options = options;
                this.userToken = userToken;
            }
        }
    }
}

