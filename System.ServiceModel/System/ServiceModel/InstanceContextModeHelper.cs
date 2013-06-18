namespace System.ServiceModel
{
    using System;

    internal static class InstanceContextModeHelper
    {
        public static bool IsDefined(InstanceContextMode x)
        {
            if ((x != InstanceContextMode.PerCall) && (x != InstanceContextMode.PerSession))
            {
                return (x == InstanceContextMode.Single);
            }
            return true;
        }
    }
}

