namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using System;
    using System.IdentityModel.Claims;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class WindowsActivationProxy : ActivationProxy
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WindowsActivationProxy(CoordinationService coordination, EndpointAddress to) : base(coordination, to)
        {
        }

        protected override IChannelFactory<IRequestReplyService> SelectChannelFactory(out MessageVersion messageVersion)
        {
            base.interoperating = false;
            EndpointIdentity identity = base.to.Identity;
            if (identity != null)
            {
                string claimType = identity.IdentityClaim.ClaimType;
                if ((claimType != ClaimTypes.Spn) && (claimType != ClaimTypes.Upn))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CreateChannelFailureException(Microsoft.Transactions.SR.GetString("InvalidTrustIdentityType", new object[] { claimType })));
                }
            }
            string scheme = base.to.Uri.Scheme;
            if (string.Compare(scheme, Uri.UriSchemeNetPipe, StringComparison.OrdinalIgnoreCase) == 0)
            {
                messageVersion = base.coordinationService.NamedPipeActivationBinding.MessageVersion;
                return base.coordinationService.NamedPipeActivationChannelFactory;
            }
            if (!base.coordinationService.Config.RemoteClientsEnabled || (string.Compare(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) != 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CreateChannelFailureException(Microsoft.Transactions.SR.GetString("InvalidSchemeWithTrustIdentity", new object[] { scheme })));
            }
            messageVersion = base.coordinationService.WindowsActivationBinding.MessageVersion;
            return base.coordinationService.WindowsActivationChannelFactory;
        }
    }
}

