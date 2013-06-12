namespace System.Net
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net.Configuration;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class ServicePointManager
    {
        private const int DefaultAspPersistentConnectionLimit = 10;
        public const int DefaultNonPersistentConnectionLimit = 4;
        public const int DefaultPersistentConnectionLimit = 2;
        private static System.Net.CertPolicyValidationCallback s_CertPolicyValidationCallback = new System.Net.CertPolicyValidationCallback();
        private static Hashtable s_ConfigTable = null;
        private static int s_ConnectionLimit = PersistentConnectionLimit;
        internal static readonly TimerThread.Callback s_IdleServicePointTimeoutDelegate = new TimerThread.Callback(ServicePointManager.IdleServicePointTimeoutCallback);
        private static int s_MaxServicePoints = 0;
        private static SecurityProtocolType s_SecurityProtocolType = (SecurityProtocolType.Tls | SecurityProtocolType.Ssl3);
        private static System.Net.ServerCertValidationCallback s_ServerCertValidationCallback = null;
        private static TimerThread.Queue s_ServicePointIdlingQueue = TimerThread.GetOrCreateQueue(0x186a0);
        private static Hashtable s_ServicePointTable = new Hashtable(10);
        internal static int s_TcpKeepAliveInterval;
        internal static int s_TcpKeepAliveTime;
        private static bool s_UserChangedLimit;
        internal static bool s_UseTcpKeepAlive = false;
        internal static readonly string SpecialConnectGroupName = "/.NET/NetClasses/HttpWebRequest/CONNECT__Group$$/";

        private ServicePointManager()
        {
        }

        [Conditional("DEBUG")]
        internal static void Debug(int requestHash)
        {
            try
            {
                foreach (WeakReference reference in s_ServicePointTable)
                {
                    ServicePoint target;
                    if ((reference != null) && reference.IsAlive)
                    {
                        target = (ServicePoint) reference.Target;
                    }
                    else
                    {
                        target = null;
                    }
                }
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
            }
        }

        internal static ServicePoint FindServicePoint(ProxyChain chain)
        {
            if (!chain.Enumerator.MoveNext())
            {
                return null;
            }
            Uri current = chain.Enumerator.Current;
            return FindServicePointHelper((current == null) ? chain.Destination : current, current != null);
        }

        public static ServicePoint FindServicePoint(Uri address)
        {
            return FindServicePoint(address, null);
        }

        internal static ServicePoint FindServicePoint(string host, int port)
        {
            if (host == null)
            {
                throw new ArgumentNullException("address");
            }
            string lookupString = null;
            bool proxyServicePoint = false;
            lookupString = "ByHost:" + host + ":" + port.ToString(CultureInfo.InvariantCulture);
            ServicePoint target = null;
            lock (s_ServicePointTable)
            {
                WeakReference reference = s_ServicePointTable[lookupString] as WeakReference;
                if (reference != null)
                {
                    target = (ServicePoint) reference.Target;
                }
                if (target != null)
                {
                    return target;
                }
                if ((s_MaxServicePoints <= 0) || (s_ServicePointTable.Count < s_MaxServicePoints))
                {
                    int internalConnectionLimit = InternalConnectionLimit;
                    bool userChangedLimit = s_UserChangedLimit;
                    string key = host + ":" + port.ToString(CultureInfo.InvariantCulture);
                    if (ConfigTable.ContainsKey(key))
                    {
                        internalConnectionLimit = (int) ConfigTable[key];
                        userChangedLimit = true;
                    }
                    target = new ServicePoint(host, port, s_ServicePointIdlingQueue, internalConnectionLimit, lookupString, userChangedLimit, proxyServicePoint);
                    reference = new WeakReference(target);
                    s_ServicePointTable[lookupString] = reference;
                    return target;
                }
                Exception exception = new InvalidOperationException(SR.GetString("net_maxsrvpoints"));
                throw exception;
            }
            return target;
        }

        public static ServicePoint FindServicePoint(string uriString, IWebProxy proxy)
        {
            Uri address = new Uri(uriString);
            return FindServicePoint(address, proxy);
        }

        public static ServicePoint FindServicePoint(Uri address, IWebProxy proxy)
        {
            ProxyChain chain;
            HttpAbortDelegate abortDelegate = null;
            int abortState = 0;
            return FindServicePoint(address, proxy, out chain, ref abortDelegate, ref abortState);
        }

        internal static ServicePoint FindServicePoint(Uri address, IWebProxy proxy, out ProxyChain chain, ref HttpAbortDelegate abortDelegate, ref int abortState)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            bool isProxyServicePoint = false;
            chain = null;
            Uri current = null;
            if ((proxy != null) && !address.IsLoopback)
            {
                IAutoWebProxy proxy2 = proxy as IAutoWebProxy;
                if (proxy2 != null)
                {
                    chain = proxy2.GetProxies(address);
                    abortDelegate = chain.HttpAbortDelegate;
                    try
                    {
                        Thread.MemoryBarrier();
                        if (abortState != 0)
                        {
                            Exception exception = new WebException(NetRes.GetWebStatusString(WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
                            throw exception;
                        }
                        chain.Enumerator.MoveNext();
                        current = chain.Enumerator.Current;
                    }
                    finally
                    {
                        abortDelegate = null;
                    }
                }
                else if (!proxy.IsBypassed(address))
                {
                    current = proxy.GetProxy(address);
                }
                if (current != null)
                {
                    address = current;
                    isProxyServicePoint = true;
                }
            }
            return FindServicePointHelper(address, isProxyServicePoint);
        }

        private static ServicePoint FindServicePointHelper(Uri address, bool isProxyServicePoint)
        {
            if (isProxyServicePoint && (address.Scheme != Uri.UriSchemeHttp))
            {
                Exception exception = new NotSupportedException(SR.GetString("net_proxyschemenotsupported", new object[] { address.Scheme }));
                throw exception;
            }
            string lookupString = MakeQueryString(address, isProxyServicePoint);
            ServicePoint target = null;
            lock (s_ServicePointTable)
            {
                WeakReference reference = s_ServicePointTable[lookupString] as WeakReference;
                if (reference != null)
                {
                    target = (ServicePoint) reference.Target;
                }
                if (target != null)
                {
                    return target;
                }
                if ((s_MaxServicePoints <= 0) || (s_ServicePointTable.Count < s_MaxServicePoints))
                {
                    int internalConnectionLimit = InternalConnectionLimit;
                    string key = MakeQueryString(address);
                    bool userChangedLimit = s_UserChangedLimit;
                    if (ConfigTable.ContainsKey(key))
                    {
                        internalConnectionLimit = (int) ConfigTable[key];
                        userChangedLimit = true;
                    }
                    target = new ServicePoint(address, s_ServicePointIdlingQueue, internalConnectionLimit, lookupString, userChangedLimit, isProxyServicePoint);
                    reference = new WeakReference(target);
                    s_ServicePointTable[lookupString] = reference;
                    return target;
                }
                Exception exception2 = new InvalidOperationException(SR.GetString("net_maxsrvpoints"));
                throw exception2;
            }
            return target;
        }

        internal static ICertificatePolicy GetLegacyCertificatePolicy()
        {
            if (s_CertPolicyValidationCallback == null)
            {
                return null;
            }
            return s_CertPolicyValidationCallback.CertificatePolicy;
        }

        private static void IdleServicePointTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ServicePoint point = (ServicePoint) context;
            lock (s_ServicePointTable)
            {
                s_ServicePointTable.Remove(point.LookupString);
            }
            point.ReleaseAllConnectionGroups();
        }

        internal static string MakeQueryString(Uri address)
        {
            if (address.IsDefaultPort)
            {
                return (address.Scheme + "://" + address.DnsSafeHost);
            }
            return (address.Scheme + "://" + address.DnsSafeHost + ":" + address.Port.ToString());
        }

        internal static string MakeQueryString(Uri address1, bool isProxy)
        {
            if (isProxy)
            {
                return (MakeQueryString(address1) + "://proxy");
            }
            return MakeQueryString(address1);
        }

        public static void SetTcpKeepAlive(bool enabled, int keepAliveTime, int keepAliveInterval)
        {
            if (enabled)
            {
                s_UseTcpKeepAlive = true;
                if (keepAliveTime <= 0)
                {
                    throw new ArgumentOutOfRangeException("keepAliveTime");
                }
                if (keepAliveInterval <= 0)
                {
                    throw new ArgumentOutOfRangeException("keepAliveInterval");
                }
                s_TcpKeepAliveTime = keepAliveTime;
                s_TcpKeepAliveInterval = keepAliveInterval;
            }
            else
            {
                s_UseTcpKeepAlive = false;
                s_TcpKeepAliveTime = 0;
                s_TcpKeepAliveInterval = 0;
            }
        }

        [Obsolete("CertificatePolicy is obsoleted for this type, please use ServerCertificateValidationCallback instead. http://go.microsoft.com/fwlink/?linkid=14202")]
        public static ICertificatePolicy CertificatePolicy
        {
            get
            {
                return GetLegacyCertificatePolicy();
            }
            set
            {
                ExceptionHelper.UnmanagedPermission.Demand();
                s_CertPolicyValidationCallback = new System.Net.CertPolicyValidationCallback(value);
            }
        }

        internal static System.Net.CertPolicyValidationCallback CertPolicyValidationCallback
        {
            get
            {
                return s_CertPolicyValidationCallback;
            }
        }

        internal static bool CheckCertificateName
        {
            get
            {
                return SettingsSectionInternal.Section.CheckCertificateName;
            }
        }

        public static bool CheckCertificateRevocationList
        {
            get
            {
                return SettingsSectionInternal.Section.CheckCertificateRevocationList;
            }
            set
            {
                ExceptionHelper.UnmanagedPermission.Demand();
                SettingsSectionInternal.Section.CheckCertificateRevocationList = value;
            }
        }

        private static Hashtable ConfigTable
        {
            get
            {
                if (s_ConfigTable == null)
                {
                    lock (s_ServicePointTable)
                    {
                        if (s_ConfigTable == null)
                        {
                            Hashtable connectionManagement = ConnectionManagementSectionInternal.GetSection().ConnectionManagement;
                            if (connectionManagement == null)
                            {
                                connectionManagement = new Hashtable();
                            }
                            if (connectionManagement.ContainsKey("*"))
                            {
                                int persistentConnectionLimit = (int) connectionManagement["*"];
                                if (persistentConnectionLimit < 1)
                                {
                                    persistentConnectionLimit = PersistentConnectionLimit;
                                }
                                s_ConnectionLimit = persistentConnectionLimit;
                            }
                            s_ConfigTable = connectionManagement;
                        }
                    }
                }
                return s_ConfigTable;
            }
        }

        public static int DefaultConnectionLimit
        {
            get
            {
                return InternalConnectionLimit;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", SR.GetString("net_toosmall"));
                }
                InternalConnectionLimit = value;
            }
        }

        public static int DnsRefreshTimeout
        {
            get
            {
                return SettingsSectionInternal.Section.DnsRefreshTimeout;
            }
            set
            {
                if (value < -1)
                {
                    SettingsSectionInternal.Section.DnsRefreshTimeout = -1;
                }
                else
                {
                    SettingsSectionInternal.Section.DnsRefreshTimeout = value;
                }
            }
        }

        public static bool EnableDnsRoundRobin
        {
            get
            {
                return SettingsSectionInternal.Section.EnableDnsRoundRobin;
            }
            set
            {
                SettingsSectionInternal.Section.EnableDnsRoundRobin = value;
            }
        }

        public static System.Net.Security.EncryptionPolicy EncryptionPolicy
        {
            get
            {
                return SettingsSectionInternal.Section.EncryptionPolicy;
            }
        }

        public static bool Expect100Continue
        {
            get
            {
                return SettingsSectionInternal.Section.Expect100Continue;
            }
            set
            {
                SettingsSectionInternal.Section.Expect100Continue = value;
            }
        }

        internal static TimerThread.Callback IdleServicePointTimeoutDelegate
        {
            get
            {
                return s_IdleServicePointTimeoutDelegate;
            }
        }

        private static int InternalConnectionLimit
        {
            get
            {
                if (s_ConfigTable == null)
                {
                    s_ConfigTable = ConfigTable;
                }
                return s_ConnectionLimit;
            }
            set
            {
                if (s_ConfigTable == null)
                {
                    s_ConfigTable = ConfigTable;
                }
                s_UserChangedLimit = true;
                s_ConnectionLimit = value;
            }
        }

        public static int MaxServicePointIdleTime
        {
            get
            {
                return s_ServicePointIdlingQueue.Duration;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (!ValidationHelper.ValidateRange(value, -1, 0x7fffffff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (s_ServicePointIdlingQueue.Duration != value)
                {
                    s_ServicePointIdlingQueue = TimerThread.GetOrCreateQueue(value);
                }
            }
        }

        public static int MaxServicePoints
        {
            get
            {
                return s_MaxServicePoints;
            }
            set
            {
                ExceptionHelper.WebPermissionUnrestricted.Demand();
                if (!ValidationHelper.ValidateRange(value, 0, 0x7fffffff))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                s_MaxServicePoints = value;
            }
        }

        private static int PersistentConnectionLimit
        {
            get
            {
                if (ComNetOS.IsAspNetServer)
                {
                    return 10;
                }
                return 2;
            }
        }

        public static SecurityProtocolType SecurityProtocol
        {
            get
            {
                return s_SecurityProtocolType;
            }
            set
            {
                if ((value & ~(SecurityProtocolType.Tls | SecurityProtocolType.Ssl3)) != 0)
                {
                    throw new NotSupportedException(SR.GetString("net_securityprotocolnotsupported"));
                }
                s_SecurityProtocolType = value;
            }
        }

        public static RemoteCertificateValidationCallback ServerCertificateValidationCallback
        {
            get
            {
                if (s_ServerCertValidationCallback == null)
                {
                    return null;
                }
                return s_ServerCertValidationCallback.ValidationCallback;
            }
            set
            {
                ExceptionHelper.InfrastructurePermission.Demand();
                s_ServerCertValidationCallback = new System.Net.ServerCertValidationCallback(value);
            }
        }

        internal static System.Net.ServerCertValidationCallback ServerCertValidationCallback
        {
            get
            {
                return s_ServerCertValidationCallback;
            }
        }

        public static bool UseNagleAlgorithm
        {
            get
            {
                return SettingsSectionInternal.Section.UseNagleAlgorithm;
            }
            set
            {
                SettingsSectionInternal.Section.UseNagleAlgorithm = value;
            }
        }
    }
}

