namespace System.ServiceModel.PeerResolvers
{
    using System;

    internal static class PeerResolverModeHelper
    {
        internal static bool IsDefined(PeerResolverMode value)
        {
            if ((value != PeerResolverMode.Auto) && (value != PeerResolverMode.Pnrp))
            {
                return (value == PeerResolverMode.Custom);
            }
            return true;
        }
    }
}

