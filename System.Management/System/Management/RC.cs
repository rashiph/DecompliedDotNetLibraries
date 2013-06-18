namespace System.Management
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal sealed class RC
    {
        private static readonly ResourceManager resMgr = new ResourceManager(Assembly.GetExecutingAssembly().GetName().Name, Assembly.GetExecutingAssembly(), null);

        private RC()
        {
        }

        public static string GetString(string strToGet)
        {
            return resMgr.GetString(strToGet, CultureInfo.CurrentCulture);
        }
    }
}

