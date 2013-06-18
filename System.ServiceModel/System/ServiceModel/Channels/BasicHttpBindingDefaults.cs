namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Text;

    internal static class BasicHttpBindingDefaults
    {
        internal const WSMessageEncoding MessageEncoding = WSMessageEncoding.Text;
        internal const BasicHttpMessageCredentialType MessageSecurityClientCredentialType = BasicHttpMessageCredentialType.UserName;
        internal const System.ServiceModel.TransferMode TransferMode = System.ServiceModel.TransferMode.Buffered;

        internal static Encoding TextEncoding
        {
            get
            {
                return TextEncoderDefaults.Encoding;
            }
        }
    }
}

