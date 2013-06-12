namespace System.Net.Sockets
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;

    internal sealed class DynamicWinsockMethods
    {
        private AcceptExDelegate acceptEx;
        private AddressFamily addressFamily;
        private ConnectExDelegate connectEx;
        private DisconnectExDelegate disconnectEx;
        private DisconnectExDelegate_Blocking disconnectEx_Blocking;
        private GetAcceptExSockaddrsDelegate getAcceptExSockaddrs;
        private object lockObject;
        private ProtocolType protocolType;
        private WSARecvMsgDelegate recvMsg;
        private WSARecvMsgDelegate_Blocking recvMsg_Blocking;
        private static List<DynamicWinsockMethods> s_MethodTable = new List<DynamicWinsockMethods>();
        private SocketType socketType;
        private TransmitPacketsDelegate transmitPackets;

        private DynamicWinsockMethods(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            this.addressFamily = addressFamily;
            this.socketType = socketType;
            this.protocolType = protocolType;
            this.lockObject = new object();
        }

        private void EnsureAcceptEx(SafeCloseSocket socketHandle)
        {
            if (this.acceptEx == null)
            {
                lock (this.lockObject)
                {
                    if (this.acceptEx == null)
                    {
                        Guid guid = new Guid("{0xb5367df1,0xcbac,0x11cf,{0x95, 0xca, 0x00, 0x80, 0x5f, 0x48, 0xa1, 0x92}}");
                        IntPtr ptr = this.LoadDynamicFunctionPointer(socketHandle, ref guid);
                        this.acceptEx = (AcceptExDelegate) Marshal.GetDelegateForFunctionPointer(ptr, typeof(AcceptExDelegate));
                    }
                }
            }
        }

        private void EnsureConnectEx(SafeCloseSocket socketHandle)
        {
            if (this.connectEx == null)
            {
                lock (this.lockObject)
                {
                    if (this.connectEx == null)
                    {
                        Guid guid = new Guid("{0x25a207b9,0x0ddf3,0x4660,{0x8e,0xe9,0x76,0xe5,0x8c,0x74,0x06,0x3e}}");
                        IntPtr ptr = this.LoadDynamicFunctionPointer(socketHandle, ref guid);
                        this.connectEx = (ConnectExDelegate) Marshal.GetDelegateForFunctionPointer(ptr, typeof(ConnectExDelegate));
                    }
                }
            }
        }

        private void EnsureDisconnectEx(SafeCloseSocket socketHandle)
        {
            if (this.disconnectEx == null)
            {
                lock (this.lockObject)
                {
                    if (this.disconnectEx == null)
                    {
                        Guid guid = new Guid("{0x7fda2e11,0x8630,0x436f,{0xa0, 0x31, 0xf5, 0x36, 0xa6, 0xee, 0xc1, 0x57}}");
                        IntPtr ptr = this.LoadDynamicFunctionPointer(socketHandle, ref guid);
                        this.disconnectEx = (DisconnectExDelegate) Marshal.GetDelegateForFunctionPointer(ptr, typeof(DisconnectExDelegate));
                        this.disconnectEx_Blocking = (DisconnectExDelegate_Blocking) Marshal.GetDelegateForFunctionPointer(ptr, typeof(DisconnectExDelegate_Blocking));
                    }
                }
            }
        }

        private void EnsureGetAcceptExSockaddrs(SafeCloseSocket socketHandle)
        {
            if (this.getAcceptExSockaddrs == null)
            {
                lock (this.lockObject)
                {
                    if (this.getAcceptExSockaddrs == null)
                    {
                        Guid guid = new Guid("{0xb5367df2,0xcbac,0x11cf,{0x95, 0xca, 0x00, 0x80, 0x5f, 0x48, 0xa1, 0x92}}");
                        IntPtr ptr = this.LoadDynamicFunctionPointer(socketHandle, ref guid);
                        this.getAcceptExSockaddrs = (GetAcceptExSockaddrsDelegate) Marshal.GetDelegateForFunctionPointer(ptr, typeof(GetAcceptExSockaddrsDelegate));
                    }
                }
            }
        }

        private void EnsureTransmitPackets(SafeCloseSocket socketHandle)
        {
            if (this.transmitPackets == null)
            {
                lock (this.lockObject)
                {
                    if (this.transmitPackets == null)
                    {
                        Guid guid = new Guid("{0xd9689da0,0x1f90,0x11d3,{0x99,0x71,0x00,0xc0,0x4f,0x68,0xc8,0x76}}");
                        IntPtr ptr = this.LoadDynamicFunctionPointer(socketHandle, ref guid);
                        this.transmitPackets = (TransmitPacketsDelegate) Marshal.GetDelegateForFunctionPointer(ptr, typeof(TransmitPacketsDelegate));
                    }
                }
            }
        }

        private void EnsureWSARecvMsg(SafeCloseSocket socketHandle)
        {
            if (this.recvMsg == null)
            {
                lock (this.lockObject)
                {
                    if (this.recvMsg == null)
                    {
                        Guid guid = new Guid("{0xf689d7c8,0x6f1f,0x436b,{0x8a,0x53,0xe5,0x4f,0xe3,0x51,0xc3,0x22}}");
                        IntPtr ptr = this.LoadDynamicFunctionPointer(socketHandle, ref guid);
                        this.recvMsg = (WSARecvMsgDelegate) Marshal.GetDelegateForFunctionPointer(ptr, typeof(WSARecvMsgDelegate));
                        this.recvMsg_Blocking = (WSARecvMsgDelegate_Blocking) Marshal.GetDelegateForFunctionPointer(ptr, typeof(WSARecvMsgDelegate_Blocking));
                    }
                }
            }
        }

        public T GetDelegate<T>(SafeCloseSocket socketHandle) where T: class
        {
            if (typeof(T) == typeof(AcceptExDelegate))
            {
                this.EnsureAcceptEx(socketHandle);
                return (T) this.acceptEx;
            }
            if (typeof(T) == typeof(GetAcceptExSockaddrsDelegate))
            {
                this.EnsureGetAcceptExSockaddrs(socketHandle);
                return (T) this.getAcceptExSockaddrs;
            }
            if (typeof(T) == typeof(ConnectExDelegate))
            {
                this.EnsureConnectEx(socketHandle);
                return (T) this.connectEx;
            }
            if (typeof(T) == typeof(DisconnectExDelegate))
            {
                this.EnsureDisconnectEx(socketHandle);
                return (T) this.disconnectEx;
            }
            if (typeof(T) == typeof(DisconnectExDelegate_Blocking))
            {
                this.EnsureDisconnectEx(socketHandle);
                return (T) this.disconnectEx_Blocking;
            }
            if (typeof(T) == typeof(WSARecvMsgDelegate))
            {
                this.EnsureWSARecvMsg(socketHandle);
                return (T) this.recvMsg;
            }
            if (typeof(T) == typeof(WSARecvMsgDelegate_Blocking))
            {
                this.EnsureWSARecvMsg(socketHandle);
                return (T) this.recvMsg_Blocking;
            }
            if (typeof(T) == typeof(TransmitPacketsDelegate))
            {
                this.EnsureTransmitPackets(socketHandle);
                return (T) this.transmitPackets;
            }
            return default(T);
        }

        public static DynamicWinsockMethods GetMethods(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            lock (s_MethodTable)
            {
                DynamicWinsockMethods methods;
                for (int i = 0; i < s_MethodTable.Count; i++)
                {
                    methods = s_MethodTable[i];
                    if (((methods.addressFamily == addressFamily) && (methods.socketType == socketType)) && (methods.protocolType == protocolType))
                    {
                        return methods;
                    }
                }
                methods = new DynamicWinsockMethods(addressFamily, socketType, protocolType);
                s_MethodTable.Add(methods);
                return methods;
            }
        }

        private IntPtr LoadDynamicFunctionPointer(SafeCloseSocket socketHandle, ref Guid guid)
        {
            int num;
            IntPtr zero = IntPtr.Zero;
            if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl(socketHandle, -939524090, ref guid, sizeof(Guid), out zero, sizeof(IntPtr), out num, IntPtr.Zero, IntPtr.Zero) != SocketError.Success)
            {
                throw new SocketException();
            }
            return zero;
        }
    }
}

