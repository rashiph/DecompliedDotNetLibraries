namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [Guid("5F9A955F-AA55-4127-A32B-33496AA8A44E")]
    public sealed class SoapUtility : ISoapUtility
    {
        public void GetServerBinPath(string rootWebServer, string inBaseUrl, string inVirtualRoot, out string binPath)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                binPath = SoapServerInfo.ServerPhysicalPath(rootWebServer, inBaseUrl, inVirtualRoot, false) + @"\bin\";
            }
            catch (SecurityException)
            {
                ComSoapPublishError.Report(Resource.FormatString("Soap_SecurityFailure"));
                throw;
            }
        }

        public void GetServerPhysicalPath(string rootWebServer, string inBaseUrl, string inVirtualRoot, out string physicalPath)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                physicalPath = SoapServerInfo.ServerPhysicalPath(rootWebServer, inBaseUrl, inVirtualRoot, false);
            }
            catch (SecurityException)
            {
                ComSoapPublishError.Report(Resource.FormatString("Soap_SecurityFailure"));
                throw;
            }
        }

        public void Present()
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
        }
    }
}

