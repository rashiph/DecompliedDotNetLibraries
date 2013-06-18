namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [Guid("346D5B9F-45E1-45c0-AADF-1B7D221E9063")]
    public sealed class SoapClientImport : ISoapClientImport
    {
        internal static string GetClientPhysicalPath(bool createDir)
        {
            uint uSize = 0x400;
            StringBuilder lpBuf = new StringBuilder((int) uSize, (int) uSize);
            if (GetSystemDirectory(lpBuf, uSize) == 0)
            {
                throw new ServicedComponentException(Resource.FormatString("Soap_GetSystemDirectoryFailure"));
            }
            string path = lpBuf.ToString() + @"\com\SOAPAssembly\";
            if (createDir && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern uint GetSystemDirectory(StringBuilder lpBuf, uint uSize);
        public void ProcessClientTlbEx(string progId, string virtualRoot, string baseUrl, string authentication, string assemblyName, string typeName)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            catch (SecurityException)
            {
                ComSoapPublishError.Report(Resource.FormatString("Soap_SecurityFailure"));
                throw;
            }
            try
            {
                string clientPhysicalPath = GetClientPhysicalPath(true);
                if (progId.Length > 0)
                {
                    Uri baseUri = new Uri(baseUrl);
                    Uri uri2 = new Uri(baseUri, virtualRoot);
                    string str2 = authentication;
                    if ((str2.Length <= 0) && (uri2.Scheme.ToLower(CultureInfo.InvariantCulture) == "https"))
                    {
                        str2 = "Windows";
                    }
                    SoapClientConfig.Write(clientPhysicalPath, uri2.AbsoluteUri, assemblyName, typeName, progId, str2);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(Resource.FormatString("Soap_ClientConfigAddFailure"));
                throw;
            }
        }
    }
}

