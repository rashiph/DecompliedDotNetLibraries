namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(false)]
    public sealed class ServicedComponentException : SystemException
    {
        private static string _default;
        private const int COR_E_SERVICEDCOMPONENT = -2146233073;

        public ServicedComponentException() : base(DefaultMessage)
        {
            base.HResult = -2146233073;
        }

        public ServicedComponentException(string message) : base(message)
        {
            base.HResult = -2146233073;
        }

        private ServicedComponentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ServicedComponentException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146233073;
        }

        private static string DefaultMessage
        {
            get
            {
                if (_default == null)
                {
                    _default = Resource.FormatString("ServicedComponentException_Default");
                }
                return _default;
            }
        }
    }
}

