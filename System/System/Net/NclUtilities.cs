namespace System.Net
{
    using System;
    using System.Collections;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal static class NclUtilities
    {
        private static IPAddress[] _LocalAddresses;
        private static object _LocalAddressesLock;
        private static NetworkAddressChangePolled s_AddressChange;
        private static ContextCallback s_ContextRelativeDemandCallback;

        private static void DemandCallback(object state)
        {
            ((CodeAccessPermission) state).Demand();
        }

        private static IPAddress[] GetLocalAddresses()
        {
            IPAddress[] addressArray;
            if (!ComNetOS.IsPostWin2K)
            {
                ArrayList list2 = new ArrayList(0x10);
                int num5 = 0;
                SafeLocalFree pAdapterInfo = null;
                uint pOutBufLen = 0;
                uint adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(SafeLocalFree.Zero, ref pOutBufLen);
                while (adaptersInfo == 0x6f)
                {
                    try
                    {
                        pAdapterInfo = SafeLocalFree.LocalAlloc((int) pOutBufLen);
                        adaptersInfo = UnsafeNetInfoNativeMethods.GetAdaptersInfo(pAdapterInfo, ref pOutBufLen);
                        if (adaptersInfo == 0)
                        {
                            IpAdapterInfo info = (IpAdapterInfo) Marshal.PtrToStructure(pAdapterInfo.DangerousGetHandle(), typeof(IpAdapterInfo));
                            while (true)
                            {
                                IPAddressCollection addresss = info.ipAddressList.ToIPAddressCollection();
                                num5 += addresss.Count;
                                list2.Add(addresss);
                                if (info.Next == IntPtr.Zero)
                                {
                                    break;
                                }
                                info = (IpAdapterInfo) Marshal.PtrToStructure(info.Next, typeof(IpAdapterInfo));
                            }
                        }
                        continue;
                    }
                    finally
                    {
                        if (pAdapterInfo != null)
                        {
                            pAdapterInfo.Close();
                        }
                    }
                }
                if ((adaptersInfo != 0) && (adaptersInfo != 0xe8))
                {
                    throw new NetworkInformationException((int) adaptersInfo);
                }
                addressArray = new IPAddress[num5];
                uint num8 = 0;
                foreach (IPAddressCollection addresss2 in list2)
                {
                    foreach (IPAddress address in addresss2)
                    {
                        addressArray[num8++] = address;
                    }
                }
                return addressArray;
            }
            ArrayList list = new ArrayList(0x10);
            int num = 0;
            SafeLocalFree adapterAddresses = null;
            GetAdaptersAddressesFlags flags = GetAdaptersAddressesFlags.SkipFriendlyName | GetAdaptersAddressesFlags.SkipDnsServer | GetAdaptersAddressesFlags.SkipMulticast | GetAdaptersAddressesFlags.SkipAnycast;
            uint outBufLen = 0;
            uint num3 = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(AddressFamily.Unspecified, (uint) flags, IntPtr.Zero, SafeLocalFree.Zero, ref outBufLen);
            while (num3 == 0x6f)
            {
                try
                {
                    adapterAddresses = SafeLocalFree.LocalAlloc((int) outBufLen);
                    num3 = UnsafeNetInfoNativeMethods.GetAdaptersAddresses(AddressFamily.Unspecified, (uint) flags, IntPtr.Zero, adapterAddresses, ref outBufLen);
                    if (num3 != 0)
                    {
                        continue;
                    }
                    IpAdapterAddresses addresses = (IpAdapterAddresses) Marshal.PtrToStructure(adapterAddresses.DangerousGetHandle(), typeof(IpAdapterAddresses));
                Label_0075:
                    if (addresses.FirstUnicastAddress != IntPtr.Zero)
                    {
                        UnicastIPAddressInformationCollection informations = SystemUnicastIPAddressInformation.ToAddressInformationCollection(addresses.FirstUnicastAddress);
                        num += informations.Count;
                        list.Add(informations);
                    }
                    if (!(addresses.next == IntPtr.Zero))
                    {
                        addresses = (IpAdapterAddresses) Marshal.PtrToStructure(addresses.next, typeof(IpAdapterAddresses));
                        goto Label_0075;
                    }
                    continue;
                }
                finally
                {
                    if (adapterAddresses != null)
                    {
                        adapterAddresses.Close();
                    }
                    adapterAddresses = null;
                }
            }
            if ((num3 != 0) && (num3 != 0xe8))
            {
                throw new NetworkInformationException((int) num3);
            }
            addressArray = new IPAddress[num];
            uint num4 = 0;
            foreach (UnicastIPAddressInformationCollection informations2 in list)
            {
                foreach (IPAddressInformation information in informations2)
                {
                    addressArray[num4++] = information.Address;
                }
            }
            return addressArray;
        }

        internal static bool GuessWhetherHostIsLoopback(string host)
        {
            string str = host.ToLowerInvariant();
            switch (str)
            {
                case "localhost":
                case "loopback":
                    return true;
            }
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.InternalGetIPGlobalProperties();
            string str2 = iPGlobalProperties.HostName.ToLowerInvariant();
            if (!(str == str2))
            {
                return (str == (str2 + "." + iPGlobalProperties.DomainName.ToLowerInvariant()));
            }
            return true;
        }

        internal static bool IsAddressLocal(IPAddress ipAddress)
        {
            IPAddress[] localAddresses = LocalAddresses;
            for (int i = 0; i < localAddresses.Length; i++)
            {
                if (ipAddress.Equals(localAddresses[i], false))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsClientFault(SecurityStatus error)
        {
            if (((((error != SecurityStatus.InvalidToken) && (error != SecurityStatus.CannotPack)) && ((error != SecurityStatus.QopNotSupported) && (error != SecurityStatus.NoCredentials))) && (((error != SecurityStatus.MessageAltered) && (error != SecurityStatus.OutOfSequence)) && ((error != SecurityStatus.IncompleteMessage) && (error != SecurityStatus.IncompleteCredentials)))) && ((((error != SecurityStatus.WrongPrincipal) && (error != SecurityStatus.TimeSkew)) && ((error != SecurityStatus.IllegalMessage) && (error != SecurityStatus.CertUnknown))) && ((error != SecurityStatus.AlgorithmMismatch) && (error != SecurityStatus.SecurityQosFailed))))
            {
                return (error == SecurityStatus.UnsupportedPreauth);
            }
            return true;
        }

        internal static bool IsCredentialFailure(SecurityStatus error)
        {
            if ((((error != SecurityStatus.LogonDenied) && (error != SecurityStatus.UnknownCredentials)) && ((error != SecurityStatus.NoImpersonation) && (error != SecurityStatus.NoAuthenticatingAuthority))) && (((error != SecurityStatus.UntrustedRoot) && (error != SecurityStatus.CertExpired)) && (error != SecurityStatus.SmartcardLogonRequired)))
            {
                return (error == SecurityStatus.BadBinding);
            }
            return true;
        }

        internal static bool IsFatal(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }
            return (((exception is OutOfMemoryException) || (exception is StackOverflowException)) || (exception is ThreadAbortException));
        }

        internal static bool IsThreadPoolLow()
        {
            int num;
            int num2;
            if (ComNetOS.IsAspNetServer)
            {
                return false;
            }
            ThreadPool.GetAvailableThreads(out num, out num2);
            return ((num < 2) || (ComNetOS.IsWinNt && (num2 < 2)));
        }

        internal static ContextCallback ContextRelativeDemandCallback
        {
            get
            {
                if (s_ContextRelativeDemandCallback == null)
                {
                    s_ContextRelativeDemandCallback = new ContextCallback(NclUtilities.DemandCallback);
                }
                return s_ContextRelativeDemandCallback;
            }
        }

        internal static bool HasShutdownStarted
        {
            get
            {
                if (!Environment.HasShutdownStarted)
                {
                    return AppDomain.CurrentDomain.IsFinalizingForUnload();
                }
                return true;
            }
        }

        internal static IPAddress[] LocalAddresses
        {
            get
            {
                if ((s_AddressChange != null) && s_AddressChange.CheckAndReset())
                {
                    return (_LocalAddresses = GetLocalAddresses());
                }
                if (_LocalAddresses != null)
                {
                    return _LocalAddresses;
                }
                lock (LocalAddressesLock)
                {
                    if (_LocalAddresses != null)
                    {
                        return _LocalAddresses;
                    }
                    s_AddressChange = new NetworkAddressChangePolled();
                    return (_LocalAddresses = GetLocalAddresses());
                }
            }
        }

        private static object LocalAddressesLock
        {
            get
            {
                if (_LocalAddressesLock == null)
                {
                    Interlocked.CompareExchange(ref _LocalAddressesLock, new object(), null);
                }
                return _LocalAddressesLock;
            }
        }
    }
}

