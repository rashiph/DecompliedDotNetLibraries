namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class SoapServerInfo
    {
        internal static bool BoolFromString(string inVal, bool inDefault)
        {
            if (inVal == null)
            {
                return inDefault;
            }
            string str = inVal.ToLower(CultureInfo.InvariantCulture);
            bool flag = inDefault;
            switch (str)
            {
                case "true":
                    flag = true;
                    break;

                case "false":
                    flag = false;
                    break;
            }
            return flag;
        }

        internal static void CheckUrl(string inBaseUrl, string inVirtualRoot, string inProtocol)
        {
            string uriString = inBaseUrl;
            if (uriString.Length <= 0)
            {
                uriString = (inProtocol + "://") + Dns.GetHostName() + "/";
            }
            Uri baseUri = new Uri(uriString);
            int upperBound = baseUri.Segments.GetUpperBound(0);
            Uri uri2 = new Uri(baseUri, inVirtualRoot);
            if (uri2.Segments.GetUpperBound(0) > (upperBound + 1))
            {
                throw new NonRootVRootException();
            }
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern uint GetSystemDirectory(StringBuilder lpBuf, uint uSize);
        internal static void ParseUrl(string inBaseUrl, string inVirtualRoot, string inProtocol, out string baseUrl, out string virtualRoot)
        {
            string str = "https";
            if (inProtocol.ToLower(CultureInfo.InvariantCulture) == "http")
            {
                str = inProtocol;
            }
            baseUrl = inBaseUrl;
            if (baseUrl.Length <= 0)
            {
                baseUrl = str + "://";
                baseUrl = baseUrl + Dns.GetHostName();
                baseUrl = baseUrl + "/";
            }
            Uri baseUri = new Uri(baseUrl);
            Uri uri = new Uri(baseUri, inVirtualRoot);
            if (uri.Scheme != str)
            {
                UriBuilder builder = new UriBuilder(uri.AbsoluteUri) {
                    Scheme = str
                };
                if ((str == "https") && (builder.Port == 80))
                {
                    builder.Port = 0x1bb;
                }
                if ((str == "http") && (builder.Port == 0x1bb))
                {
                    builder.Port = 80;
                }
                uri = builder.Uri;
            }
            string[] segments = uri.Segments;
            virtualRoot = segments[segments.GetUpperBound(0)];
            baseUrl = uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.Length - virtualRoot.Length);
            char[] trimChars = new char[] { '/' };
            virtualRoot = virtualRoot.TrimEnd(trimChars);
        }

        internal static string ServerPhysicalPath(string rootWebServer, string inBaseUrl, string inVirtualRoot, bool createDir)
        {
            string str = "";
            string baseUrl = "";
            string virtualRoot = "";
            ParseUrl(inBaseUrl, inVirtualRoot, "", out baseUrl, out virtualRoot);
            if (virtualRoot.Length > 0)
            {
                StringBuilder lpBuf = new StringBuilder(0x400, 0x400);
                uint uSize = 0x400;
                if (GetSystemDirectory(lpBuf, uSize) == 0)
                {
                    throw new ServicedComponentException(Resource.FormatString("Soap_GetSystemDirectoryFailure"));
                }
                if (lpBuf.ToString().Length <= 0)
                {
                    return str;
                }
                str = lpBuf.ToString() + @"\com\SoapVRoots\" + virtualRoot;
                if (createDir)
                {
                    string path = str + @"\bin";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            return str;
        }
    }
}

