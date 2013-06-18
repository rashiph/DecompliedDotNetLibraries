namespace System.ServiceModel.Activation
{
    using System;

    internal class HttpsHostedTransportConfiguration : HttpHostedTransportConfiguration
    {
        internal HttpsHostedTransportConfiguration() : base(Uri.UriSchemeHttps)
        {
        }
    }
}

