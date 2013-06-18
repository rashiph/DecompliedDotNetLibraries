namespace System.ServiceModel.Security
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    internal static class StoreLocationHelper
    {
        internal static bool IsDefined(StoreLocation value)
        {
            if (value != StoreLocation.CurrentUser)
            {
                return (value == StoreLocation.LocalMachine);
            }
            return true;
        }
    }
}

