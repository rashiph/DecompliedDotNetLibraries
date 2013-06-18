namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.DirectoryServices;
    using System.Web;

    internal static class IisHelper
    {
        private static bool _bIsSslRequired;
        private static string _iisAppUrl;

        internal static void Initialize()
        {
            try
            {
                string str = HttpContext.Current.Request.ServerVariables["APPL_MD_PATH"];
                bool flag = false;
                if (str.StartsWith("/LM/", StringComparison.Ordinal))
                {
                    DirectoryEntry entry = new DirectoryEntry("IIS://localhost/" + str.Substring(4));
                    flag = (bool) entry.Properties["AccessSSL"][0];
                }
                _bIsSslRequired = flag;
            }
            catch
            {
            }
        }

        internal static string ApplicationUrl
        {
            get
            {
                return _iisAppUrl;
            }
            set
            {
                _iisAppUrl = value;
            }
        }

        internal static bool IsSslRequired
        {
            get
            {
                return _bIsSslRequired;
            }
        }
    }
}

