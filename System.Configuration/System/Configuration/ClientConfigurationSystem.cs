namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Threading;

    internal sealed class ClientConfigurationSystem : IInternalConfigSystem
    {
        private IInternalConfigRecord _completeConfigRecord;
        private ClientConfigurationHost _configHost;
        private IInternalConfigRoot _configRoot;
        private IConfigSystem _configSystem = new ConfigSystem();
        private Exception _initError;
        private bool _isAppConfigHttp;
        private bool _isInitInProgress;
        private bool _isMachineConfigInited;
        private bool _isUserConfigInited;
        private IInternalConfigRecord _machineConfigRecord;
        private const string SystemDiagnosticsConfigKey = "system.diagnostics";
        private const string SystemNetGroupKey = "system.net/";

        internal ClientConfigurationSystem()
        {
            this._configSystem.Init(typeof(ClientConfigurationHost), new object[2]);
            this._configHost = (ClientConfigurationHost) this._configSystem.Host;
            this._configRoot = this._configSystem.Root;
            this._configRoot.ConfigRemoved += new InternalConfigEventHandler(this.OnConfigRemoved);
            this._isAppConfigHttp = this._configHost.IsAppConfigHttp;
            string schemeDelimiter = Uri.SchemeDelimiter;
        }

        private bool DoesSectionOnlyUseMachineConfig(string configKey)
        {
            return (this._isAppConfigHttp && configKey.StartsWith("system.net/", StringComparison.Ordinal));
        }

        private void EnsureInit(string configKey)
        {
            bool flag = false;
            lock (this)
            {
                if (!this._isUserConfigInited)
                {
                    if (!this._isInitInProgress)
                    {
                        this._isInitInProgress = true;
                        flag = true;
                    }
                    else if (!this.IsSectionUsedInInit(configKey))
                    {
                        Monitor.Wait(this);
                    }
                }
            }
            if (flag)
            {
                try
                {
                    try
                    {
                        string str;
                        this._machineConfigRecord = this._configRoot.GetConfigRecord("MACHINE");
                        this._machineConfigRecord.ThrowIfInitErrors();
                        this._isMachineConfigInited = true;
                        if (this._isAppConfigHttp)
                        {
                            ConfigurationManagerHelperFactory.Instance.EnsureNetConfigLoaded();
                        }
                        this._configHost.RefreshConfigPaths();
                        if (this._configHost.HasLocalConfig)
                        {
                            str = "MACHINE/EXE/ROAMING_USER/LOCAL_USER";
                        }
                        else if (this._configHost.HasRoamingConfig)
                        {
                            str = "MACHINE/EXE/ROAMING_USER";
                        }
                        else
                        {
                            str = "MACHINE/EXE";
                        }
                        this._completeConfigRecord = this._configRoot.GetConfigRecord(str);
                        this._completeConfigRecord.ThrowIfInitErrors();
                        this._isUserConfigInited = true;
                    }
                    catch (Exception exception)
                    {
                        this._initError = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_client_config_init_error"), exception);
                        throw this._initError;
                    }
                }
                catch
                {
                    ConfigurationManager.SetInitError(this._initError);
                    this._isMachineConfigInited = true;
                    this._isUserConfigInited = true;
                    throw;
                }
                finally
                {
                    lock (this)
                    {
                        try
                        {
                            ConfigurationManager.CompleteConfigInit();
                            this._isInitInProgress = false;
                        }
                        finally
                        {
                            Monitor.PulseAll(this);
                        }
                    }
                }
            }
        }

        private bool IsSectionUsedInInit(string configKey)
        {
            return ((configKey == "system.diagnostics") || (this._isAppConfigHttp && configKey.StartsWith("system.net/", StringComparison.Ordinal)));
        }

        private void OnConfigRemoved(object sender, InternalConfigEventArgs e)
        {
            try
            {
                IInternalConfigRecord configRecord = this._configRoot.GetConfigRecord(this._completeConfigRecord.ConfigPath);
                this._completeConfigRecord = configRecord;
                this._completeConfigRecord.ThrowIfInitErrors();
            }
            catch (Exception exception)
            {
                this._initError = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_client_config_init_error"), exception);
                ConfigurationManager.SetInitError(this._initError);
                throw this._initError;
            }
        }

        private void PrepareClientConfigSystem(string sectionName)
        {
            if (!this._isUserConfigInited)
            {
                this.EnsureInit(sectionName);
            }
            if (this._initError != null)
            {
                throw this._initError;
            }
        }

        object IInternalConfigSystem.GetSection(string sectionName)
        {
            this.PrepareClientConfigSystem(sectionName);
            IInternalConfigRecord record = null;
            if (this.DoesSectionOnlyUseMachineConfig(sectionName))
            {
                if (this._isMachineConfigInited)
                {
                    record = this._machineConfigRecord;
                }
            }
            else if (this._isUserConfigInited)
            {
                record = this._completeConfigRecord;
            }
            if (record != null)
            {
                return record.GetSection(sectionName);
            }
            return null;
        }

        void IInternalConfigSystem.RefreshConfig(string sectionName)
        {
            this.PrepareClientConfigSystem(sectionName);
            if (this._isMachineConfigInited)
            {
                this._machineConfigRecord.RefreshSection(sectionName);
            }
        }

        bool IInternalConfigSystem.SupportsUserConfig
        {
            get
            {
                return true;
            }
        }
    }
}

