namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;

    internal static class X509SecurityTokenReferenceStyleHelper
    {
        public static bool IsDefined(X509KeyIdentifierClauseType value)
        {
            if (((value != X509KeyIdentifierClauseType.Any) && (value != X509KeyIdentifierClauseType.IssuerSerial)) && ((value != X509KeyIdentifierClauseType.SubjectKeyIdentifier) && (value != X509KeyIdentifierClauseType.Thumbprint)))
            {
                return (value == X509KeyIdentifierClauseType.RawDataKeyIdentifier);
            }
            return true;
        }

        public static void Validate(X509KeyIdentifierClauseType value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(X509KeyIdentifierClauseType)));
            }
        }
    }
}

