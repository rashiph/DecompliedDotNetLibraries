namespace System.Configuration.Internal
{
    using Microsoft.Win32;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;

    internal sealed class InternalConfigHost : IInternalConfigHost
    {
        private IInternalConfigRoot _configRoot;
        private const FileAttributes InvalidAttributesForWrite = (FileAttributes.Hidden | FileAttributes.ReadOnly);

        internal InternalConfigHost()
        {
        }

        public bool IsSecondaryRoot(string configPath)
        {
            return false;
        }

        internal static void StaticDeleteStream(string streamName)
        {
            File.Delete(streamName);
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        internal static string StaticGetStreamNameForConfigSource(string streamName, string configSource)
        {
            if (!Path.IsPathRooted(streamName))
            {
                throw ExceptionUtil.ParameterInvalid("streamName");
            }
            streamName = Path.GetFullPath(streamName);
            string directoryOrRootName = UrlPath.GetDirectoryOrRootName(streamName);
            string fullPath = Path.GetFullPath(Path.Combine(directoryOrRootName, configSource));
            string subdir = UrlPath.GetDirectoryOrRootName(fullPath);
            if (!UrlPath.IsEqualOrSubdirectory(directoryOrRootName, subdir))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Config_source_not_under_config_dir", new object[] { configSource }));
            }
            return fullPath;
        }

        internal static object StaticGetStreamVersion(string streamName)
        {
            Microsoft.Win32.UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA win_file_attribute_data;
            bool exists = false;
            long fileSize = 0L;
            DateTime minValue = DateTime.MinValue;
            DateTime utcLastWriteTime = DateTime.MinValue;
            if (Microsoft.Win32.UnsafeNativeMethods.GetFileAttributesEx(streamName, 0, out win_file_attribute_data) && ((win_file_attribute_data.fileAttributes & 0x10) == 0))
            {
                exists = true;
                fileSize = (win_file_attribute_data.fileSizeHigh << 0x20) | win_file_attribute_data.fileSizeLow;
                minValue = DateTime.FromFileTimeUtc((long) ((win_file_attribute_data.ftCreationTimeHigh << 0x20) | win_file_attribute_data.ftCreationTimeLow));
                utcLastWriteTime = DateTime.FromFileTimeUtc((long) ((win_file_attribute_data.ftLastWriteTimeHigh << 0x20) | win_file_attribute_data.ftLastWriteTimeLow));
            }
            return new FileVersion(exists, fileSize, minValue, utcLastWriteTime);
        }

        internal static bool StaticIsFile(string streamName)
        {
            return Path.IsPathRooted(streamName);
        }

        internal static Stream StaticOpenStreamForRead(string streamName)
        {
            if (string.IsNullOrEmpty(streamName))
            {
                throw ExceptionUtil.UnexpectedError("InternalConfigHost::StaticOpenStreamForRead");
            }
            if (!FileUtil.FileExists(streamName, true))
            {
                return null;
            }
            return new FileStream(streamName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        internal static Stream StaticOpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext, bool assertPermissions)
        {
            Stream stream;
            bool flag = false;
            if (string.IsNullOrEmpty(streamName))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_no_stream_to_write"));
            }
            string directoryName = Path.GetDirectoryName(streamName);
            try
            {
                if (!Directory.Exists(directoryName))
                {
                    if (assertPermissions)
                    {
                        new FileIOPermission(PermissionState.Unrestricted).Assert();
                        flag = true;
                    }
                    Directory.CreateDirectory(directoryName);
                }
            }
            catch
            {
            }
            finally
            {
                if (flag)
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            WriteFileContext context = null;
            flag = false;
            if (assertPermissions)
            {
                new FileIOPermission(FileIOPermissionAccess.AllAccess, directoryName).Assert();
                flag = true;
            }
            try
            {
                context = new WriteFileContext(streamName, templateStreamName);
                if (File.Exists(streamName))
                {
                    FileInfo info = new FileInfo(streamName);
                    if ((info.Attributes & (FileAttributes.Hidden | FileAttributes.ReadOnly)) != 0)
                    {
                        throw new IOException(System.Configuration.SR.GetString("Config_invalid_attributes_for_write", new object[] { streamName }));
                    }
                }
                try
                {
                    stream = new FileStream(context.TempNewFilename, FileMode.Create, FileAccess.Write, FileShare.Read);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_write_failed", new object[] { streamName }), exception);
                }
            }
            catch
            {
                if (context != null)
                {
                    context.Complete(streamName, false);
                }
                throw;
            }
            finally
            {
                if (flag)
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            writeContext = context;
            return stream;
        }

        internal static void StaticWriteCompleted(string streamName, bool success, object writeContext, bool assertPermissions)
        {
            WriteFileContext context = (WriteFileContext) writeContext;
            bool flag = false;
            if (assertPermissions)
            {
                string directoryName = Path.GetDirectoryName(streamName);
                string[] pathList = new string[] { streamName, context.TempNewFilename, directoryName };
                new FileIOPermission(FileIOPermissionAccess.AllAccess, AccessControlActions.Change | AccessControlActions.View, pathList).Assert();
                flag = true;
            }
            try
            {
                context.Complete(streamName, success);
            }
            finally
            {
                if (flag)
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
        }

        object IInternalConfigHost.CreateConfigurationContext(string configPath, string locationSubPath)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.CreateConfigurationContext");
        }

        object IInternalConfigHost.CreateDeprecatedConfigContext(string configPath)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.CreateDeprecatedConfigContext");
        }

        string IInternalConfigHost.DecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
            return ProtectedConfigurationSection.DecryptSection(encryptedXml, protectionProvider);
        }

        void IInternalConfigHost.DeleteStream(string streamName)
        {
            StaticDeleteStream(streamName);
        }

        string IInternalConfigHost.EncryptSection(string clearTextXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
            return ProtectedConfigurationSection.EncryptSection(clearTextXml, protectionProvider);
        }

        string IInternalConfigHost.GetConfigPathFromLocationSubPath(string configPath, string locationSubPath)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.GetConfigPathFromLocationSubPath");
        }

        Type IInternalConfigHost.GetConfigType(string typeName, bool throwOnError)
        {
            return Type.GetType(typeName, throwOnError);
        }

        string IInternalConfigHost.GetConfigTypeName(Type t)
        {
            return t.AssemblyQualifiedName;
        }

        void IInternalConfigHost.GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
        {
            permissionSet = null;
            isHostReady = true;
        }

        string IInternalConfigHost.GetStreamName(string configPath)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.GetStreamName");
        }

        string IInternalConfigHost.GetStreamNameForConfigSource(string streamName, string configSource)
        {
            return StaticGetStreamNameForConfigSource(streamName, configSource);
        }

        object IInternalConfigHost.GetStreamVersion(string streamName)
        {
            return StaticGetStreamVersion(streamName);
        }

        IDisposable IInternalConfigHost.Impersonate()
        {
            return null;
        }

        void IInternalConfigHost.Init(IInternalConfigRoot configRoot, params object[] hostInitParams)
        {
            this._configRoot = configRoot;
        }

        void IInternalConfigHost.InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams)
        {
            this._configRoot = configRoot;
            configPath = null;
            locationConfigPath = null;
        }

        bool IInternalConfigHost.IsAboveApplication(string configPath)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.IsAboveApplication");
        }

        bool IInternalConfigHost.IsConfigRecordRequired(string configPath)
        {
            return true;
        }

        bool IInternalConfigHost.IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
        {
            return true;
        }

        bool IInternalConfigHost.IsFile(string streamName)
        {
            return StaticIsFile(streamName);
        }

        bool IInternalConfigHost.IsFullTrustSectionWithoutAptcaAllowed(IInternalConfigRecord configRecord)
        {
            return System.Configuration.TypeUtil.IsCallerFullTrust;
        }

        bool IInternalConfigHost.IsInitDelayed(IInternalConfigRecord configRecord)
        {
            return false;
        }

        bool IInternalConfigHost.IsLocationApplicable(string configPath)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.IsLocationApplicable");
        }

        bool IInternalConfigHost.IsTrustedConfigPath(string configPath)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.IsTrustedConfigPath");
        }

        Stream IInternalConfigHost.OpenStreamForRead(string streamName)
        {
            return ((IInternalConfigHost) this).OpenStreamForRead(streamName, false);
        }

        Stream IInternalConfigHost.OpenStreamForRead(string streamName, bool assertPermissions)
        {
            Stream stream = null;
            bool flag = false;
            if (assertPermissions || !this._configRoot.IsDesignTime)
            {
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, streamName).Assert();
                flag = true;
            }
            try
            {
                stream = StaticOpenStreamForRead(streamName);
            }
            finally
            {
                if (flag)
                {
                    CodeAccessPermission.RevertAssert();
                }
            }
            return stream;
        }

        Stream IInternalConfigHost.OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext)
        {
            return ((IInternalConfigHost) this).OpenStreamForWrite(streamName, templateStreamName, ref writeContext, false);
        }

        Stream IInternalConfigHost.OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext, bool assertPermissions)
        {
            return StaticOpenStreamForWrite(streamName, templateStreamName, ref writeContext, assertPermissions);
        }

        bool IInternalConfigHost.PrefetchAll(string configPath, string streamName)
        {
            return false;
        }

        bool IInternalConfigHost.PrefetchSection(string sectionGroupName, string sectionName)
        {
            return false;
        }

        void IInternalConfigHost.RequireCompleteInit(IInternalConfigRecord configRecord)
        {
        }

        object IInternalConfigHost.StartMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.StartMonitoringStreamForChanges");
        }

        void IInternalConfigHost.StopMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
        {
            throw ExceptionUtil.UnexpectedError("IInternalConfigHost.StopMonitoringStreamForChanges");
        }

        void IInternalConfigHost.VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo)
        {
        }

        void IInternalConfigHost.WriteCompleted(string streamName, bool success, object writeContext)
        {
            ((IInternalConfigHost) this).WriteCompleted(streamName, success, writeContext, false);
        }

        void IInternalConfigHost.WriteCompleted(string streamName, bool success, object writeContext, bool assertPermissions)
        {
            StaticWriteCompleted(streamName, success, writeContext, assertPermissions);
        }

        bool IInternalConfigHost.IsRemote
        {
            get
            {
                return false;
            }
        }

        bool IInternalConfigHost.SupportsChangeNotifications
        {
            get
            {
                return false;
            }
        }

        bool IInternalConfigHost.SupportsLocation
        {
            get
            {
                return false;
            }
        }

        bool IInternalConfigHost.SupportsPath
        {
            get
            {
                return false;
            }
        }

        bool IInternalConfigHost.SupportsRefresh
        {
            get
            {
                return false;
            }
        }
    }
}

