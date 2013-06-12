namespace System.Runtime.Remoting
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable, ComVisible(true), SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class ObjRef : IObjectReference, ISerializable
    {
        internal IChannelInfo channelInfo;
        internal int domainID;
        internal IEnvoyInfo envoyInfo;
        internal const int FLG_LITE_OBJREF = 4;
        internal const int FLG_MARSHALED_OBJECT = 1;
        internal const int FLG_PROXY_ATTRIBUTE = 8;
        internal const int FLG_WELLKNOWN_OBJREF = 2;
        internal int objrefFlags;
        private static Type orType = typeof(ObjRef);
        internal GCHandle srvIdentity;
        internal IRemotingTypeInfo typeInfo;
        internal string uri;

        public ObjRef()
        {
            this.objrefFlags = 0;
        }

        [SecurityCritical]
        private ObjRef(ObjRef o)
        {
            this.uri = o.uri;
            this.typeInfo = o.typeInfo;
            this.envoyInfo = o.envoyInfo;
            this.channelInfo = o.channelInfo;
            this.objrefFlags = o.objrefFlags;
            this.SetServerIdentity(o.GetServerIdentity());
            this.SetDomainID(o.GetDomainID());
        }

        [SecurityCritical]
        public ObjRef(MarshalByRefObject o, Type requestedType)
        {
            bool flag;
            if (o == null)
            {
                throw new ArgumentNullException("o");
            }
            RuntimeType type = requestedType as RuntimeType;
            if ((requestedType != null) && (type == null))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            Identity idObj = MarshalByRefObject.GetIdentity(o, out flag);
            this.Init(o, idObj, type);
        }

        [SecurityCritical]
        protected ObjRef(SerializationInfo info, StreamingContext context)
        {
            string str = null;
            bool flag = false;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name.Equals("uri"))
                {
                    this.uri = (string) enumerator.Value;
                }
                else
                {
                    if (enumerator.Name.Equals("typeInfo"))
                    {
                        this.typeInfo = (IRemotingTypeInfo) enumerator.Value;
                        continue;
                    }
                    if (enumerator.Name.Equals("envoyInfo"))
                    {
                        this.envoyInfo = (IEnvoyInfo) enumerator.Value;
                        continue;
                    }
                    if (enumerator.Name.Equals("channelInfo"))
                    {
                        this.channelInfo = (IChannelInfo) enumerator.Value;
                        continue;
                    }
                    if (enumerator.Name.Equals("objrefFlags"))
                    {
                        object obj2 = enumerator.Value;
                        if (obj2.GetType() == typeof(string))
                        {
                            this.objrefFlags = ((IConvertible) obj2).ToInt32(null);
                        }
                        else
                        {
                            this.objrefFlags = (int) obj2;
                        }
                        continue;
                    }
                    if (enumerator.Name.Equals("fIsMarshalled"))
                    {
                        int num;
                        object obj3 = enumerator.Value;
                        if (obj3.GetType() == typeof(string))
                        {
                            num = ((IConvertible) obj3).ToInt32(null);
                        }
                        else
                        {
                            num = (int) obj3;
                        }
                        if (num == 0)
                        {
                            flag = true;
                        }
                        continue;
                    }
                    if (enumerator.Name.Equals("url"))
                    {
                        str = (string) enumerator.Value;
                    }
                    else
                    {
                        if (enumerator.Name.Equals("SrvIdentity"))
                        {
                            this.SetServerIdentity((GCHandle) enumerator.Value);
                            continue;
                        }
                        if (enumerator.Name.Equals("DomainId"))
                        {
                            this.SetDomainID((int) enumerator.Value);
                        }
                    }
                }
            }
            if (!flag)
            {
                this.objrefFlags |= 1;
            }
            else
            {
                this.objrefFlags &= -2;
            }
            if (str != null)
            {
                this.uri = str;
                this.objrefFlags |= 4;
            }
        }

        [SecurityCritical]
        internal bool CanSmuggle()
        {
            if ((base.GetType() != typeof(ObjRef)) || this.IsObjRefLite())
            {
                return false;
            }
            Type type = null;
            if (this.typeInfo != null)
            {
                type = this.typeInfo.GetType();
            }
            Type type2 = null;
            if (this.channelInfo != null)
            {
                type2 = this.channelInfo.GetType();
            }
            if ((((type != null) && !(type == typeof(System.Runtime.Remoting.TypeInfo))) && !(type == typeof(DynamicTypeInfo))) || ((this.envoyInfo != null) || ((type2 != null) && !(type2 == typeof(System.Runtime.Remoting.ChannelInfo)))))
            {
                return false;
            }
            if (this.channelInfo != null)
            {
                foreach (object obj2 in this.channelInfo.ChannelData)
                {
                    if (!(obj2 is CrossAppDomainData))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [SecurityCritical]
        internal ObjRef CreateSmuggleableCopy()
        {
            return new ObjRef(this);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private CrossAppDomainData GetAppDomainChannelData()
        {
            int index = 0;
            CrossAppDomainData data = null;
            while (index < this.ChannelInfo.ChannelData.Length)
            {
                data = this.ChannelInfo.ChannelData[index] as CrossAppDomainData;
                if (data != null)
                {
                    return data;
                }
                index++;
            }
            return null;
        }

        [SecurityCritical]
        private IChannelInfo GetChannelInfoHelper()
        {
            System.Runtime.Remoting.ChannelInfo channelInfo = this.channelInfo as System.Runtime.Remoting.ChannelInfo;
            if (channelInfo == null)
            {
                return this.channelInfo;
            }
            object[] channelData = channelInfo.ChannelData;
            if (channelData == null)
            {
                return channelInfo;
            }
            string[] data = (string[]) CallContext.GetData("__bashChannelUrl");
            if (data == null)
            {
                return channelInfo;
            }
            string str = data[0];
            string str2 = data[1];
            System.Runtime.Remoting.ChannelInfo info2 = new System.Runtime.Remoting.ChannelInfo {
                ChannelData = new object[channelData.Length]
            };
            for (int i = 0; i < channelData.Length; i++)
            {
                info2.ChannelData[i] = channelData[i];
                ChannelDataStore store = info2.ChannelData[i] as ChannelDataStore;
                if (store != null)
                {
                    string[] channelUris = store.ChannelUris;
                    if (((channelUris != null) && (channelUris.Length == 1)) && channelUris[0].Equals(str))
                    {
                        ChannelDataStore store2 = store.InternalShallowCopy();
                        store2.ChannelUris = new string[] { str2 };
                        info2.ChannelData[i] = store2;
                    }
                }
            }
            return info2;
        }

        [SecurityCritical]
        private object GetCustomMarshaledCOMObject(object ret)
        {
            if (this.TypeInfo is DynamicTypeInfo)
            {
                object typedObjectForIUnknown = null;
                IntPtr nULL = Win32Native.NULL;
                if (!this.IsFromThisProcess() || this.IsFromThisAppDomain())
                {
                    return ret;
                }
                try
                {
                    bool flag;
                    nULL = ((__ComObject) ret).GetIUnknown(out flag);
                    if (!(nULL != Win32Native.NULL) || flag)
                    {
                        return ret;
                    }
                    string typeName = this.TypeInfo.TypeName;
                    string str2 = null;
                    string assemName = null;
                    System.Runtime.Remoting.TypeInfo.ParseTypeAndAssembly(typeName, out str2, out assemName);
                    Assembly assembly = FormatterServices.LoadAssemblyFromStringNoThrow(assemName);
                    if (assembly == null)
                    {
                        throw new RemotingException(Environment.GetResourceString("Serialization_AssemblyNotFound", new object[] { assemName }));
                    }
                    Type t = assembly.GetType(str2, false, false);
                    if ((t != null) && !t.IsVisible)
                    {
                        t = null;
                    }
                    typedObjectForIUnknown = Marshal.GetTypedObjectForIUnknown(nULL, t);
                    if (typedObjectForIUnknown != null)
                    {
                        ret = typedObjectForIUnknown;
                    }
                }
                finally
                {
                    if (nULL != Win32Native.NULL)
                    {
                        Marshal.Release(nULL);
                    }
                }
            }
            return ret;
        }

        internal int GetDomainID()
        {
            return this.domainID;
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.SetType(orType);
            if (!this.IsObjRefLite())
            {
                info.AddValue("uri", this.uri, typeof(string));
                info.AddValue("objrefFlags", this.objrefFlags);
                info.AddValue("typeInfo", this.typeInfo, typeof(IRemotingTypeInfo));
                info.AddValue("envoyInfo", this.envoyInfo, typeof(IEnvoyInfo));
                info.AddValue("channelInfo", this.GetChannelInfoHelper(), typeof(IChannelInfo));
            }
            else
            {
                info.AddValue("url", this.uri, typeof(string));
            }
        }

        [SecurityCritical]
        public virtual object GetRealObject(StreamingContext context)
        {
            return this.GetRealObjectHelper();
        }

        [SecurityCritical]
        internal object GetRealObjectHelper()
        {
            if (!this.IsMarshaledObject())
            {
                return this;
            }
            if (this.IsObjRefLite())
            {
                int index = this.uri.IndexOf(RemotingConfiguration.ApplicationId);
                if (index > 0)
                {
                    this.uri = this.uri.Substring(index - 1);
                }
            }
            bool fRefine = !(base.GetType() == typeof(ObjRef));
            object ret = RemotingServices.Unmarshal(this, fRefine);
            return this.GetCustomMarshaledCOMObject(ret);
        }

        [SecurityCritical]
        internal IntPtr GetServerContext(out int domainId)
        {
            IntPtr zero = IntPtr.Zero;
            domainId = 0;
            if (this.IsFromThisProcess())
            {
                CrossAppDomainData appDomainChannelData = this.GetAppDomainChannelData();
                domainId = appDomainChannelData.DomainID;
                if (AppDomain.IsDomainIdValid(appDomainChannelData.DomainID))
                {
                    zero = appDomainChannelData.ContextID;
                }
            }
            return zero;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal int GetServerDomainId()
        {
            if (!this.IsFromThisProcess())
            {
                return 0;
            }
            return this.GetAppDomainChannelData().DomainID;
        }

        internal GCHandle GetServerIdentity()
        {
            return this.srvIdentity;
        }

        internal bool HasProxyAttribute()
        {
            return ((this.objrefFlags & 8) == 8);
        }

        [SecurityCritical]
        internal void Init(object o, Identity idObj, RuntimeType requestedType)
        {
            this.uri = idObj.URI;
            MarshalByRefObject tPOrObject = idObj.TPOrObject;
            RuntimeType c = null;
            if (!RemotingServices.IsTransparentProxy(tPOrObject))
            {
                c = (RuntimeType) tPOrObject.GetType();
            }
            else
            {
                c = (RuntimeType) RemotingServices.GetRealProxy(tPOrObject).GetProxiedType();
            }
            RuntimeType typeOfObj = (null == requestedType) ? c : requestedType;
            if (((null != requestedType) && !requestedType.IsAssignableFrom(c)) && !typeof(IMessageSink).IsAssignableFrom(c))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_InvalidRequestedType"), new object[] { requestedType.ToString() }));
            }
            if (c.IsCOMObject)
            {
                DynamicTypeInfo info = new DynamicTypeInfo(typeOfObj);
                this.TypeInfo = info;
            }
            else
            {
                RemotingTypeCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(typeOfObj);
                this.TypeInfo = reflectionCachedData.TypeInfo;
            }
            if (!idObj.IsWellKnown())
            {
                this.EnvoyInfo = System.Runtime.Remoting.EnvoyInfo.CreateEnvoyInfo(idObj as ServerIdentity);
                IChannelInfo info2 = new System.Runtime.Remoting.ChannelInfo();
                if (o is AppDomain)
                {
                    object[] channelData = info2.ChannelData;
                    int length = channelData.Length;
                    object[] destinationArray = new object[length];
                    Array.Copy(channelData, destinationArray, length);
                    for (int i = 0; i < length; i++)
                    {
                        if (!(destinationArray[i] is CrossAppDomainData))
                        {
                            destinationArray[i] = null;
                        }
                    }
                    info2.ChannelData = destinationArray;
                }
                this.ChannelInfo = info2;
                if (c.HasProxyAttribute)
                {
                    this.SetHasProxyAttribute();
                }
            }
            else
            {
                this.SetWellKnown();
            }
            if (ShouldUseUrlObjRef())
            {
                if (this.IsWellKnown())
                {
                    this.SetObjRefLite();
                }
                else
                {
                    string str = ChannelServices.FindFirstHttpUrlForObject(this.URI);
                    if (str != null)
                    {
                        this.URI = str;
                        this.SetObjRefLite();
                    }
                }
            }
        }

        [SecurityCritical]
        public bool IsFromThisAppDomain()
        {
            CrossAppDomainData appDomainChannelData = this.GetAppDomainChannelData();
            return ((appDomainChannelData != null) && appDomainChannelData.IsFromThisAppDomain());
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        public bool IsFromThisProcess()
        {
            if (this.IsWellKnown())
            {
                return false;
            }
            CrossAppDomainData appDomainChannelData = this.GetAppDomainChannelData();
            return ((appDomainChannelData != null) && appDomainChannelData.IsFromThisProcess());
        }

        internal bool IsMarshaledObject()
        {
            return ((this.objrefFlags & 1) == 1);
        }

        internal bool IsObjRefLite()
        {
            return ((this.objrefFlags & 4) == 4);
        }

        [SecurityCritical]
        internal static bool IsWellFormed(ObjRef objectRef)
        {
            bool flag = true;
            return ((((objectRef != null) && (objectRef.URI != null)) && ((objectRef.IsWellKnown() || objectRef.IsObjRefLite()) || ((objectRef.GetType() != orType) || (objectRef.ChannelInfo != null)))) && flag);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal bool IsWellKnown()
        {
            return ((this.objrefFlags & 2) == 2);
        }

        internal void SetDomainID(int id)
        {
            this.domainID = id;
        }

        internal void SetHasProxyAttribute()
        {
            this.objrefFlags |= 8;
        }

        internal void SetMarshaledObject()
        {
            this.objrefFlags |= 1;
        }

        internal void SetObjRefLite()
        {
            this.objrefFlags |= 4;
        }

        internal void SetServerIdentity(GCHandle hndSrvIdentity)
        {
            this.srvIdentity = hndSrvIdentity;
        }

        internal void SetWellKnown()
        {
            this.objrefFlags |= 2;
        }

        internal static bool ShouldUseUrlObjRef()
        {
            return RemotingConfigHandler.UrlObjRefMode;
        }

        public virtual IChannelInfo ChannelInfo
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return this.channelInfo;
            }
            set
            {
                this.channelInfo = value;
            }
        }

        public virtual IEnvoyInfo EnvoyInfo
        {
            get
            {
                return this.envoyInfo;
            }
            set
            {
                this.envoyInfo = value;
            }
        }

        public virtual IRemotingTypeInfo TypeInfo
        {
            get
            {
                return this.typeInfo;
            }
            set
            {
                this.typeInfo = value;
            }
        }

        public virtual string URI
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }
    }
}

