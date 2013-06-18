namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;

    internal static class AuthenticationLevelHelper
    {
        internal static string ToString(AuthenticationLevel authenticationLevel)
        {
            if (authenticationLevel == AuthenticationLevel.MutualAuthRequested)
            {
                return "mutualAuthRequested";
            }
            if (authenticationLevel == AuthenticationLevel.MutualAuthRequired)
            {
                return "mutualAuthRequired";
            }
            if (authenticationLevel == AuthenticationLevel.None)
            {
                return "none";
            }
            return authenticationLevel.ToString();
        }
    }
}

