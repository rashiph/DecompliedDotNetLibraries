namespace System.ServiceModel
{
    using System;

    internal static class AddressFilterModeHelper
    {
        public static bool IsDefined(AddressFilterMode x)
        {
            if ((x != AddressFilterMode.Exact) && (x != AddressFilterMode.Prefix))
            {
                return (x == AddressFilterMode.Any);
            }
            return true;
        }
    }
}

