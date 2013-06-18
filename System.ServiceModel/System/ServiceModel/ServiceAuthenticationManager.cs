namespace System.ServiceModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;

    public class ServiceAuthenticationManager
    {
        public virtual ReadOnlyCollection<IAuthorizationPolicy> Authenticate(ReadOnlyCollection<IAuthorizationPolicy> authPolicy, Uri listenUri, ref Message message)
        {
            return authPolicy;
        }
    }
}

