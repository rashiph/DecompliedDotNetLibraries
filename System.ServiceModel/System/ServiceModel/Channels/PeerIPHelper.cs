namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Threading;

    internal class PeerIPHelper
    {
        private AddressChangeHelper addressChangeHelper;
        private Socket ipv6Socket;
        private const uint IsatapIdentifier = 0xfe5e0000;
        private bool isOpen;
        private readonly IPAddress listenAddress;
        private IPAddress[] localAddresses;
        private const uint Six2FourPrefix = 0x220;
        private const uint TeredoPrefix = 0x120;
        private object thisLock;

        public event EventHandler AddressChanged;

        public PeerIPHelper()
        {
            this.Initialize();
        }

        public PeerIPHelper(IPAddress listenAddress)
        {
            if (listenAddress == null)
            {
                throw Fx.AssertAndThrow("listenAddress expected to be non-null");
            }
            this.listenAddress = listenAddress;
            this.Initialize();
        }

        public bool AddressesChanged(ReadOnlyCollection<IPAddress> addresses)
        {
            lock (this.ThisLock)
            {
                if (addresses.Count != this.localAddresses.Length)
                {
                    return true;
                }
                foreach (IPAddress address in this.localAddresses)
                {
                    if (!addresses.Contains(address))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static IPAddress CloneAddress(IPAddress source, bool maskScopeId)
        {
            if (maskScopeId || V4Address(source))
            {
                return new IPAddress(source.GetAddressBytes());
            }
            return new IPAddress(source.GetAddressBytes(), source.ScopeId);
        }

        private static ReadOnlyCollection<IPAddress> CloneAddresses(IPAddress[] sourceArray)
        {
            IPAddress[] list = new IPAddress[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; i++)
            {
                list[i] = CloneAddress(sourceArray[i], false);
            }
            return new ReadOnlyCollection<IPAddress>(list);
        }

        public static ReadOnlyCollection<IPAddress> CloneAddresses(ReadOnlyCollection<IPAddress> sourceCollection, bool maskScopeId)
        {
            IPAddress[] list = new IPAddress[sourceCollection.Count];
            for (int i = 0; i < sourceCollection.Count; i++)
            {
                list[i] = CloneAddress(sourceCollection[i], maskScopeId);
            }
            return new ReadOnlyCollection<IPAddress>(list);
        }

        public void Close()
        {
            if (this.isOpen)
            {
                lock (this.ThisLock)
                {
                    if (this.isOpen)
                    {
                        this.addressChangeHelper.Unregister();
                        if (this.ipv6Socket != null)
                        {
                            this.ipv6Socket.Close();
                        }
                        this.isOpen = false;
                        this.addressChangeHelper = null;
                    }
                }
            }
        }

        private static IPAddress[] CreateAddressArray(IPAddress address)
        {
            return new IPAddress[] { CloneAddress(address, false) };
        }

        private IPAddress[] GetAddresses()
        {
            List<IPAddress> sourceAddresses = new List<IPAddress>();
            List<IPAddress> list2 = new List<IPAddress>();
            if ((this.listenAddress != null) && ValidAddress(this.listenAddress))
            {
                return CreateAddressArray(this.listenAddress);
            }
            foreach (NetworkInterface interface2 in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ValidInterface(interface2))
                {
                    IPInterfaceProperties iPProperties = interface2.GetIPProperties();
                    if (iPProperties != null)
                    {
                        foreach (UnicastIPAddressInformation information in iPProperties.UnicastAddresses)
                        {
                            if (NonTransientAddress(information))
                            {
                                if (information.SuffixOrigin == SuffixOrigin.Random)
                                {
                                    list2.Add(information.Address);
                                }
                                else
                                {
                                    sourceAddresses.Add(information.Address);
                                }
                            }
                        }
                    }
                }
            }
            if (sourceAddresses.Count > 0)
            {
                return ReorderAddresses(sourceAddresses);
            }
            return list2.ToArray();
        }

        private static AddressType GetAddressType(IPAddress address)
        {
            AddressType unknown = AddressType.Unknown;
            byte[] addressBytes = address.GetAddressBytes();
            if (BitConverter.ToUInt16(addressBytes, 0) == 0x220)
            {
                return AddressType.Six2Four;
            }
            if (BitConverter.ToUInt32(addressBytes, 0) == 0x120)
            {
                return AddressType.Teredo;
            }
            if (BitConverter.ToUInt32(addressBytes, 8) == 0xfe5e0000)
            {
                unknown = AddressType.Isatap;
            }
            return unknown;
        }

        public static EndpointAddress GetIPEndpointAddress(EndpointAddress epr, IPAddress address)
        {
            EndpointAddressBuilder builder = new EndpointAddressBuilder(epr) {
                Uri = GetIPUri(epr.Uri, address)
            };
            return builder.ToEndpointAddress();
        }

        public static Uri GetIPUri(Uri uri, IPAddress ipAddress)
        {
            UriBuilder builder = new UriBuilder(uri);
            if (V6Address(ipAddress) && (ipAddress.IsIPv6LinkLocal || ipAddress.IsIPv6SiteLocal))
            {
                builder.Host = new IPAddress(ipAddress.GetAddressBytes(), ipAddress.ScopeId).ToString();
            }
            else
            {
                builder.Host = ipAddress.ToString();
            }
            return builder.Uri;
        }

        public ReadOnlyCollection<IPAddress> GetLocalAddresses()
        {
            lock (this.ThisLock)
            {
                return CloneAddresses(this.localAddresses);
            }
        }

        private void Initialize()
        {
            this.localAddresses = new IPAddress[0];
            this.thisLock = new object();
        }

        private static bool NonTransientAddress(UnicastIPAddressInformation address)
        {
            return !address.IsTransient;
        }

        private void OnAddressChanged()
        {
            bool flag = false;
            IPAddress[] addresses = this.GetAddresses();
            lock (this.ThisLock)
            {
                if (this.AddressesChanged(Array.AsReadOnly<IPAddress>(addresses)))
                {
                    this.localAddresses = addresses;
                    flag = true;
                }
            }
            if (flag)
            {
                EventHandler addressChanged = this.AddressChanged;
                if ((addressChanged != null) && this.isOpen)
                {
                    addressChanged(this, EventArgs.Empty);
                }
            }
        }

        public void Open()
        {
            lock (this.ThisLock)
            {
                this.addressChangeHelper = new AddressChangeHelper(new AddressChangeHelper.AddedChangedCallback(this.OnAddressChanged));
                this.localAddresses = this.GetAddresses();
                if (Socket.OSSupportsIPv6)
                {
                    this.ipv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.IP);
                }
                this.isOpen = true;
            }
        }

        internal static IPAddress[] ReorderAddresses(IEnumerable<IPAddress> sourceAddresses)
        {
            List<IPAddress> list = new List<IPAddress>();
            List<IPAddress> collection = new List<IPAddress>();
            IPAddress item = null;
            IPAddress address2 = null;
            IPAddress address3 = null;
            IPAddress address4 = null;
            IPAddress address5 = null;
            foreach (IPAddress address6 in sourceAddresses)
            {
                if (address6.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (item != null)
                    {
                        collection.Add(address6);
                    }
                    else
                    {
                        item = address6;
                    }
                    continue;
                }
                if (address6.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    collection.Add(address6);
                    continue;
                }
                if (address6.IsIPv6LinkLocal || address6.IsIPv6SiteLocal)
                {
                    collection.Add(address6);
                    continue;
                }
                switch (GetAddressType(address6))
                {
                    case AddressType.Teredo:
                    {
                        if (address4 != null)
                        {
                            break;
                        }
                        address4 = address6;
                        continue;
                    }
                    case AddressType.Isatap:
                    {
                        if (address3 != null)
                        {
                            goto Label_00DC;
                        }
                        address3 = address6;
                        continue;
                    }
                    case AddressType.Six2Four:
                    {
                        if (address5 != null)
                        {
                            goto Label_00C8;
                        }
                        address5 = address6;
                        continue;
                    }
                    default:
                        goto Label_00E6;
                }
                collection.Add(address6);
                continue;
            Label_00C8:
                collection.Add(address6);
                continue;
            Label_00DC:
                collection.Add(address6);
                continue;
            Label_00E6:
                if (address2 != null)
                {
                    collection.Add(address6);
                }
                else
                {
                    address2 = address6;
                }
            }
            if (address5 != null)
            {
                list.Add(address5);
            }
            if (address4 != null)
            {
                list.Add(address4);
            }
            if (address3 != null)
            {
                list.Add(address3);
            }
            if (address2 != null)
            {
                list.Add(address2);
            }
            if (item != null)
            {
                list.Add(item);
            }
            list.AddRange(collection);
            return list.ToArray();
        }

        public ReadOnlyCollection<IPAddress> SortAddresses(ReadOnlyCollection<IPAddress> addresses)
        {
            ReadOnlyCollection<IPAddress> onlys = SocketAddressList.SortAddresses(this.ipv6Socket, this.listenAddress, addresses);
            if (this.listenAddress != null)
            {
                if (this.listenAddress.IsIPv6LinkLocal)
                {
                    foreach (IPAddress address in onlys)
                    {
                        if (address.IsIPv6LinkLocal)
                        {
                            address.ScopeId = this.listenAddress.ScopeId;
                        }
                    }
                    return onlys;
                }
                if (!this.listenAddress.IsIPv6SiteLocal)
                {
                    return onlys;
                }
                foreach (IPAddress address2 in onlys)
                {
                    if (address2.IsIPv6SiteLocal)
                    {
                        address2.ScopeId = this.listenAddress.ScopeId;
                    }
                }
            }
            return onlys;
        }

        public static bool V4Address(IPAddress address)
        {
            return (address.AddressFamily == AddressFamily.InterNetwork);
        }

        public static bool V6Address(IPAddress address)
        {
            return (address.AddressFamily == AddressFamily.InterNetworkV6);
        }

        public static bool ValidAddress(IPAddress address)
        {
            foreach (NetworkInterface interface2 in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ValidInterface(interface2))
                {
                    IPInterfaceProperties iPProperties = interface2.GetIPProperties();
                    if (iPProperties != null)
                    {
                        foreach (UnicastIPAddressInformation information in iPProperties.UnicastAddresses)
                        {
                            if (address.Equals(information.Address))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool ValidInterface(NetworkInterface networkIf)
        {
            return ((networkIf.NetworkInterfaceType != NetworkInterfaceType.Loopback) && (networkIf.OperationalStatus == OperationalStatus.Up));
        }

        internal int AddressChangeWaitTimeout
        {
            set
            {
                this.addressChangeHelper.Timeout = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private class AddressChangeHelper
        {
            private AddedChangedCallback addressChanged;
            public int Timeout = 0x1388;
            private IOThreadTimer timer;

            public AddressChangeHelper(AddedChangedCallback addressChanged)
            {
                this.addressChanged = addressChanged;
                this.timer = new IOThreadTimer(new Action<object>(this.FireAddressChange), null, true);
                NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(this.OnAddressChange);
            }

            private void FireAddressChange(object asyncState)
            {
                this.timer.Cancel();
                this.addressChanged();
            }

            private void OnAddressChange(object sender, EventArgs args)
            {
                this.timer.Set(this.Timeout);
            }

            public void Unregister()
            {
                NetworkChange.NetworkAddressChanged -= new NetworkAddressChangedEventHandler(this.OnAddressChange);
            }

            public delegate void AddedChangedCallback();
        }

        private enum AddressType
        {
            Unknown,
            Teredo,
            Isatap,
            Six2Four
        }
    }
}

