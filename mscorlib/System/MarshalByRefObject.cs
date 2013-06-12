namespace System
{
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Lifetime;
    using System.Security;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public abstract class MarshalByRefObject
    {
        private object __identity;

        protected MarshalByRefObject()
        {
        }

        internal ServerIdentity __RaceSetServerIdentity(ServerIdentity id)
        {
            if (this.__identity == null)
            {
                if (!id.IsContextBound)
                {
                    id.RaceSetTransparentProxy(this);
                }
                Interlocked.CompareExchange(ref this.__identity, id, null);
            }
            return (ServerIdentity) this.__identity;
        }

        internal void __ResetServerIdentity()
        {
            this.__identity = null;
        }

        [SecuritySafeCritical]
        internal bool CanCastToXmlType(string xmlTypeName, string xmlTypeNamespace)
        {
            Type interopTypeFromXmlType = SoapServices.GetInteropTypeFromXmlType(xmlTypeName, xmlTypeNamespace);
            if (interopTypeFromXmlType == null)
            {
                string str;
                string str2;
                string str3;
                if (!SoapServices.DecodeXmlNamespaceForClrTypeNamespace(xmlTypeNamespace, out str, out str2))
                {
                    return false;
                }
                if ((str != null) && (str.Length > 0))
                {
                    str3 = str + "." + xmlTypeName;
                }
                else
                {
                    str3 = xmlTypeName;
                }
                try
                {
                    interopTypeFromXmlType = Assembly.Load(str2).GetType(str3, false, false);
                }
                catch
                {
                    return false;
                }
            }
            return ((interopTypeFromXmlType != null) && interopTypeFromXmlType.IsAssignableFrom(base.GetType()));
        }

        [SecuritySafeCritical]
        internal static bool CanCastToXmlTypeHelper(RuntimeType castType, MarshalByRefObject o)
        {
            if (castType == null)
            {
                throw new ArgumentNullException("castType");
            }
            if (!castType.IsInterface && !castType.IsMarshalByRef)
            {
                return false;
            }
            string xmlType = null;
            string xmlTypeNamespace = null;
            if (!SoapServices.GetXmlTypeForInteropType(castType, out xmlType, out xmlTypeNamespace))
            {
                xmlType = castType.Name;
                xmlTypeNamespace = SoapServices.CodeXmlNamespaceForClrTypeNamespace(castType.Namespace, castType.GetRuntimeAssembly().GetSimpleName());
            }
            return o.CanCastToXmlType(xmlType, xmlTypeNamespace);
        }

        [SecurityCritical]
        public virtual ObjRef CreateObjRef(Type requestedType)
        {
            if (this.__identity == null)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_NoIdentityEntry"));
            }
            return new ObjRef(this, requestedType);
        }

        [SecuritySafeCritical]
        internal IntPtr GetComIUnknown(bool fIsBeingMarshalled)
        {
            if (RemotingServices.IsTransparentProxy(this))
            {
                return RemotingServices.GetRealProxy(this).GetCOMIUnknown(fIsBeingMarshalled);
            }
            return Marshal.GetIUnknownForObject(this);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern IntPtr GetComIUnknown(MarshalByRefObject o);
        [SecuritySafeCritical]
        internal static System.Runtime.Remoting.Identity GetIdentity(MarshalByRefObject obj)
        {
            bool flag;
            return GetIdentity(obj, out flag);
        }

        [SecuritySafeCritical]
        internal static System.Runtime.Remoting.Identity GetIdentity(MarshalByRefObject obj, out bool fServer)
        {
            fServer = true;
            System.Runtime.Remoting.Identity identity = null;
            if (obj == null)
            {
                return identity;
            }
            if (!RemotingServices.IsTransparentProxy(obj))
            {
                return (System.Runtime.Remoting.Identity) obj.Identity;
            }
            fServer = false;
            return RemotingServices.GetRealProxy(obj).IdentityObject;
        }

        [SecurityCritical]
        public object GetLifetimeService()
        {
            return LifetimeServices.GetLease(this);
        }

        [SecurityCritical]
        public virtual object InitializeLifetimeService()
        {
            return LifetimeServices.GetLeaseInitial(this);
        }

        internal object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            Type type = base.GetType();
            if (!type.IsCOMObject)
            {
                throw new InvalidOperationException(Environment.GetResourceString("Arg_InvokeMember"));
            }
            return type.InvokeMember(name, invokeAttr, binder, this, args, modifiers, culture, namedParameters);
        }

        internal bool IsInstanceOfType(Type T)
        {
            return T.IsInstanceOfType(this);
        }

        protected MarshalByRefObject MemberwiseClone(bool cloneIdentity)
        {
            MarshalByRefObject obj2 = (MarshalByRefObject) base.MemberwiseClone();
            if (!cloneIdentity)
            {
                obj2.Identity = null;
            }
            return obj2;
        }

        private object Identity
        {
            get
            {
                return this.__identity;
            }
            set
            {
                this.__identity = value;
            }
        }
    }
}

