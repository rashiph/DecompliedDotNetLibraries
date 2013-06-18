namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class LdapSessionOptions
    {
        private System.DirectoryServices.Protocols.ReferralCallback callbackRoutine = new System.DirectoryServices.Protocols.ReferralCallback();
        internal QueryClientCertificateCallback clientCertificateDelegate;
        private LdapConnection connection;
        private DEREFERENCECONNECTIONInternal dereferenceDelegate;
        private NOTIFYOFNEWCONNECTIONInternal notifiyDelegate;
        private QUERYFORCONNECTIONInternal queryDelegate;
        private VerifyServerCertificateCallback serverCertificateDelegate;
        private VERIFYSERVERCERT serverCertificateRoutine;

        internal LdapSessionOptions(LdapConnection connection)
        {
            this.connection = connection;
            this.queryDelegate = new QUERYFORCONNECTIONInternal(this.ProcessQueryConnection);
            this.notifiyDelegate = new NOTIFYOFNEWCONNECTIONInternal(this.ProcessNotifyConnection);
            this.dereferenceDelegate = new DEREFERENCECONNECTIONInternal(this.ProcessDereferenceConnection);
            this.serverCertificateRoutine = new VERIFYSERVERCERT(this.ProcessServerCertificate);
        }

        public void FastConcurrentBind()
        {
            if (this.connection.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            int inValue = 1;
            this.ProtocolVersion = 3;
            int error = Wldap32.ldap_set_option_int(this.connection.ldapHandle, LdapOption.LDAP_OPT_FAST_CONCURRENT_BIND, ref inValue);
            if ((error == 0x59) && !Utility.IsWin2k3AboveOS)
            {
                throw new PlatformNotSupportedException(Res.GetString("ConcurrentBindNotSupport"));
            }
            ErrorChecking.CheckAndSetLdapError(error);
        }

        private int GetIntValueHelper(LdapOption option)
        {
            if (this.connection.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            int outValue = 0;
            ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_get_option_int(this.connection.ldapHandle, option, ref outValue));
            return outValue;
        }

        private string GetStringValueHelper(LdapOption option, bool releasePtr)
        {
            if (this.connection.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            IntPtr outValue = new IntPtr(0);
            ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_get_option_ptr(this.connection.ldapHandle, option, ref outValue));
            string str = null;
            if (outValue != IntPtr.Zero)
            {
                str = Marshal.PtrToStringUni(outValue);
            }
            if (releasePtr)
            {
                Wldap32.ldap_memfree(outValue);
            }
            return str;
        }

        private void ProcessCallBackRoutine(System.DirectoryServices.Protocols.ReferralCallback tempCallback)
        {
            LdapReferralCallback outValue = new LdapReferralCallback {
                sizeofcallback = Marshal.SizeOf(typeof(LdapReferralCallback)),
                query = (tempCallback.QueryForConnection == null) ? null : this.queryDelegate,
                notify = (tempCallback.NotifyNewConnection == null) ? null : this.notifiyDelegate,
                dereference = (tempCallback.DereferenceConnection == null) ? null : this.dereferenceDelegate
            };
            ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_set_option_referral(this.connection.ldapHandle, LdapOption.LDAP_OPT_REFERRAL_CALLBACK, ref outValue));
        }

        private int ProcessDereferenceConnection(IntPtr PrimaryConnection, IntPtr ConnectionToDereference)
        {
            if ((ConnectionToDereference != IntPtr.Zero) && (this.callbackRoutine.DereferenceConnection != null))
            {
                LdapConnection connectionToDereference = null;
                WeakReference reference = null;
                lock (LdapConnection.objectLock)
                {
                    reference = (WeakReference) LdapConnection.handleTable[ConnectionToDereference];
                }
                if ((reference == null) || !reference.IsAlive)
                {
                    connectionToDereference = new LdapConnection((LdapDirectoryIdentifier) this.connection.Directory, this.connection.GetCredential(), this.connection.AuthType, ConnectionToDereference);
                }
                else
                {
                    connectionToDereference = (LdapConnection) reference.Target;
                }
                this.callbackRoutine.DereferenceConnection(this.connection, connectionToDereference);
            }
            return 1;
        }

        private bool ProcessNotifyConnection(IntPtr PrimaryConnection, IntPtr ReferralFromConnection, IntPtr NewDNPtr, string HostName, IntPtr NewConnection, int PortNumber, SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentity, Luid CurrentUser, int ErrorCodeFromBind)
        {
            string newDistinguishedName = null;
            if (!(NewConnection != IntPtr.Zero) || (this.callbackRoutine.NotifyNewConnection == null))
            {
                return false;
            }
            if (NewDNPtr != IntPtr.Zero)
            {
                newDistinguishedName = Marshal.PtrToStringUni(NewDNPtr);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(HostName);
            builder.Append(":");
            builder.Append(PortNumber);
            LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(builder.ToString());
            NetworkCredential credential = this.ProcessSecAuthIdentity(SecAuthIdentity);
            LdapConnection target = null;
            LdapConnection connection2 = null;
            WeakReference reference = null;
            lock (LdapConnection.objectLock)
            {
                if (ReferralFromConnection != IntPtr.Zero)
                {
                    reference = (WeakReference) LdapConnection.handleTable[ReferralFromConnection];
                    if ((reference != null) && reference.IsAlive)
                    {
                        connection2 = (LdapConnection) reference.Target;
                    }
                    else
                    {
                        if (reference != null)
                        {
                            LdapConnection.handleTable.Remove(ReferralFromConnection);
                        }
                        connection2 = new LdapConnection((LdapDirectoryIdentifier) this.connection.Directory, this.connection.GetCredential(), this.connection.AuthType, ReferralFromConnection);
                        LdapConnection.handleTable.Add(ReferralFromConnection, new WeakReference(connection2));
                    }
                }
                if (NewConnection != IntPtr.Zero)
                {
                    reference = (WeakReference) LdapConnection.handleTable[NewConnection];
                    if ((reference != null) && reference.IsAlive)
                    {
                        target = (LdapConnection) reference.Target;
                    }
                    else
                    {
                        if (reference != null)
                        {
                            LdapConnection.handleTable.Remove(NewConnection);
                        }
                        target = new LdapConnection(identifier, credential, this.connection.AuthType, NewConnection);
                        LdapConnection.handleTable.Add(NewConnection, new WeakReference(target));
                    }
                }
            }
            long currentUserToken = ((long) ((ulong) CurrentUser.LowPart)) + (CurrentUser.HighPart << 0x20);
            bool flag = this.callbackRoutine.NotifyNewConnection(this.connection, connection2, newDistinguishedName, identifier, target, credential, currentUserToken, ErrorCodeFromBind);
            if (flag)
            {
                target.needDispose = true;
            }
            return flag;
        }

        private int ProcessQueryConnection(IntPtr PrimaryConnection, IntPtr ReferralFromConnection, IntPtr NewDNPtr, string HostName, int PortNumber, SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentity, Luid CurrentUserToken, ref IntPtr ConnectionToUse)
        {
            ConnectionToUse = IntPtr.Zero;
            string newDistinguishedName = null;
            if (this.callbackRoutine.QueryForConnection == null)
            {
                return 1;
            }
            if (NewDNPtr != IntPtr.Zero)
            {
                newDistinguishedName = Marshal.PtrToStringUni(NewDNPtr);
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(HostName);
            builder.Append(":");
            builder.Append(PortNumber);
            LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(builder.ToString());
            NetworkCredential credential = this.ProcessSecAuthIdentity(SecAuthIdentity);
            LdapConnection target = null;
            WeakReference reference = null;
            if (ReferralFromConnection != IntPtr.Zero)
            {
                lock (LdapConnection.objectLock)
                {
                    reference = (WeakReference) LdapConnection.handleTable[ReferralFromConnection];
                    if ((reference != null) && reference.IsAlive)
                    {
                        target = (LdapConnection) reference.Target;
                    }
                    else
                    {
                        if (reference != null)
                        {
                            LdapConnection.handleTable.Remove(ReferralFromConnection);
                        }
                        target = new LdapConnection((LdapDirectoryIdentifier) this.connection.Directory, this.connection.GetCredential(), this.connection.AuthType, ReferralFromConnection);
                        LdapConnection.handleTable.Add(ReferralFromConnection, new WeakReference(target));
                    }
                }
            }
            long currentUserToken = ((long) ((ulong) CurrentUserToken.LowPart)) + (CurrentUserToken.HighPart << 0x20);
            LdapConnection connection2 = this.callbackRoutine.QueryForConnection(this.connection, target, newDistinguishedName, identifier, credential, currentUserToken);
            if (connection2 != null)
            {
                ConnectionToUse = connection2.ldapHandle;
            }
            return 0;
        }

        private NetworkCredential ProcessSecAuthIdentity(SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentit)
        {
            if (SecAuthIdentit == null)
            {
                return new NetworkCredential();
            }
            string user = SecAuthIdentit.user;
            string domain = SecAuthIdentit.domain;
            return new NetworkCredential(user, SecAuthIdentit.password, domain);
        }

        private bool ProcessServerCertificate(IntPtr Connection, IntPtr pServerCert)
        {
            bool flag = true;
            if (this.serverCertificateDelegate == null)
            {
                return flag;
            }
            IntPtr zero = IntPtr.Zero;
            X509Certificate certificate = null;
            try
            {
                zero = Marshal.ReadIntPtr(pServerCert);
                certificate = new X509Certificate(zero);
            }
            finally
            {
                Wldap32.CertFreeCRLContext(zero);
            }
            return this.serverCertificateDelegate(this.connection, certificate);
        }

        private void SetIntValueHelper(LdapOption option, int value)
        {
            if (this.connection.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            int inValue = value;
            ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_set_option_int(this.connection.ldapHandle, option, ref inValue));
        }

        private void SetStringValueHelper(LdapOption option, string value)
        {
            if (this.connection.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            IntPtr inValue = new IntPtr(0);
            if (value != null)
            {
                inValue = Marshal.StringToHGlobalUni(value);
            }
            try
            {
                ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_set_option_ptr(this.connection.ldapHandle, option, ref inValue));
            }
            finally
            {
                if (inValue != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(inValue);
                }
            }
        }

        public unsafe void StartTransportLayerSecurity(DirectoryControlCollection controls)
        {
            IntPtr zero = IntPtr.Zero;
            LdapControl[] controlArray = null;
            IntPtr clientControls = IntPtr.Zero;
            LdapControl[] controlArray2 = null;
            IntPtr message = IntPtr.Zero;
            IntPtr referral = IntPtr.Zero;
            int serverReturnValue = 0;
            Uri[] uriArray = null;
            if (Utility.IsWin2kOS)
            {
                throw new PlatformNotSupportedException(Res.GetString("TLSNotSupported"));
            }
            if (this.connection.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            try
            {
                IntPtr ptr = IntPtr.Zero;
                IntPtr ptr6 = IntPtr.Zero;
                controlArray = this.connection.BuildControlArray(controls, true);
                int cb = Marshal.SizeOf(typeof(LdapControl));
                if (controlArray != null)
                {
                    zero = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * (controlArray.Length + 1)));
                    for (int i = 0; i < controlArray.Length; i++)
                    {
                        ptr = Marshal.AllocHGlobal(cb);
                        Marshal.StructureToPtr(controlArray[i], ptr, false);
                        ptr6 = (IntPtr) (((long) zero) + (Marshal.SizeOf(typeof(IntPtr)) * i));
                        Marshal.WriteIntPtr(ptr6, ptr);
                    }
                    ptr6 = (IntPtr) (((long) zero) + (Marshal.SizeOf(typeof(IntPtr)) * controlArray.Length));
                    Marshal.WriteIntPtr(ptr6, IntPtr.Zero);
                }
                controlArray2 = this.connection.BuildControlArray(controls, false);
                if (controlArray2 != null)
                {
                    clientControls = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(IntPtr)) * (controlArray2.Length + 1)));
                    for (int j = 0; j < controlArray2.Length; j++)
                    {
                        ptr = Marshal.AllocHGlobal(cb);
                        Marshal.StructureToPtr(controlArray2[j], ptr, false);
                        ptr6 = (IntPtr) (((long) clientControls) + (Marshal.SizeOf(typeof(IntPtr)) * j));
                        Marshal.WriteIntPtr(ptr6, ptr);
                    }
                    ptr6 = (IntPtr) (((long) clientControls) + (Marshal.SizeOf(typeof(IntPtr)) * controlArray2.Length));
                    Marshal.WriteIntPtr(ptr6, IntPtr.Zero);
                }
                int errorCode = Wldap32.ldap_start_tls(this.connection.ldapHandle, ref serverReturnValue, ref message, zero, clientControls);
                if (((message != IntPtr.Zero) && (Wldap32.ldap_parse_result_referral(this.connection.ldapHandle, message, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref referral, IntPtr.Zero, 0) == 0)) && (referral != IntPtr.Zero))
                {
                    char** chPtr = (char**) referral;
                    char* chPtr2 = chPtr[0];
                    int index = 0;
                    ArrayList list = new ArrayList();
                    while (chPtr2 != null)
                    {
                        string str = Marshal.PtrToStringUni((IntPtr) chPtr2);
                        list.Add(str);
                        index++;
                        chPtr2 = chPtr[index];
                    }
                    if (referral != IntPtr.Zero)
                    {
                        Wldap32.ldap_value_free(referral);
                        referral = IntPtr.Zero;
                    }
                    if (list.Count > 0)
                    {
                        uriArray = new Uri[list.Count];
                        for (int k = 0; k < list.Count; k++)
                        {
                            uriArray[k] = new Uri((string) list[k]);
                        }
                    }
                }
                if (errorCode != 0)
                {
                    string str2 = Res.GetString("DefaultLdapError");
                    if (Utility.IsResultCode((ResultCode) errorCode))
                    {
                        if (errorCode == 80)
                        {
                            errorCode = serverReturnValue;
                        }
                        str2 = OperationErrorMappings.MapResultCode(errorCode);
                        ExtendedResponse response = new ExtendedResponse(null, null, (ResultCode) errorCode, str2, uriArray) {
                            name = "1.3.6.1.4.1.1466.20037"
                        };
                        throw new TlsOperationException(response);
                    }
                    if (Utility.IsLdapError((LdapError) errorCode))
                    {
                        str2 = LdapErrorMappings.MapResultCode(errorCode);
                        throw new LdapException(errorCode, str2);
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    for (int m = 0; m < controlArray.Length; m++)
                    {
                        IntPtr hglobal = Marshal.ReadIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * m);
                        if (hglobal != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(hglobal);
                        }
                    }
                    Marshal.FreeHGlobal(zero);
                }
                if (controlArray != null)
                {
                    for (int n = 0; n < controlArray.Length; n++)
                    {
                        if (controlArray[n].ldctl_oid != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(controlArray[n].ldctl_oid);
                        }
                        if ((controlArray[n].ldctl_value != null) && (controlArray[n].ldctl_value.bv_val != IntPtr.Zero))
                        {
                            Marshal.FreeHGlobal(controlArray[n].ldctl_value.bv_val);
                        }
                    }
                }
                if (clientControls != IntPtr.Zero)
                {
                    for (int num11 = 0; num11 < controlArray2.Length; num11++)
                    {
                        IntPtr ptr8 = Marshal.ReadIntPtr(clientControls, Marshal.SizeOf(typeof(IntPtr)) * num11);
                        if (ptr8 != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr8);
                        }
                    }
                    Marshal.FreeHGlobal(clientControls);
                }
                if (controlArray2 != null)
                {
                    for (int num12 = 0; num12 < controlArray2.Length; num12++)
                    {
                        if (controlArray2[num12].ldctl_oid != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(controlArray2[num12].ldctl_oid);
                        }
                        if ((controlArray2[num12].ldctl_value != null) && (controlArray2[num12].ldctl_value.bv_val != IntPtr.Zero))
                        {
                            Marshal.FreeHGlobal(controlArray2[num12].ldctl_value.bv_val);
                        }
                    }
                }
                if (referral != IntPtr.Zero)
                {
                    Wldap32.ldap_value_free(referral);
                }
            }
        }

        public void StopTransportLayerSecurity()
        {
            if (Utility.IsWin2kOS)
            {
                throw new PlatformNotSupportedException(Res.GetString("TLSNotSupported"));
            }
            if (this.connection.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (Wldap32.ldap_stop_tls(this.connection.ldapHandle) == 0)
            {
                throw new TlsOperationException(null, Res.GetString("TLSStopFailure"));
            }
        }

        public bool AutoReconnect
        {
            get
            {
                return (this.GetIntValueHelper(LdapOption.LDAP_OPT_AUTO_RECONNECT) == 1);
            }
            set
            {
                int num;
                if (value)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_AUTO_RECONNECT, num);
            }
        }

        internal DereferenceAlias DerefAlias
        {
            get
            {
                return (DereferenceAlias) this.GetIntValueHelper(LdapOption.LDAP_OPT_DEREF);
            }
            set
            {
                this.SetIntValueHelper(LdapOption.LDAP_OPT_DEREF, (int) value);
            }
        }

        public string DomainName
        {
            get
            {
                return this.GetStringValueHelper(LdapOption.LDAP_OPT_DNSDOMAIN_NAME, true);
            }
            set
            {
                this.SetStringValueHelper(LdapOption.LDAP_OPT_DNSDOMAIN_NAME, value);
            }
        }

        internal bool FQDN
        {
            set
            {
                this.SetIntValueHelper(LdapOption.LDAP_OPT_AREC_EXCLUSIVE, 1);
            }
        }

        public string HostName
        {
            get
            {
                return this.GetStringValueHelper(LdapOption.LDAP_OPT_HOST_NAME, false);
            }
            set
            {
                this.SetStringValueHelper(LdapOption.LDAP_OPT_HOST_NAME, value);
            }
        }

        public bool HostReachable
        {
            get
            {
                return (this.GetIntValueHelper(LdapOption.LDAP_OPT_HOST_REACHABLE) == 1);
            }
        }

        public LocatorFlags LocatorFlag
        {
            get
            {
                return (LocatorFlags) this.GetIntValueHelper(LdapOption.LDAP_OPT_GETDSNAME_FLAGS);
            }
            set
            {
                this.SetIntValueHelper(LdapOption.LDAP_OPT_GETDSNAME_FLAGS, (int) value);
            }
        }

        public TimeSpan PingKeepAliveTimeout
        {
            get
            {
                return new TimeSpan(this.GetIntValueHelper(LdapOption.LDAP_OPT_PING_KEEP_ALIVE) * 0x989680L);
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
                }
                if (value.TotalSeconds > 2147483647.0)
                {
                    throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
                }
                int num = (int) (value.Ticks / 0x989680L);
                this.SetIntValueHelper(LdapOption.LDAP_OPT_PING_KEEP_ALIVE, num);
            }
        }

        public int PingLimit
        {
            get
            {
                return this.GetIntValueHelper(LdapOption.LDAP_OPT_PING_LIMIT);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_PING_LIMIT, value);
            }
        }

        public TimeSpan PingWaitTimeout
        {
            get
            {
                return new TimeSpan(this.GetIntValueHelper(LdapOption.LDAP_OPT_PING_WAIT_TIME) * 0x2710L);
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
                }
                if (value.TotalMilliseconds > 2147483647.0)
                {
                    throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
                }
                int num = (int) (value.Ticks / 0x2710L);
                this.SetIntValueHelper(LdapOption.LDAP_OPT_PING_WAIT_TIME, num);
            }
        }

        public int ProtocolVersion
        {
            get
            {
                return this.GetIntValueHelper(LdapOption.LDAP_OPT_VERSION);
            }
            set
            {
                this.SetIntValueHelper(LdapOption.LDAP_OPT_VERSION, value);
            }
        }

        public QueryClientCertificateCallback QueryClientCertificate
        {
            get
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.clientCertificateDelegate;
            }
            set
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (value != null)
                {
                    int errorCode = Wldap32.ldap_set_option_clientcert(this.connection.ldapHandle, LdapOption.LDAP_OPT_CLIENT_CERTIFICATE, this.connection.clientCertificateRoutine);
                    if (errorCode != 0)
                    {
                        if (Utility.IsLdapError((LdapError) errorCode))
                        {
                            string message = LdapErrorMappings.MapResultCode(errorCode);
                            throw new LdapException(errorCode, message);
                        }
                        throw new LdapException(errorCode);
                    }
                    this.connection.automaticBind = false;
                }
                this.clientCertificateDelegate = value;
            }
        }

        public System.DirectoryServices.Protocols.ReferralCallback ReferralCallback
        {
            get
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.callbackRoutine;
            }
            set
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                System.DirectoryServices.Protocols.ReferralCallback tempCallback = new System.DirectoryServices.Protocols.ReferralCallback();
                if (value != null)
                {
                    tempCallback.QueryForConnection = value.QueryForConnection;
                    tempCallback.NotifyNewConnection = value.NotifyNewConnection;
                    tempCallback.DereferenceConnection = value.DereferenceConnection;
                }
                else
                {
                    tempCallback.QueryForConnection = null;
                    tempCallback.NotifyNewConnection = null;
                    tempCallback.DereferenceConnection = null;
                }
                this.ProcessCallBackRoutine(tempCallback);
                this.callbackRoutine = value;
            }
        }

        public ReferralChasingOptions ReferralChasing
        {
            get
            {
                int intValueHelper = this.GetIntValueHelper(LdapOption.LDAP_OPT_REFERRALS);
                if (intValueHelper == 1)
                {
                    return ReferralChasingOptions.All;
                }
                return (ReferralChasingOptions) intValueHelper;
            }
            set
            {
                if ((value & ~ReferralChasingOptions.All) != ReferralChasingOptions.None)
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ReferralChasingOptions));
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_REFERRALS, (int) value);
            }
        }

        public int ReferralHopLimit
        {
            get
            {
                return this.GetIntValueHelper(LdapOption.LDAP_OPT_REFERRAL_HOP_LIMIT);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("ValidValue"), "value");
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_REFERRAL_HOP_LIMIT, value);
            }
        }

        public bool RootDseCache
        {
            get
            {
                return (this.GetIntValueHelper(LdapOption.LDAP_OPT_ROOTDSE_CACHE) == 1);
            }
            set
            {
                int num;
                if (value)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_ROOTDSE_CACHE, num);
            }
        }

        public string SaslMethod
        {
            get
            {
                return this.GetStringValueHelper(LdapOption.LDAP_OPT_SASL_METHOD, true);
            }
            set
            {
                this.SetStringValueHelper(LdapOption.LDAP_OPT_SASL_METHOD, value);
            }
        }

        public bool Sealing
        {
            get
            {
                return (this.GetIntValueHelper(LdapOption.LDAP_OPT_ENCRYPT) == 1);
            }
            set
            {
                int num;
                if (value)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_ENCRYPT, num);
            }
        }

        public bool SecureSocketLayer
        {
            get
            {
                return (this.GetIntValueHelper(LdapOption.LDAP_OPT_SSL) == 1);
            }
            set
            {
                int num;
                if (value)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_SSL, num);
            }
        }

        public object SecurityContext
        {
            get
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                SecurityHandle outValue = new SecurityHandle();
                ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_get_option_sechandle(this.connection.ldapHandle, LdapOption.LDAP_OPT_SECURITY_CONTEXT, ref outValue));
                return outValue;
            }
        }

        public TimeSpan SendTimeout
        {
            get
            {
                return new TimeSpan(this.GetIntValueHelper(LdapOption.LDAP_OPT_SEND_TIMEOUT) * 0x989680L);
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
                }
                if (value.TotalSeconds > 2147483647.0)
                {
                    throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
                }
                int num = (int) (value.Ticks / 0x989680L);
                this.SetIntValueHelper(LdapOption.LDAP_OPT_SEND_TIMEOUT, num);
            }
        }

        internal string ServerErrorMessage
        {
            get
            {
                return this.GetStringValueHelper(LdapOption.LDAP_OPT_SERVER_ERROR, true);
            }
        }

        public bool Signing
        {
            get
            {
                return (this.GetIntValueHelper(LdapOption.LDAP_OPT_SIGN) == 1);
            }
            set
            {
                int num;
                if (value)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_SIGN, num);
            }
        }

        public SecurityPackageContextConnectionInformation SslInformation
        {
            get
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                SecurityPackageContextConnectionInformation outValue = new SecurityPackageContextConnectionInformation();
                ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_get_option_secInfo(this.connection.ldapHandle, LdapOption.LDAP_OPT_SSL_INFO, outValue));
                return outValue;
            }
        }

        public int SspiFlag
        {
            get
            {
                return this.GetIntValueHelper(LdapOption.LDAP_OPT_SSPI_FLAGS);
            }
            set
            {
                this.SetIntValueHelper(LdapOption.LDAP_OPT_SSPI_FLAGS, value);
            }
        }

        public bool TcpKeepAlive
        {
            get
            {
                return (this.GetIntValueHelper(LdapOption.LDAP_OPT_TCP_KEEPALIVE) == 1);
            }
            set
            {
                int num;
                if (value)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
                this.SetIntValueHelper(LdapOption.LDAP_OPT_TCP_KEEPALIVE, num);
            }
        }

        public VerifyServerCertificateCallback VerifyServerCertificate
        {
            get
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.serverCertificateDelegate;
            }
            set
            {
                if (this.connection.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (value != null)
                {
                    ErrorChecking.CheckAndSetLdapError(Wldap32.ldap_set_option_servercert(this.connection.ldapHandle, LdapOption.LDAP_OPT_SERVER_CERTIFICATE, this.serverCertificateRoutine));
                }
                this.serverCertificateDelegate = value;
            }
        }
    }
}

