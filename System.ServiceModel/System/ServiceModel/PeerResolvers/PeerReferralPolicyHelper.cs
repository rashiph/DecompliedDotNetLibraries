namespace System.ServiceModel.PeerResolvers
{
    using System;

    internal static class PeerReferralPolicyHelper
    {
        internal static bool IsDefined(PeerReferralPolicy value)
        {
            if ((value != PeerReferralPolicy.Service) && (value != PeerReferralPolicy.Share))
            {
                return (value == PeerReferralPolicy.DoNotShare);
            }
            return true;
        }
    }
}

