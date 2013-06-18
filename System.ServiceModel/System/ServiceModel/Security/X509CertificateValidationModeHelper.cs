namespace System.ServiceModel.Security
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class X509CertificateValidationModeHelper
    {
        public static bool IsDefined(X509CertificateValidationMode validationMode)
        {
            if (((validationMode != X509CertificateValidationMode.None) && (validationMode != X509CertificateValidationMode.PeerTrust)) && ((validationMode != X509CertificateValidationMode.ChainTrust) && (validationMode != X509CertificateValidationMode.PeerOrChainTrust)))
            {
                return (validationMode == X509CertificateValidationMode.Custom);
            }
            return true;
        }

        internal static void Validate(X509CertificateValidationMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(X509CertificateValidationMode)));
            }
        }
    }
}

