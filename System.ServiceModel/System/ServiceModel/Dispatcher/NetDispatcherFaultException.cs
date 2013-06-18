namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;

    internal class NetDispatcherFaultException : FaultException
    {
        public NetDispatcherFaultException(FaultReason reason, FaultCode code, Exception innerException) : base(reason, code, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault", innerException)
        {
        }

        public NetDispatcherFaultException(string reason, FaultCode code, Exception innerException) : base(reason, code, "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault", innerException)
        {
        }
    }
}

