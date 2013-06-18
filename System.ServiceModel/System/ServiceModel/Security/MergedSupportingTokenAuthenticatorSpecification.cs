namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MergedSupportingTokenAuthenticatorSpecification
    {
        public Collection<SupportingTokenAuthenticatorSpecification> SupportingTokenAuthenticators;
        public bool ExpectSignedTokens;
        public bool ExpectEndorsingTokens;
        public bool ExpectBasicTokens;
    }
}

