namespace System.Resources
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class MissingSatelliteAssemblyException : SystemException
    {
        private string _cultureName;

        public MissingSatelliteAssemblyException() : base(Environment.GetResourceString("MissingSatelliteAssembly_Default"))
        {
            base.SetErrorCode(-2146233034);
        }

        public MissingSatelliteAssemblyException(string message) : base(message)
        {
            base.SetErrorCode(-2146233034);
        }

        [SecuritySafeCritical]
        protected MissingSatelliteAssemblyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MissingSatelliteAssemblyException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233034);
        }

        public MissingSatelliteAssemblyException(string message, string cultureName) : base(message)
        {
            base.SetErrorCode(-2146233034);
            this._cultureName = cultureName;
        }

        public string CultureName
        {
            get
            {
                return this._cultureName;
            }
        }
    }
}

