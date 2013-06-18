namespace System.EnterpriseServices
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal static class Resource
    {
        private static ResourceManager _resmgr;

        internal static string FormatString(string key)
        {
            return GetString(key);
        }

        internal static string FormatString(string key, object a1)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString(key), new object[] { a1 });
        }

        internal static string FormatString(string key, object a1, object a2)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString(key), new object[] { a1, a2 });
        }

        internal static string FormatString(string key, object a1, object a2, object a3)
        {
            return string.Format(CultureInfo.CurrentCulture, GetString(key), new object[] { a1, a2, a3 });
        }

        internal static string GetString(string key)
        {
            InitResourceManager();
            return _resmgr.GetString(key, null);
        }

        private static void InitResourceManager()
        {
            if (_resmgr == null)
            {
                _resmgr = new ResourceManager("System.EnterpriseServices", typeof(Resource).Module.Assembly);
            }
        }
    }
}

