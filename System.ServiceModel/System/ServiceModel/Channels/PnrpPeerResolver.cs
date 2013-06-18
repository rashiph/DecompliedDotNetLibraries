namespace System.ServiceModel.Channels
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.PeerResolvers;
    using System.ServiceProcess;
    using System.Text;

    internal sealed class PnrpPeerResolver : PeerResolver
    {
        internal const int CommentLength = 80;
        internal const string GlobalCloudName = "Global_";
        internal const byte GuidEscape = 0xff;
        private static bool isPnrpAvailable = false;
        private static bool isPnrpInstalled;
        private string localExtension;
        internal const int MaxAddressEntries = 10;
        internal const int MaxAddressEntriesV1 = 4;
        internal const int MaxGuids = 2;
        internal const int MaxPathLength = 200;
        private static TimeSpan MaxResolveTimeout = new TimeSpan(0, 0, 0x2d);
        private static TimeSpan MaxTimeout = new TimeSpan(0, 10, 0);
        internal const int MinGuids = 1;
        internal const char PathSeparator = '/';
        internal const byte PayloadVersion = 1;
        public const int PNRPINFO_HINT = 1;
        private static Random randomGenerator = new Random();
        private PeerReferralPolicy referralPolicy;
        private UnsafePnrpNativeMethods.PeerNameRegistrar registrar;
        private string remoteExtension;
        private const UnsafePnrpNativeMethods.PnrpResolveCriteria resolutionScope = UnsafePnrpNativeMethods.PnrpResolveCriteria.NearestNonCurrentProcess;
        private static object SharedLock = new object();
        internal const byte TcpTransport = 1;
        private static TimeSpan TimeToWaitForStatus = TimeSpan.FromSeconds(15.0);

        static PnrpPeerResolver()
        {
            using (UnsafePnrpNativeMethods.DiscoveryBase base2 = new UnsafePnrpNativeMethods.DiscoveryBase())
            {
                isPnrpInstalled = base2.IsPnrpInstalled();
                isPnrpAvailable = base2.IsPnrpAvailable(TimeToWaitForStatus);
            }
        }

        internal PnrpPeerResolver() : this(PeerReferralPolicy.Share)
        {
        }

        internal PnrpPeerResolver(PeerReferralPolicy referralPolicy)
        {
            this.registrar = new UnsafePnrpNativeMethods.PeerNameRegistrar();
            this.referralPolicy = PeerReferralPolicy.Share;
            this.referralPolicy = referralPolicy;
        }

        internal PnrpResolveScope EnumerateClouds(bool forResolve, Dictionary<uint, string> LinkCloudNames, Dictionary<uint, string> SiteCloudNames)
        {
            bool flag = false;
            PnrpResolveScope none = PnrpResolveScope.None;
            LinkCloudNames.Clear();
            SiteCloudNames.Clear();
            UnsafePnrpNativeMethods.CloudInfo[] clouds = UnsafePnrpNativeMethods.PeerCloudEnumerator.GetClouds();
            if (forResolve)
            {
                foreach (UnsafePnrpNativeMethods.CloudInfo info in clouds)
                {
                    if (info.State == UnsafePnrpNativeMethods.PnrpCloudState.Active)
                    {
                        if (info.Scope == UnsafePnrpNativeMethods.PnrpScope.Global)
                        {
                            none |= PnrpResolveScope.Global;
                            flag = true;
                        }
                        else if (info.Scope == UnsafePnrpNativeMethods.PnrpScope.LinkLocal)
                        {
                            LinkCloudNames.Add(info.ScopeId, info.Name);
                            none |= PnrpResolveScope.LinkLocal;
                            flag = true;
                        }
                        else if (info.Scope == UnsafePnrpNativeMethods.PnrpScope.SiteLocal)
                        {
                            SiteCloudNames.Add(info.ScopeId, info.Name);
                            none |= PnrpResolveScope.SiteLocal;
                            flag = true;
                        }
                    }
                }
            }
            if (!flag)
            {
                foreach (UnsafePnrpNativeMethods.CloudInfo info2 in clouds)
                {
                    if (((info2.State != UnsafePnrpNativeMethods.PnrpCloudState.Dead) && (info2.State != UnsafePnrpNativeMethods.PnrpCloudState.Disabled)) && (info2.State != UnsafePnrpNativeMethods.PnrpCloudState.NoNet))
                    {
                        if (info2.Scope == UnsafePnrpNativeMethods.PnrpScope.Global)
                        {
                            none |= PnrpResolveScope.Global;
                        }
                        else if (info2.Scope == UnsafePnrpNativeMethods.PnrpScope.LinkLocal)
                        {
                            LinkCloudNames.Add(info2.ScopeId, info2.Name);
                            none |= PnrpResolveScope.LinkLocal;
                        }
                        else if (info2.Scope == UnsafePnrpNativeMethods.PnrpScope.SiteLocal)
                        {
                            SiteCloudNames.Add(info2.ScopeId, info2.Name);
                            none |= PnrpResolveScope.SiteLocal;
                        }
                    }
                }
            }
            return none;
        }

        public override bool Equals(object other)
        {
            return (other is PnrpPeerResolver);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static IPEndPoint GetHint()
        {
            byte[] buffer = new byte[0x10];
            lock (SharedLock)
            {
                randomGenerator.NextBytes(buffer);
            }
            return new IPEndPoint(new IPAddress(buffer), 0);
        }

        public static bool HasPeerNodeForMesh(string meshId)
        {
            PeerNodeImplementation result = null;
            return PeerNodeImplementation.TryGet(meshId, out result);
        }

        private void MergeResults(Dictionary<string, PnrpRegistration> results, List<PnrpRegistration> regs)
        {
            PnrpRegistration registration = null;
            foreach (PnrpRegistration registration2 in regs)
            {
                if (!results.TryGetValue(registration2.Comment, out registration))
                {
                    registration = registration2;
                    results.Add(registration2.Comment, registration2);
                    registration.addressList = new List<IPEndPoint>();
                }
                registration.addressList.AddRange(registration2.Addresses);
                registration2.Addresses = null;
            }
        }

        private void MergeResults(List<PeerNodeAddress> nodeAddressList, List<PnrpRegistration> globalRegistrations, List<PnrpRegistration> linkRegistrations, List<PnrpRegistration> siteRegistrations)
        {
            Dictionary<string, PnrpRegistration> results = new Dictionary<string, PnrpRegistration>();
            this.MergeResults(results, globalRegistrations);
            this.MergeResults(results, siteRegistrations);
            this.MergeResults(results, linkRegistrations);
            foreach (PnrpRegistration registration in results.Values)
            {
                registration.Addresses = registration.addressList.ToArray();
                PeerNodeAddress item = this.PeerNodeAddressFromPnrpRegistration(registration);
                if (item != null)
                {
                    nodeAddressList.Add(item);
                }
            }
        }

        private static string NameFromProtocol(byte number)
        {
            if (number != 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerPnrpIllegalUri")));
            }
            return Uri.UriSchemeNetTcp;
        }

        private void ParseServiceUri(Uri uri, out string scheme, out Guid[] result)
        {
            if (((uri != null) && (ProtocolFromName(uri.Scheme) != 0)) && !string.IsNullOrEmpty(uri.AbsolutePath))
            {
                scheme = uri.Scheme;
                string[] strArray = uri.AbsolutePath.Trim(new char[] { ' ', '/' }).Split(new char[] { '/' });
                if (((string.Compare(strArray[0], "PeerChannelEndpoints", StringComparison.OrdinalIgnoreCase) == 0) && (strArray.Length >= 1)) && (strArray.Length <= 3))
                {
                    result = new Guid[strArray.Length - 1];
                    try
                    {
                        for (int i = 1; i < strArray.Length; i++)
                        {
                            result[i - 1] = Fx.CreateGuid(strArray[i]);
                        }
                        return;
                    }
                    catch (FormatException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerPnrpIllegalUri"), exception));
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerPnrpIllegalUri")));
        }

        private PeerNodeAddress PeerNodeAddressFromPnrpRegistration(PnrpRegistration input)
        {
            Action<IPEndPoint> action = null;
            Action<Guid> action2 = null;
            List<IPAddress> addresses = new List<IPAddress>();
            PeerNodeAddress address = null;
            StringBuilder pathBuilder = new StringBuilder(200);
            int version = 0;
            try
            {
                Guid[] guidArray;
                string str;
                if ((input == null) || string.IsNullOrEmpty(input.Comment))
                {
                    return null;
                }
                if (action == null)
                {
                    action = delegate (IPEndPoint obj) {
                        addresses.Add(obj.Address);
                    };
                }
                Array.ForEach<IPEndPoint>(input.Addresses, action);
                if (addresses.Count == 0)
                {
                    return address;
                }
                UriBuilder builder = new UriBuilder {
                    Port = input.Addresses[0].Port,
                    Host = addresses[0].ToString()
                };
                pathBuilder.Append("PeerChannelEndpoints");
                CharEncoder.Decode(input.Comment, out version, out str, out guidArray);
                if (((version != 1) || (guidArray == null)) || ((guidArray.Length > 2) || (guidArray.Length < 1)))
                {
                    return address;
                }
                builder.Scheme = str;
                if (action2 == null)
                {
                    action2 = delegate (Guid guid) {
                        pathBuilder.Append('/' + string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { guid.ToString() }));
                    };
                }
                Array.ForEach<Guid>(guidArray, action2);
                builder.Path = string.Format(CultureInfo.InvariantCulture, "{0}", new object[] { pathBuilder.ToString() });
                address = new PeerNodeAddress(new EndpointAddress(builder.Uri, new AddressHeader[0]), new ReadOnlyCollection<IPAddress>(addresses));
            }
            catch (ArgumentException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
            }
            catch (FormatException exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
            }
            catch (IndexOutOfRangeException exception3)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
            }
            return address;
        }

        private void PeerNodeAddressToPnrpRegistrations(string meshName, Dictionary<uint, string> LinkCloudNames, Dictionary<uint, string> SiteCloudNames, PeerNodeAddress input, out PnrpRegistration[] linkRegs, out PnrpRegistration[] siteRegs, out PnrpRegistration global)
        {
            string str;
            Guid[] guidArray;
            new PnrpRegistration();
            Dictionary<uint, PnrpRegistration> dictionary = new Dictionary<uint, PnrpRegistration>();
            Dictionary<uint, PnrpRegistration> dictionary2 = new Dictionary<uint, PnrpRegistration>();
            PnrpRegistration registration = null;
            this.ParseServiceUri(input.EndpointAddress.Uri, out str, out guidArray);
            int port = input.EndpointAddress.Uri.Port;
            if (port <= 0)
            {
                port = 0x328;
            }
            string peerName = string.Format(CultureInfo.InvariantCulture, "0.{0}", new object[] { meshName });
            string comment = CharEncoder.Encode(1, str, guidArray);
            global = null;
            string str4 = string.Empty;
            foreach (IPAddress address in input.IPAddresses)
            {
                if ((address.AddressFamily == AddressFamily.InterNetworkV6) && (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal))
                {
                    if (address.IsIPv6LinkLocal)
                    {
                        if (!dictionary.TryGetValue((uint) address.ScopeId, out registration))
                        {
                            if (!LinkCloudNames.TryGetValue((uint) address.ScopeId, out str4))
                            {
                                continue;
                            }
                            registration = PnrpRegistration.Create(peerName, comment, str4);
                            dictionary.Add((uint) address.ScopeId, registration);
                        }
                    }
                    else if (!dictionary2.TryGetValue((uint) address.ScopeId, out registration))
                    {
                        if (!SiteCloudNames.TryGetValue((uint) address.ScopeId, out str4))
                        {
                            continue;
                        }
                        registration = PnrpRegistration.Create(peerName, comment, str4);
                        dictionary2.Add((uint) address.ScopeId, registration);
                    }
                    registration.addressList.Add(new IPEndPoint(address, port));
                }
                else
                {
                    if (global == null)
                    {
                        global = PnrpRegistration.Create(peerName, comment, "Global_");
                    }
                    global.addressList.Add(new IPEndPoint(address, port));
                }
            }
            if (global != null)
            {
                if (global.addressList != null)
                {
                    this.TrimToMaxAddresses(global.addressList);
                    global.Addresses = global.addressList.ToArray();
                }
                else
                {
                    global.Addresses = new IPEndPoint[0];
                }
            }
            if (dictionary.Count != 0)
            {
                foreach (PnrpRegistration registration2 in dictionary.Values)
                {
                    if (registration2.addressList != null)
                    {
                        this.TrimToMaxAddresses(registration2.addressList);
                        registration2.Addresses = registration2.addressList.ToArray();
                    }
                    else
                    {
                        registration2.Addresses = new IPEndPoint[0];
                    }
                }
                linkRegs = new PnrpRegistration[dictionary.Count];
                dictionary.Values.CopyTo(linkRegs, 0);
            }
            else
            {
                linkRegs = new PnrpRegistration[0];
            }
            if (dictionary2.Count != 0)
            {
                foreach (PnrpRegistration registration3 in dictionary2.Values)
                {
                    if (registration3.addressList != null)
                    {
                        this.TrimToMaxAddresses(registration3.addressList);
                        registration3.Addresses = registration3.addressList.ToArray();
                    }
                    else
                    {
                        registration3.Addresses = new IPEndPoint[0];
                    }
                }
                siteRegs = new PnrpRegistration[dictionary2.Count];
                dictionary2.Values.CopyTo(siteRegs, 0);
            }
            else
            {
                siteRegs = new PnrpRegistration[0];
            }
        }

        private static int ProtocolFromName(string name)
        {
            if (name != Uri.UriSchemeNetTcp)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name", System.ServiceModel.SR.GetString("PeerPnrpIllegalUri"));
            }
            return 1;
        }

        public override object Register(string meshId, PeerNodeAddress nodeAddress, TimeSpan timeout)
        {
            this.ThrowIfNoPnrp();
            PnrpRegistration global = null;
            PnrpRegistration[] linkRegs = null;
            PnrpRegistration[] siteRegs = null;
            RegistrationHandle registrationId = new RegistrationHandle(meshId);
            Dictionary<uint, string> siteCloudNames = new Dictionary<uint, string>();
            Dictionary<uint, string> linkCloudNames = new Dictionary<uint, string>();
            PnrpResolveScope scope = this.EnumerateClouds(false, linkCloudNames, siteCloudNames);
            if (scope == PnrpResolveScope.None)
            {
                PeerExceptionHelper.ThrowInvalidOperation_PnrpNoClouds();
            }
            if (this.localExtension != null)
            {
                meshId = meshId + this.localExtension;
            }
            try
            {
                this.PeerNodeAddressToPnrpRegistrations(meshId, linkCloudNames, siteCloudNames, nodeAddress, out linkRegs, out siteRegs, out global);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerPnrpIllegalUri"), exception));
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            try
            {
                PnrpResolveScope none = PnrpResolveScope.None;
                if (((global != null) && (global.Addresses.Length > 0)) && ((scope & PnrpResolveScope.Global) != PnrpResolveScope.None))
                {
                    this.registrar.Register(global, helper.RemainingTime());
                    registrationId.AddCloud(global.CloudName);
                    none |= PnrpResolveScope.Global;
                }
                if (linkRegs.Length > 0)
                {
                    foreach (PnrpRegistration registration2 in linkRegs)
                    {
                        if (registration2.Addresses.Length > 0)
                        {
                            this.registrar.Register(registration2, helper.RemainingTime());
                            registrationId.AddCloud(registration2.CloudName);
                        }
                    }
                    none |= PnrpResolveScope.LinkLocal;
                }
                if (siteRegs.Length > 0)
                {
                    foreach (PnrpRegistration registration3 in siteRegs)
                    {
                        if (registration3.Addresses.Length > 0)
                        {
                            this.registrar.Register(registration3, helper.RemainingTime());
                            registrationId.AddCloud(registration3.CloudName);
                        }
                    }
                    none |= PnrpResolveScope.SiteLocal;
                }
                if (none == PnrpResolveScope.None)
                {
                    PeerExceptionHelper.ThrowInvalidOperation_PnrpAddressesUnsupported();
                }
            }
            catch (SocketException)
            {
                try
                {
                    this.Unregister(registrationId, helper.RemainingTime());
                }
                catch (SocketException exception2)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
                throw;
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PnrpRegisterTraceRecord extendedData = new PnrpRegisterTraceRecord(meshId, global, siteRegs, linkRegs);
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40048, System.ServiceModel.SR.GetString("TraceCodePnrpRegisteredAddresses"), extendedData, this, null);
            }
            return registrationId;
        }

        public override ReadOnlyCollection<PeerNodeAddress> Resolve(string meshId, int maxAddresses, TimeSpan timeout)
        {
            UnsafePnrpNativeMethods.PeerNameResolver resolver;
            this.ThrowIfNoPnrp();
            List<UnsafePnrpNativeMethods.PeerNameResolver> list = new List<UnsafePnrpNativeMethods.PeerNameResolver>();
            List<PnrpRegistration> results = new List<PnrpRegistration>();
            List<PnrpRegistration> list3 = new List<PnrpRegistration>();
            List<PnrpRegistration> list4 = new List<PnrpRegistration>();
            List<WaitHandle> list5 = new List<WaitHandle>();
            Dictionary<uint, string> siteCloudNames = new Dictionary<uint, string>();
            Dictionary<uint, string> linkCloudNames = new Dictionary<uint, string>();
            UnsafePnrpNativeMethods.PnrpResolveCriteria nearestNonCurrentProcess = UnsafePnrpNativeMethods.PnrpResolveCriteria.NearestNonCurrentProcess;
            TimeoutHelper helper = new TimeoutHelper((TimeSpan.Compare(timeout, MaxResolveTimeout) <= 0) ? timeout : MaxResolveTimeout);
            if (!HasPeerNodeForMesh(meshId))
            {
                nearestNonCurrentProcess = UnsafePnrpNativeMethods.PnrpResolveCriteria.Any;
            }
            PnrpResolveScope scope = this.EnumerateClouds(true, linkCloudNames, siteCloudNames);
            if (this.remoteExtension != null)
            {
                meshId = meshId + this.remoteExtension;
            }
            string peerName = string.Format(CultureInfo.InvariantCulture, "0.{0}", new object[] { meshId });
            if ((scope & PnrpResolveScope.Global) != PnrpResolveScope.None)
            {
                resolver = new UnsafePnrpNativeMethods.PeerNameResolver(peerName, maxAddresses, nearestNonCurrentProcess, 0, "Global_", helper.RemainingTime(), results);
                list5.Add(resolver.AsyncWaitHandle);
                list.Add(resolver);
            }
            if ((scope & PnrpResolveScope.LinkLocal) != PnrpResolveScope.None)
            {
                foreach (KeyValuePair<uint, string> pair in linkCloudNames)
                {
                    resolver = new UnsafePnrpNativeMethods.PeerNameResolver(peerName, maxAddresses, nearestNonCurrentProcess, pair.Key, pair.Value, helper.RemainingTime(), list3);
                    list5.Add(resolver.AsyncWaitHandle);
                    list.Add(resolver);
                }
            }
            if ((scope & PnrpResolveScope.SiteLocal) != PnrpResolveScope.None)
            {
                foreach (KeyValuePair<uint, string> pair2 in siteCloudNames)
                {
                    resolver = new UnsafePnrpNativeMethods.PeerNameResolver(peerName, maxAddresses, nearestNonCurrentProcess, pair2.Key, pair2.Value, helper.RemainingTime(), list4);
                    list5.Add(resolver.AsyncWaitHandle);
                    list.Add(resolver);
                }
            }
            if (list5.Count == 0)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("PnrpNoClouds"));
                    PnrpResolveExceptionTraceRecord extendedData = new PnrpResolveExceptionTraceRecord(meshId, string.Empty, exception);
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x4004a, System.ServiceModel.SR.GetString("TraceCodePnrpResolvedAddresses"), extendedData, this, null);
                }
                return new ReadOnlyCollection<PeerNodeAddress>(new List<PeerNodeAddress>());
            }
            Exception exception2 = null;
            foreach (UnsafePnrpNativeMethods.PeerNameResolver resolver2 in list)
            {
                try
                {
                    resolver2.End();
                }
                catch (SocketException exception3)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    exception2 = exception3;
                }
            }
            List<PeerNodeAddress> nodeAddressList = new List<PeerNodeAddress>();
            this.MergeResults(nodeAddressList, results, list3, list4);
            if ((exception2 != null) && (nodeAddressList.Count == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception2);
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PnrpPeerResolverTraceRecord record2 = new PnrpPeerResolverTraceRecord(meshId, nodeAddressList);
                TraceUtility.TraceEvent(TraceEventType.Information, 0x4004a, System.ServiceModel.SR.GetString("TraceCodePnrpResolvedAddresses"), record2, this, null);
            }
            return new ReadOnlyCollection<PeerNodeAddress>(nodeAddressList);
        }

        internal void SetMeshExtensions(string local, string remote)
        {
            this.localExtension = local;
            this.remoteExtension = remote;
        }

        private void ThrowIfNoPnrp()
        {
            if (!isPnrpAvailable)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PeerPnrpNotAvailable")));
            }
        }

        private void TrimToMaxAddresses(List<IPEndPoint> addressList)
        {
            if (addressList.Count > 10)
            {
                addressList.RemoveRange(10, addressList.Count - 10);
            }
        }

        public override void Unregister(object registrationId, TimeSpan timeout)
        {
            RegistrationHandle handle = registrationId as RegistrationHandle;
            if ((handle == null) || string.IsNullOrEmpty(handle.PeerName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerInvalidRegistrationId", new object[] { handle }), "registrationId"));
            }
            string peerName = handle.PeerName;
            string str2 = string.Format(CultureInfo.InvariantCulture, "0.{0}", new object[] { peerName });
            this.registrar.Unregister(str2, handle.Clouds, timeout);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PnrpPeerResolverTraceRecord extendedData = new PnrpPeerResolverTraceRecord(peerName, new List<PeerNodeAddress>());
                TraceUtility.TraceEvent(TraceEventType.Information, 0x40049, System.ServiceModel.SR.GetString("TraceCodePnrpUnregisteredAddresses"), extendedData, this, null);
            }
        }

        public override void Update(object registrationId, PeerNodeAddress updatedNodeAddress, TimeSpan timeout)
        {
            RegistrationHandle handle = registrationId as RegistrationHandle;
            if ((handle == null) || string.IsNullOrEmpty(handle.PeerName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerInvalidRegistrationId", new object[] { handle }), "registrationId"));
            }
            string peerName = handle.PeerName;
            this.Register(peerName, updatedNodeAddress, timeout);
        }

        public override bool CanShareReferrals
        {
            get
            {
                return (this.referralPolicy != PeerReferralPolicy.DoNotShare);
            }
        }

        public static bool IsPnrpAvailable
        {
            get
            {
                return isPnrpAvailable;
            }
        }

        public static bool IsPnrpInstalled
        {
            get
            {
                return isPnrpInstalled;
            }
        }

        private static Encoding PnrpEncoder
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        internal class CharEncoder
        {
            private static void CheckAtLimit(int current)
            {
                if ((current + 1) >= 80)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerPnrpIllegalUri")));
                }
            }

            internal static void Decode(string buffer, out int version, out string protocolName, out Guid[] guids)
            {
                char[] chars = buffer.ToCharArray();
                int offset = 0;
                version = DecodeByte(ref offset, chars);
                byte number = DecodeByte(ref offset, chars);
                protocolName = PnrpPeerResolver.NameFromProtocol(number);
                int num3 = DecodeByte(ref offset, chars);
                guids = new Guid[num3];
                for (int i = 0; i < num3; i++)
                {
                    byte[] b = new byte[0x10];
                    for (int j = 0; j < 0x10; j++)
                    {
                        b[j] = DecodeByte(ref offset, chars);
                    }
                    guids[i] = new Guid(b);
                }
            }

            private static byte DecodeByte(ref int offset, char[] chars)
            {
                byte @byte = GetByte(offset++, chars);
                if (@byte == 0xff)
                {
                    @byte = GetByte(offset++, chars);
                }
                return @byte;
            }

            internal static string Encode(int version, string protocolName, Guid[] guids)
            {
                byte[] bytes = new byte[80];
                int offset = 0;
                int num2 = PnrpPeerResolver.ProtocolFromName(protocolName);
                EncodeByte(Convert.ToByte(version), ref offset, bytes);
                EncodeByte(Convert.ToByte(num2), ref offset, bytes);
                EncodeByte(Convert.ToByte(guids.Length), ref offset, bytes);
                foreach (Guid guid in guids)
                {
                    foreach (byte num3 in guid.ToByteArray())
                    {
                        EncodeByte(Convert.ToByte(num3), ref offset, bytes);
                    }
                }
                if (((offset % 2) != 0) && (offset < bytes.Length))
                {
                    bytes[offset] = 0xff;
                }
                int num4 = offset;
                int num5 = (num4 / 2) + (num4 % 2);
                char[] chArray = new char[num5];
                offset = 0;
                for (int i = 0; i < num5; i++)
                {
                    chArray[i] = Convert.ToChar((int) ((bytes[offset++] * 0x100) + bytes[offset++]));
                }
                return new string(chArray);
            }

            private static void EncodeByte(byte b, ref int offset, byte[] bytes)
            {
                if ((b == 0) || (b == 0xff))
                {
                    CheckAtLimit(offset);
                    bytes[offset++] = 0xff;
                }
                CheckAtLimit(offset);
                bytes[offset++] = b;
            }

            private static byte GetByte(int offset, char[] chars)
            {
                int index = offset / 2;
                int num2 = offset % 2;
                return Convert.ToByte((num2 == 1) ? ((int) (chars[index] & '\x00ff')) : ((int) (chars[index] / 'Ā')));
            }
        }

        internal enum PnrpErrorCodes
        {
            WSA_PNRP_CLOUD_DISABLED = 0x2cee,
            WSA_PNRP_CLOUD_IS_RESOLVE_ONLY = 0x2cf1,
            WSA_PNRP_CLOUD_NOT_FOUND = 0x2ced,
            WSA_PNRP_DUPLICATE_PEER_NAME = 0x2cf4,
            WSA_PNRP_ERROR_BASE = 0x2cec,
            WSA_PNRP_FW_PORT_BLOCKED = 0x2cf3
        }

        internal class PnrpException : SocketException
        {
            private string message;

            internal PnrpException(int errorCode, string cloud) : base(errorCode)
            {
                this.LoadMessage(errorCode, cloud);
            }

            private void LoadMessage(int errorCode, string cloud)
            {
                string str;
                switch (((PnrpPeerResolver.PnrpErrorCodes) errorCode))
                {
                    case PnrpPeerResolver.PnrpErrorCodes.WSA_PNRP_CLOUD_NOT_FOUND:
                        str = "PnrpCloudNotFound";
                        break;

                    case PnrpPeerResolver.PnrpErrorCodes.WSA_PNRP_CLOUD_DISABLED:
                        str = "PnrpCloudDisabled";
                        break;

                    case PnrpPeerResolver.PnrpErrorCodes.WSA_PNRP_CLOUD_IS_RESOLVE_ONLY:
                        str = "PnrpCloudResolveOnly";
                        break;

                    case PnrpPeerResolver.PnrpErrorCodes.WSA_PNRP_FW_PORT_BLOCKED:
                        str = "PnrpPortBlocked";
                        break;

                    case PnrpPeerResolver.PnrpErrorCodes.WSA_PNRP_DUPLICATE_PEER_NAME:
                        str = "PnrpDuplicatePeerName";
                        break;

                    default:
                        str = null;
                        break;
                }
                if (str != null)
                {
                    this.message = System.ServiceModel.SR.GetString(str, new object[] { cloud });
                }
            }

            public override string Message
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.message))
                    {
                        return this.message;
                    }
                    return base.Message;
                }
            }
        }

        internal class PnrpRegistration
        {
            public IPEndPoint[] Addresses;
            public List<IPEndPoint> addressList;
            public string CloudName;
            public string Comment;
            public string PeerName;

            internal static PnrpPeerResolver.PnrpRegistration Create(string peerName, string comment, string cloudName)
            {
                return new PnrpPeerResolver.PnrpRegistration { Comment = comment, CloudName = cloudName, PeerName = peerName, addressList = new List<IPEndPoint>() };
            }
        }

        [Flags]
        internal enum PnrpResolveScope
        {
            All = 7,
            Global = 1,
            LinkLocal = 4,
            None = 0,
            SiteLocal = 2
        }

        private class RegistrationHandle
        {
            public List<string> Clouds;
            public string PeerName;

            public RegistrationHandle(string peerName)
            {
                this.PeerName = peerName;
                this.Clouds = new List<string>();
            }

            public void AddCloud(string name)
            {
                this.Clouds.Add(name);
            }
        }

        internal static class UnsafePnrpNativeMethods
        {
            private const int MaxAddresses = 10;
            private const int MaxAddressesV1 = 4;
            private static Guid NsProviderCloud = new Guid(0x3fe89ce, 0x766d, 0x4976, 0xb9, 0xc1, 0xbb, 0x9b, 0xc4, 0x2c, 0x7b, 0x4d);
            private static Guid NsProviderName = new Guid(0x3fe89cd, 0x766d, 0x4976, 0xb9, 0xc1, 0xbb, 0x9b, 0xc4, 0x2c, 0x7b, 0x4d);
            private const short RequiredWinsockVersion = 0x202;
            private static Guid SvcIdCloud = new Guid(0xc2239ce6, 0xc0, 0x4fbf, 0xba, 0xd6, 0x18, 0x13, 0x93, 0x85, 0xa4, 0x9a);
            private static Guid SvcIdName = new Guid(0xc2239ce7, 0xc0, 0x4fbf, 0xba, 0xd6, 0x18, 0x13, 0x93, 0x85, 0xa4, 0x9a);
            private static Guid SvcIdNameV1 = new Guid(0xc2239ce5, 0xc0, 0x4fbf, 0xba, 0xd6, 0x18, 0x13, 0x93, 0x85, 0xa4, 0x9a);

            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi)]
            private static extern int WSACleanup();
            [DllImport("ws2_32.dll", CharSet=CharSet.Unicode)]
            private static extern int WSAEnumNameSpaceProviders(ref int lpdwBufferLength, IntPtr lpnspBuffer);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi)]
            private static extern int WSAGetLastError();
            [DllImport("ws2_32.dll", CharSet=CharSet.Unicode)]
            private static extern int WSALookupServiceBegin(CriticalAllocHandle query, WsaNspControlFlags dwControlFlags, out CriticalLookupHandle hLookup);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ws2_32.dll", CharSet=CharSet.Unicode)]
            private static extern int WSALookupServiceEnd(IntPtr hLookup);
            [DllImport("ws2_32.dll", CharSet=CharSet.Unicode)]
            private static extern int WSALookupServiceNext(CriticalLookupHandle hLookup, WsaNspControlFlags dwControlFlags, ref int lpdwBufferLength, IntPtr Results);
            [DllImport("ws2_32.dll", CharSet=CharSet.Unicode)]
            private static extern int WSASetService(CriticalAllocHandle querySet, WsaSetServiceOp essOperation, int dwControlFlags);
            [DllImport("ws2_32.dll", CharSet=CharSet.Ansi)]
            private static extern int WSAStartup(short wVersionRequested, ref WsaData lpWSAData);

            [StructLayout(LayoutKind.Sequential)]
            internal struct BlobNative
            {
                public int cbSize;
                public IntPtr pBlobData;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct BlobSafe
            {
                public int cbSize;
                public CriticalAllocHandle pBlobData;
            }

            internal class CloudInfo
            {
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudFlags Flags;
                public string Name;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpScope Scope;
                public uint ScopeId;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudState State;
            }

            internal class CriticalAllocHandlePnrpBlob : CriticalAllocHandle
            {
                public static CriticalAllocHandle FromPnrpBlob(object input)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.BlobSafe safe = new PnrpPeerResolver.UnsafePnrpNativeMethods.BlobSafe();
                    if (input != null)
                    {
                        if (input.GetType() != typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo))
                        {
                            PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative native3;
                            if (input.GetType() != typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo))
                            {
                                throw Fx.AssertAndThrow("Unknown payload type!");
                            }
                            int num2 = Marshal.SizeOf(input.GetType());
                            safe.pBlobData = CriticalAllocHandle.FromSize(num2 + Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative)));
                            native3.cbSize = num2;
                            native3.pBlobData = (IntPtr) (safe.pBlobData.ToInt64() + Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative)));
                            Marshal.StructureToPtr(native3, (IntPtr) safe.pBlobData, false);
                            PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo structure = (PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo) input;
                            structure.dwSize = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo));
                            Marshal.StructureToPtr(structure, native3.pBlobData, false);
                            safe.cbSize = num2;
                        }
                        else
                        {
                            PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative native;
                            int num = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfoNative));
                            safe.pBlobData = CriticalAllocHandle.FromSize(num + Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative)));
                            native.cbSize = num;
                            native.pBlobData = (IntPtr) (safe.pBlobData.ToInt64() + Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative)));
                            Marshal.StructureToPtr(native, (IntPtr) safe.pBlobData, false);
                            PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo source = (PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo) input;
                            source.dwSize = num;
                            PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfoNative target = new PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfoNative();
                            PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo.ToPnrpInfoNative(source, ref target);
                            Marshal.StructureToPtr(target, native.pBlobData, false);
                            safe.cbSize = num;
                        }
                    }
                    return safe.pBlobData;
                }
            }

            internal class CriticalAllocHandleString : CriticalAllocHandle
            {
                public static CriticalAllocHandle FromString(string input)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleString str = new PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleString();
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        str.SetHandle(Marshal.StringToHGlobalUni(input));
                    }
                    return str;
                }
            }

            internal class CriticalAllocHandleWsaQuerySetSafe : CriticalAllocHandle
            {
                private static int CalculateSize(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safeQuerySet)
                {
                    int num = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative));
                    if (safeQuerySet.addressList != null)
                    {
                        num += safeQuerySet.addressList.Length * Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoNative));
                    }
                    return num;
                }

                public static CriticalAllocHandle FromWsaQuerySetSafe(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safeQuerySet)
                {
                    CriticalAllocHandle handle = CriticalAllocHandle.FromSize(CalculateSize(safeQuerySet));
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe.StructureToPtr(safeQuerySet, (IntPtr) handle);
                    return handle;
                }
            }

            internal class CriticalLookupHandle : CriticalHandleZeroOrMinusOneIsInvalid
            {
                protected override bool ReleaseHandle()
                {
                    return (PnrpPeerResolver.UnsafePnrpNativeMethods.WSALookupServiceEnd(base.handle) == 0);
                }
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            internal struct CsAddrInfo
            {
                public IPEndPoint LocalAddr;
                public IPEndPoint RemoteAddr;
                public int iSocketType;
                public int iProtocol;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct CsAddrInfoNative
            {
                public PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_NATIVE LocalAddr;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_NATIVE RemoteAddr;
                public int iSocketType;
                public int iProtocol;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal class CsAddrInfoSafe : IDisposable
            {
                public PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE LocalAddr;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE RemoteAddr;
                public int iSocketType;
                public int iProtocol;
                private bool disposed;
                public static PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe[] FromAddresses(PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfo[] addresses)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe[] safeArray = null;
                    if ((addresses == null) || (addresses.Length == 0))
                    {
                        return null;
                    }
                    safeArray = new PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe[addresses.Length];
                    int num = 0;
                    foreach (PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfo info in addresses)
                    {
                        PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe safe = new PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe {
                            LocalAddr = PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(info.LocalAddr),
                            RemoteAddr = PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(info.RemoteAddr),
                            iProtocol = info.iProtocol,
                            iSocketType = info.iSocketType
                        };
                        safeArray[num++] = safe;
                    }
                    return safeArray;
                }

                public static void StructureToPtr(PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe input, IntPtr target)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoNative native;
                    native.iProtocol = input.iProtocol;
                    native.iSocketType = input.iSocketType;
                    native.LocalAddr.iSockaddrLength = input.LocalAddr.iSockaddrLength;
                    native.LocalAddr.lpSockAddr = (IntPtr) input.LocalAddr.lpSockAddr;
                    native.RemoteAddr.iSockaddrLength = input.RemoteAddr.iSockaddrLength;
                    native.RemoteAddr.lpSockAddr = (IntPtr) input.RemoteAddr.lpSockAddr;
                    Marshal.StructureToPtr(native, target, false);
                }

                ~CsAddrInfoSafe()
                {
                    this.Dispose(false);
                }

                public virtual void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }

                private void Dispose(bool disposing)
                {
                    if (this.disposed && disposing)
                    {
                        this.LocalAddr.Dispose();
                        this.RemoteAddr.Dispose();
                    }
                    this.disposed = true;
                }
            }

            internal class DiscoveryBase : MarshalByRefObject, IDisposable
            {
                private bool disposed;
                private static int refCount = 0;
                private static object refCountLock = new object();

                public DiscoveryBase()
                {
                    lock (refCountLock)
                    {
                        if (refCount == 0)
                        {
                            PnrpPeerResolver.UnsafePnrpNativeMethods.WsaData lpWSAData = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaData();
                            int errorCode = PnrpPeerResolver.UnsafePnrpNativeMethods.WSAStartup(0x202, ref lpWSAData);
                            if (errorCode != 0)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException(errorCode));
                            }
                        }
                        refCount++;
                    }
                }

                public void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }

                public void Dispose(bool disposing)
                {
                    if (!this.disposed)
                    {
                        lock (refCountLock)
                        {
                            refCount--;
                            if (refCount == 0)
                            {
                                PnrpPeerResolver.UnsafePnrpNativeMethods.WSACleanup();
                            }
                        }
                    }
                    this.disposed = true;
                }

                ~DiscoveryBase()
                {
                    this.Dispose(false);
                }

                private int InvokeService(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet registerQuery, PnrpPeerResolver.UnsafePnrpNativeMethods.WsaSetServiceOp op, int flags)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safeQuerySet = PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet.ToWsaQuerySetSafe(registerQuery);
                    int num = 0;
                    using (safeQuerySet)
                    {
                        CriticalAllocHandle querySet = PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(safeQuerySet);
                        using (querySet)
                        {
                            if (PnrpPeerResolver.UnsafePnrpNativeMethods.WSASetService(querySet, op, flags) != 0)
                            {
                                num = PnrpPeerResolver.UnsafePnrpNativeMethods.WSAGetLastError();
                            }
                        }
                    }
                    return num;
                }

                public bool IsPnrpAvailable(TimeSpan waitForService)
                {
                    if (!this.IsPnrpInstalled())
                    {
                        return false;
                    }
                    if (!this.IsPnrpServiceRunning(waitForService))
                    {
                        return false;
                    }
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet registerQuery = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet {
                        NSProviderId = PnrpPeerResolver.UnsafePnrpNativeMethods.NsProviderName,
                        ServiceClassId = PnrpPeerResolver.UnsafePnrpNativeMethods.SvcIdNameV1
                    };
                    int num = this.InvokeService(registerQuery, PnrpPeerResolver.UnsafePnrpNativeMethods.WsaSetServiceOp.Register, 0);
                    if ((num != 0x2726) && (num != 0x2afc))
                    {
                        return false;
                    }
                    return true;
                }

                public bool IsPnrpInstalled()
                {
                    int num2;
                    int lpdwBufferLength = 0;
                    CriticalAllocHandle handle = null;
                    while (true)
                    {
                        num2 = PnrpPeerResolver.UnsafePnrpNativeMethods.WSAEnumNameSpaceProviders(ref lpdwBufferLength, (IntPtr) handle);
                        if (num2 != -1)
                        {
                            break;
                        }
                        if (PnrpPeerResolver.UnsafePnrpNativeMethods.WSAGetLastError() != 0x271e)
                        {
                            return false;
                        }
                        handle = CriticalAllocHandle.FromSize(lpdwBufferLength);
                    }
                    for (int i = 0; i < num2; i++)
                    {
                        IntPtr ptr2 = (IntPtr) handle;
                        IntPtr ptr = (IntPtr) (ptr2.ToInt64() + (i * Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaNamespaceInfo))));
                        PnrpPeerResolver.UnsafePnrpNativeMethods.WsaNamespaceInfo info = (PnrpPeerResolver.UnsafePnrpNativeMethods.WsaNamespaceInfo) Marshal.PtrToStructure(ptr, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaNamespaceInfo));
                        if ((info.NSProviderId == PnrpPeerResolver.UnsafePnrpNativeMethods.NsProviderName) && (info.fActive != 0))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                public bool IsPnrpServiceRunning(TimeSpan waitForService)
                {
                    bool flag;
                    TimeoutHelper helper = new TimeoutHelper(waitForService);
                    try
                    {
                        using (ServiceController controller = new ServiceController("pnrpsvc"))
                        {
                            try
                            {
                                if (controller.Status == ServiceControllerStatus.StopPending)
                                {
                                    controller.WaitForStatus(ServiceControllerStatus.Stopped, helper.RemainingTime());
                                }
                                if (controller.Status == ServiceControllerStatus.Stopped)
                                {
                                    controller.Start();
                                }
                                controller.WaitForStatus(ServiceControllerStatus.Running, helper.RemainingTime());
                            }
                            catch (Exception exception)
                            {
                                if (Fx.IsFatal(exception))
                                {
                                    throw;
                                }
                                if (!(exception is InvalidOperationException) && !(exception is System.ServiceProcess.TimeoutException))
                                {
                                    throw;
                                }
                                return false;
                            }
                            flag = controller.Status == ServiceControllerStatus.Running;
                        }
                    }
                    catch (InvalidOperationException exception2)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        flag = false;
                    }
                    return flag;
                }
            }

            [Serializable]
            internal enum NspNamespaces
            {
                Cloud = 0x27,
                Name = 0x26
            }

            public class PeerCloudEnumerator : PnrpPeerResolver.UnsafePnrpNativeMethods.DiscoveryBase
            {
                public static PnrpPeerResolver.UnsafePnrpNativeMethods.CloudInfo[] GetClouds()
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalLookupHandle handle;
                    int num = 0;
                    ArrayList list = new ArrayList();
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet input = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet();
                    PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo info = new PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo {
                        dwSize = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo))
                    };
                    info.Cloud.Scope = PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpScope.Any;
                    info.dwCloudState = PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudState.Virtual;
                    info.Flags = PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudFlags.None;
                    input.NameSpace = PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces.Cloud;
                    input.NSProviderId = PnrpPeerResolver.UnsafePnrpNativeMethods.NsProviderCloud;
                    input.ServiceClassId = PnrpPeerResolver.UnsafePnrpNativeMethods.SvcIdCloud;
                    input.Blob = info;
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safeQuerySet = PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet.ToWsaQuerySetSafe(input);
                    using (safeQuerySet)
                    {
                        num = PnrpPeerResolver.UnsafePnrpNativeMethods.WSALookupServiceBegin(PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(safeQuerySet), PnrpPeerResolver.UnsafePnrpNativeMethods.WsaNspControlFlags.ReturnAll, out handle);
                    }
                    if (num != 0)
                    {
                        SocketException exception = new SocketException(PnrpPeerResolver.UnsafePnrpNativeMethods.WSAGetLastError());
                        Utility.CloseInvalidOutCriticalHandle(handle);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                    }
                    int size = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe)) + 200;
                    CriticalAllocHandle handle3 = CriticalAllocHandle.FromSize(size);
                    using (handle)
                    {
                        while (true)
                        {
                            while (PnrpPeerResolver.UnsafePnrpNativeMethods.WSALookupServiceNext(handle, 0, ref size, (IntPtr) handle3) != 0)
                            {
                                int errorCode = PnrpPeerResolver.UnsafePnrpNativeMethods.WSAGetLastError();
                                switch (errorCode)
                                {
                                    case 0x2776:
                                    case 0x277e:
                                        goto Label_01F5;
                                }
                                if (errorCode != 0x271e)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException(errorCode));
                                }
                                if (handle3 != null)
                                {
                                    handle3.Dispose();
                                    handle3 = null;
                                }
                                handle3 = CriticalAllocHandle.FromSize(size);
                            }
                            if (handle3 != IntPtr.Zero)
                            {
                                PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet set2 = PnrpPeerResolver.UnsafePnrpNativeMethods.PeerNameResolver.MarshalWsaQuerySetNativeToWsaQuerySet((IntPtr) handle3, 0);
                                PnrpPeerResolver.UnsafePnrpNativeMethods.CloudInfo info2 = new PnrpPeerResolver.UnsafePnrpNativeMethods.CloudInfo();
                                PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo blob = (PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo) set2.Blob;
                                info2.Name = set2.ServiceInstanceName;
                                info2.Scope = blob.Cloud.Scope;
                                info2.ScopeId = blob.Cloud.ScopeId;
                                info2.State = blob.dwCloudState;
                                info2.Flags = blob.Flags;
                                list.Add(info2);
                            }
                        }
                    }
                Label_01F5:
                    return (PnrpPeerResolver.UnsafePnrpNativeMethods.CloudInfo[]) list.ToArray(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.CloudInfo));
                }
            }

            internal class PeerNameRegistrar : PnrpPeerResolver.UnsafePnrpNativeMethods.DiscoveryBase
            {
                private const int RegistrationLifetime = 0xe10;

                private void DeleteService(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet registerQuery)
                {
                    InvokeService(registerQuery, PnrpPeerResolver.UnsafePnrpNativeMethods.WsaSetServiceOp.Delete, 0);
                }

                private static void InvokeService(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet registerQuery, PnrpPeerResolver.UnsafePnrpNativeMethods.WsaSetServiceOp op, int flags)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safeQuerySet = PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet.ToWsaQuerySetSafe(registerQuery);
                    using (safeQuerySet)
                    {
                        if (PnrpPeerResolver.UnsafePnrpNativeMethods.WSASetService(PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(safeQuerySet), op, flags) != 0)
                        {
                            PeerExceptionHelper.ThrowPnrpError(PnrpPeerResolver.UnsafePnrpNativeMethods.WSAGetLastError(), registerQuery.Context);
                        }
                    }
                }

                public void Register(PnrpPeerResolver.PnrpRegistration registration, TimeSpan timeout)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo info;
                    info = new PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo {
                        dwLifetime = 0xe10,
                        lpwszIdentity = null,
                        dwSize = Marshal.SizeOf(info),
                        dwFlags = 1
                    };
                    IPEndPoint hint = PnrpPeerResolver.GetHint();
                    info.saHint = PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(hint);
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet registerQuery = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet {
                        NameSpace = PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces.Name,
                        NSProviderId = PnrpPeerResolver.UnsafePnrpNativeMethods.NsProviderName,
                        ServiceClassId = PnrpPeerResolver.UnsafePnrpNativeMethods.SvcIdNameV1,
                        ServiceInstanceName = registration.PeerName,
                        Comment = registration.Comment,
                        Context = registration.CloudName
                    };
                    if (registration.Addresses != null)
                    {
                        registerQuery.CsAddrInfos = new PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfo[registration.Addresses.Length];
                        for (int i = 0; i < registration.Addresses.Length; i++)
                        {
                            registerQuery.CsAddrInfos[i].LocalAddr = registration.Addresses[i];
                            registerQuery.CsAddrInfos[i].iProtocol = 6;
                            registerQuery.CsAddrInfos[i].iSocketType = 1;
                        }
                    }
                    registerQuery.Blob = info;
                    this.RegisterService(registerQuery);
                }

                private void RegisterService(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet registerQuery)
                {
                    try
                    {
                        InvokeService(registerQuery, PnrpPeerResolver.UnsafePnrpNativeMethods.WsaSetServiceOp.Register, 0);
                    }
                    catch (PnrpPeerResolver.PnrpException)
                    {
                        if (4 >= registerQuery.CsAddrInfos.Length)
                        {
                            throw;
                        }
                        List<PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfo> list = new List<PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfo>(registerQuery.CsAddrInfos);
                        list.RemoveRange(4, registerQuery.CsAddrInfos.Length - 4);
                        registerQuery.CsAddrInfos = list.ToArray();
                        InvokeService(registerQuery, PnrpPeerResolver.UnsafePnrpNativeMethods.WsaSetServiceOp.Register, 0);
                    }
                }

                public void Unregister(string peerName, List<string> clouds, TimeSpan timeout)
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    foreach (string str in clouds)
                    {
                        try
                        {
                            this.Unregister(peerName, str, helper.RemainingTime());
                        }
                        catch (SocketException exception)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                }

                public void Unregister(string peerName, string cloudName, TimeSpan timeout)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo info = new PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo {
                        lpwszIdentity = null,
                        dwSize = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo))
                    };
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet registerQuery = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet {
                        NameSpace = PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces.Name,
                        NSProviderId = PnrpPeerResolver.UnsafePnrpNativeMethods.NsProviderName,
                        ServiceClassId = PnrpPeerResolver.UnsafePnrpNativeMethods.SvcIdNameV1,
                        ServiceInstanceName = peerName,
                        Context = cloudName,
                        Blob = info
                    };
                    this.DeleteService(registerQuery);
                }
            }

            internal class PeerNameResolver : AsyncResult
            {
                private Exception lastException;
                private PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet resolveQuery;
                private List<PnrpPeerResolver.PnrpRegistration> results;
                private uint scopeId;
                private TimeoutHelper timeoutHelper;

                public PeerNameResolver(string peerName, int numberOfResultsRequested, PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpResolveCriteria resolveCriteria, TimeSpan timeout, List<PnrpPeerResolver.PnrpRegistration> results) : this(peerName, numberOfResultsRequested, resolveCriteria, 0, "Global_", timeout, results)
                {
                }

                public PeerNameResolver(string peerName, int numberOfResultsRequested, PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpResolveCriteria resolveCriteria, uint scopeId, string cloudName, TimeSpan timeout, List<PnrpPeerResolver.PnrpRegistration> results) : base(null, null)
                {
                    if (timeout > PnrpPeerResolver.MaxTimeout)
                    {
                        timeout = PnrpPeerResolver.MaxTimeout;
                    }
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo info = new PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo {
                        dwSize = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo)),
                        nMaxResolve = numberOfResultsRequested,
                        dwTimeout = (int) timeout.TotalSeconds,
                        dwLifetime = 0,
                        enNameState = (PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpRegisteredIdState) 0,
                        lpwszIdentity = null,
                        dwFlags = 1
                    };
                    IPEndPoint hint = PnrpPeerResolver.GetHint();
                    info.enResolveCriteria = resolveCriteria;
                    info.saHint = PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE.SocketAddressFromIPEndPoint(hint);
                    this.resolveQuery = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet();
                    this.resolveQuery.ServiceInstanceName = peerName;
                    this.resolveQuery.ServiceClassId = PnrpPeerResolver.UnsafePnrpNativeMethods.SvcIdNameV1;
                    this.resolveQuery.NameSpace = PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces.Name;
                    this.resolveQuery.NSProviderId = PnrpPeerResolver.UnsafePnrpNativeMethods.NsProviderName;
                    this.resolveQuery.Context = cloudName;
                    this.resolveQuery.Blob = info;
                    this.results = results;
                    this.scopeId = scopeId;
                    ActionItem.Schedule(new Action<object>(this.SyncEnumeration), null);
                }

                public void End()
                {
                    AsyncResult.End<PnrpPeerResolver.UnsafePnrpNativeMethods.PeerNameResolver>(this);
                }

                private static IPEndPoint IPEndPointFromSocketAddress(PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_NATIVE socketAddress, uint scopeId)
                {
                    IPEndPoint point = null;
                    if (!(socketAddress.lpSockAddr != IntPtr.Zero))
                    {
                        return point;
                    }
                    AddressFamily family = (AddressFamily) Marshal.ReadInt16(socketAddress.lpSockAddr);
                    if (family == AddressFamily.InterNetwork)
                    {
                        if (socketAddress.iSockaddrLength == Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in)))
                        {
                            PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in _in = (PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in) Marshal.PtrToStructure(socketAddress.lpSockAddr, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in));
                            point = new IPEndPoint(new IPAddress(_in.sin_addr), _in.sin_port);
                        }
                        return point;
                    }
                    if ((family != AddressFamily.InterNetworkV6) || (socketAddress.iSockaddrLength != Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in6))))
                    {
                        return point;
                    }
                    PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in6 _in2 = (PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in6) Marshal.PtrToStructure(socketAddress.lpSockAddr, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in6));
                    if ((scopeId != 0) && (_in2.sin6_scope_id != 0))
                    {
                        scopeId = _in2.sin6_scope_id;
                    }
                    return new IPEndPoint(new IPAddress(_in2.sin6_addr, (long) scopeId), _in2.sin6_port);
                }

                internal static PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet MarshalWsaQuerySetNativeToWsaQuerySet(IntPtr pNativeData)
                {
                    return MarshalWsaQuerySetNativeToWsaQuerySet(pNativeData, 0);
                }

                internal static PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet MarshalWsaQuerySetNativeToWsaQuerySet(IntPtr pNativeData, uint scopeId)
                {
                    if (pNativeData == IntPtr.Zero)
                    {
                        return null;
                    }
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet set = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet();
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative native = (PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative) Marshal.PtrToStructure(pNativeData, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative));
                    int num = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoNative));
                    set.Context = Marshal.PtrToStringUni(native.lpszContext);
                    set.NameSpace = native.dwNameSpace;
                    set.ServiceInstanceName = Marshal.PtrToStringUni(native.lpszServiceInstanceName);
                    set.Comment = Marshal.PtrToStringUni(native.lpszComment);
                    set.CsAddrInfos = new PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfo[native.dwNumberOfCsAddrs];
                    for (int i = 0; i < native.dwNumberOfCsAddrs; i++)
                    {
                        IntPtr ptr = (IntPtr) (native.lpcsaBuffer.ToInt64() + (i * num));
                        PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoNative native2 = (PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoNative) Marshal.PtrToStructure(ptr, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoNative));
                        set.CsAddrInfos[i].iProtocol = native2.iProtocol;
                        set.CsAddrInfos[i].iSocketType = native2.iSocketType;
                        set.CsAddrInfos[i].LocalAddr = IPEndPointFromSocketAddress(native2.LocalAddr, scopeId);
                        set.CsAddrInfos[i].RemoteAddr = IPEndPointFromSocketAddress(native2.RemoteAddr, scopeId);
                    }
                    if (native.lpNSProviderId != IntPtr.Zero)
                    {
                        set.NSProviderId = (Guid) Marshal.PtrToStructure(native.lpNSProviderId, typeof(Guid));
                    }
                    if (native.lpServiceClassId != IntPtr.Zero)
                    {
                        set.ServiceClassId = (Guid) Marshal.PtrToStructure(native.lpServiceClassId, typeof(Guid));
                    }
                    if (set.NameSpace == PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces.Cloud)
                    {
                        if (native.lpBlob != IntPtr.Zero)
                        {
                            set.Blob = new PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo();
                            PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative native3 = (PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative) Marshal.PtrToStructure(native.lpBlob, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.BlobNative));
                            if (native3.pBlobData != IntPtr.Zero)
                            {
                                set.Blob = (PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo) Marshal.PtrToStructure(native3.pBlobData, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudInfo));
                            }
                        }
                        return set;
                    }
                    if ((set.NameSpace == PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces.Name) && (native.lpBlob != IntPtr.Zero))
                    {
                        set.Blob = new PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo();
                        PnrpPeerResolver.UnsafePnrpNativeMethods.BlobSafe safe = (PnrpPeerResolver.UnsafePnrpNativeMethods.BlobSafe) Marshal.PtrToStructure(native.lpBlob, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.BlobSafe));
                        if (safe.pBlobData != IntPtr.Zero)
                        {
                            PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo info = (PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo) Marshal.PtrToStructure((IntPtr) safe.pBlobData, typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo));
                            set.Blob = info;
                        }
                    }
                    return set;
                }

                public void SyncEnumeration(object state)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalLookupHandle handle;
                    int num = 0;
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safeQuerySet = PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet.ToWsaQuerySetSafe(this.resolveQuery);
                    using (safeQuerySet)
                    {
                        num = PnrpPeerResolver.UnsafePnrpNativeMethods.WSALookupServiceBegin(PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleWsaQuerySetSafe.FromWsaQuerySetSafe(safeQuerySet), PnrpPeerResolver.UnsafePnrpNativeMethods.WsaNspControlFlags.ReturnAll, out handle);
                    }
                    if (num != 0)
                    {
                        this.lastException = new PnrpPeerResolver.PnrpException(PnrpPeerResolver.UnsafePnrpNativeMethods.WSAGetLastError(), this.resolveQuery.Context);
                        Utility.CloseInvalidOutCriticalHandle(handle);
                        base.Complete(false, this.lastException);
                    }
                    else
                    {
                        PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet set = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet();
                        int size = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe)) + 400;
                        CriticalAllocHandle handle3 = CriticalAllocHandle.FromSize(size);
                        try
                        {
                            using (handle)
                            {
                            Label_0090:
                                if (this.timeoutHelper.RemainingTime() != TimeSpan.Zero)
                                {
                                    if (PnrpPeerResolver.UnsafePnrpNativeMethods.WSALookupServiceNext(handle, 0, ref size, (IntPtr) handle3) != 0)
                                    {
                                        int errorCode = PnrpPeerResolver.UnsafePnrpNativeMethods.WSAGetLastError();
                                        switch (errorCode)
                                        {
                                            case 0x2776:
                                            case 0x277e:
                                                return;

                                            case 0x271e:
                                                handle3 = CriticalAllocHandle.FromSize(size);
                                                goto Label_0090;
                                        }
                                        PeerExceptionHelper.ThrowPnrpError(errorCode, set.Context);
                                        goto Label_0090;
                                    }
                                    if (handle3 == IntPtr.Zero)
                                    {
                                        goto Label_0090;
                                    }
                                    set = MarshalWsaQuerySetNativeToWsaQuerySet((IntPtr) handle3, this.scopeId);
                                    PnrpPeerResolver.PnrpRegistration item = new PnrpPeerResolver.PnrpRegistration {
                                        CloudName = set.Context,
                                        Comment = set.Comment,
                                        PeerName = set.ServiceInstanceName,
                                        Addresses = new IPEndPoint[set.CsAddrInfos.Length]
                                    };
                                    for (int i = 0; i < set.CsAddrInfos.Length; i++)
                                    {
                                        item.Addresses[i] = set.CsAddrInfos[i].LocalAddr;
                                    }
                                    lock (this.results)
                                    {
                                        this.results.Add(item);
                                        goto Label_0090;
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                PnrpResolveExceptionTraceRecord extendedData = new PnrpResolveExceptionTraceRecord(this.resolveQuery.ServiceInstanceName, this.resolveQuery.Context, exception);
                                if (DiagnosticUtility.ShouldTraceError)
                                {
                                    TraceUtility.TraceEvent(TraceEventType.Error, 0x4004b, System.ServiceModel.SR.GetString("TraceCodePnrpResolveException"), extendedData, this, null);
                                }
                            }
                            this.lastException = exception;
                        }
                        finally
                        {
                            base.Complete(false, this.lastException);
                        }
                    }
                }
            }

            [Serializable, Flags]
            internal enum PnrpCloudFlags
            {
                None,
                LocalName
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            internal struct PnrpCloudId
            {
                public int AddressFamily;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpScope Scope;
                public uint ScopeId;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct PnrpCloudInfo
            {
                public int dwSize;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudId Cloud;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudState dwCloudState;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpCloudFlags Flags;
            }

            [Serializable]
            internal enum PnrpCloudState
            {
                Virtual,
                Synchronizing,
                Active,
                Dead,
                Disabled,
                NoNet,
                Alone
            }

            [Serializable]
            internal enum PnrpExtendedPayloadType
            {
                None,
                Binary,
                String
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct PnrpInfo
            {
                public int dwSize;
                public string lpwszIdentity;
                public int nMaxResolve;
                public int dwTimeout;
                public int dwLifetime;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpResolveCriteria enResolveCriteria;
                public int dwFlags;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE saHint;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpRegisteredIdState enNameState;
                public static void ToPnrpInfoNative(PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfo source, ref PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpInfoNative target)
                {
                    target.dwSize = source.dwSize;
                    target.lpwszIdentity = source.lpwszIdentity;
                    target.nMaxResolve = source.nMaxResolve;
                    target.dwTimeout = source.dwTimeout;
                    target.dwLifetime = source.dwLifetime;
                    target.enResolveCriteria = source.enResolveCriteria;
                    target.dwFlags = source.dwFlags;
                    if (source.saHint != null)
                    {
                        target.saHint.lpSockAddr = (IntPtr) source.saHint.lpSockAddr;
                        target.saHint.iSockaddrLength = source.saHint.iSockaddrLength;
                    }
                    else
                    {
                        target.saHint.lpSockAddr = IntPtr.Zero;
                        target.saHint.iSockaddrLength = 0;
                    }
                    target.enNameState = source.enNameState;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct PnrpInfoNative
            {
                public int dwSize;
                public string lpwszIdentity;
                public int nMaxResolve;
                public int dwTimeout;
                public int dwLifetime;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpResolveCriteria enResolveCriteria;
                public int dwFlags;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_NATIVE saHint;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.PnrpRegisteredIdState enNameState;
            }

            [Serializable]
            internal enum PnrpRegisteredIdState
            {
                Ok = 1,
                Problem = 2
            }

            [Serializable]
            internal enum PnrpResolveCriteria
            {
                Default,
                Remote,
                NearestRemote,
                NonCurrentProcess,
                NearestNonCurrentProcess,
                Any,
                Nearest
            }

            internal enum PnrpScope
            {
                Any,
                Global,
                SiteLocal,
                LinkLocal
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            internal struct sockaddr_in
            {
                public short sin_family;
                public ushort sin_port;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst=4)]
                public byte[] sin_addr;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
                public byte[] sin_zero;
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            internal struct sockaddr_in6
            {
                public short sin6_family;
                public ushort sin6_port;
                public uint sin6_flowinfo;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
                public byte[] sin6_addr;
                public uint sin6_scope_id;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct SOCKET_ADDRESS_NATIVE
            {
                public IntPtr lpSockAddr;
                public int iSockaddrLength;
            }

            internal class SOCKET_ADDRESS_SAFE : IDisposable
            {
                private bool disposed;
                public int iSockaddrLength;
                public CriticalAllocHandle lpSockAddr;

                public virtual void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }

                private void Dispose(bool disposing)
                {
                    if (!this.disposed && disposing)
                    {
                        this.lpSockAddr.Dispose();
                    }
                    this.disposed = true;
                }

                ~SOCKET_ADDRESS_SAFE()
                {
                    this.Dispose(false);
                }

                public static PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE SocketAddressFromIPEndPoint(IPEndPoint endpoint)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE socket_address_safe = new PnrpPeerResolver.UnsafePnrpNativeMethods.SOCKET_ADDRESS_SAFE();
                    if (endpoint != null)
                    {
                        if (endpoint.AddressFamily == AddressFamily.InterNetwork)
                        {
                            socket_address_safe.iSockaddrLength = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in));
                            socket_address_safe.lpSockAddr = CriticalAllocHandle.FromSize(socket_address_safe.iSockaddrLength);
                            PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in structure = new PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in {
                                sin_family = 2,
                                sin_port = (ushort) endpoint.Port,
                                sin_addr = endpoint.Address.GetAddressBytes()
                            };
                            Marshal.StructureToPtr(structure, (IntPtr) socket_address_safe.lpSockAddr, false);
                            return socket_address_safe;
                        }
                        if (endpoint.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            socket_address_safe.iSockaddrLength = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in6));
                            socket_address_safe.lpSockAddr = CriticalAllocHandle.FromSize(socket_address_safe.iSockaddrLength);
                            PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in6 _in2 = new PnrpPeerResolver.UnsafePnrpNativeMethods.sockaddr_in6 {
                                sin6_family = 0x17,
                                sin6_port = (ushort) endpoint.Port,
                                sin6_addr = endpoint.Address.GetAddressBytes(),
                                sin6_scope_id = (uint) endpoint.Address.ScopeId
                            };
                            Marshal.StructureToPtr(_in2, (IntPtr) socket_address_safe.lpSockAddr, false);
                        }
                    }
                    return socket_address_safe;
                }
            }

            [Serializable, StructLayout(LayoutKind.Sequential)]
            internal struct WsaData
            {
                public short wVersion;
                public short wHighVersion;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x101)]
                public string szDescription;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x81)]
                public string szSystemStatus;
                public short iMaxSockets;
                public short iMaxUdpDg;
                public IntPtr lpVendorInfo;
            }

            internal enum WsaError
            {
                WSA_E_NO_MORE = 0x277e,
                WSAEFAULT = 0x271e,
                WSAEINVAL = 0x2726,
                WSAENOMORE = 0x2776,
                WSANO_DATA = 0x2afc
            }

            [Serializable, StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct WsaNamespaceInfo
            {
                public Guid NSProviderId;
                public int dwNameSpace;
                public int fActive;
                public int dwVersion;
                public IntPtr lpszIdentifier;
            }

            [Flags]
            internal enum WsaNspControlFlags
            {
                Containers = 2,
                Deep = 1,
                FlushCache = 0x1000,
                FlushPrevious = 0x2000,
                Nearest = 8,
                NoContainers = 4,
                ResService = 0x8000,
                ReturnAddr = 0x100,
                ReturnAliases = 0x400,
                ReturnAll = 0xff0,
                ReturnBlob = 0x200,
                ReturnComment = 0x80,
                ReturnName = 0x10,
                ReturnQueryString = 0x800,
                ReturnType = 0x20,
                ReturnVersion = 0x40
            }

            internal class WsaQuerySet
            {
                public object Blob;
                public string Comment;
                public string Context;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfo[] CsAddrInfos;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces NameSpace;
                public Guid NSProviderId;
                public Guid ServiceClassId;
                public string ServiceInstanceName;

                public static PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe ToWsaQuerySetSafe(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySet input)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safe = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe();
                    if (input != null)
                    {
                        safe.dwSize = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative));
                        safe.lpszServiceInstanceName = PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleString.FromString(input.ServiceInstanceName);
                        safe.lpServiceClassId = CriticalAllocHandleGuid.FromGuid(input.ServiceClassId);
                        safe.lpszComment = PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleString.FromString(input.Comment);
                        safe.dwNameSpace = input.NameSpace;
                        safe.lpNSProviderId = CriticalAllocHandleGuid.FromGuid(input.NSProviderId);
                        safe.lpszContext = PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandleString.FromString(input.Context);
                        safe.dwNumberOfProtocols = 0;
                        safe.lpafpProtocols = IntPtr.Zero;
                        safe.lpszQueryString = IntPtr.Zero;
                        if (input.CsAddrInfos != null)
                        {
                            safe.dwNumberOfCsAddrs = input.CsAddrInfos.Length;
                            safe.addressList = PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe.FromAddresses(input.CsAddrInfos);
                        }
                        safe.dwOutputFlags = 0;
                        safe.lpBlob = PnrpPeerResolver.UnsafePnrpNativeMethods.CriticalAllocHandlePnrpBlob.FromPnrpBlob(input.Blob);
                    }
                    return safe;
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct WsaQuerySetNative
            {
                public int dwSize;
                public IntPtr lpszServiceInstanceName;
                public IntPtr lpServiceClassId;
                public IntPtr lpVersion;
                public IntPtr lpszComment;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces dwNameSpace;
                public IntPtr lpNSProviderId;
                public IntPtr lpszContext;
                public int dwNumberOfProtocols;
                public IntPtr lpafpProtocols;
                public IntPtr lpszQueryString;
                public int dwNumberOfCsAddrs;
                public IntPtr lpcsaBuffer;
                public int dwOutputFlags;
                public IntPtr lpBlob;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal class WsaQuerySetSafe : IDisposable
            {
                public int dwSize;
                public CriticalAllocHandle lpszServiceInstanceName;
                public CriticalAllocHandle lpServiceClassId;
                public IntPtr lpVersion;
                public CriticalAllocHandle lpszComment;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.NspNamespaces dwNameSpace;
                public CriticalAllocHandle lpNSProviderId;
                public CriticalAllocHandle lpszContext;
                public int dwNumberOfProtocols;
                public IntPtr lpafpProtocols;
                public IntPtr lpszQueryString;
                public int dwNumberOfCsAddrs;
                public PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe[] addressList;
                public int dwOutputFlags;
                public CriticalAllocHandle lpBlob;
                private bool disposed;
                ~WsaQuerySetSafe()
                {
                    this.Dispose(false);
                }

                public virtual void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }

                private void Dispose(bool disposing)
                {
                    if (!this.disposed && disposing)
                    {
                        if (this.lpszServiceInstanceName != null)
                        {
                            this.lpszServiceInstanceName.Dispose();
                        }
                        if (this.lpServiceClassId != null)
                        {
                            this.lpServiceClassId.Dispose();
                        }
                        if (this.lpszComment != null)
                        {
                            this.lpszComment.Dispose();
                        }
                        if (this.lpNSProviderId != null)
                        {
                            this.lpNSProviderId.Dispose();
                        }
                        if (this.lpBlob != null)
                        {
                            this.lpBlob.Dispose();
                        }
                        if (this.addressList != null)
                        {
                            foreach (PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe safe in this.addressList)
                            {
                                safe.Dispose();
                            }
                        }
                    }
                    this.disposed = true;
                }

                public static void StructureToPtr(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe input, IntPtr target)
                {
                    PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative structure = new PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative {
                        dwSize = input.dwSize,
                        lpszServiceInstanceName = (IntPtr) input.lpszServiceInstanceName,
                        lpServiceClassId = (IntPtr) input.lpServiceClassId,
                        lpVersion = IntPtr.Zero,
                        lpszComment = (IntPtr) input.lpszComment,
                        dwNameSpace = input.dwNameSpace,
                        lpNSProviderId = (IntPtr) input.lpNSProviderId,
                        lpszContext = (IntPtr) input.lpszContext,
                        dwNumberOfProtocols = 0,
                        lpafpProtocols = IntPtr.Zero,
                        lpszQueryString = IntPtr.Zero,
                        dwNumberOfCsAddrs = input.dwNumberOfCsAddrs,
                        dwOutputFlags = 0,
                        lpBlob = (IntPtr) input.lpBlob
                    };
                    long num = target.ToInt64() + Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetNative));
                    structure.lpcsaBuffer = (IntPtr) num;
                    Marshal.StructureToPtr(structure, target, false);
                    MarshalSafeAddressesToNative(input, (IntPtr) num);
                }

                public static void MarshalSafeAddressesToNative(PnrpPeerResolver.UnsafePnrpNativeMethods.WsaQuerySetSafe safeQuery, IntPtr target)
                {
                    if ((safeQuery.addressList != null) && (safeQuery.addressList.Length > 0))
                    {
                        int num = Marshal.SizeOf(typeof(PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoNative));
                        long num2 = target.ToInt64();
                        foreach (PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe safe in safeQuery.addressList)
                        {
                            PnrpPeerResolver.UnsafePnrpNativeMethods.CsAddrInfoSafe.StructureToPtr(safe, (IntPtr) num2);
                            num2 += num;
                        }
                    }
                }
            }

            internal enum WsaSetServiceOp
            {
                Register,
                Deregister,
                Delete
            }
        }
    }
}

