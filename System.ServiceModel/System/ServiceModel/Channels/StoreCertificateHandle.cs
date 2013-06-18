namespace System.ServiceModel.Channels
{
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class StoreCertificateHandle : CertificateHandle
    {
        private StoreCertificateHandle()
        {
            base.delete = true;
        }
    }
}

