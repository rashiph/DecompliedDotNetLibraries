namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Util;

    [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
    internal sealed class RemoteWebConfigurationHost : DelegatingConfigHost
    {
        private string _ConfigPath;
        private string _Domain;
        private WindowsIdentity _Identity;
        private string _Password;
        private Hashtable _PathMap;
        private string _Server;
        private string _Username;
        private const string KEY_MACHINE = "MACHINE";
        private static object s_version = new object();

        internal RemoteWebConfigurationHost()
        {
        }

        private string CallEncryptOrDecrypt(bool doEncrypt, string xmlString, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
            string str = null;
            WindowsImpersonationContext context = null;
            string assemblyQualifiedName = protectionProvider.GetType().AssemblyQualifiedName;
            ProviderSettings settings = protectedConfigSection.Providers[protectionProvider.Name];
            if (settings == null)
            {
                throw System.Web.Util.ExceptionUtil.ParameterInvalid("protectionProvider");
            }
            NameValueCollection parameters = settings.Parameters;
            if (parameters == null)
            {
                parameters = new NameValueCollection();
            }
            string[] allKeys = parameters.AllKeys;
            string[] parameterValues = new string[allKeys.Length];
            for (int i = 0; i < allKeys.Length; i++)
            {
                parameterValues[i] = parameters[allKeys[i]];
            }
            if (this._Identity != null)
            {
                context = this._Identity.Impersonate();
            }
            try
            {
                try
                {
                    IRemoteWebConfigurationHostServer o = CreateRemoteObject(this._Server, this._Username, this._Domain, this._Password);
                    try
                    {
                        str = o.DoEncryptOrDecrypt(doEncrypt, xmlString, protectionProvider.Name, assemblyQualifiedName, allKeys, parameterValues);
                    }
                    finally
                    {
                        while (Marshal.ReleaseComObject(o) > 0)
                        {
                        }
                    }
                    return str;
                }
                finally
                {
                    if (context != null)
                    {
                        context.Undo();
                    }
                }
            }
            catch
            {
            }
            return str;
        }

        internal static IRemoteWebConfigurationHostServer CreateRemoteObject(string server, string username, string domain, string password)
        {
            IRemoteWebConfigurationHostServer server2;
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    return CreateRemoteObjectUsingGetTypeFromCLSID(server);
                }
                if (IntPtr.Size == 8)
                {
                    return CreateRemoteObjectOn64BitPlatform(server, username, domain, password);
                }
                server2 = CreateRemoteObjectOn32BitPlatform(server, username, domain, password);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147221164)
                {
                    throw new Exception(System.Web.SR.GetString("Make_sure_remote_server_is_enabled_for_config_access"));
                }
                throw;
            }
            return server2;
        }

        private static IRemoteWebConfigurationHostServer CreateRemoteObjectOn32BitPlatform(string server, string username, string domain, string password)
        {
            IRemoteWebConfigurationHostServer objectForIUnknown;
            MULTI_QI[] amqi = new MULTI_QI[1];
            IntPtr zero = IntPtr.Zero;
            COAUTHINFO structure = null;
            IntPtr ptr = IntPtr.Zero;
            COSERVERINFO srv = null;
            Guid gUID = typeof(RemoteWebConfigurationHostServer).GUID;
            int errorCode = 0;
            COAUTHIDENTITY coauthidentity = null;
            IntPtr ptr3 = IntPtr.Zero;
            try
            {
                zero = Marshal.AllocCoTaskMem(0x10);
                Marshal.StructureToPtr(typeof(IRemoteWebConfigurationHostServer).GUID, zero, false);
                amqi[0] = new MULTI_QI(zero);
                coauthidentity = new COAUTHIDENTITY(username, domain, password);
                ptr3 = Marshal.AllocCoTaskMem(Marshal.SizeOf(coauthidentity));
                Marshal.StructureToPtr(coauthidentity, ptr3, false);
                structure = new COAUTHINFO(RpcAuthent.WinNT, RpcAuthor.None, null, RpcLevel.Default, RpcImpers.Impersonate, ptr3);
                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, ptr, false);
                srv = new COSERVERINFO(server, ptr);
                errorCode = System.Web.UnsafeNativeMethods.CoCreateInstanceEx(ref gUID, IntPtr.Zero, 0x10, srv, 1, amqi);
                if (errorCode == -2147221164)
                {
                    throw new Exception(System.Web.SR.GetString("Make_sure_remote_server_is_enabled_for_config_access"));
                }
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                if (amqi[0].hr < 0)
                {
                    Marshal.ThrowExceptionForHR(amqi[0].hr);
                }
                errorCode = System.Web.UnsafeNativeMethods.CoSetProxyBlanket(amqi[0].pItf, RpcAuthent.WinNT, RpcAuthor.None, null, RpcLevel.Default, RpcImpers.Impersonate, ptr3, 0);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                objectForIUnknown = (IRemoteWebConfigurationHostServer) Marshal.GetObjectForIUnknown(amqi[0].pItf);
            }
            finally
            {
                if (amqi[0].pItf != IntPtr.Zero)
                {
                    Marshal.Release(amqi[0].pItf);
                    amqi[0].pItf = IntPtr.Zero;
                }
                amqi[0].piid = IntPtr.Zero;
                if (ptr != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(ptr, typeof(COAUTHINFO));
                    Marshal.FreeCoTaskMem(ptr);
                }
                if (ptr3 != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(ptr3, typeof(COAUTHIDENTITY));
                    Marshal.FreeCoTaskMem(ptr3);
                }
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            return objectForIUnknown;
        }

        private static IRemoteWebConfigurationHostServer CreateRemoteObjectOn64BitPlatform(string server, string username, string domain, string password)
        {
            IRemoteWebConfigurationHostServer objectForIUnknown;
            MULTI_QI_X64[] amqi = new MULTI_QI_X64[1];
            IntPtr zero = IntPtr.Zero;
            COAUTHINFO_X64 structure = null;
            IntPtr ptr = IntPtr.Zero;
            COSERVERINFO_X64 srv = null;
            Guid gUID = typeof(RemoteWebConfigurationHostServer).GUID;
            int errorCode = 0;
            COAUTHIDENTITY_X64 coauthidentity_x = null;
            IntPtr ptr3 = IntPtr.Zero;
            try
            {
                zero = Marshal.AllocCoTaskMem(0x10);
                Marshal.StructureToPtr(typeof(IRemoteWebConfigurationHostServer).GUID, zero, false);
                amqi[0] = new MULTI_QI_X64(zero);
                coauthidentity_x = new COAUTHIDENTITY_X64(username, domain, password);
                ptr3 = Marshal.AllocCoTaskMem(Marshal.SizeOf(coauthidentity_x));
                Marshal.StructureToPtr(coauthidentity_x, ptr3, false);
                structure = new COAUTHINFO_X64(RpcAuthent.WinNT, RpcAuthor.None, null, RpcLevel.Default, RpcImpers.Impersonate, ptr3);
                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
                Marshal.StructureToPtr(structure, ptr, false);
                srv = new COSERVERINFO_X64(server, ptr);
                errorCode = System.Web.UnsafeNativeMethods.CoCreateInstanceEx(ref gUID, IntPtr.Zero, 0x10, srv, 1, amqi);
                if (errorCode == -2147221164)
                {
                    throw new Exception(System.Web.SR.GetString("Make_sure_remote_server_is_enabled_for_config_access"));
                }
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                if (amqi[0].hr < 0)
                {
                    Marshal.ThrowExceptionForHR(amqi[0].hr);
                }
                errorCode = System.Web.UnsafeNativeMethods.CoSetProxyBlanket(amqi[0].pItf, RpcAuthent.WinNT, RpcAuthor.None, null, RpcLevel.Default, RpcImpers.Impersonate, ptr3, 0);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                objectForIUnknown = (IRemoteWebConfigurationHostServer) Marshal.GetObjectForIUnknown(amqi[0].pItf);
            }
            finally
            {
                if (amqi[0].pItf != IntPtr.Zero)
                {
                    Marshal.Release(amqi[0].pItf);
                    amqi[0].pItf = IntPtr.Zero;
                }
                amqi[0].piid = IntPtr.Zero;
                if (ptr != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(ptr, typeof(COAUTHINFO_X64));
                    Marshal.FreeCoTaskMem(ptr);
                }
                if (ptr3 != IntPtr.Zero)
                {
                    Marshal.DestroyStructure(ptr3, typeof(COAUTHIDENTITY_X64));
                    Marshal.FreeCoTaskMem(ptr3);
                }
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            return objectForIUnknown;
        }

        private static IRemoteWebConfigurationHostServer CreateRemoteObjectUsingGetTypeFromCLSID(string server)
        {
            return (IRemoteWebConfigurationHostServer) Activator.CreateInstance(Type.GetTypeFromCLSID(typeof(RemoteWebConfigurationHostServer).GUID, server, true));
        }

        public override string DecryptSection(string encryptedXmlString, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
            return this.CallEncryptOrDecrypt(false, encryptedXmlString, protectionProvider, protectedConfigSection);
        }

        public override void DeleteStream(string StreamName)
        {
        }

        public override string EncryptSection(string clearTextXmlString, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
            return this.CallEncryptOrDecrypt(true, clearTextXmlString, protectionProvider, protectedConfigSection);
        }

        private static string GetDomainFromFullName(string fullUserName)
        {
            if (string.IsNullOrEmpty(fullUserName))
            {
                return null;
            }
            if (fullUserName.Contains("@"))
            {
                return null;
            }
            string[] strArray = fullUserName.Split(new char[] { '\\' });
            if (strArray.Length == 1)
            {
                return ".";
            }
            return strArray[0];
        }

        public override void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
        {
            WebConfigurationHost.StaticGetRestrictedPermissions(configRecord, out permissionSet, out isHostReady);
        }

        public override string GetStreamName(string configPath)
        {
            return (string) this._PathMap[configPath];
        }

        public override object GetStreamVersion(string streamName)
        {
            bool flag;
            long num;
            long num2;
            long num3;
            WindowsImpersonationContext context = null;
            try
            {
                if (this._Identity != null)
                {
                    context = this._Identity.Impersonate();
                }
                try
                {
                    IRemoteWebConfigurationHostServer o = CreateRemoteObject(this._Server, this._Username, this._Domain, this._Password);
                    try
                    {
                        o.GetFileDetails(streamName, out flag, out num, out num2, out num3);
                    }
                    finally
                    {
                        while (Marshal.ReleaseComObject(o) > 0)
                        {
                        }
                    }
                }
                finally
                {
                    if (context != null)
                    {
                        context.Undo();
                    }
                }
            }
            catch
            {
                throw;
            }
            return new FileDetails(flag, num, DateTime.FromFileTimeUtc(num2), DateTime.FromFileTimeUtc(num3));
        }

        private static string GetUserNameFromFullName(string fullUserName)
        {
            if (string.IsNullOrEmpty(fullUserName))
            {
                return null;
            }
            if (fullUserName.Contains("@"))
            {
                return fullUserName;
            }
            string[] strArray = fullUserName.Split(new char[] { '\\' });
            if (strArray.Length == 1)
            {
                return fullUserName;
            }
            return strArray[1];
        }

        public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams)
        {
            throw System.Web.Util.ExceptionUtil.UnexpectedError("RemoteWebConfigurationHost::Init");
        }

        public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot root, params object[] hostInitConfigurationParams)
        {
            string str6;
            WebLevel level = (WebLevel) hostInitConfigurationParams[0];
            string str = (string) hostInitConfigurationParams[2];
            string site = (string) hostInitConfigurationParams[3];
            if (locationSubPath == null)
            {
                locationSubPath = (string) hostInitConfigurationParams[4];
            }
            string str3 = (string) hostInitConfigurationParams[5];
            string fullUserName = (string) hostInitConfigurationParams[6];
            string password = (string) hostInitConfigurationParams[7];
            IntPtr userToken = (IntPtr) hostInitConfigurationParams[8];
            configPath = null;
            locationConfigPath = null;
            this._Server = str3;
            this._Username = GetUserNameFromFullName(fullUserName);
            this._Domain = GetDomainFromFullName(fullUserName);
            this._Password = password;
            this._Identity = (userToken == IntPtr.Zero) ? null : new WindowsIdentity(userToken);
            this._PathMap = new Hashtable(StringComparer.OrdinalIgnoreCase);
            try
            {
                WindowsImpersonationContext context = (this._Identity != null) ? this._Identity.Impersonate() : null;
                try
                {
                    IRemoteWebConfigurationHostServer o = CreateRemoteObject(str3, this._Username, this._Domain, password);
                    try
                    {
                        str6 = o.GetFilePaths((int) level, str, site, locationSubPath);
                    }
                    finally
                    {
                        while (Marshal.ReleaseComObject(o) > 0)
                        {
                        }
                    }
                }
                finally
                {
                    if (context != null)
                    {
                        context.Undo();
                    }
                }
            }
            catch
            {
                throw;
            }
            if (str6 == null)
            {
                throw System.Web.Util.ExceptionUtil.UnexpectedError("RemoteWebConfigurationHost::InitForConfiguration");
            }
            string[] strArray = str6.Split(RemoteWebConfigurationHostServer.FilePathsSeparatorParams);
            if ((strArray.Length < 7) || (((strArray.Length - 5) % 2) != 0))
            {
                throw System.Web.Util.ExceptionUtil.UnexpectedError("RemoteWebConfigurationHost::InitForConfiguration");
            }
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].Length == 0)
                {
                    strArray[i] = null;
                }
            }
            string virtualPath = strArray[0];
            string str8 = strArray[1];
            string str9 = strArray[2];
            configPath = strArray[3];
            locationConfigPath = strArray[4];
            this._ConfigPath = configPath;
            WebConfigurationFileMap fileMap = new WebConfigurationFileMap();
            VirtualPath path = VirtualPath.CreateAbsoluteAllowNull(virtualPath);
            fileMap.Site = str9;
            for (int j = 5; j < strArray.Length; j += 2)
            {
                string key = strArray[j];
                string str11 = strArray[j + 1];
                this._PathMap.Add(key, str11);
                if (WebConfigurationHost.IsMachineConfigPath(key))
                {
                    fileMap.MachineConfigFilename = str11;
                }
                else
                {
                    string virtualPathString;
                    bool flag;
                    if (WebConfigurationHost.IsRootWebConfigPath(key))
                    {
                        virtualPathString = null;
                        flag = false;
                    }
                    else
                    {
                        VirtualPath path2;
                        string str13;
                        WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(key, out str13, out path2);
                        virtualPathString = VirtualPath.GetVirtualPathString(path2);
                        flag = path2 == path;
                    }
                    fileMap.VirtualDirectories.Add(virtualPathString, new VirtualDirectoryMapping(Path.GetDirectoryName(str11), flag));
                }
            }
            WebConfigurationHost host = new WebConfigurationHost();
            object[] hostInitParams = new object[6];
            hostInitParams[0] = true;
            hostInitParams[1] = new UserMapPath(fileMap, false);
            hostInitParams[3] = virtualPath;
            hostInitParams[4] = str8;
            hostInitParams[5] = str9;
            host.Init(root, hostInitParams);
            base.Host = host;
        }

        public override bool IsConfigRecordRequired(string configPath)
        {
            return (configPath.Length <= this._ConfigPath.Length);
        }

        public override bool IsFile(string StreamName)
        {
            return false;
        }

        public override Stream OpenStreamForRead(string streamName)
        {
            RemoteWebConfigurationHostStream stream = new RemoteWebConfigurationHostStream(false, this._Server, streamName, null, this._Username, this._Domain, this._Password, this._Identity);
            if ((stream != null) && (stream.Length >= 1L))
            {
                return stream;
            }
            return null;
        }

        public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext)
        {
            RemoteWebConfigurationHostStream stream = new RemoteWebConfigurationHostStream(true, this._Server, streamName, templateStreamName, this._Username, this._Domain, this._Password, this._Identity);
            writeContext = stream;
            return stream;
        }

        public override bool PrefetchAll(string configPath, string StreamName)
        {
            return true;
        }

        public override bool PrefetchSection(string sectionGroupName, string sectionName)
        {
            return true;
        }

        public override void WriteCompleted(string streamName, bool success, object writeContext)
        {
            if (success)
            {
                ((RemoteWebConfigurationHostStream) writeContext).FlushForWriteCompleted();
            }
        }

        public override bool IsRemote
        {
            get
            {
                return true;
            }
        }
    }
}

