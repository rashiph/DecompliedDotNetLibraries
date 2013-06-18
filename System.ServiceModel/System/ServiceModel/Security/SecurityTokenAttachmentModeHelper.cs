namespace System.ServiceModel.Security
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class SecurityTokenAttachmentModeHelper
    {
        internal static void Categorize(SecurityTokenAttachmentMode value, out bool isBasic, out bool isSignedButNotBasic, out ReceiveSecurityHeaderBindingModes mode)
        {
            Validate(value);
            switch (value)
            {
                case SecurityTokenAttachmentMode.Signed:
                    isBasic = false;
                    isSignedButNotBasic = true;
                    mode = ReceiveSecurityHeaderBindingModes.Signed;
                    return;

                case SecurityTokenAttachmentMode.Endorsing:
                    isBasic = false;
                    isSignedButNotBasic = false;
                    mode = ReceiveSecurityHeaderBindingModes.Endorsing;
                    return;

                case SecurityTokenAttachmentMode.SignedEndorsing:
                    isBasic = false;
                    isSignedButNotBasic = true;
                    mode = ReceiveSecurityHeaderBindingModes.SignedEndorsing;
                    return;

                case SecurityTokenAttachmentMode.SignedEncrypted:
                    isBasic = true;
                    isSignedButNotBasic = false;
                    mode = ReceiveSecurityHeaderBindingModes.Basic;
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
        }

        internal static bool IsDefined(SecurityTokenAttachmentMode value)
        {
            if (((value != SecurityTokenAttachmentMode.Endorsing) && (value != SecurityTokenAttachmentMode.Signed)) && (value != SecurityTokenAttachmentMode.SignedEncrypted))
            {
                return (value == SecurityTokenAttachmentMode.SignedEndorsing);
            }
            return true;
        }

        internal static void Validate(SecurityTokenAttachmentMode value)
        {
            if (!IsDefined(value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, typeof(SecurityTokenAttachmentMode)));
            }
        }
    }
}

