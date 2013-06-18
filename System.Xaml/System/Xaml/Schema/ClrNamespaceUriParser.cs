namespace System.Xaml.Schema
{
    using System;
    using System.Runtime.InteropServices;
    using System.Xaml;
    using System.Xaml.MS.Impl;

    internal static class ClrNamespaceUriParser
    {
        public static string GetUri(string clrNs, string assemblyName)
        {
            return string.Format(TypeConverterHelper.InvariantEnglishUS, "clr-namespace:{0};assembly={1}", new object[] { clrNs, assemblyName });
        }

        public static bool TryParseUri(string uriInput, out string clrNs, out string assemblyName)
        {
            string str;
            return TryParseUri(uriInput, out clrNs, out assemblyName, out str, false);
        }

        private static bool TryParseUri(string uriInput, out string clrNs, out string assemblyName, out string error, bool returnErrors)
        {
            clrNs = null;
            assemblyName = null;
            error = null;
            int index = KS.IndexOf(uriInput, ":");
            if (-1 == index)
            {
                if (returnErrors)
                {
                    error = System.Xaml.SR.Get("MissingTagInNamespace", new object[] { ":", uriInput });
                }
                return false;
            }
            if (!KS.Eq(uriInput.Substring(0, index), "clr-namespace"))
            {
                if (returnErrors)
                {
                    error = System.Xaml.SR.Get("MissingTagInNamespace", new object[] { "clr-namespace", uriInput });
                }
                return false;
            }
            int startIndex = index + 1;
            int num3 = KS.IndexOf(uriInput, ";");
            if (-1 == num3)
            {
                clrNs = uriInput.Substring(startIndex);
                assemblyName = null;
                return true;
            }
            int length = num3 - startIndex;
            clrNs = uriInput.Substring(startIndex, length);
            int num5 = num3 + 1;
            int num6 = KS.IndexOf(uriInput, "=");
            if (-1 == num6)
            {
                if (returnErrors)
                {
                    error = System.Xaml.SR.Get("MissingTagInNamespace", new object[] { "=", uriInput });
                }
                return false;
            }
            if (!KS.Eq(uriInput.Substring(num5, num6 - num5), "assembly"))
            {
                if (returnErrors)
                {
                    error = System.Xaml.SR.Get("AssemblyTagMissing", new object[] { "assembly", uriInput });
                }
                return false;
            }
            assemblyName = uriInput.Substring(num6 + 1);
            return true;
        }
    }
}

