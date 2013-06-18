namespace System.ServiceModel
{
    using System;

    internal static class ReleaseInstanceModeHelper
    {
        public static bool IsDefined(ReleaseInstanceMode x)
        {
            if (((x != ReleaseInstanceMode.None) && (x != ReleaseInstanceMode.BeforeCall)) && (x != ReleaseInstanceMode.AfterCall))
            {
                return (x == ReleaseInstanceMode.BeforeAndAfterCall);
            }
            return true;
        }
    }
}

