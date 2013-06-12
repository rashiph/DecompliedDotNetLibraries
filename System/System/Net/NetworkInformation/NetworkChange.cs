namespace System.Net.NetworkInformation
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public sealed class NetworkChange
    {
        public static  event NetworkAddressChangedEventHandler NetworkAddressChanged
        {
            add
            {
                if (!ComNetOS.IsWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
                }
                AddressChangeListener.Start(value);
            }
            remove
            {
                AddressChangeListener.Stop(value);
            }
        }

        public static  event NetworkAvailabilityChangedEventHandler NetworkAvailabilityChanged
        {
            add
            {
                if (!ComNetOS.IsWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("Win2000Required"));
                }
                AvailabilityChangeListener.Start(value);
            }
            remove
            {
                AvailabilityChangeListener.Stop(value);
            }
        }

        private NetworkChange()
        {
        }

        internal static bool CanListenForNetworkChanges
        {
            get
            {
                if (!ComNetOS.IsWin2K)
                {
                    return false;
                }
                return true;
            }
        }

        internal static class AddressChangeListener
        {
            private static ListDictionary s_callerArray = new ListDictionary();
            private static SafeCloseSocketAndEvent s_ipv4Socket = null;
            private static WaitHandle s_ipv4WaitHandle = null;
            private static SafeCloseSocketAndEvent s_ipv6Socket = null;
            private static WaitHandle s_ipv6WaitHandle = null;
            private static bool s_isListening = false;
            private static bool s_isPending = false;
            private static RegisteredWaitHandle s_registeredWait;
            private static ContextCallback s_runHandlerCallback = new ContextCallback(NetworkChange.AddressChangeListener.RunHandlerCallback);

            private static void AddressChangedCallback(object stateObject, bool signaled)
            {
                lock (s_callerArray)
                {
                    s_isPending = false;
                    if (s_isListening)
                    {
                        s_isListening = false;
                        DictionaryEntry[] array = new DictionaryEntry[s_callerArray.Count];
                        s_callerArray.CopyTo(array, 0);
                        StartHelper(null, false, (StartIPOptions) stateObject);
                        for (int i = 0; i < array.Length; i++)
                        {
                            NetworkAddressChangedEventHandler key = (NetworkAddressChangedEventHandler) array[i].Key;
                            ExecutionContext context = (ExecutionContext) array[i].Value;
                            if (context == null)
                            {
                                key(null, EventArgs.Empty);
                            }
                            else
                            {
                                ExecutionContext.Run(context.CreateCopy(), s_runHandlerCallback, key);
                            }
                        }
                    }
                }
            }

            private static void RunHandlerCallback(object state)
            {
                ((NetworkAddressChangedEventHandler) state)(null, EventArgs.Empty);
            }

            internal static void Start(NetworkAddressChangedEventHandler caller)
            {
                StartHelper(caller, true, StartIPOptions.Both);
            }

            private static void StartHelper(NetworkAddressChangedEventHandler caller, bool captureContext, StartIPOptions startIPOptions)
            {
                lock (s_callerArray)
                {
                    if (s_ipv4Socket == null)
                    {
                        int num;
                        Socket.InitializeSockets();
                        if (Socket.OSSupportsIPv4)
                        {
                            num = -1;
                            s_ipv4Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP, true, false);
                            UnsafeNclNativeMethods.OSSOCK.ioctlsocket(s_ipv4Socket, -2147195266, ref num);
                            s_ipv4WaitHandle = s_ipv4Socket.GetEventHandle();
                        }
                        if (Socket.OSSupportsIPv6)
                        {
                            num = -1;
                            s_ipv6Socket = SafeCloseSocketAndEvent.CreateWSASocketWithEvent(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.IP, true, false);
                            UnsafeNclNativeMethods.OSSOCK.ioctlsocket(s_ipv6Socket, -2147195266, ref num);
                            s_ipv6WaitHandle = s_ipv6Socket.GetEventHandle();
                        }
                    }
                    if ((caller != null) && !s_callerArray.Contains(caller))
                    {
                        s_callerArray.Add(caller, captureContext ? ExecutionContext.Capture() : null);
                    }
                    if (!s_isListening && (s_callerArray.Count != 0))
                    {
                        if (!s_isPending)
                        {
                            int num2;
                            if (Socket.OSSupportsIPv4 && ((startIPOptions & StartIPOptions.StartIPv4) != StartIPOptions.None))
                            {
                                s_registeredWait = ThreadPool.UnsafeRegisterWaitForSingleObject(s_ipv4WaitHandle, new WaitOrTimerCallback(NetworkChange.AddressChangeListener.AddressChangedCallback), StartIPOptions.StartIPv4, -1, true);
                                if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(s_ipv4Socket.DangerousGetHandle(), 0x28000017, null, 0, null, 0, out num2, SafeNativeOverlapped.Zero, IntPtr.Zero) != SocketError.Success)
                                {
                                    NetworkInformationException exception = new NetworkInformationException();
                                    if (exception.ErrorCode != 0x2733L)
                                    {
                                        throw exception;
                                    }
                                }
                                if (UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(s_ipv4Socket, s_ipv4Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange) != SocketError.Success)
                                {
                                    throw new NetworkInformationException();
                                }
                            }
                            if (Socket.OSSupportsIPv6 && ((startIPOptions & StartIPOptions.StartIPv6) != StartIPOptions.None))
                            {
                                s_registeredWait = ThreadPool.UnsafeRegisterWaitForSingleObject(s_ipv6WaitHandle, new WaitOrTimerCallback(NetworkChange.AddressChangeListener.AddressChangedCallback), StartIPOptions.StartIPv6, -1, true);
                                if (UnsafeNclNativeMethods.OSSOCK.WSAIoctl_Blocking(s_ipv6Socket.DangerousGetHandle(), 0x28000017, null, 0, null, 0, out num2, SafeNativeOverlapped.Zero, IntPtr.Zero) != SocketError.Success)
                                {
                                    NetworkInformationException exception2 = new NetworkInformationException();
                                    if (exception2.ErrorCode != 0x2733L)
                                    {
                                        throw exception2;
                                    }
                                }
                                if (UnsafeNclNativeMethods.OSSOCK.WSAEventSelect(s_ipv6Socket, s_ipv6Socket.GetEventHandle().SafeWaitHandle, AsyncEventBits.FdAddressListChange) != SocketError.Success)
                                {
                                    throw new NetworkInformationException();
                                }
                            }
                        }
                        s_isListening = true;
                        s_isPending = true;
                    }
                }
            }

            internal static void Stop(object caller)
            {
                lock (s_callerArray)
                {
                    s_callerArray.Remove(caller);
                    if ((s_callerArray.Count == 0) && s_isListening)
                    {
                        s_isListening = false;
                    }
                }
            }

            internal static void UnsafeStart(NetworkAddressChangedEventHandler caller)
            {
                StartHelper(caller, false, StartIPOptions.Both);
            }
        }

        internal static class AvailabilityChangeListener
        {
            private static NetworkAddressChangedEventHandler addressChange = null;
            private static bool isAvailable = false;
            private static ListDictionary s_availabilityCallerArray = null;
            private static ContextCallback s_RunHandlerCallback = new ContextCallback(NetworkChange.AvailabilityChangeListener.RunHandlerCallback);
            private static object syncObject = new object();

            private static void ChangedAddress(object sender, EventArgs eventArgs)
            {
                lock (syncObject)
                {
                    bool isNetworkAvailable = SystemNetworkInterface.InternalGetIsNetworkAvailable();
                    if (isNetworkAvailable != isAvailable)
                    {
                        isAvailable = isNetworkAvailable;
                        DictionaryEntry[] array = new DictionaryEntry[s_availabilityCallerArray.Count];
                        s_availabilityCallerArray.CopyTo(array, 0);
                        for (int i = 0; i < array.Length; i++)
                        {
                            NetworkAvailabilityChangedEventHandler key = (NetworkAvailabilityChangedEventHandler) array[i].Key;
                            ExecutionContext context = (ExecutionContext) array[i].Value;
                            if (context == null)
                            {
                                key(null, new NetworkAvailabilityEventArgs(isAvailable));
                            }
                            else
                            {
                                ExecutionContext.Run(context.CreateCopy(), s_RunHandlerCallback, key);
                            }
                        }
                    }
                }
            }

            private static void RunHandlerCallback(object state)
            {
                ((NetworkAvailabilityChangedEventHandler) state)(null, new NetworkAvailabilityEventArgs(isAvailable));
            }

            internal static void Start(NetworkAvailabilityChangedEventHandler caller)
            {
                lock (syncObject)
                {
                    if (s_availabilityCallerArray == null)
                    {
                        s_availabilityCallerArray = new ListDictionary();
                        addressChange = new NetworkAddressChangedEventHandler(NetworkChange.AvailabilityChangeListener.ChangedAddress);
                    }
                    if (s_availabilityCallerArray.Count == 0)
                    {
                        isAvailable = NetworkInterface.GetIsNetworkAvailable();
                        NetworkChange.AddressChangeListener.UnsafeStart(addressChange);
                    }
                    if ((caller != null) && !s_availabilityCallerArray.Contains(caller))
                    {
                        s_availabilityCallerArray.Add(caller, ExecutionContext.Capture());
                    }
                }
            }

            internal static void Stop(NetworkAvailabilityChangedEventHandler caller)
            {
                lock (syncObject)
                {
                    s_availabilityCallerArray.Remove(caller);
                    if (s_availabilityCallerArray.Count == 0)
                    {
                        NetworkChange.AddressChangeListener.Stop(addressChange);
                    }
                }
            }
        }
    }
}

