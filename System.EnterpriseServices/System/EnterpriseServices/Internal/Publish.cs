namespace System.EnterpriseServices.Internal
{
    using Microsoft.Win32;
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Remoting;
    using System.Security.Permissions;
    using System.Text;

    [Guid("d8013eef-730b-45e2-ba24-874b7242c425")]
    public class Publish : IComSoapPublisher
    {
        [DllImport("Fusion.dll", CharSet=CharSet.Auto)]
        internal static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);
        public void CreateMailBox(string RootMailServer, string MailBox, out string SmtpName, out string Domain, out string PhysicalPath, out string Error)
        {
            SmtpName = "";
            Domain = "";
            PhysicalPath = "";
            Error = "";
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            ComSoapPublishError.Report(Resource.FormatString("Soap_SmtpNotImplemented"));
            int length = MailBox.Length;
        }

        public void CreateVirtualRoot(string Operation, string FullUrl, out string BaseUrl, out string VirtualRoot, out string PhysicalPath, out string Error)
        {
            BaseUrl = "";
            VirtualRoot = "";
            PhysicalPath = "";
            Error = "";
            if (FullUrl.Length > 0)
            {
                try
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                    ParseUrl(FullUrl, out BaseUrl, out VirtualRoot);
                    if (VirtualRoot.Length > 0)
                    {
                        string rootWeb = "IIS://localhost/W3SVC/1/ROOT";
                        bool createDir = true;
                        if ((Operation.ToLower(CultureInfo.InvariantCulture) == "delete") || (Operation.ToLower(CultureInfo.InvariantCulture) == "addcomponent"))
                        {
                            createDir = false;
                        }
                        string binDirectory = "";
                        this.GetVRootPhysicalPath(VirtualRoot, out PhysicalPath, out binDirectory, createDir);
                        if (PhysicalPath.Length <= 0)
                        {
                            Error = Resource.FormatString("Soap_VRootDirectoryCreationFailed");
                        }
                        else if (createDir)
                        {
                            ServerWebConfig config = new ServerWebConfig();
                            string error = "";
                            config.Create(PhysicalPath, "Web", out error);
                            new DiscoFile().Create(PhysicalPath, "Default.disco");
                            new HomePage().Create(PhysicalPath, VirtualRoot, "Default.aspx", "Default.disco");
                            string str4 = "";
                            try
                            {
                                new IISVirtualRoot().Create(rootWeb, PhysicalPath, VirtualRoot, out str4);
                            }
                            catch (Exception exception)
                            {
                                if ((exception is NullReferenceException) || (exception is SEHException))
                                {
                                    throw;
                                }
                                if (str4.Length <= 0)
                                {
                                    string str5 = Resource.FormatString("Soap_VRootCreationFailed");
                                    str4 = string.Format(CultureInfo.CurrentCulture, str5 + " " + VirtualRoot + " " + exception.ToString(), new object[0]);
                                }
                            }
                            if (str4.Length > 0)
                            {
                                Error = str4;
                            }
                        }
                    }
                }
                catch (Exception exception2)
                {
                    if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                    {
                        throw;
                    }
                    Error = exception2.ToString();
                    ComSoapPublishError.Report(Error);
                }
            }
        }

        public void DeleteMailBox(string RootMailServer, string MailBox, out string Error)
        {
            Error = "";
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            ComSoapPublishError.Report(Resource.FormatString("Soap_SmtpNotImplemented"));
            int length = MailBox.Length;
        }

        public void DeleteVirtualRoot(string RootWebServer, string FullUrl, out string Error)
        {
            Error = "";
            try
            {
                if (FullUrl.Length > 0)
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                    int length = RootWebServer.Length;
                    string baseUrl = "";
                    string virtualRoot = "";
                    ParseUrl(FullUrl, out baseUrl, out virtualRoot);
                    if (virtualRoot.Length > 0)
                    {
                        string physicalPath = "";
                        string binDirectory = "";
                        this.GetVRootPhysicalPath(virtualRoot, out physicalPath, out binDirectory, false);
                        if (physicalPath.Length <= 0)
                        {
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                Error = exception.ToString();
                ComSoapPublishError.Report(exception.ToString());
            }
        }

        public void GacInstall(string AssemblyPath)
        {
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            string filename = Path.Combine(MsCorLibDirectory, "fusion.dll");
            if (LoadLibrary(filename) == IntPtr.Zero)
            {
                throw new DllNotFoundException(filename);
            }
            this.PrivateGacInstall(AssemblyPath);
        }

        public void GacRemove(string AssemblyPath)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                string gacName = new AssemblyManager().GetGacName(AssemblyPath);
                IAssemblyCache ppAsmCache = null;
                int num = CreateAssemblyCache(out ppAsmCache, 0);
                uint pulDisposition = 0;
                if (num == 0)
                {
                    num = ppAsmCache.UninstallAssembly(0, gacName, IntPtr.Zero, out pulDisposition);
                }
                if (num != 0)
                {
                    string str2 = Resource.FormatString("Soap_GacRemoveFailed");
                    ComSoapPublishError.Report(str2 + " " + AssemblyPath + " " + gacName);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
        }

        public void GetAssemblyNameForCache(string TypeLibPath, out string CachePath)
        {
            CacheInfo.GetMetadataName(TypeLibPath, null, out CachePath);
            CachePath = CacheInfo.GetCacheName(CachePath, TypeLibPath);
        }

        public static string GetClientPhysicalPath(bool CreateDir)
        {
            StringBuilder lpBuf = new StringBuilder(0x400, 0x400);
            uint uSize = 0x400;
            GetSystemDirectory(lpBuf, uSize);
            string path = lpBuf.ToString() + @"\com\SOAPAssembly\";
            if (CreateDir)
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    path = string.Empty;
                    ComSoapPublishError.Report(exception.ToString());
                }
            }
            return path;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern uint GetSystemDirectory(StringBuilder lpBuf, uint uSize);
        public string GetTypeNameFromProgId(string AssemblyPath, string ProgId)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
            string typeNameFromProgId = "";
            AppDomainSetup info = new AppDomainSetup();
            AppDomain domain = AppDomain.CreateDomain("SoapDomain", null, info);
            if (domain != null)
            {
                try
                {
                    ObjectHandle handle = domain.CreateInstance(typeof(AssemblyManager).Assembly.FullName, typeof(AssemblyManager).FullName);
                    if (handle != null)
                    {
                        typeNameFromProgId = ((AssemblyManager) handle.Unwrap()).InternalGetTypeNameFromProgId(AssemblyPath, ProgId);
                    }
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
            return typeNameFromProgId;
        }

        private bool GetVRootPhysicalPath(string VirtualRoot, out string PhysicalPath, out string BinDirectory, bool CreateDir)
        {
            bool flag = true;
            StringBuilder lpBuf = new StringBuilder(0x400, 0x400);
            uint uSize = 0x400;
            GetSystemDirectory(lpBuf, uSize);
            string path = lpBuf.ToString() + @"\com\SOAPVRoots\";
            PhysicalPath = path + VirtualRoot + @"\";
            BinDirectory = PhysicalPath + @"bin\";
            if (CreateDir)
            {
                try
                {
                    try
                    {
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                    }
                    catch (Exception exception)
                    {
                        if ((exception is NullReferenceException) || (exception is SEHException))
                        {
                            throw;
                        }
                    }
                    try
                    {
                        if (!Directory.Exists(PhysicalPath))
                        {
                            Directory.CreateDirectory(PhysicalPath);
                        }
                    }
                    catch (Exception exception2)
                    {
                        if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                        {
                            throw;
                        }
                    }
                    try
                    {
                        if (!Directory.Exists(BinDirectory))
                        {
                            Directory.CreateDirectory(BinDirectory);
                            flag = false;
                        }
                    }
                    catch (Exception exception3)
                    {
                        if ((exception3 is NullReferenceException) || (exception3 is SEHException))
                        {
                            throw;
                        }
                        return flag;
                    }
                }
                catch (Exception exception4)
                {
                    if ((exception4 is NullReferenceException) || (exception4 is SEHException))
                    {
                        throw;
                    }
                    PhysicalPath = string.Empty;
                    BinDirectory = string.Empty;
                    ComSoapPublishError.Report(exception4.ToString());
                }
                return flag;
            }
            return Directory.Exists(BinDirectory);
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string filename);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode)]
        internal static extern int LoadTypeLib([MarshalAs(UnmanagedType.LPWStr)] string file, out ITypeLib tlib);
        public static void ParseUrl(string FullUrl, out string BaseUrl, out string VirtualRoot)
        {
            try
            {
                Uri uri = new Uri(FullUrl);
                string[] segments = uri.Segments;
                VirtualRoot = segments[segments.GetUpperBound(0)];
                BaseUrl = FullUrl.Substring(0, FullUrl.Length - VirtualRoot.Length);
                char[] trimChars = new char[] { '/' };
                VirtualRoot = VirtualRoot.TrimEnd(trimChars);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                BaseUrl = string.Empty;
                VirtualRoot = FullUrl;
            }
            if (BaseUrl.Length <= 0)
            {
                try
                {
                    BaseUrl = "http://";
                    BaseUrl = BaseUrl + Dns.GetHostName();
                    BaseUrl = BaseUrl + "/";
                }
                catch (Exception exception2)
                {
                    if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                    {
                        throw;
                    }
                    ComSoapPublishError.Report(exception2.ToString());
                }
            }
        }

        private void PrivateGacInstall(string AssemblyPath)
        {
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                IAssemblyCache ppAsmCache = null;
                int num = CreateAssemblyCache(out ppAsmCache, 0);
                if (num == 0)
                {
                    num = ppAsmCache.InstallAssembly(0, AssemblyPath, IntPtr.Zero);
                }
                if (num != 0)
                {
                    ComSoapPublishError.Report(Resource.FormatString("Soap_GacInstallFailed") + " " + AssemblyPath);
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
        }

        public void ProcessClientTlb(string ProgId, string SrcTlbPath, string PhysicalPath, string VRoot, string BaseUrl, string Mode, string Transport, out string AssemblyName, out string TypeName, out string Error)
        {
            AssemblyName = "";
            TypeName = "";
            Error = "";
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                string clientPhysicalPath = GetClientPhysicalPath(true);
                if (!SrcTlbPath.ToLower(CultureInfo.InvariantCulture).EndsWith("mscoree.dll", StringComparison.Ordinal) && (SrcTlbPath.Length > 0))
                {
                    AssemblyName = new GenerateMetadata().Generate(SrcTlbPath, clientPhysicalPath);
                    if (ProgId.Length > 0)
                    {
                        TypeName = this.GetTypeNameFromProgId(clientPhysicalPath + AssemblyName + ".dll", ProgId);
                    }
                }
                else if (ProgId.Length > 0)
                {
                    string g = (string) Registry.ClassesRoot.OpenSubKey(ProgId + @"\CLSID").GetValue("");
                    RegistryKey key2 = Registry.ClassesRoot.OpenSubKey(@"CLSID\{" + new Guid(g) + @"}\InprocServer32");
                    AssemblyName = (string) key2.GetValue("Assembly");
                    int index = AssemblyName.IndexOf(",");
                    if (index > 0)
                    {
                        AssemblyName = AssemblyName.Substring(0, index);
                    }
                    TypeName = (string) key2.GetValue("Class");
                }
                if (ProgId.Length > 0)
                {
                    Uri baseUri = new Uri(BaseUrl);
                    Uri uri2 = new Uri(baseUri, VRoot);
                    if (uri2.Scheme.ToLower(CultureInfo.InvariantCulture) == "https")
                    {
                        string authentication = "Windows";
                        SoapClientConfig.Write(clientPhysicalPath, uri2.AbsoluteUri, AssemblyName, TypeName, ProgId, authentication);
                    }
                    else
                    {
                        ClientRemotingConfig.Write(clientPhysicalPath, VRoot, BaseUrl, AssemblyName, TypeName, ProgId, Mode, Transport);
                    }
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                Error = exception.ToString();
                ComSoapPublishError.Report(Error);
            }
        }

        public void ProcessServerTlb(string ProgId, string SrcTlbPath, string PhysicalPath, string Operation, out string strAssemblyName, out string TypeName, out string Error)
        {
            string name = string.Empty;
            strAssemblyName = string.Empty;
            TypeName = string.Empty;
            Error = string.Empty;
            bool flag = false;
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                if ((Operation != null) && (Operation.ToLower(CultureInfo.InvariantCulture) == "delete"))
                {
                    flag = true;
                }
                if (SrcTlbPath.Length > 0)
                {
                    if (!PhysicalPath.EndsWith("/", StringComparison.Ordinal) && !PhysicalPath.EndsWith(@"\", StringComparison.Ordinal))
                    {
                        PhysicalPath = PhysicalPath + @"\";
                    }
                    string str2 = SrcTlbPath.ToLower(CultureInfo.InvariantCulture);
                    if (str2.EndsWith("mscoree.dll", StringComparison.Ordinal))
                    {
                        Type typeFromProgID = Type.GetTypeFromProgID(ProgId);
                        if (typeFromProgID.FullName == "System.__ComObject")
                        {
                            throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_DependencyNotInGAC"));
                        }
                        TypeName = typeFromProgID.FullName;
                        name = typeFromProgID.Assembly.GetName().Name;
                    }
                    else if (str2.EndsWith("scrobj.dll", StringComparison.Ordinal))
                    {
                        if (!flag)
                        {
                            throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_WSCNotSupported"));
                        }
                    }
                    else
                    {
                        GenerateMetadata metadata = new GenerateMetadata();
                        if (flag)
                        {
                            name = metadata.GetAssemblyName(SrcTlbPath, PhysicalPath + @"bin\");
                        }
                        else
                        {
                            name = metadata.GenerateSigned(SrcTlbPath, PhysicalPath + @"bin\", false, out Error);
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            try
                            {
                                TypeName = this.GetTypeNameFromProgId(PhysicalPath + @"bin\" + name + ".dll", ProgId);
                            }
                            catch (DirectoryNotFoundException)
                            {
                                if (!flag)
                                {
                                    throw;
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                if (!flag)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    if ((!string.IsNullOrEmpty(ProgId) && !string.IsNullOrEmpty(name)) && !string.IsNullOrEmpty(TypeName))
                    {
                        ServerWebConfig config = new ServerWebConfig();
                        DiscoFile file = new DiscoFile();
                        string assemblyFile = PhysicalPath + @"bin\" + name + ".dll";
                        if (flag)
                        {
                            config.DeleteElement(PhysicalPath + "Web.Config", name, TypeName, ProgId, "SingleCall", assemblyFile);
                            file.DeleteElement(PhysicalPath + "Default.disco", ProgId + ".soap?WSDL");
                        }
                        else
                        {
                            config.AddGacElement(PhysicalPath + "Web.Config", name, TypeName, ProgId, "SingleCall", assemblyFile);
                            file.AddElement(PhysicalPath + "Default.disco", ProgId + ".soap?WSDL");
                        }
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        string fileName = PhysicalPath + @"bin\" + name + ".dll";
                        strAssemblyName = new AssemblyManager().GetFullName(fileName);
                    }
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                Error = exception.ToString();
                ComSoapPublishError.Report(Error);
                if ((typeof(ServicedComponentException) == exception.GetType()) || (typeof(RegistrationException) == exception.GetType()))
                {
                    throw;
                }
            }
        }

        public void RegisterAssembly(string AssemblyPath)
        {
            try
            {
                RegistryPermission permission = new RegistryPermission(PermissionState.Unrestricted);
                permission.Demand();
                permission.Assert();
                Assembly assembly = Assembly.LoadFrom(AssemblyPath);
                new RegistrationServices().RegisterAssembly(assembly, AssemblyRegistrationFlags.SetCodeBase);
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                foreach (AssemblyName name in assembly.GetReferencedAssemblies())
                {
                    if ((name.Name == "System.EnterpriseServices") && (version < name.Version))
                    {
                        Uri uri = new Uri(assembly.Location);
                        if (uri.IsFile && (uri.LocalPath != ""))
                        {
                            foreach (string str2 in Directory.GetFiles(uri.LocalPath.Remove(uri.LocalPath.Length - Path.GetFileName(uri.LocalPath).Length, Path.GetFileName(uri.LocalPath).Length), "*.tlb"))
                            {
                                ITypeLib lib;
                                object obj2;
                                Guid guid = new Guid("90883F05-3D28-11D2-8F17-00A0C9A6186D");
                                Marshal.ThrowExceptionForHR(LoadTypeLib(str2, out lib));
                                if ((((System.EnterpriseServices.Internal.ITypeLib2) lib).GetCustData(ref guid, out obj2) == 0) && (((string) obj2) == assembly.FullName))
                                {
                                    Marshal.ReleaseComObject(lib);
                                    RegistrationDriver.GenerateTypeLibrary(assembly, str2, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
        }

        public void UnRegisterAssembly(string AssemblyPath)
        {
            try
            {
                RegistryPermission permission = new RegistryPermission(PermissionState.Unrestricted);
                permission.Demand();
                permission.Assert();
                Assembly assembly = Assembly.LoadFrom(AssemblyPath);
                new RegistrationServices().UnregisterAssembly(assembly);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
        }

        private static string MsCorLibDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetAssembly(typeof(object)).Location.Replace('/', '\\'));
            }
        }
    }
}

