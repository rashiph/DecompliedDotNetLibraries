namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    public static class Dns
    {
        private const int HostNameBufferLength = 0x100;
        private const int MaxHostName = 0xff;
        private static WaitCallback resolveCallback = new WaitCallback(Dns.ResolveCallback);
        private static DnsPermission s_DnsPermission = new DnsPermission(PermissionState.Unrestricted);

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static IAsyncResult BeginGetHostAddresses(string hostNameOrAddress, AsyncCallback requestCallback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostAddresses", hostNameOrAddress);
            }
            IAsyncResult retObject = HostResolutionBeginHelper(hostNameOrAddress, true, true, true, true, requestCallback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostAddresses", retObject);
            }
            return retObject;
        }

        [Obsolete("BeginGetHostByName is obsoleted for this type, please use BeginGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202"), HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static IAsyncResult BeginGetHostByName(string hostName, AsyncCallback requestCallback, object stateObject)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostByName", hostName);
            }
            IAsyncResult retObject = HostResolutionBeginHelper(hostName, true, true, false, false, requestCallback, stateObject);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostByName", retObject);
            }
            return retObject;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static IAsyncResult BeginGetHostEntry(IPAddress address, AsyncCallback requestCallback, object stateObject)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostEntry", address);
            }
            IAsyncResult retObject = HostResolutionBeginHelper(address, true, true, requestCallback, stateObject);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostEntry", retObject);
            }
            return retObject;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static IAsyncResult BeginGetHostEntry(string hostNameOrAddress, AsyncCallback requestCallback, object stateObject)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "BeginGetHostEntry", hostNameOrAddress);
            }
            IAsyncResult retObject = HostResolutionBeginHelper(hostNameOrAddress, false, true, true, true, requestCallback, stateObject);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "BeginGetHostEntry", retObject);
            }
            return retObject;
        }

        [Obsolete("BeginResolve is obsoleted for this type, please use BeginGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202"), HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public static IAsyncResult BeginResolve(string hostName, AsyncCallback requestCallback, object stateObject)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "BeginResolve", hostName);
            }
            IAsyncResult retObject = HostResolutionBeginHelper(hostName, false, true, false, false, requestCallback, stateObject);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "BeginResolve", retObject);
            }
            return retObject;
        }

        public static IPAddress[] EndGetHostAddresses(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "EndGetHostAddresses", asyncResult);
            }
            IPHostEntry retObject = HostResolutionEndHelper(asyncResult);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "EndGetHostAddresses", retObject);
            }
            return retObject.AddressList;
        }

        [Obsolete("EndGetHostByName is obsoleted for this type, please use EndGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static IPHostEntry EndGetHostByName(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "EndGetHostByName", asyncResult);
            }
            IPHostEntry retObject = HostResolutionEndHelper(asyncResult);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "EndGetHostByName", retObject);
            }
            return retObject;
        }

        public static IPHostEntry EndGetHostEntry(IAsyncResult asyncResult)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "EndGetHostEntry", asyncResult);
            }
            IPHostEntry retObject = HostResolutionEndHelper(asyncResult);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "EndGetHostEntry", retObject);
            }
            return retObject;
        }

        [Obsolete("EndResolve is obsoleted for this type, please use EndGetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static IPHostEntry EndResolve(IAsyncResult asyncResult)
        {
            IPHostEntry unresolveAnswer;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "EndResolve", asyncResult);
            }
            try
            {
                unresolveAnswer = HostResolutionEndHelper(asyncResult);
            }
            catch (SocketException exception)
            {
                IPAddress address = ((ResolveAsyncResult) asyncResult).address;
                if (address == null)
                {
                    throw;
                }
                if (Logging.On)
                {
                    Logging.PrintWarning(Logging.Sockets, "DNS", "DNS.EndResolve", exception.Message);
                }
                unresolveAnswer = GetUnresolveAnswer(address);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "EndResolve", unresolveAnswer);
            }
            return unresolveAnswer;
        }

        private static IPHostEntry GetAddrInfo(string name)
        {
            IPHostEntry entry;
            SocketError socketError = TryGetAddrInfo(name, out entry);
            if (socketError != SocketError.Success)
            {
                throw new SocketException(socketError);
            }
            return entry;
        }

        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            IPAddress address;
            IPAddress[] addressList;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "GetHostAddresses", hostNameOrAddress);
            }
            s_DnsPermission.Demand();
            if (hostNameOrAddress == null)
            {
                throw new ArgumentNullException("hostNameOrAddress");
            }
            if (IPAddress.TryParse(hostNameOrAddress, out address))
            {
                if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
                {
                    throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "hostNameOrAddress");
                }
                addressList = new IPAddress[] { address };
            }
            else
            {
                addressList = InternalGetHostByName(hostNameOrAddress, true).AddressList;
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "GetHostAddresses", addressList);
            }
            return addressList;
        }

        [Obsolete("GetHostByAddress is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static IPHostEntry GetHostByAddress(IPAddress address)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "GetHostByAddress", "");
            }
            s_DnsPermission.Demand();
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            IPHostEntry hostByAddress = InternalGetHostByAddress(address, false);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "GetHostByAddress", hostByAddress);
            }
            return hostByAddress;
        }

        [Obsolete("GetHostByAddress is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static IPHostEntry GetHostByAddress(string address)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "GetHostByAddress", address);
            }
            s_DnsPermission.Demand();
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            IPHostEntry hostByAddress = InternalGetHostByAddress(IPAddress.Parse(address), false);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "GetHostByAddress", hostByAddress);
            }
            return hostByAddress;
        }

        [Obsolete("GetHostByName is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static IPHostEntry GetHostByName(string hostName)
        {
            IPAddress address;
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            s_DnsPermission.Demand();
            if (IPAddress.TryParse(hostName, out address))
            {
                return GetUnresolveAnswer(address);
            }
            return InternalGetHostByName(hostName, false);
        }

        public static IPHostEntry GetHostEntry(IPAddress address)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "GetHostEntry", "");
            }
            s_DnsPermission.Demand();
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            {
                throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
            }
            IPHostEntry hostByAddress = InternalGetHostByAddress(address, true);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "GetHostEntry", hostByAddress);
            }
            return hostByAddress;
        }

        public static IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            IPAddress address;
            IPHostEntry hostByAddress;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "GetHostEntry", hostNameOrAddress);
            }
            s_DnsPermission.Demand();
            if (hostNameOrAddress == null)
            {
                throw new ArgumentNullException("hostNameOrAddress");
            }
            if (IPAddress.TryParse(hostNameOrAddress, out address))
            {
                if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
                {
                    throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "hostNameOrAddress");
                }
                hostByAddress = InternalGetHostByAddress(address, true);
            }
            else
            {
                hostByAddress = InternalGetHostByName(hostNameOrAddress, true);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "GetHostEntry", hostByAddress);
            }
            return hostByAddress;
        }

        public static string GetHostName()
        {
            s_DnsPermission.Demand();
            Socket.InitializeSockets();
            StringBuilder hostName = new StringBuilder(0x100);
            if (UnsafeNclNativeMethods.OSSOCK.gethostname(hostName, 0x100) != SocketError.Success)
            {
                throw new SocketException();
            }
            return hostName.ToString();
        }

        private static IPHostEntry GetUnresolveAnswer(IPAddress address)
        {
            return new IPHostEntry { HostName = address.ToString(), Aliases = new string[0], AddressList = new IPAddress[] { address } };
        }

        private static IAsyncResult HostResolutionBeginHelper(IPAddress address, bool flowContext, bool includeIPv6, AsyncCallback requestCallback, object state)
        {
            s_DnsPermission.Demand();
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
            {
                throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "address");
            }
            ResolveAsyncResult result = new ResolveAsyncResult(address, null, includeIPv6, state, requestCallback);
            if (flowContext)
            {
                result.StartPostingAsyncOp(false);
            }
            ThreadPool.UnsafeQueueUserWorkItem(resolveCallback, result);
            result.FinishPostingAsyncOp();
            return result;
        }

        private static IAsyncResult HostResolutionBeginHelper(string hostName, bool justReturnParsedIp, bool flowContext, bool includeIPv6, bool throwOnIPAny, AsyncCallback requestCallback, object state)
        {
            IPAddress address;
            ResolveAsyncResult result;
            s_DnsPermission.Demand();
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            if (IPAddress.TryParse(hostName, out address))
            {
                if (throwOnIPAny && (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any)))
                {
                    throw new ArgumentException(SR.GetString("net_invalid_ip_addr"), "hostNameOrAddress");
                }
                result = new ResolveAsyncResult(address, null, includeIPv6, state, requestCallback);
                if (justReturnParsedIp)
                {
                    IPHostEntry unresolveAnswer = GetUnresolveAnswer(address);
                    result.StartPostingAsyncOp(false);
                    result.InvokeCallback(unresolveAnswer);
                    result.FinishPostingAsyncOp();
                    return result;
                }
            }
            else
            {
                result = new ResolveAsyncResult(hostName, null, includeIPv6, state, requestCallback);
            }
            if (flowContext)
            {
                result.StartPostingAsyncOp(false);
            }
            ThreadPool.UnsafeQueueUserWorkItem(resolveCallback, result);
            result.FinishPostingAsyncOp();
            return result;
        }

        private static IPHostEntry HostResolutionEndHelper(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            ResolveAsyncResult result = asyncResult as ResolveAsyncResult;
            if (result == null)
            {
                throw new ArgumentException(SR.GetString("net_io_invalidasyncresult"), "asyncResult");
            }
            if (result.EndCalled)
            {
                throw new InvalidOperationException(SR.GetString("net_io_invalidendcall", new object[] { "EndResolve" }));
            }
            result.InternalWaitForCompletion();
            result.EndCalled = true;
            Exception exception = result.Result as Exception;
            if (exception != null)
            {
                throw exception;
            }
            return (IPHostEntry) result.Result;
        }

        internal static IPHostEntry InternalGetHostByAddress(IPAddress address, bool includeIPv6)
        {
            SocketError success = SocketError.Success;
            Exception e = null;
            if (Socket.LegacySupportsIPv6 || (includeIPv6 && ComNetOS.IsPostWin2K))
            {
                string name = TryGetNameInfo(address, out success);
                if (success == SocketError.Success)
                {
                    IPHostEntry entry;
                    success = TryGetAddrInfo(name, out entry);
                    if ((success != SocketError.Success) && Logging.On)
                    {
                        Logging.Exception(Logging.Sockets, "DNS", "InternalGetHostByAddress", new SocketException(success));
                    }
                    return entry;
                }
                e = new SocketException(success);
            }
            else
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    throw new SocketException(SocketError.ProtocolNotSupported);
                }
                int addr = (int) address.m_Address;
                IntPtr nativePointer = UnsafeNclNativeMethods.OSSOCK.gethostbyaddr(ref addr, Marshal.SizeOf(typeof(int)), ProtocolFamily.InterNetwork);
                if (nativePointer != IntPtr.Zero)
                {
                    return NativeToHostEntry(nativePointer);
                }
                e = new SocketException();
            }
            if (Logging.On)
            {
                Logging.Exception(Logging.Sockets, "DNS", "InternalGetHostByAddress", e);
            }
            throw e;
        }

        internal static IPHostEntry InternalGetHostByName(string hostName)
        {
            return InternalGetHostByName(hostName, true);
        }

        internal static IPHostEntry InternalGetHostByName(string hostName, bool includeIPv6)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "GetHostByName", hostName);
            }
            IPHostEntry retObject = null;
            if ((hostName.Length > 0xff) || ((hostName.Length == 0xff) && (hostName[0xfe] != '.')))
            {
                object[] args = new object[] { "hostName", 0xff.ToString(NumberFormatInfo.CurrentInfo) };
                throw new ArgumentOutOfRangeException("hostName", SR.GetString("net_toolong", args));
            }
            if (Socket.LegacySupportsIPv6 || (includeIPv6 && ComNetOS.IsPostWin2K))
            {
                retObject = GetAddrInfo(hostName);
            }
            else
            {
                IntPtr nativePointer = UnsafeNclNativeMethods.OSSOCK.gethostbyname(hostName);
                if (nativePointer == IntPtr.Zero)
                {
                    IPAddress address;
                    SocketException exception = new SocketException();
                    if (!IPAddress.TryParse(hostName, out address))
                    {
                        throw exception;
                    }
                    retObject = GetUnresolveAnswer(address);
                    if (Logging.On)
                    {
                        Logging.Exit(Logging.Sockets, "DNS", "GetHostByName", retObject);
                    }
                    return retObject;
                }
                retObject = NativeToHostEntry(nativePointer);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "GetHostByName", retObject);
            }
            return retObject;
        }

        internal static IPHostEntry InternalResolveFast(string hostName, int timeout, out bool timedOut)
        {
            timedOut = false;
            if ((hostName.Length > 0) && (hostName.Length <= 0xff))
            {
                IPAddress address;
                if (IPAddress.TryParse(hostName, out address))
                {
                    return GetUnresolveAnswer(address);
                }
                if (Socket.OSSupportsIPv6)
                {
                    try
                    {
                        return GetAddrInfo(hostName);
                    }
                    catch (Exception)
                    {
                        goto Label_0058;
                    }
                }
                IntPtr nativePointer = UnsafeNclNativeMethods.OSSOCK.gethostbyname(hostName);
                if (nativePointer != IntPtr.Zero)
                {
                    return NativeToHostEntry(nativePointer);
                }
            }
        Label_0058:
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "InternalResolveFast", (string) null);
            }
            return null;
        }

        private static IPHostEntry NativeToHostEntry(IntPtr nativePointer)
        {
            hostent hostent = (hostent) Marshal.PtrToStructure(nativePointer, typeof(hostent));
            IPHostEntry entry = new IPHostEntry();
            if (hostent.h_name != IntPtr.Zero)
            {
                entry.HostName = Marshal.PtrToStringAnsi(hostent.h_name);
            }
            ArrayList list = new ArrayList();
            IntPtr ptr = hostent.h_addr_list;
            nativePointer = Marshal.ReadIntPtr(ptr);
            while (nativePointer != IntPtr.Zero)
            {
                int newAddress = Marshal.ReadInt32(nativePointer);
                list.Add(new IPAddress(newAddress));
                ptr = IntPtrHelper.Add(ptr, IntPtr.Size);
                nativePointer = Marshal.ReadIntPtr(ptr);
            }
            entry.AddressList = new IPAddress[list.Count];
            list.CopyTo(entry.AddressList, 0);
            list.Clear();
            ptr = hostent.h_aliases;
            nativePointer = Marshal.ReadIntPtr(ptr);
            while (nativePointer != IntPtr.Zero)
            {
                string str = Marshal.PtrToStringAnsi(nativePointer);
                list.Add(str);
                ptr = IntPtrHelper.Add(ptr, IntPtr.Size);
                nativePointer = Marshal.ReadIntPtr(ptr);
            }
            entry.Aliases = new string[list.Count];
            list.CopyTo(entry.Aliases, 0);
            return entry;
        }

        [Obsolete("Resolve is obsoleted for this type, please use GetHostEntry instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static IPHostEntry Resolve(string hostName)
        {
            IPAddress address;
            IPHostEntry hostByAddress;
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "Resolve", hostName);
            }
            s_DnsPermission.Demand();
            if (hostName == null)
            {
                throw new ArgumentNullException("hostName");
            }
            if (IPAddress.TryParse(hostName, out address) && ((address.AddressFamily != AddressFamily.InterNetworkV6) || Socket.LegacySupportsIPv6))
            {
                try
                {
                    hostByAddress = InternalGetHostByAddress(address, false);
                }
                catch (SocketException exception)
                {
                    if (Logging.On)
                    {
                        Logging.PrintWarning(Logging.Sockets, "DNS", "DNS.Resolve", exception.Message);
                    }
                    hostByAddress = GetUnresolveAnswer(address);
                }
            }
            else
            {
                hostByAddress = InternalGetHostByName(hostName, false);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "Resolve", hostByAddress);
            }
            return hostByAddress;
        }

        private static void ResolveCallback(object context)
        {
            IPHostEntry hostByAddress;
            ResolveAsyncResult result = (ResolveAsyncResult) context;
            try
            {
                if (result.address != null)
                {
                    hostByAddress = InternalGetHostByAddress(result.address, result.includeIPv6);
                }
                else
                {
                    hostByAddress = InternalGetHostByName(result.hostName, result.includeIPv6);
                }
            }
            catch (Exception exception)
            {
                if (((exception is OutOfMemoryException) || (exception is ThreadAbortException)) || (exception is StackOverflowException))
                {
                    throw;
                }
                result.InvokeCallback(exception);
                return;
            }
            result.InvokeCallback(hostByAddress);
        }

        private static unsafe SocketError TryGetAddrInfo(string name, out IPHostEntry hostinfo)
        {
            if (!ComNetOS.IsPostWin2K)
            {
                throw new SocketException(SocketError.OperationNotSupported);
            }
            SafeFreeAddrInfo outAddrInfo = null;
            ArrayList list = new ArrayList();
            string str = null;
            AddressInfo hints = new AddressInfo {
                ai_flags = AddressInfoHints.AI_CANONNAME,
                ai_family = AddressFamily.Unspecified
            };
            try
            {
                SocketError error = (SocketError) SafeFreeAddrInfo.GetAddrInfo(name, null, ref hints, out outAddrInfo);
                if (error != SocketError.Success)
                {
                    hostinfo = new IPHostEntry();
                    hostinfo.HostName = name;
                    hostinfo.Aliases = new string[0];
                    hostinfo.AddressList = new IPAddress[0];
                    return error;
                }
                for (AddressInfo* infoPtr = (AddressInfo*) outAddrInfo.DangerousGetHandle(); infoPtr != null; infoPtr = infoPtr->ai_next)
                {
                    if ((str == null) && (infoPtr->ai_canonname != null))
                    {
                        str = new string(infoPtr->ai_canonname);
                    }
                    if ((infoPtr->ai_family == AddressFamily.InterNetwork) || ((infoPtr->ai_family == AddressFamily.InterNetworkV6) && Socket.OSSupportsIPv6))
                    {
                        SocketAddress socketAddress = new SocketAddress(infoPtr->ai_family, infoPtr->ai_addrlen);
                        for (int i = 0; i < infoPtr->ai_addrlen; i++)
                        {
                            socketAddress.m_Buffer[i] = infoPtr->ai_addr[i];
                        }
                        if (infoPtr->ai_family == AddressFamily.InterNetwork)
                        {
                            list.Add(((IPEndPoint) IPEndPoint.Any.Create(socketAddress)).Address);
                        }
                        else
                        {
                            list.Add(((IPEndPoint) IPEndPoint.IPv6Any.Create(socketAddress)).Address);
                        }
                    }
                }
            }
            finally
            {
                if (outAddrInfo != null)
                {
                    outAddrInfo.Close();
                }
            }
            hostinfo = new IPHostEntry();
            hostinfo.HostName = (str != null) ? str : name;
            hostinfo.Aliases = new string[0];
            hostinfo.AddressList = new IPAddress[list.Count];
            list.CopyTo(hostinfo.AddressList);
            return SocketError.Success;
        }

        internal static string TryGetNameInfo(IPAddress addr, out SocketError errorCode)
        {
            if (!ComNetOS.IsPostWin2K)
            {
                throw new SocketException(SocketError.OperationNotSupported);
            }
            SocketAddress address = new IPEndPoint(addr, 0).Serialize();
            StringBuilder host = new StringBuilder(0x401);
            int flags = 4;
            Socket.InitializeSockets();
            errorCode = UnsafeNclNativeMethods.OSSOCK.getnameinfo(address.m_Buffer, address.m_Size, host, host.Capacity, null, 0, flags);
            if (errorCode != SocketError.Success)
            {
                return null;
            }
            return host.ToString();
        }

        internal static IAsyncResult UnsafeBeginGetHostAddresses(string hostName, AsyncCallback requestCallback, object state)
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Sockets, "DNS", "UnsafeBeginGetHostAddresses", hostName);
            }
            IAsyncResult retObject = HostResolutionBeginHelper(hostName, true, false, true, true, requestCallback, state);
            if (Logging.On)
            {
                Logging.Exit(Logging.Sockets, "DNS", "UnsafeBeginGetHostAddresses", retObject);
            }
            return retObject;
        }

        private class ResolveAsyncResult : ContextAwareResult
        {
            internal IPAddress address;
            internal readonly string hostName;
            internal bool includeIPv6;

            internal ResolveAsyncResult(IPAddress address, object myObject, bool includeIPv6, object myState, AsyncCallback myCallBack) : base(myObject, myState, myCallBack)
            {
                this.includeIPv6 = includeIPv6;
                this.address = address;
            }

            internal ResolveAsyncResult(string hostName, object myObject, bool includeIPv6, object myState, AsyncCallback myCallBack) : base(myObject, myState, myCallBack)
            {
                this.hostName = hostName;
                this.includeIPv6 = includeIPv6;
            }
        }
    }
}

