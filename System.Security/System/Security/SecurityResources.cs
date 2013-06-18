namespace System.Security
{
    using System;
    using System.Resources;

    internal static class SecurityResources
    {
        private static ResourceManager s_resMgr;

        internal static string GetResourceString(string key)
        {
            if (s_resMgr == null)
            {
                s_resMgr = new ResourceManager("system.security", typeof(SecurityResources).Assembly);
            }
            return s_resMgr.GetString(key, null);
        }
    }
}

