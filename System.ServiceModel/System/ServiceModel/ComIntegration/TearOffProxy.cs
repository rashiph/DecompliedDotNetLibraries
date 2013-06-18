namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.ServiceModel;

    internal class TearOffProxy : RealProxy, IDisposable
    {
        private Dictionary<MethodBase, MethodBase> baseTypeToInterfaceMethod;
        private ICreateServiceChannel serviceChannelCreator;

        internal TearOffProxy(ICreateServiceChannel serviceChannelCreator, Type proxiedType) : base(proxiedType)
        {
            if (serviceChannelCreator == null)
            {
                throw Fx.AssertAndThrow("ServiceChannelCreator cannot be null");
            }
            this.serviceChannelCreator = serviceChannelCreator;
            this.baseTypeToInterfaceMethod = new Dictionary<MethodBase, MethodBase>();
        }

        public override IMessage Invoke(IMessage message)
        {
            RealProxy proxy = null;
            IMethodCallMessage mcm = message as IMethodCallMessage;
            try
            {
                proxy = this.serviceChannelCreator.CreateChannel();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                return new ReturnMessage(DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(exception.GetBaseException().Message, Marshal.GetHRForException(exception.GetBaseException()))), mcm);
            }
            MethodBase methodBase = mcm.MethodBase;
            IRemotingTypeInfo info = proxy as IRemotingTypeInfo;
            if (info == null)
            {
                throw Fx.AssertAndThrow("Type Info cannot be null");
            }
            if (info.CanCastTo(methodBase.DeclaringType, null))
            {
                IMessage message3 = proxy.Invoke(message);
                ReturnMessage message4 = message3 as ReturnMessage;
                if ((message4 != null) && (message4.Exception != null))
                {
                    return new ReturnMessage(DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(message4.Exception.GetBaseException().Message, Marshal.GetHRForException(message4.Exception.GetBaseException()))), mcm);
                }
                return message3;
            }
            return new ReturnMessage(DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("OperationNotFound", new object[] { methodBase.Name }), HR.DISP_E_UNKNOWNNAME)), mcm);
        }

        void IDisposable.Dispose()
        {
            this.serviceChannelCreator = null;
        }
    }
}

