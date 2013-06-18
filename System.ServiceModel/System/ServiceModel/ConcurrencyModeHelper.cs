namespace System.ServiceModel
{
    using System;

    internal static class ConcurrencyModeHelper
    {
        public static bool IsDefined(ConcurrencyMode x)
        {
            if ((x != ConcurrencyMode.Single) && (x != ConcurrencyMode.Reentrant))
            {
                return (x == ConcurrencyMode.Multiple);
            }
            return true;
        }
    }
}

