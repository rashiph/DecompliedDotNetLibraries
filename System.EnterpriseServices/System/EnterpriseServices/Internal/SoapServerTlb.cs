namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;

    [Guid("F6B6768F-F99E-4152-8ED2-0412F78517FB")]
    public sealed class SoapServerTlb : ISoapServerTlb
    {
        public void AddServerTlb(string progId, string classId, string interfaceId, string srcTlbPath, string rootWebServer, string inBaseUrl, string inVirtualRoot, string clientActivated, string wellKnown, string discoFile, string operation, out string strAssemblyName, out string typeName)
        {
            string name = string.Empty;
            strAssemblyName = string.Empty;
            typeName = string.Empty;
            bool flag = false;
            bool inDefault = false;
            bool flag3 = false;
            bool flag4 = true;
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
                if ((operation != null) && (operation.ToLower(CultureInfo.InvariantCulture) == "delete"))
                {
                    flag = true;
                }
                if (srcTlbPath.Length > 0)
                {
                    inDefault = SoapServerInfo.BoolFromString(discoFile, inDefault);
                    flag3 = SoapServerInfo.BoolFromString(wellKnown, flag3);
                    flag4 = SoapServerInfo.BoolFromString(clientActivated, flag4);
                    string str3 = SoapServerInfo.ServerPhysicalPath(rootWebServer, inBaseUrl, inVirtualRoot, !flag);
                    string str4 = srcTlbPath.ToLower(CultureInfo.InvariantCulture);
                    if (str4.EndsWith("mscoree.dll", StringComparison.Ordinal))
                    {
                        Type typeFromProgID = Type.GetTypeFromProgID(progId);
                        typeName = typeFromProgID.FullName;
                        name = typeFromProgID.Assembly.GetName().Name;
                    }
                    else if (str4.EndsWith("scrobj.dll", StringComparison.Ordinal))
                    {
                        if (!flag)
                        {
                            throw new ServicedComponentException(Resource.FormatString("ServicedComponentException_WSCNotSupported"));
                        }
                    }
                    else
                    {
                        string error = "";
                        GenerateMetadata metadata = new GenerateMetadata();
                        if (flag)
                        {
                            name = metadata.GetAssemblyName(srcTlbPath, str3 + @"\bin\");
                        }
                        else
                        {
                            name = metadata.GenerateSigned(srcTlbPath, str3 + @"\bin\", false, out error);
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            try
                            {
                                typeName = this.GetTypeName(str3 + @"\bin\" + name + ".dll", progId, classId);
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
                    if ((!string.IsNullOrEmpty(progId) && !string.IsNullOrEmpty(name)) && !string.IsNullOrEmpty(typeName))
                    {
                        DiscoFile file = new DiscoFile();
                        string assemblyFile = str3 + @"\bin\" + name + ".dll";
                        if (flag)
                        {
                            SoapServerConfig.DeleteComponent(str3 + @"\Web.Config", name, typeName, progId, assemblyFile);
                            file.DeleteElement(str3 + @"\Default.disco", progId + ".soap?WSDL");
                        }
                        else
                        {
                            SoapServerConfig.AddComponent(str3 + @"\Web.Config", name, typeName, progId, assemblyFile, "SingleCall", flag3, flag4);
                            if (inDefault)
                            {
                                file.AddElement(str3 + @"\Default.disco", progId + ".soap?WSDL");
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        string fileName = str3 + @"bin\" + name + ".dll";
                        strAssemblyName = new AssemblyManager().GetFullName(fileName);
                    }
                }
            }
            catch (ServicedComponentException exception)
            {
                this.ThrowHelper("Soap_PublishServerTlbFailure", exception);
            }
            catch (RegistrationException exception2)
            {
                this.ThrowHelper("Soap_PublishServerTlbFailure", exception2);
            }
            catch (Exception exception3)
            {
                if ((exception3 is NullReferenceException) || (exception3 is SEHException))
                {
                    throw;
                }
                this.ThrowHelper("Soap_PublishServerTlbFailure", null);
            }
        }

        public void DeleteServerTlb(string progId, string classId, string interfaceId, string srcTlbPath, string rootWebServer, string baseUrl, string virtualRoot, string operation, string assemblyName, string typeName)
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
            string name = assemblyName;
            if ((((progId.Length > 0) || (classId.Length > 0)) || ((assemblyName.Length > 0) || (typeName.Length > 0))) && ((baseUrl.Length > 0) || (virtualRoot.Length > 0)))
            {
                string str3 = SoapServerInfo.ServerPhysicalPath(rootWebServer, baseUrl, virtualRoot, false);
                string str4 = srcTlbPath.ToLower(CultureInfo.InvariantCulture);
                if (!str4.EndsWith("scrobj.dll", StringComparison.Ordinal))
                {
                    if (str4.EndsWith("mscoree.dll", StringComparison.Ordinal))
                    {
                        Type typeFromProgID = Type.GetTypeFromProgID(progId);
                        typeName = typeFromProgID.FullName;
                        name = typeFromProgID.Assembly.GetName().Name;
                    }
                    else
                    {
                        name = new GenerateMetadata().GetAssemblyName(srcTlbPath, str3 + @"\bin\");
                        if (name.Length > 0)
                        {
                            try
                            {
                                typeName = this.GetTypeName(str3 + @"\bin\" + name + ".dll", progId, classId);
                            }
                            catch (DirectoryNotFoundException)
                            {
                            }
                            catch (FileNotFoundException)
                            {
                            }
                        }
                    }
                    if ((!string.IsNullOrEmpty(progId) && !string.IsNullOrEmpty(name)) && !string.IsNullOrEmpty(typeName))
                    {
                        DiscoFile file = new DiscoFile();
                        string assemblyFile = str3 + @"\bin\" + name + ".dll";
                        SoapServerConfig.DeleteComponent(str3 + @"\Web.Config", name, typeName, progId, assemblyFile);
                        file.DeleteElement(str3 + @"\Default.disco", progId + ".soap?WSDL");
                    }
                }
            }
        }

        internal string GetTypeName(string assemblyPath, string progId, string classId)
        {
            string typeNameFromProgId = "";
            AssemblyManager manager = null;
            AppDomain domain = AppDomain.CreateDomain("SoapDomain");
            if (domain != null)
            {
                try
                {
                    AssemblyName name = typeof(AssemblyManager).Assembly.GetName();
                    ObjectHandle handle = domain.CreateInstance(name.FullName, typeof(AssemblyManager).FullName, false, BindingFlags.Default, null, null, null, null);
                    if (handle == null)
                    {
                        return typeNameFromProgId;
                    }
                    manager = (AssemblyManager) handle.Unwrap();
                    if (classId.Length > 0)
                    {
                        return manager.InternalGetTypeNameFromClassId(assemblyPath, classId);
                    }
                    typeNameFromProgId = manager.InternalGetTypeNameFromProgId(assemblyPath, progId);
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
            return typeNameFromProgId;
        }

        private void ThrowHelper(string messageId, Exception e)
        {
            ComSoapPublishError.Report(Resource.FormatString(messageId));
            if (e != null)
            {
                throw e;
            }
        }
    }
}

