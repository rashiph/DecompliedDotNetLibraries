namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal static class SoapUtil
    {
        internal static ResourceManager SystemResMgr;
        internal static Type typeofBoolean = typeof(bool);
        internal static Type typeofObject = typeof(object);
        internal static Type typeofSoapFault = typeof(SoapFault);
        internal static Type typeofString = typeof(string);
        internal static Assembly urtAssembly = Assembly.GetAssembly(typeofString);
        internal static string urtAssemblyString = urtAssembly.FullName;

        [Conditional("SER_LOGGING")]
        internal static void DumpHash(string tag, Hashtable hashTable)
        {
            IDictionaryEnumerator enumerator = hashTable.GetEnumerator();
            while (enumerator.MoveNext())
            {
            }
        }

        internal static string GetResourceString(string key)
        {
            if (SystemResMgr == null)
            {
                InitResourceManager();
            }
            return SystemResMgr.GetString(key, null);
        }

        internal static string GetResourceString(string key, params object[] values)
        {
            if (SystemResMgr == null)
            {
                InitResourceManager();
            }
            string format = SystemResMgr.GetString(key, null);
            return string.Format(CultureInfo.CurrentCulture, format, values);
        }

        private static ResourceManager InitResourceManager()
        {
            if (SystemResMgr == null)
            {
                SystemResMgr = new ResourceManager("SoapFormatter", typeof(SoapParser).Module.Assembly);
            }
            return SystemResMgr;
        }
    }
}

