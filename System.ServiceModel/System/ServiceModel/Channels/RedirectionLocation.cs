namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    [Serializable]
    public class RedirectionLocation
    {
        private RedirectionLocation()
        {
        }

        public RedirectionLocation(Uri address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (!address.IsAbsoluteUri)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("address", System.ServiceModel.SR.GetString("UriMustBeAbsolute"));
            }
            this.Address = address;
        }

        public Uri Address { get; private set; }
    }
}

