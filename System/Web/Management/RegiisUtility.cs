namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;

    public sealed class RegiisUtility : IRegiisUtility
    {
        private const string DefaultRsaKeyContainerName = "NetFrameworkConfigurationKey";
        private const long DO_RSA_ACL_KEY_ADD = 0x1000000000L;
        private const long DO_RSA_ACL_KEY_DEL = 0x2000000000L;
        private const long DO_RSA_ADD_KEY = 0x400000000L;
        private const long DO_RSA_DECRYPT = 0x200000000L;
        private const long DO_RSA_DECRYPT_FILE = 0x8000000000000L;
        private const long DO_RSA_DEL_KEY = 0x800000000L;
        private const long DO_RSA_ENCRYPT = 0x100000000L;
        private const long DO_RSA_ENCRYPT_FILE = 0x4000000000000L;
        private const long DO_RSA_EXPORT_KEY = 0x4000000000L;
        private const long DO_RSA_EXPORTABLE = 0x400000000000L;
        private const long DO_RSA_FULL_ACCESS = 0x800000000000L;
        private const long DO_RSA_IMPORT_KEY = 0x8000000000L;
        private const long DO_RSA_PKM = 0x80000000000L;
        private const long DO_RSA_PKU = 0x100000000000L;
        private const long DO_RSA_PRIVATE = 0x1000000000000L;
        private const string NewLine = "\n\r";
        private const int WATSettingAuthMode = 3;
        private const int WATSettingAuthSettings = 2;
        private const int WATSettingLocalOnly = 0;
        private const int WATSettingMax = 4;
        private const int WATSettingRequireSSL = 1;
        private const int WATValueDoNothing = 0;
        private const int WATValueFalse = 2;
        private const int WATValueForms = 5;
        private const int WATValueHosted = 3;
        private const int WATValueLocal = 4;
        private const int WATValueTrue = 1;
        private const int WATValueWindows = 6;

        private RsaProtectedConfigurationProvider CreateRSAProvider(string containerName, string csp, long options)
        {
            RsaProtectedConfigurationProvider provider = new RsaProtectedConfigurationProvider();
            NameValueCollection config = new NameValueCollection();
            config.Add("keyContainerName", containerName);
            config.Add("cspProviderName", csp);
            config.Add("useMachineContainer", ((options & 0x100000000000L) != 0L) ? "false" : "true");
            provider.Initialize("foo", config);
            return provider;
        }

        private void DoKeyAclChange(string containerName, string account, string csp, long options)
        {
            if ((containerName == null) || (containerName.Length < 1))
            {
                containerName = "NetFrameworkConfigurationKey";
            }
            MakeSureContainerExists(containerName, csp, (options & 0x100000000000L) == 0L);
            int num = 0;
            if ((options & 0x1000000000L) != 0L)
            {
                num |= 1;
            }
            if ((options & 0x100000000000L) == 0L)
            {
                num |= 2;
            }
            if ((options & 0x800000000000L) != 0L)
            {
                num |= 4;
            }
            int errorCode = System.Web.UnsafeNativeMethods.ChangeAccessToKeyContainer(containerName, account, csp, num);
            if (errorCode != 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        private void DoKeyCreate(string containerName, string csp, long options, int keySize)
        {
            if ((containerName == null) || (containerName.Length < 1))
            {
                containerName = "NetFrameworkConfigurationKey";
            }
            uint num = (uint) System.Web.UnsafeNativeMethods.DoesKeyContainerExist(containerName, csp, ((options & 0x100000000000L) == 0L) ? 1 : 0);
            switch (num)
            {
                case 0:
                    throw new Exception(System.Web.SR.GetString("RSA_Key_Container_already_exists"));

                case 0x80070005:
                    throw new Exception(System.Web.SR.GetString("RSA_Key_Container_access_denied"));

                case 0x80090016:
                {
                    RsaProtectedConfigurationProvider provider = this.CreateRSAProvider(containerName, csp, options);
                    try
                    {
                        provider.AddKey(keySize, (options & 0x400000000000L) != 0L);
                    }
                    catch
                    {
                        provider.DeleteKey();
                        throw;
                    }
                    return;
                }
            }
            Marshal.ThrowExceptionForHR((int) num);
        }

        private void DoKeyDelete(string containerName, string csp, long options)
        {
            if ((containerName == null) || (containerName.Length < 1))
            {
                containerName = "NetFrameworkConfigurationKey";
            }
            MakeSureContainerExists(containerName, csp, (options & 0x100000000000L) == 0L);
            this.CreateRSAProvider(containerName, csp, options).DeleteKey();
        }

        private void DoKeyExport(string containerName, string fileName, string csp, long options)
        {
            if (!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(Environment.CurrentDirectory, fileName);
            }
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                throw new DirectoryNotFoundException();
            }
            if ((containerName == null) || (containerName.Length < 1))
            {
                containerName = "NetFrameworkConfigurationKey";
            }
            MakeSureContainerExists(containerName, csp, (options & 0x100000000000L) == 0L);
            this.CreateRSAProvider(containerName, csp, options).ExportKey(fileName, (options & 0x1000000000000L) != 0L);
        }

        private void DoKeyImport(string containerName, string fileName, string csp, long options)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException();
            }
            if ((containerName == null) || (containerName.Length < 1))
            {
                containerName = "NetFrameworkConfigurationKey";
            }
            this.CreateRSAProvider(containerName, csp, options).ImportKey(fileName, (options & 0x400000000000L) != 0L);
        }

        private void DoProtectSection(string configSection, string providerName, string appPath, string site, string location, bool useMachineConfig)
        {
            System.Configuration.Configuration configuration;
            ConfigurationSection section = this.GetConfigSection(configSection, appPath, site, location, useMachineConfig, out configuration);
            if (section == null)
            {
                throw new Exception(System.Web.SR.GetString("Configuration_Section_not_found", new object[] { configSection }));
            }
            section.SectionInformation.ProtectSection(providerName);
            configuration.Save();
        }

        private void DoProtectSectionFile(string configSection, string dirName, string providerName)
        {
            System.Configuration.Configuration configuration;
            ConfigurationSection section = this.GetConfigSectionFile(configSection, dirName, out configuration);
            if (section == null)
            {
                throw new Exception(System.Web.SR.GetString("Configuration_Section_not_found", new object[] { configSection }));
            }
            section.SectionInformation.ProtectSection(providerName);
            configuration.Save();
        }

        private void DoUnprotectSection(string configSection, string appPath, string site, string location, bool useMachineConfig)
        {
            System.Configuration.Configuration configuration;
            ConfigurationSection section = this.GetConfigSection(configSection, appPath, site, location, useMachineConfig, out configuration);
            if (section == null)
            {
                throw new Exception(System.Web.SR.GetString("Configuration_Section_not_found", new object[] { configSection }));
            }
            section.SectionInformation.UnprotectSection();
            configuration.Save();
        }

        private void DoUnprotectSectionFile(string configSection, string dirName)
        {
            System.Configuration.Configuration configuration;
            ConfigurationSection section = this.GetConfigSectionFile(configSection, dirName, out configuration);
            if (section == null)
            {
                throw new Exception(System.Web.SR.GetString("Configuration_Section_not_found", new object[] { configSection }));
            }
            section.SectionInformation.UnprotectSection();
            configuration.Save();
        }

        private ConfigurationSection GetConfigSection(string configSection, string appPath, string site, string location, bool useMachineConfig, out System.Configuration.Configuration config)
        {
            if (string.IsNullOrEmpty(appPath))
            {
                appPath = null;
            }
            if (string.IsNullOrEmpty(location))
            {
                location = null;
            }
            try
            {
                if (useMachineConfig)
                {
                    config = WebConfigurationManager.OpenMachineConfiguration(location);
                }
                else
                {
                    config = WebConfigurationManager.OpenWebConfiguration(appPath, site, location);
                }
            }
            catch (Exception exception)
            {
                if (useMachineConfig)
                {
                    throw new Exception(System.Web.SR.GetString("Configuration_for_machine_config_not_found"), exception);
                }
                throw new Exception(System.Web.SR.GetString("Configuration_for_path_not_found", new object[] { appPath, string.IsNullOrEmpty(site) ? System.Web.SR.GetString("DefaultSiteName") : site }), exception);
            }
            return config.GetSection(configSection);
        }

        private ConfigurationSection GetConfigSectionFile(string configSection, string dirName, out System.Configuration.Configuration config)
        {
            if (dirName == ".")
            {
                dirName = Environment.CurrentDirectory;
            }
            else
            {
                if (!Path.IsPathRooted(dirName))
                {
                    dirName = Path.Combine(Environment.CurrentDirectory, dirName);
                }
                if (!Directory.Exists(dirName))
                {
                    throw new Exception(System.Web.SR.GetString("Configuration_for_physical_path_not_found", new object[] { dirName }));
                }
            }
            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
            string virtualDirectory = dirName.Replace('\\', '/');
            if ((virtualDirectory.Length > 2) && (virtualDirectory[1] == ':'))
            {
                virtualDirectory = virtualDirectory.Substring(2);
            }
            else if (virtualDirectory.StartsWith("//", StringComparison.Ordinal))
            {
                virtualDirectory = "/";
            }
            fileMap.VirtualDirectories.Add(virtualDirectory, new VirtualDirectoryMapping(dirName, true));
            try
            {
                config = WebConfigurationManager.OpenMappedWebConfiguration(fileMap, virtualDirectory);
            }
            catch (Exception exception)
            {
                throw new Exception(System.Web.SR.GetString("Configuration_for_physical_path_not_found", new object[] { dirName }), exception);
            }
            return config.GetSection(configSection);
        }

        private void GetExceptionMessage(Exception exception, StringBuilder sb)
        {
            if (sb.Length != 0)
            {
                sb.Append("\n\r");
            }
            if (exception is ConfigurationErrorsException)
            {
                foreach (ConfigurationErrorsException exception2 in ((ConfigurationErrorsException) exception).Errors)
                {
                    sb.Append(exception2.Message);
                    sb.Append("\n\r");
                    if (exception2.InnerException != null)
                    {
                        sb.Append("\n\r");
                        sb.Append(exception2.InnerException.Message);
                        sb.Append("\n\r");
                    }
                }
            }
            else
            {
                sb.Append(exception.Message);
                sb.Append("\n\r");
                if (exception.InnerException != null)
                {
                    this.GetExceptionMessage(exception.InnerException, sb);
                }
            }
        }

        private static void MakeSureContainerExists(string containerName, string csp, bool machineContainer)
        {
            uint num = (uint) System.Web.UnsafeNativeMethods.DoesKeyContainerExist(containerName, csp, machineContainer ? 1 : 0);
            switch (num)
            {
                case 0:
                    return;

                case 0x80070005:
                    throw new Exception(System.Web.SR.GetString("RSA_Key_Container_access_denied"));

                case 0x80090016:
                    throw new Exception(System.Web.SR.GetString("RSA_Key_Container_not_found"));
            }
            Marshal.ThrowExceptionForHR((int) num);
        }

        public void ProtectedConfigAction(long options, string firstArgument, string secondArgument, string providerName, string appPath, string site, string cspOrLocation, int keySize, out IntPtr exception)
        {
            exception = IntPtr.Zero;
            try
            {
                if ((options & 0x100000000L) != 0L)
                {
                    this.DoProtectSection(firstArgument, providerName, appPath, site, cspOrLocation, (options & 0x80000000000L) != 0L);
                }
                else if ((options & 0x200000000L) != 0L)
                {
                    this.DoUnprotectSection(firstArgument, appPath, site, cspOrLocation, (options & 0x80000000000L) != 0L);
                }
                else if ((options & 0x4000000000000L) != 0L)
                {
                    this.DoProtectSectionFile(firstArgument, secondArgument, providerName);
                }
                else if ((options & 0x8000000000000L) != 0L)
                {
                    this.DoUnprotectSectionFile(firstArgument, secondArgument);
                }
                else if ((options & 0x400000000L) != 0L)
                {
                    this.DoKeyCreate(firstArgument, cspOrLocation, options, keySize);
                }
                else if ((options & 0x800000000L) != 0L)
                {
                    this.DoKeyDelete(firstArgument, cspOrLocation, options);
                }
                else if ((options & 0x4000000000L) != 0L)
                {
                    this.DoKeyExport(firstArgument, secondArgument, cspOrLocation, options);
                }
                else if ((options & 0x8000000000L) != 0L)
                {
                    this.DoKeyImport(firstArgument, secondArgument, cspOrLocation, options);
                }
                else if (((options & 0x1000000000L) != 0L) || ((options & 0x2000000000L) != 0L))
                {
                    this.DoKeyAclChange(firstArgument, secondArgument, cspOrLocation, options);
                }
                else
                {
                    exception = Marshal.StringToBSTR(System.Web.SR.GetString("Command_not_recognized"));
                }
            }
            catch (Exception exception2)
            {
                StringBuilder sb = new StringBuilder();
                this.GetExceptionMessage(exception2, sb);
                exception = Marshal.StringToBSTR(sb.ToString());
            }
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public void RegisterAsnetMmcAssembly(int doReg, string typeName, string binaryDirectory, out IntPtr exception)
        {
            exception = IntPtr.Zero;
            try
            {
                Assembly assembly = Assembly.GetAssembly(Type.GetType(typeName, true));
                RegistrationServices services = new RegistrationServices();
                if (doReg != 0)
                {
                    if (!services.RegisterAssembly(assembly, AssemblyRegistrationFlags.None))
                    {
                        exception = Marshal.StringToBSTR(new Exception(System.Web.SR.GetString("Unable_To_Register_Assembly", new object[] { assembly.FullName })).ToString());
                    }
                    TypeLibConverter converter = new TypeLibConverter();
                    ConversionEventSink notifySink = new ConversionEventSink();
                    ((IRegisterCreateITypeLib) converter.ConvertAssemblyToTypeLib(assembly, Path.Combine(binaryDirectory, "AspNetMMCExt.tlb"), TypeLibExporterFlags.None, notifySink)).SaveAllChanges();
                }
                else
                {
                    if (!services.UnregisterAssembly(assembly))
                    {
                        exception = Marshal.StringToBSTR(new Exception(System.Web.SR.GetString("Unable_To_UnRegister_Assembly", new object[] { assembly.FullName })).ToString());
                    }
                    try
                    {
                        File.Delete(Path.Combine(binaryDirectory, "AspNetMMCExt.tlb"));
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception exception2)
            {
                exception = Marshal.StringToBSTR(exception2.ToString());
            }
        }

        public void RegisterSystemWebAssembly(int doReg, out IntPtr exception)
        {
            exception = IntPtr.Zero;
            try
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                RegistrationServices services = new RegistrationServices();
                if (doReg != 0)
                {
                    if (!services.RegisterAssembly(executingAssembly, AssemblyRegistrationFlags.None))
                    {
                        exception = Marshal.StringToBSTR(new Exception(System.Web.SR.GetString("Unable_To_Register_Assembly", new object[] { executingAssembly.FullName })).ToString());
                    }
                }
                else if (!services.UnregisterAssembly(executingAssembly))
                {
                    exception = Marshal.StringToBSTR(new Exception(System.Web.SR.GetString("Unable_To_UnRegister_Assembly", new object[] { executingAssembly.FullName })).ToString());
                }
            }
            catch (Exception exception2)
            {
                exception = Marshal.StringToBSTR(exception2.ToString());
            }
        }

        public void RemoveBrowserCaps(out IntPtr exception)
        {
            try
            {
                new BrowserCapabilitiesCodeGenerator().UninstallInternal();
                exception = IntPtr.Zero;
            }
            catch (Exception exception2)
            {
                exception = Marshal.StringToBSTR(exception2.Message);
            }
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00020406-0000-0000-C000-000000000046"), ComVisible(false)]
        private interface IRegisterCreateITypeLib
        {
            void CreateTypeInfo();
            void SetName();
            void SetVersion();
            void SetGuid();
            void SetDocString();
            void SetHelpFileName();
            void SetHelpContext();
            void SetLcid();
            void SetLibFlags();
            void SaveAllChanges();
        }
    }
}

