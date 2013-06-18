namespace System.ServiceModel
{
    using System;

    internal static class ImpersonationOptionHelper
    {
        internal static bool AllowedOrRequired(ImpersonationOption option)
        {
            if (option != ImpersonationOption.Allowed)
            {
                return (option == ImpersonationOption.Required);
            }
            return true;
        }

        public static bool IsDefined(ImpersonationOption option)
        {
            if ((option != ImpersonationOption.NotAllowed) && (option != ImpersonationOption.Allowed))
            {
                return (option == ImpersonationOption.Required);
            }
            return true;
        }
    }
}

