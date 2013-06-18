namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;

    internal static class ExceptionHelper
    {
        public static void ThrowIfCardSpaceException(int status)
        {
            switch (status)
            {
                case -1073413888:
                    throw InfoCardTrace.ThrowHelperError(new CardSpaceException(Microsoft.InfoCards.SR.GetString("ClientAPIInfocardError")));

                case -1073413887:
                case -1073413886:
                case -1073413876:
                case -1073413873:
                case -1073413872:
                case -1073413868:
                case -1073413867:
                    return;

                case -1073413885:
                    throw InfoCardTrace.ThrowHelperError(new IdentityValidationException(Microsoft.InfoCards.SR.GetString("ClientAPIInvalidIdentity")));

                case -1073413884:
                    throw InfoCardTrace.ThrowHelperError(new CardSpaceException(Microsoft.InfoCards.SR.GetString("ClientAPICannotImport")));

                case -1073413877:
                    throw InfoCardTrace.ThrowHelperError(new PolicyValidationException(Microsoft.InfoCards.SR.GetString("ClientAPIInvalidPolicy")));

                case -1073413875:
                    throw InfoCardTrace.ThrowHelperError(new ServiceBusyException(Microsoft.InfoCards.SR.GetString("ClientAPIServiceBusy")));

                case -1073413874:
                    throw InfoCardTrace.ThrowHelperError(new ServiceNotStartedException(Microsoft.InfoCards.SR.GetString("ClientAPIServiceNotStartedError")));

                case -1073413871:
                    throw InfoCardTrace.ThrowHelperError(new StsCommunicationException(Microsoft.InfoCards.SR.GetString("ClientStsCommunicationException")));

                case -1073413870:
                    throw InfoCardTrace.ThrowHelperError(new UntrustedRecipientException(Microsoft.InfoCards.SR.GetString("ClientAPIUntrustedRecipientError")));

                case -1073413869:
                    throw InfoCardTrace.ThrowHelperError(new UserCancellationException(Microsoft.InfoCards.SR.GetString("ClientAPIUserCancellationError")));

                case -1073413866:
                    throw InfoCardTrace.ThrowHelperError(new UnsupportedPolicyOptionsException(Microsoft.InfoCards.SR.GetString("ClientAPIUnsupportedPolicyOptions")));

                case -1073413862:
                    throw InfoCardTrace.ThrowHelperError(new UIInitializationException(Microsoft.InfoCards.SR.GetString("ClientAPIUIInitializationFailed")));
            }
        }
    }
}

