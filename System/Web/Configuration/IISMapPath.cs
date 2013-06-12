namespace System.Web.Configuration
{
    using System;
    using System.Web.Hosting;

    internal static class IISMapPath
    {
        internal static IConfigMapPath GetInstance()
        {
            if (ServerConfig.UseMetabase)
            {
                return (IConfigMapPath) MetabaseServerConfig.GetInstance();
            }
            if (ServerConfig.IISExpressVersion != null)
            {
                return (IConfigMapPath) ServerConfig.GetInstance();
            }
            ProcessHost defaultHost = ProcessHost.DefaultHost;
            IProcessHostSupportFunctions supportFunctions = null;
            if (defaultHost != null)
            {
                supportFunctions = defaultHost.SupportFunctions;
            }
            if (supportFunctions == null)
            {
                supportFunctions = HostingEnvironment.SupportFunctions;
            }
            return new ProcessHostMapPath(supportFunctions);
        }

        internal static bool IsSiteId(string siteName)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                return false;
            }
            for (int i = 0; i < siteName.Length; i++)
            {
                if (!char.IsDigit(siteName[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

