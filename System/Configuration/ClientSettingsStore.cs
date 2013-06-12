namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration.Internal;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    internal sealed class ClientSettingsStore
    {
        private const string ApplicationSettingsGroupName = "applicationSettings";
        private const string ApplicationSettingsGroupPrefix = "applicationSettings/";
        private const string UserSettingsGroupName = "userSettings";
        private const string UserSettingsGroupPrefix = "userSettings/";

        private void DeclareSection(System.Configuration.Configuration config, string sectionName)
        {
            if (config.GetSectionGroup("userSettings") == null)
            {
                ConfigurationSectionGroup group2 = new UserSettingsGroup();
                config.SectionGroups.Add("userSettings", group2);
            }
            ConfigurationSectionGroup sectionGroup = config.GetSectionGroup("userSettings");
            if ((sectionGroup != null) && (sectionGroup.Sections[sectionName] == null))
            {
                ConfigurationSection section = new ClientSettingsSection {
                    SectionInformation = { AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser, RequirePermission = false }
                };
                sectionGroup.Sections.Add(sectionName, section);
            }
        }

        private ClientSettingsSection GetConfigSection(System.Configuration.Configuration config, string sectionName, bool declare)
        {
            string str = "userSettings/" + sectionName;
            ClientSettingsSection section = null;
            if (config != null)
            {
                section = config.GetSection(str) as ClientSettingsSection;
                if ((section == null) && declare)
                {
                    this.DeclareSection(config, sectionName);
                    section = config.GetSection(str) as ClientSettingsSection;
                }
            }
            return section;
        }

        private System.Configuration.Configuration GetUserConfig(bool isRoaming)
        {
            ConfigurationUserLevel userLevel = isRoaming ? ConfigurationUserLevel.PerUserRoaming : ConfigurationUserLevel.PerUserRoamingAndLocal;
            return ClientSettingsConfigurationHost.OpenExeConfiguration(userLevel);
        }

        internal ConnectionStringSettingsCollection ReadConnectionStrings()
        {
            return System.Configuration.PrivilegedConfigurationManager.ConnectionStrings;
        }

        internal IDictionary ReadSettings(string sectionName, bool isUserScoped)
        {
            IDictionary dictionary = new Hashtable();
            if (!isUserScoped || ConfigurationManagerInternalFactory.Instance.SupportsUserConfig)
            {
                string str = isUserScoped ? "userSettings/" : "applicationSettings/";
                ConfigurationManager.RefreshSection(str + sectionName);
                ClientSettingsSection section = ConfigurationManager.GetSection(str + sectionName) as ClientSettingsSection;
                if (section == null)
                {
                    return dictionary;
                }
                foreach (SettingElement element in section.Settings)
                {
                    dictionary[element.Name] = new StoredSetting(element.SerializeAs, element.Value.ValueXml);
                }
            }
            return dictionary;
        }

        internal static IDictionary ReadSettingsFromFile(string configFileName, string sectionName, bool isUserScoped)
        {
            IDictionary dictionary = new Hashtable();
            if (!isUserScoped || ConfigurationManagerInternalFactory.Instance.SupportsUserConfig)
            {
                string str = isUserScoped ? "userSettings/" : "applicationSettings/";
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                ConfigurationUserLevel userLevel = isUserScoped ? ConfigurationUserLevel.PerUserRoaming : ConfigurationUserLevel.None;
                if (isUserScoped)
                {
                    fileMap.ExeConfigFilename = ConfigurationManagerInternalFactory.Instance.ApplicationConfigUri;
                    fileMap.RoamingUserConfigFilename = configFileName;
                }
                else
                {
                    fileMap.ExeConfigFilename = configFileName;
                }
                ClientSettingsSection section = ConfigurationManager.OpenMappedExeConfiguration(fileMap, userLevel).GetSection(str + sectionName) as ClientSettingsSection;
                if (section == null)
                {
                    return dictionary;
                }
                foreach (SettingElement element in section.Settings)
                {
                    dictionary[element.Name] = new StoredSetting(element.SerializeAs, element.Value.ValueXml);
                }
            }
            return dictionary;
        }

        internal void RevertToParent(string sectionName, bool isRoaming)
        {
            if (!ConfigurationManagerInternalFactory.Instance.SupportsUserConfig)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("UserSettingsNotSupported"));
            }
            System.Configuration.Configuration userConfig = this.GetUserConfig(isRoaming);
            ClientSettingsSection section = this.GetConfigSection(userConfig, sectionName, false);
            if (section != null)
            {
                section.SectionInformation.RevertToParent();
                userConfig.Save();
            }
        }

        internal void WriteSettings(string sectionName, bool isRoaming, IDictionary newSettings)
        {
            if (!ConfigurationManagerInternalFactory.Instance.SupportsUserConfig)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("UserSettingsNotSupported"));
            }
            System.Configuration.Configuration userConfig = this.GetUserConfig(isRoaming);
            ClientSettingsSection section = this.GetConfigSection(userConfig, sectionName, true);
            if (section != null)
            {
                SettingElementCollection settings = section.Settings;
                foreach (DictionaryEntry entry in newSettings)
                {
                    SettingElement element = settings.Get((string) entry.Key);
                    if (element == null)
                    {
                        element = new SettingElement {
                            Name = (string) entry.Key
                        };
                        settings.Add(element);
                    }
                    StoredSetting setting = (StoredSetting) entry.Value;
                    element.SerializeAs = setting.SerializeAs;
                    element.Value.ValueXml = setting.Value;
                }
                try
                {
                    userConfig.Save();
                    return;
                }
                catch (ConfigurationErrorsException exception)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("SettingsSaveFailed", new object[] { exception.Message }), exception);
                }
            }
            throw new ConfigurationErrorsException(System.SR.GetString("SettingsSaveFailedNoSection"));
        }

        private sealed class ClientSettingsConfigurationHost : DelegatingConfigHost
        {
            private const string ClientConfigurationHostTypeName = "System.Configuration.ClientConfigurationHost,System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            private const string InternalConfigConfigurationFactoryTypeName = "System.Configuration.Internal.InternalConfigConfigurationFactory,System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            private static IInternalConfigConfigurationFactory s_configFactory;

            private ClientSettingsConfigurationHost()
            {
            }

            public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams)
            {
            }

            public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams)
            {
                object[] objArray;
                ConfigurationUserLevel level = (ConfigurationUserLevel) hostInitConfigurationParams[0];
                string roamingUserConfigPath = null;
                base.Host = (IInternalConfigHost) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission("System.Configuration.ClientConfigurationHost,System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                ConfigurationUserLevel level2 = level;
                if (level2 != ConfigurationUserLevel.None)
                {
                    if (level2 != ConfigurationUserLevel.PerUserRoaming)
                    {
                        if (level2 != ConfigurationUserLevel.PerUserRoamingAndLocal)
                        {
                            throw new ArgumentException(System.SR.GetString("UnknownUserLevel"));
                        }
                        roamingUserConfigPath = this.ClientHost.GetLocalUserConfigPath();
                        goto Label_006D;
                    }
                }
                else
                {
                    roamingUserConfigPath = this.ClientHost.GetExeConfigPath();
                    goto Label_006D;
                }
                roamingUserConfigPath = this.ClientHost.GetRoamingUserConfigPath();
            Label_006D:
                objArray = new object[3];
                objArray[2] = roamingUserConfigPath;
                base.Host.InitForConfiguration(ref locationSubPath, out configPath, out locationConfigPath, configRoot, objArray);
            }

            private bool IsKnownConfigFile(string filename)
            {
                if ((!string.Equals(filename, ConfigurationManagerInternalFactory.Instance.MachineConfigPath, StringComparison.OrdinalIgnoreCase) && !string.Equals(filename, ConfigurationManagerInternalFactory.Instance.ApplicationConfigUri, StringComparison.OrdinalIgnoreCase)) && !string.Equals(filename, ConfigurationManagerInternalFactory.Instance.ExeLocalConfigPath, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Equals(filename, ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigPath, StringComparison.OrdinalIgnoreCase);
                }
                return true;
            }

            internal static System.Configuration.Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel)
            {
                return ConfigFactory.Create(typeof(ClientSettingsStore.ClientSettingsConfigurationHost), new object[] { userLevel });
            }

            public override Stream OpenStreamForRead(string streamName)
            {
                if (this.IsKnownConfigFile(streamName))
                {
                    return base.Host.OpenStreamForRead(streamName, true);
                }
                return base.Host.OpenStreamForRead(streamName);
            }

            public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext)
            {
                if (string.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeLocalConfigPath, StringComparison.OrdinalIgnoreCase))
                {
                    return new ClientSettingsStore.QuotaEnforcedStream(base.Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext, true), false);
                }
                if (string.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigPath, StringComparison.OrdinalIgnoreCase))
                {
                    return new ClientSettingsStore.QuotaEnforcedStream(base.Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext, true), true);
                }
                return base.Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
            }

            public override void WriteCompleted(string streamName, bool success, object writeContext)
            {
                if (string.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeLocalConfigPath, StringComparison.OrdinalIgnoreCase) || string.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigPath, StringComparison.OrdinalIgnoreCase))
                {
                    base.Host.WriteCompleted(streamName, success, writeContext, true);
                }
                else
                {
                    base.Host.WriteCompleted(streamName, success, writeContext);
                }
            }

            private IInternalConfigClientHost ClientHost
            {
                get
                {
                    return (IInternalConfigClientHost) base.Host;
                }
            }

            internal static IInternalConfigConfigurationFactory ConfigFactory
            {
                get
                {
                    if (s_configFactory == null)
                    {
                        s_configFactory = (IInternalConfigConfigurationFactory) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission("System.Configuration.Internal.InternalConfigConfigurationFactory,System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    }
                    return s_configFactory;
                }
            }
        }

        private sealed class QuotaEnforcedStream : Stream
        {
            private bool _isRoaming;
            private Stream _originalStream;

            internal QuotaEnforcedStream(Stream originalStream, bool isRoaming)
            {
                this._originalStream = originalStream;
                this._isRoaming = isRoaming;
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
            {
                return this._originalStream.BeginRead(buffer, offset, numBytes, userCallback, stateObject);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
            {
                if (!this.CanWrite)
                {
                    throw new NotSupportedException();
                }
                long length = this._originalStream.Length;
                long num2 = this._originalStream.CanSeek ? (this._originalStream.Position + numBytes) : (this._originalStream.Length + numBytes);
                this.EnsureQuota(Math.Max(length, num2));
                return this._originalStream.BeginWrite(buffer, offset, numBytes, userCallback, stateObject);
            }

            public override void Close()
            {
                this._originalStream.Close();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this._originalStream != null))
                {
                    this._originalStream.Dispose();
                    this._originalStream = null;
                }
                base.Dispose(disposing);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                return this._originalStream.EndRead(asyncResult);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                this._originalStream.EndWrite(asyncResult);
            }

            private void EnsureQuota(long size)
            {
                new IsolatedStorageFilePermission(PermissionState.None) { UserQuota = size, UsageAllowed = this._isRoaming ? IsolatedStorageContainment.DomainIsolationByRoamingUser : IsolatedStorageContainment.DomainIsolationByUser }.Demand();
            }

            public override void Flush()
            {
                this._originalStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this._originalStream.Read(buffer, offset, count);
            }

            public override int ReadByte()
            {
                return this._originalStream.ReadByte();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long num2;
                if (!this.CanSeek)
                {
                    throw new NotSupportedException();
                }
                long length = this._originalStream.Length;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        num2 = offset;
                        break;

                    case SeekOrigin.Current:
                        num2 = this._originalStream.Position + offset;
                        break;

                    case SeekOrigin.End:
                        num2 = length + offset;
                        break;

                    default:
                        throw new ArgumentException(System.SR.GetString("UnknownSeekOrigin"), "origin");
                }
                this.EnsureQuota(Math.Max(length, num2));
                return this._originalStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                long length = this._originalStream.Length;
                long num2 = value;
                this.EnsureQuota(Math.Max(length, num2));
                this._originalStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (!this.CanWrite)
                {
                    throw new NotSupportedException();
                }
                long length = this._originalStream.Length;
                long num2 = this._originalStream.CanSeek ? (this._originalStream.Position + count) : (this._originalStream.Length + count);
                this.EnsureQuota(Math.Max(length, num2));
                this._originalStream.Write(buffer, offset, count);
            }

            public override void WriteByte(byte value)
            {
                if (!this.CanWrite)
                {
                    throw new NotSupportedException();
                }
                long length = this._originalStream.Length;
                long num2 = this._originalStream.CanSeek ? (this._originalStream.Position + 1L) : (this._originalStream.Length + 1L);
                this.EnsureQuota(Math.Max(length, num2));
                this._originalStream.WriteByte(value);
            }

            public override bool CanRead
            {
                get
                {
                    return this._originalStream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return this._originalStream.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this._originalStream.CanWrite;
                }
            }

            public override long Length
            {
                get
                {
                    return this._originalStream.Length;
                }
            }

            public override long Position
            {
                get
                {
                    return this._originalStream.Position;
                }
                set
                {
                    if (value < 0L)
                    {
                        throw new ArgumentOutOfRangeException("value", System.SR.GetString("PositionOutOfRange"));
                    }
                    this.Seek(value, SeekOrigin.Begin);
                }
            }
        }
    }
}

