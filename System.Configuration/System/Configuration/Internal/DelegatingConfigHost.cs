namespace System.Configuration.Internal
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;

    public class DelegatingConfigHost : IInternalConfigHost
    {
        private IInternalConfigHost _host;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected DelegatingConfigHost()
        {
        }

        public virtual object CreateConfigurationContext(string configPath, string locationSubPath)
        {
            return this.Host.CreateConfigurationContext(configPath, locationSubPath);
        }

        public virtual object CreateDeprecatedConfigContext(string configPath)
        {
            return this.Host.CreateDeprecatedConfigContext(configPath);
        }

        public virtual string DecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
            return this.Host.DecryptSection(encryptedXml, protectionProvider, protectedConfigSection);
        }

        public virtual void DeleteStream(string streamName)
        {
            this.Host.DeleteStream(streamName);
        }

        public virtual string EncryptSection(string clearTextXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfigSection)
        {
            return this.Host.EncryptSection(clearTextXml, protectionProvider, protectedConfigSection);
        }

        public virtual string GetConfigPathFromLocationSubPath(string configPath, string locationSubPath)
        {
            return this.Host.GetConfigPathFromLocationSubPath(configPath, locationSubPath);
        }

        public virtual Type GetConfigType(string typeName, bool throwOnError)
        {
            return this.Host.GetConfigType(typeName, throwOnError);
        }

        public virtual string GetConfigTypeName(Type t)
        {
            return this.Host.GetConfigTypeName(t);
        }

        public virtual void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
        {
            this.Host.GetRestrictedPermissions(configRecord, out permissionSet, out isHostReady);
        }

        public virtual string GetStreamName(string configPath)
        {
            return this.Host.GetStreamName(configPath);
        }

        public virtual string GetStreamNameForConfigSource(string streamName, string configSource)
        {
            return this.Host.GetStreamNameForConfigSource(streamName, configSource);
        }

        public virtual object GetStreamVersion(string streamName)
        {
            return this.Host.GetStreamVersion(streamName);
        }

        public virtual IDisposable Impersonate()
        {
            return this.Host.Impersonate();
        }

        public virtual void Init(IInternalConfigRoot configRoot, params object[] hostInitParams)
        {
            this.Host.Init(configRoot, hostInitParams);
        }

        public virtual void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams)
        {
            this.Host.InitForConfiguration(ref locationSubPath, out configPath, out locationConfigPath, configRoot, hostInitConfigurationParams);
        }

        public virtual bool IsAboveApplication(string configPath)
        {
            return this.Host.IsAboveApplication(configPath);
        }

        public virtual bool IsConfigRecordRequired(string configPath)
        {
            return this.Host.IsConfigRecordRequired(configPath);
        }

        public virtual bool IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
        {
            return this.Host.IsDefinitionAllowed(configPath, allowDefinition, allowExeDefinition);
        }

        public virtual bool IsFile(string streamName)
        {
            return this.Host.IsFile(streamName);
        }

        public virtual bool IsFullTrustSectionWithoutAptcaAllowed(IInternalConfigRecord configRecord)
        {
            return this.Host.IsFullTrustSectionWithoutAptcaAllowed(configRecord);
        }

        public virtual bool IsInitDelayed(IInternalConfigRecord configRecord)
        {
            return this.Host.IsInitDelayed(configRecord);
        }

        public virtual bool IsLocationApplicable(string configPath)
        {
            return this.Host.IsLocationApplicable(configPath);
        }

        public virtual bool IsSecondaryRoot(string configPath)
        {
            return this.Host.IsSecondaryRoot(configPath);
        }

        public virtual bool IsTrustedConfigPath(string configPath)
        {
            return this.Host.IsTrustedConfigPath(configPath);
        }

        public virtual Stream OpenStreamForRead(string streamName)
        {
            return this.Host.OpenStreamForRead(streamName);
        }

        public virtual Stream OpenStreamForRead(string streamName, bool assertPermissions)
        {
            return this.Host.OpenStreamForRead(streamName, assertPermissions);
        }

        public virtual Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext)
        {
            return this.Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
        }

        public virtual Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext, bool assertPermissions)
        {
            return this.Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext, assertPermissions);
        }

        public virtual bool PrefetchAll(string configPath, string streamName)
        {
            return this.Host.PrefetchAll(configPath, streamName);
        }

        public virtual bool PrefetchSection(string sectionGroupName, string sectionName)
        {
            return this.Host.PrefetchSection(sectionGroupName, sectionName);
        }

        public virtual void RequireCompleteInit(IInternalConfigRecord configRecord)
        {
            this.Host.RequireCompleteInit(configRecord);
        }

        public virtual object StartMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
        {
            return this.Host.StartMonitoringStreamForChanges(streamName, callback);
        }

        public virtual void StopMonitoringStreamForChanges(string streamName, StreamChangeCallback callback)
        {
            this.Host.StopMonitoringStreamForChanges(streamName, callback);
        }

        public virtual void VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo)
        {
            this.Host.VerifyDefinitionAllowed(configPath, allowDefinition, allowExeDefinition, errorInfo);
        }

        public virtual void WriteCompleted(string streamName, bool success, object writeContext)
        {
            this.Host.WriteCompleted(streamName, success, writeContext);
        }

        public virtual void WriteCompleted(string streamName, bool success, object writeContext, bool assertPermissions)
        {
            this.Host.WriteCompleted(streamName, success, writeContext, assertPermissions);
        }

        protected IInternalConfigHost Host
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._host;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._host = value;
            }
        }

        public virtual bool IsRemote
        {
            get
            {
                return this.Host.IsRemote;
            }
        }

        public virtual bool SupportsChangeNotifications
        {
            get
            {
                return this.Host.SupportsChangeNotifications;
            }
        }

        public virtual bool SupportsLocation
        {
            get
            {
                return this.Host.SupportsLocation;
            }
        }

        public virtual bool SupportsPath
        {
            get
            {
                return this.Host.SupportsPath;
            }
        }

        public virtual bool SupportsRefresh
        {
            get
            {
                return this.Host.SupportsRefresh;
            }
        }
    }
}

