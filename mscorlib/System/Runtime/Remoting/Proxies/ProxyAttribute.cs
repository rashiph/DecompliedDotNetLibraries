namespace System.Runtime.Remoting.Proxies
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Contexts;
    using System.Security;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true), SecurityCritical, ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class ProxyAttribute : Attribute, IContextAttribute
    {
        [SecurityCritical]
        public virtual MarshalByRefObject CreateInstance(Type serverType)
        {
            if (serverType == null)
            {
                throw new ArgumentNullException("serverType");
            }
            RuntimeType type = serverType as RuntimeType;
            if (type == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            if (!serverType.IsContextful)
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Activation_MBR_ProxyAttribute"));
            }
            if (serverType.IsAbstract)
            {
                throw new RemotingException(Environment.GetResourceString("Acc_CreateAbst"));
            }
            return this.CreateInstanceInternal(type);
        }

        internal MarshalByRefObject CreateInstanceInternal(RuntimeType serverType)
        {
            return ActivationServices.CreateInstance(serverType);
        }

        [SecurityCritical]
        public virtual RealProxy CreateProxy(ObjRef objRef, Type serverType, object serverObject, Context serverContext)
        {
            RemotingProxy rp = new RemotingProxy(serverType);
            if (serverContext != null)
            {
                RealProxy.SetStubData(rp, serverContext.InternalContextID);
            }
            if ((objRef != null) && objRef.GetServerIdentity().IsAllocated)
            {
                rp.SetSrvInfo(objRef.GetServerIdentity(), objRef.GetDomainID());
            }
            rp.Initialized = true;
            Type type = serverType;
            if ((!type.IsContextful && !type.IsMarshalByRef) && (serverContext != null))
            {
                throw new RemotingException(Environment.GetResourceString("Remoting_Activation_MBR_ProxyAttribute"));
            }
            return rp;
        }

        [SecurityCritical, ComVisible(true)]
        public void GetPropertiesForNewContext(IConstructionCallMessage msg)
        {
        }

        [SecurityCritical, ComVisible(true)]
        public bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            return true;
        }
    }
}

