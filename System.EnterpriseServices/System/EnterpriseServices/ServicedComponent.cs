namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Thunk;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;

    [Serializable, ServicedComponentProxy]
    public abstract class ServicedComponent : ContextBoundObject, IRemoteDispatch, IDisposable, IManagedObject, IServicedComponentInfo
    {
        private bool _calledDispose;
        private bool _denyRemoteDispatch;
        private MethodInfo _finalize;
        private static RWHashTableEx _finalizeCache = new RWHashTableEx();
        private static Type _typeofSC = typeof(ServicedComponent);
        private const BindingFlags bfLookupAll = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        private const string c_strFieldGetterName = "FieldGetter";
        private const string c_strFieldSetterName = "FieldSetter";
        private const string c_strIsInstanceOfTypeName = "IsInstanceOfType";
        private static MethodBase s_mbFieldGetter;
        private static MethodBase s_mbFieldSetter;
        private static MethodBase s_mbIsInstanceOfType;

        public ServicedComponent()
        {
            (RemotingServices.GetRealProxy(this) as ServicedComponentProxy).SuppressFinalizeServer();
            Type t = base.GetType();
            this._denyRemoteDispatch = ServicedComponentInfo.AreMethodsSecure(t);
            bool bFound = false;
            this._finalize = _finalizeCache.Get(t, out bFound) as MethodInfo;
            if (!bFound)
            {
                this._finalize = GetDeclaredFinalizer(t);
                _finalizeCache.Put(t, this._finalize);
            }
            this._calledDispose = false;
        }

        internal void _callFinalize(bool disposing)
        {
            if (!this._calledDispose)
            {
                this._calledDispose = true;
                this.Dispose(disposing);
            }
            if (this._finalize != null)
            {
                this._finalize.Invoke(this, new object[0]);
            }
        }

        internal void _internalDeactivate(bool disposing)
        {
            ComponentServices.DeactivateObject(this, disposing);
        }

        protected internal virtual void Activate()
        {
        }

        protected internal virtual bool CanBePooled()
        {
            return false;
        }

        private void CheckMethodAccess(IMessage request)
        {
            MethodBase mi = null;
            MethodBase m = null;
            IMethodMessage message = request as IMethodMessage;
            if (message == null)
            {
                throw new UnauthorizedAccessException();
            }
            mi = message.MethodBase;
            m = ReflectionCache.ConvertToClassMI(base.GetType(), mi) as MethodBase;
            if (m == null)
            {
                throw new UnauthorizedAccessException();
            }
            if (ServicedComponentInfo.HasSpecialMethodAttributes(m))
            {
                throw new UnauthorizedAccessException(Resource.FormatString("ServicedComponentException_SecurityMapping"));
            }
            if ((!mi.IsPublic || mi.IsStatic) && !IsMethodAllowedRemotely(mi))
            {
                throw new UnauthorizedAccessException(Resource.FormatString("ServicedComponentException_SecurityNoPrivateAccess"));
            }
            Type declaringType = mi.DeclaringType;
            if (!declaringType.IsPublic && !declaringType.IsNestedPublic)
            {
                throw new UnauthorizedAccessException(Resource.FormatString("ServicedComponentException_SecurityNoPrivateAccess"));
            }
            for (declaringType = mi.DeclaringType.DeclaringType; declaringType != null; declaringType = declaringType.DeclaringType)
            {
                if (!declaringType.IsPublic && !declaringType.IsNestedPublic)
                {
                    throw new UnauthorizedAccessException(Resource.FormatString("ServicedComponentException_SecurityNoPrivateAccess"));
                }
            }
        }

        protected internal virtual void Construct(string s)
        {
        }

        protected internal virtual void Deactivate()
        {
        }

        public void Dispose()
        {
            DisposeObject(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public static void DisposeObject(ServicedComponent sc)
        {
            RealProxy realProxy = RemotingServices.GetRealProxy(sc);
            if (realProxy is ServicedComponentProxy)
            {
                ServicedComponentProxy proxy2 = (ServicedComponentProxy) realProxy;
                RemotingServices.Disconnect(sc);
                proxy2.Dispose(true);
            }
            else if (realProxy is RemoteServicedComponentProxy)
            {
                RemoteServicedComponentProxy proxy3 = (RemoteServicedComponentProxy) realProxy;
                sc.Dispose();
                proxy3.Dispose(true);
            }
            else
            {
                sc.Dispose();
            }
        }

        internal void DoSetCOMIUnknown(IntPtr pUnk)
        {
            RemotingServices.GetRealProxy(this).SetCOMIUnknown(pUnk);
        }

        private static MethodInfo GetDeclaredFinalizer(Type t)
        {
            MethodInfo method = null;
            while (t != _typeofSC)
            {
                method = t.GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (method != null)
                {
                    break;
                }
                t = t.BaseType;
            }
            bool flag1 = method != null;
            return method;
        }

        internal static bool IsMethodAllowedRemotely(MethodBase method)
        {
            if (s_mbFieldGetter == null)
            {
                s_mbFieldGetter = typeof(object).GetMethod("FieldGetter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            }
            if (s_mbFieldSetter == null)
            {
                s_mbFieldSetter = typeof(object).GetMethod("FieldSetter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            }
            if (s_mbIsInstanceOfType == null)
            {
                s_mbIsInstanceOfType = typeof(MarshalByRefObject).GetMethod("IsInstanceOfType", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            }
            if (!(method == s_mbFieldGetter) && !(method == s_mbFieldSetter))
            {
                return (method == s_mbIsInstanceOfType);
            }
            return true;
        }

        private string RemoteDispatchHelper(string s, out bool failed)
        {
            if (this._denyRemoteDispatch)
            {
                throw new UnauthorizedAccessException(Resource.FormatString("ServicedComponentException_SecurityMapping"));
            }
            IMessage request = ComponentServices.ConvertToMessage(s, this);
            this.CheckMethodAccess(request);
            IMessage reqMsg = RemotingServices.GetRealProxy(this).Invoke(request);
            IMethodReturnMessage message3 = reqMsg as IMethodReturnMessage;
            if ((message3 != null) && (message3.Exception != null))
            {
                failed = true;
            }
            else
            {
                failed = false;
            }
            return ComponentServices.ConvertToString(reqMsg);
        }

        void IManagedObject.GetObjectIdentity(ref string s, ref int AppDomainID, ref int ccw)
        {
            throw new NotSupportedException(Resource.GetString("Err_IManagedObjectGetObjectIdentity"));
        }

        void IManagedObject.GetSerializedBuffer(ref string s)
        {
            throw new NotSupportedException(Resource.GetString("Err_IManagedObjectGetSerializedBuffer"));
        }

        [AutoComplete(true)]
        string IRemoteDispatch.RemoteDispatchAutoDone(string s)
        {
            bool failed = false;
            string str = this.RemoteDispatchHelper(s, out failed);
            if (failed)
            {
                ContextUtil.SetAbort();
            }
            return str;
        }

        [AutoComplete(false)]
        string IRemoteDispatch.RemoteDispatchNotAutoDone(string s)
        {
            bool failed = false;
            return this.RemoteDispatchHelper(s, out failed);
        }

        void IServicedComponentInfo.GetComponentInfo(ref int infoMask, out string[] infoArray)
        {
            int num = 0;
            ArrayList list = new ArrayList();
            if ((infoMask & Proxy.INFO_PROCESSID) != 0)
            {
                list.Add(RemotingConfiguration.ProcessId);
                num |= Proxy.INFO_PROCESSID;
            }
            if ((infoMask & Proxy.INFO_APPDOMAINID) != 0)
            {
                list.Add(RemotingConfiguration.ApplicationId);
                num |= Proxy.INFO_APPDOMAINID;
            }
            if ((infoMask & Proxy.INFO_URI) != 0)
            {
                string objectUri = RemotingServices.GetObjectUri(this);
                if (objectUri == null)
                {
                    RemotingServices.Marshal(this);
                    objectUri = RemotingServices.GetObjectUri(this);
                }
                list.Add(objectUri);
                num |= Proxy.INFO_URI;
            }
            infoArray = (string[]) list.ToArray(typeof(string));
            infoMask = num;
        }
    }
}

