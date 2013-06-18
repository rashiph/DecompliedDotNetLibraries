namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [Guid("CAA817CC-0C04-4d22-A05C-2B7E162F4E8F")]
    public sealed class SoapServerVRoot : ISoapServerVRoot
    {
        public void CreateVirtualRootEx(string rootWebServer, string inBaseUrl, string inVirtualRoot, string homePage, string discoFile, string secureSockets, string authentication, string operation, out string baseUrl, out string virtualRoot, out string physicalPath)
        {
            baseUrl = "";
            virtualRoot = "";
            physicalPath = "";
            bool inDefault = true;
            bool windowsAuth = true;
            bool anonymous = false;
            bool flag4 = false;
            bool flag5 = false;
            bool impersonate = true;
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                if ((inBaseUrl.Length > 0) || (inVirtualRoot.Length > 0))
                {
                    string str = "IIS://localhost/W3SVC/1/ROOT";
                    if (rootWebServer.Length > 0)
                    {
                        str = rootWebServer;
                    }
                    if (authentication.ToLower(CultureInfo.InvariantCulture) == "anonymous")
                    {
                        anonymous = true;
                        windowsAuth = false;
                        impersonate = false;
                    }
                    flag4 = SoapServerInfo.BoolFromString(discoFile, flag4);
                    flag5 = SoapServerInfo.BoolFromString(homePage, flag5);
                    inDefault = SoapServerInfo.BoolFromString(secureSockets, inDefault);
                    string inProtocol = "https";
                    if (!inDefault)
                    {
                        inProtocol = "http";
                    }
                    SoapServerInfo.CheckUrl(inBaseUrl, inVirtualRoot, inProtocol);
                    SoapServerInfo.ParseUrl(inBaseUrl, inVirtualRoot, inProtocol, out baseUrl, out virtualRoot);
                    physicalPath = SoapServerInfo.ServerPhysicalPath(str, inBaseUrl, inVirtualRoot, true);
                    SoapServerConfig.Create(physicalPath, impersonate, windowsAuth);
                    if (flag4)
                    {
                        new DiscoFile().Create(physicalPath, "Default.disco");
                    }
                    else if (File.Exists(physicalPath + @"\Default.disco"))
                    {
                        File.Delete(physicalPath + @"\Default.disco");
                    }
                    if (flag5)
                    {
                        HomePage page = new HomePage();
                        string discoRef = "";
                        if (flag4)
                        {
                            discoRef = "Default.disco";
                        }
                        page.Create(physicalPath, virtualRoot, "Default.aspx", discoRef);
                    }
                    else if (File.Exists(physicalPath + @"\Default.aspx"))
                    {
                        File.Delete(physicalPath + @"\Default.aspx");
                    }
                    IISVirtualRootEx.CreateOrModify(str, physicalPath, virtualRoot, inDefault, windowsAuth, anonymous, flag5);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(Resource.FormatString("Soap_VRootCreationFailed") + " " + virtualRoot);
                throw;
            }
        }

        public void DeleteVirtualRootEx(string rootWebServer, string inBaseUrl, string inVirtualRoot)
        {
            try
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
                if ((inBaseUrl.Length > 0) || (inVirtualRoot.Length > 0))
                {
                    int length = rootWebServer.Length;
                    string inProtocol = "";
                    string baseUrl = "";
                    string virtualRoot = "";
                    SoapServerInfo.ParseUrl(inBaseUrl, inVirtualRoot, inProtocol, out baseUrl, out virtualRoot);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(Resource.FormatString("Soap_VRootDirectoryDeletionFailed"));
                throw;
            }
        }

        public void GetVirtualRootStatus(string RootWebServer, string inBaseUrl, string inVirtualRoot, out string Exists, out string SSL, out string WindowsAuth, out string Anonymous, out string HomePage, out string DiscoFile, out string PhysicalPath, out string BaseUrl, out string VirtualRoot)
        {
            string rootWebServer = "IIS://localhost/W3SVC/1/ROOT";
            if (RootWebServer.Length > 0)
            {
                rootWebServer = RootWebServer;
            }
            Exists = "false";
            SSL = "false";
            WindowsAuth = "false";
            Anonymous = "false";
            HomePage = "false";
            DiscoFile = "false";
            SoapServerInfo.ParseUrl(inBaseUrl, inVirtualRoot, "http", out BaseUrl, out VirtualRoot);
            PhysicalPath = SoapServerInfo.ServerPhysicalPath(rootWebServer, BaseUrl, VirtualRoot, false);
            bool bExists = false;
            bool bSSL = false;
            bool bWindowsAuth = false;
            bool bAnonymous = false;
            bool bHomePage = false;
            bool bDiscoFile = false;
            IISVirtualRootEx.GetStatus(rootWebServer, PhysicalPath, VirtualRoot, out bExists, out bSSL, out bWindowsAuth, out bAnonymous, out bHomePage, out bDiscoFile);
            if (bExists)
            {
                Exists = "true";
            }
            if (bSSL)
            {
                SSL = "true";
                SoapServerInfo.ParseUrl(inBaseUrl, inVirtualRoot, "https", out BaseUrl, out VirtualRoot);
            }
            if (bWindowsAuth)
            {
                WindowsAuth = "true";
            }
            if (bAnonymous)
            {
                Anonymous = "true";
            }
            if (bHomePage)
            {
                HomePage = "true";
            }
            if (bDiscoFile)
            {
                DiscoFile = "true";
            }
        }
    }
}

