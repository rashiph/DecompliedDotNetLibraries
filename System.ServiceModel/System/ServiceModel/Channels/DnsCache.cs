namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.ServiceModel;

    internal static class DnsCache
    {
        private static readonly TimeSpan cacheTimeout = TimeSpan.FromSeconds(2.0);
        private static string machineName;
        private const int mruWatermark = 0x40;
        private static MruCache<string, DnsCacheEntry> resolveCache = new MruCache<string, DnsCacheEntry>(0x40);

        public static IPHostEntry Resolve(string hostName)
        {
            IPHostEntry hostEntry = null;
            DateTime utcNow = DateTime.UtcNow;
            lock (ThisLock)
            {
                DnsCacheEntry entry2;
                if (resolveCache.TryGetValue(hostName, out entry2))
                {
                    if (utcNow.Subtract(entry2.TimeStamp) > cacheTimeout)
                    {
                        resolveCache.Remove(hostName);
                    }
                    else
                    {
                        if (entry2.HostEntry == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("DnsResolveFailed", new object[] { hostName })));
                        }
                        hostEntry = entry2.HostEntry;
                    }
                }
            }
            if (hostEntry == null)
            {
                SocketException innerException = null;
                try
                {
                    hostEntry = Dns.GetHostEntry(hostName);
                }
                catch (SocketException exception2)
                {
                    innerException = exception2;
                }
                lock (ThisLock)
                {
                    resolveCache.Remove(hostName);
                    resolveCache.Add(hostName, new DnsCacheEntry(hostEntry, utcNow));
                }
                if (innerException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(System.ServiceModel.SR.GetString("DnsResolveFailed", new object[] { hostName }), innerException));
                }
            }
            return hostEntry;
        }

        public static string MachineName
        {
            get
            {
                if (machineName == null)
                {
                    lock (ThisLock)
                    {
                        if (machineName == null)
                        {
                            try
                            {
                                machineName = Dns.GetHostEntry(string.Empty).HostName;
                            }
                            catch (SocketException exception)
                            {
                                if (DiagnosticUtility.ShouldTraceInformation)
                                {
                                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                                }
                                machineName = UnsafeNativeMethods.GetComputerName(System.ServiceModel.Channels.ComputerNameFormat.PhysicalNetBIOS);
                            }
                        }
                    }
                }
                return machineName;
            }
        }

        private static object ThisLock
        {
            get
            {
                return resolveCache;
            }
        }

        private class DnsCacheEntry
        {
            private IPHostEntry hostEntry;
            private DateTime timeStamp;

            public DnsCacheEntry(IPHostEntry hostEntry, DateTime timeStamp)
            {
                this.hostEntry = hostEntry;
                this.timeStamp = timeStamp;
            }

            public IPHostEntry HostEntry
            {
                get
                {
                    return this.hostEntry;
                }
            }

            public DateTime TimeStamp
            {
                get
                {
                    return this.timeStamp;
                }
            }
        }
    }
}

