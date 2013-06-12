namespace System.Runtime.Remoting
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Lifetime;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Cryptography;
    using System.Threading;

    internal class Identity
    {
        internal object _channelSink;
        internal DynamicPropertyHolder _dph;
        internal object _envoyChain;
        internal int _flags;
        internal System.Runtime.Remoting.Lifetime.Lease _lease;
        internal object _objRef;
        protected string _ObjURI;
        internal object _tpOrObject;
        protected string _URL;
        protected const int IDFLG_CONTEXT_BOUND = 0x10;
        protected const int IDFLG_DISCONNECTED_FULL = 1;
        protected const int IDFLG_DISCONNECTED_REM = 2;
        protected const int IDFLG_IN_IDTABLE = 4;
        protected const int IDFLG_SERVER_SINGLECALL = 0x200;
        protected const int IDFLG_SERVER_SINGLETON = 0x400;
        protected const int IDFLG_WELLKNOWN = 0x100;
        private static string s_configuredAppDomainGuid = null;
        private static string s_configuredAppDomainGuidString = null;
        private static string s_IDGuidString = ("/" + s_originalAppDomainGuid.ToLower(CultureInfo.InvariantCulture) + "/");
        private static string s_originalAppDomainGuid = Guid.NewGuid().ToString().Replace('-', '_');
        private static string s_originalAppDomainGuidString = ("/" + s_originalAppDomainGuid.ToLower(CultureInfo.InvariantCulture) + "/");
        private static RNGCryptoServiceProvider s_rng = new RNGCryptoServiceProvider();

        internal Identity(bool bContextBound)
        {
            if (bContextBound)
            {
                this._flags |= 0x10;
            }
        }

        internal Identity(string objURI, string URL)
        {
            if (URL != null)
            {
                this._flags |= 0x100;
                this._URL = URL;
            }
            this.SetOrCreateURI(objURI, true);
        }

        [SecurityCritical]
        internal bool AddProxySideDynamicProperty(IDynamicProperty prop)
        {
            lock (this)
            {
                if (this._dph == null)
                {
                    DynamicPropertyHolder holder = new DynamicPropertyHolder();
                    lock (this)
                    {
                        if (this._dph == null)
                        {
                            this._dph = holder;
                        }
                    }
                }
                return this._dph.AddDynamicProperty(prop);
            }
        }

        [Conditional("_DEBUG"), SecurityCritical]
        internal virtual void AssertValid()
        {
            if (this.URI != null)
            {
                IdentityHolder.ResolveIdentity(this.URI);
            }
        }

        internal static string GetNewLogicalCallID()
        {
            return (IDGuidString + GetNextSeqNum());
        }

        private static int GetNextSeqNum()
        {
            return SharedStatics.Remoting_Identity_GetNextSeqNum();
        }

        private static byte[] GetRandomBytes()
        {
            byte[] data = new byte[0x12];
            s_rng.GetBytes(data);
            return data;
        }

        internal bool IsDisconnected()
        {
            if (!this.IsFullyDisconnected())
            {
                return this.IsRemoteDisconnected();
            }
            return true;
        }

        internal bool IsFullyDisconnected()
        {
            return ((this._flags & 1) == 1);
        }

        internal bool IsInIDTable()
        {
            return ((this._flags & 4) == 4);
        }

        internal bool IsRemoteDisconnected()
        {
            return ((this._flags & 2) == 2);
        }

        internal bool IsWellKnown()
        {
            return ((this._flags & 0x100) == 0x100);
        }

        internal IMessageSink RaceSetChannelSink(IMessageSink channelSink)
        {
            if (this._channelSink == null)
            {
                Interlocked.CompareExchange(ref this._channelSink, channelSink, null);
            }
            return (IMessageSink) this._channelSink;
        }

        internal IMessageSink RaceSetEnvoyChain(IMessageSink envoyChain)
        {
            if (this._envoyChain == null)
            {
                Interlocked.CompareExchange(ref this._envoyChain, envoyChain, null);
            }
            return (IMessageSink) this._envoyChain;
        }

        [SecurityCritical]
        internal ObjRef RaceSetObjRef(ObjRef objRefGiven)
        {
            if (this._objRef == null)
            {
                Interlocked.CompareExchange(ref this._objRef, objRefGiven, null);
            }
            return (ObjRef) this._objRef;
        }

        internal object RaceSetTransparentProxy(object tpObj)
        {
            if (this._tpOrObject == null)
            {
                Interlocked.CompareExchange(ref this._tpOrObject, tpObj, null);
            }
            return this._tpOrObject;
        }

        internal static string RemoveAppNameOrAppGuidIfNecessary(string uri)
        {
            if (((uri != null) && (uri.Length > 1)) && (uri[0] == '/'))
            {
                string str;
                if (s_configuredAppDomainGuidString != null)
                {
                    str = s_configuredAppDomainGuidString;
                    if ((uri.Length > str.Length) && StringStartsWith(uri, str))
                    {
                        return uri.Substring(str.Length);
                    }
                }
                str = s_originalAppDomainGuidString;
                if ((uri.Length > str.Length) && StringStartsWith(uri, str))
                {
                    return uri.Substring(str.Length);
                }
                string applicationName = RemotingConfiguration.ApplicationName;
                if (((applicationName != null) && (uri.Length > (applicationName.Length + 2))) && ((string.Compare(uri, 1, applicationName, 0, applicationName.Length, true, CultureInfo.InvariantCulture) == 0) && (uri[applicationName.Length + 1] == '/')))
                {
                    return uri.Substring(applicationName.Length + 2);
                }
                uri = uri.Substring(1);
            }
            return uri;
        }

        [SecurityCritical]
        internal bool RemoveProxySideDynamicProperty(string name)
        {
            lock (this)
            {
                if (this._dph == null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Contexts_NoProperty"), new object[] { name }));
                }
                return this._dph.RemoveDynamicProperty(name);
            }
        }

        [SecurityCritical]
        internal void ResetInIDTable(bool bResetURI)
        {
            int num;
            int num2;
            do
            {
                num = this._flags;
                num2 = this._flags & -5;
            }
            while (num != Interlocked.CompareExchange(ref this._flags, num2, num));
            if (bResetURI)
            {
                ((ObjRef) this._objRef).URI = null;
                this._ObjURI = null;
            }
        }

        internal void SetFullyConnected()
        {
            int num;
            int num2;
            do
            {
                num = this._flags;
                num2 = this._flags & -4;
            }
            while (num != Interlocked.CompareExchange(ref this._flags, num2, num));
        }

        internal void SetInIDTable()
        {
            int num;
            int num2;
            do
            {
                num = this._flags;
                num2 = this._flags | 4;
            }
            while (num != Interlocked.CompareExchange(ref this._flags, num2, num));
        }

        internal void SetOrCreateURI(string uri)
        {
            this.SetOrCreateURI(uri, false);
        }

        internal void SetOrCreateURI(string uri, bool bIdCtor)
        {
            if (!bIdCtor && (this._ObjURI != null))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_SetObjectUriForMarshal__UriExists"));
            }
            if (uri == null)
            {
                string str = Convert.ToBase64String(GetRandomBytes());
                this._ObjURI = (IDGuidString + str.Replace('/', '_') + "_" + GetNextSeqNum().ToString(CultureInfo.InvariantCulture.NumberFormat) + ".rem").ToLower(CultureInfo.InvariantCulture);
            }
            else if (this is ServerIdentity)
            {
                this._ObjURI = IDGuidString + uri;
            }
            else
            {
                this._ObjURI = uri;
            }
        }

        private static bool StringStartsWith(string s1, string prefix)
        {
            if (s1.Length < prefix.Length)
            {
                return false;
            }
            return (string.CompareOrdinal(s1, 0, prefix, 0, prefix.Length) == 0);
        }

        internal static string AppDomainUniqueId
        {
            get
            {
                if (s_configuredAppDomainGuid != null)
                {
                    return s_configuredAppDomainGuid;
                }
                return s_originalAppDomainGuid;
            }
        }

        internal IMessageSink ChannelSink
        {
            get
            {
                return (IMessageSink) this._channelSink;
            }
        }

        internal IMessageSink EnvoyChain
        {
            get
            {
                return (IMessageSink) this._envoyChain;
            }
        }

        internal static string IDGuidString
        {
            get
            {
                return s_IDGuidString;
            }
        }

        internal bool IsContextBound
        {
            get
            {
                return ((this._flags & 0x10) == 0x10);
            }
        }

        internal System.Runtime.Remoting.Lifetime.Lease Lease
        {
            get
            {
                return this._lease;
            }
            set
            {
                this._lease = value;
            }
        }

        internal ObjRef ObjectRef
        {
            [SecurityCritical]
            get
            {
                return (ObjRef) this._objRef;
            }
        }

        internal string ObjURI
        {
            get
            {
                return this._ObjURI;
            }
        }

        internal static string ProcessGuid
        {
            get
            {
                return ProcessIDGuid;
            }
        }

        internal static string ProcessIDGuid
        {
            get
            {
                return SharedStatics.Remoting_Identity_IDGuid;
            }
        }

        internal ArrayWithSize ProxySideDynamicSinks
        {
            [SecurityCritical]
            get
            {
                if (this._dph == null)
                {
                    return null;
                }
                return this._dph.DynamicSinks;
            }
        }

        internal MarshalByRefObject TPOrObject
        {
            get
            {
                return (MarshalByRefObject) this._tpOrObject;
            }
        }

        internal string URI
        {
            get
            {
                if (this.IsWellKnown())
                {
                    return this._URL;
                }
                return this._ObjURI;
            }
        }
    }
}

