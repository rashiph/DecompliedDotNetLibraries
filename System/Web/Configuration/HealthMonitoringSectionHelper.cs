namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Management;

    internal class HealthMonitoringSectionHelper
    {
        private static ArrayList[,] _cachedMatchedRules;
        private Hashtable _cachedMatchedRulesForCustomEvents;
        internal Hashtable _customEvaluatorInstances;
        private bool _enabled;
        internal ProviderInstances _providerInstances;
        internal ArrayList _ruleInfos;
        private System.Web.Configuration.HealthMonitoringSection _section;
        private static HealthMonitoringSectionHelper s_helper;
        private static RuleInfoComparer s_ruleInfoComparer = new RuleInfoComparer();

        private HealthMonitoringSectionHelper()
        {
            try
            {
                this._section = RuntimeConfig.GetAppConfig().HealthMonitoring;
            }
            catch (Exception exception)
            {
                if (HttpRuntime.InitializationException == null)
                {
                    HttpRuntime.InitializationException = exception;
                }
                this._section = RuntimeConfig.GetAppLKGConfig().HealthMonitoring;
                if (this._section == null)
                {
                    throw;
                }
            }
            this._enabled = this._section.Enabled;
            if (this._enabled)
            {
                this.BasicSanityCheck();
                this._ruleInfos = new ArrayList();
                this._customEvaluatorInstances = new Hashtable();
                this._providerInstances = new ProviderInstances(this._section);
                this._cachedMatchedRulesForCustomEvents = new Hashtable(new WebBaseEventKeyComparer());
                _cachedMatchedRules = new ArrayList[WebEventCodes.GetEventArrayDimensionSize(0), WebEventCodes.GetEventArrayDimensionSize(1)];
                this.BuildRuleInfos();
                this._providerInstances.CleanupUninitProviders();
            }
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        private void BasicSanityCheck()
        {
            Type type;
            foreach (ProviderSettings settings in this._section.Providers)
            {
                type = ConfigUtil.GetType(settings.Type, "type", settings);
                System.Web.Configuration.HandlerBase.CheckAssignableType(settings.ElementInformation.Properties["type"].Source, settings.ElementInformation.Properties["type"].LineNumber, typeof(WebEventProvider), type);
            }
            foreach (EventMappingSettings settings2 in this._section.EventMappings)
            {
                type = ConfigUtil.GetType(settings2.Type, "type", settings2);
                if (settings2.StartEventCode > settings2.EndEventCode)
                {
                    string str = "startEventCode";
                    if (settings2.ElementInformation.Properties[str].LineNumber == 0)
                    {
                        str = "endEventCode";
                    }
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Event_name_invalid_code_range"), settings2.ElementInformation.Properties[str].Source, settings2.ElementInformation.Properties[str].LineNumber);
                }
                System.Web.Configuration.HandlerBase.CheckAssignableType(settings2.ElementInformation.Properties["type"].Source, settings2.ElementInformation.Properties["type"].LineNumber, typeof(WebBaseEvent), type);
                settings2.RealType = type;
            }
            foreach (RuleSettings settings3 in this._section.Rules)
            {
                string provider = settings3.Provider;
                if (!string.IsNullOrEmpty(provider) && (this._section.Providers[provider] == null))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Health_mon_provider_not_found", new object[] { provider }), settings3.ElementInformation.Properties["provider"].Source, settings3.ElementInformation.Properties["provider"].LineNumber);
                }
                string profile = settings3.Profile;
                if (!string.IsNullOrEmpty(profile) && (this._section.Profiles[profile] == null))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Health_mon_profile_not_found", new object[] { profile }), settings3.ElementInformation.Properties["profile"].Source, settings3.ElementInformation.Properties["profile"].LineNumber);
                }
                if (this._section.EventMappings[settings3.EventName] == null)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Event_name_not_found", new object[] { settings3.EventName }), settings3.ElementInformation.Properties["eventName"].Source, settings3.ElementInformation.Properties["eventName"].LineNumber);
                }
            }
        }

        private void BuildRuleInfos()
        {
            foreach (RuleSettings settings in this._section.Rules)
            {
                RuleInfo ruleInfo = this.CreateRuleInfo(settings);
                this.DisplayRuleInfo(ruleInfo);
                this._ruleInfos.Add(ruleInfo);
            }
            this._ruleInfos.Sort(s_ruleInfoComparer);
        }

        private RuleInfo CreateRuleInfo(RuleSettings ruleSettings)
        {
            RuleInfo ruleInfo = new RuleInfo(ruleSettings, this._section);
            this.MergeValuesWithProfile(ruleInfo);
            this.InitReferencedProvider(ruleInfo);
            this.InitCustomEvaluator(ruleInfo);
            return ruleInfo;
        }

        private void DisplayRuleInfo(RuleInfo ruleInfo)
        {
        }

        internal ArrayList FindFiringRuleInfos(Type eventType, int eventCode)
        {
            ArrayList list;
            object obj2;
            bool flag = eventCode < 0x186a0;
            CustomWebEventKey key = null;
            int num = 0;
            int num2 = 0;
            if (flag)
            {
                WebEventCodes.GetEventArrayIndexsFromEventCode(eventCode, out num, out num2);
                list = _cachedMatchedRules[num, num2];
            }
            else
            {
                key = new CustomWebEventKey(eventType, eventCode);
                list = (ArrayList) this._cachedMatchedRulesForCustomEvents[key];
            }
            if (list != null)
            {
                return list;
            }
            if (flag)
            {
                obj2 = _cachedMatchedRules;
            }
            else
            {
                obj2 = this._cachedMatchedRulesForCustomEvents;
            }
            lock (obj2)
            {
                if (flag)
                {
                    list = _cachedMatchedRules[num, num2];
                }
                else
                {
                    list = (ArrayList) this._cachedMatchedRulesForCustomEvents[key];
                }
                if (list != null)
                {
                    return list;
                }
                ArrayList list2 = new ArrayList();
                for (int i = this._ruleInfos.Count - 1; i >= 0; i--)
                {
                    RuleInfo ruleInfo = (RuleInfo) this._ruleInfos[i];
                    if (ruleInfo.Match(eventType, eventCode))
                    {
                        list2.Add(new FiringRuleInfo(ruleInfo));
                    }
                }
                int count = list2.Count;
                for (int j = 0; j < count; j++)
                {
                    FiringRuleInfo info2 = (FiringRuleInfo) list2[j];
                    if (info2._ruleInfo._referencedProvider != null)
                    {
                        for (int k = j + 1; k < count; k++)
                        {
                            FiringRuleInfo info3 = (FiringRuleInfo) list2[k];
                            if (((info3._ruleInfo._referencedProvider != null) && (info3._indexOfFirstRuleInfoWithSameProvider == -1)) && (info2._ruleInfo._referencedProvider == info3._ruleInfo._referencedProvider))
                            {
                                if (info2._indexOfFirstRuleInfoWithSameProvider == -1)
                                {
                                    info2._indexOfFirstRuleInfoWithSameProvider = j;
                                }
                                info3._indexOfFirstRuleInfoWithSameProvider = j;
                            }
                        }
                    }
                }
                if (flag)
                {
                    _cachedMatchedRules[num, num2] = list2;
                }
                else
                {
                    this._cachedMatchedRulesForCustomEvents[key] = list2;
                }
                return list2;
            }
        }

        internal static HealthMonitoringSectionHelper GetHelper()
        {
            if (s_helper == null)
            {
                s_helper = new HealthMonitoringSectionHelper();
            }
            return s_helper;
        }

        private void InitCustomEvaluator(RuleInfo ruleInfo)
        {
            string str = ruleInfo._customEvaluator;
            if ((str == null) || (str.Trim().Length == 0))
            {
                ruleInfo._customEvaluatorType = null;
            }
            else
            {
                ruleInfo._customEvaluatorType = ConfigUtil.GetType(ruleInfo._customEvaluator, "custom", ruleInfo._customEvaluatorConfig);
                System.Web.Configuration.HandlerBase.CheckAssignableType(ruleInfo._customEvaluatorConfig.ElementInformation.Properties["custom"].Source, ruleInfo._customEvaluatorConfig.ElementInformation.Properties["custom"].LineNumber, typeof(IWebEventCustomEvaluator), ruleInfo._customEvaluatorType);
                if (this._customEvaluatorInstances[ruleInfo._customEvaluatorType] == null)
                {
                    this._customEvaluatorInstances[ruleInfo._customEvaluatorType] = HttpRuntime.CreatePublicInstance(ruleInfo._customEvaluatorType);
                }
            }
        }

        private void InitReferencedProvider(RuleInfo ruleInfo)
        {
            string str = ruleInfo._ruleSettings.Provider;
            if (!string.IsNullOrEmpty(str))
            {
                WebEventProvider provider = this._providerInstances[str];
                ruleInfo._referencedProvider = provider;
            }
        }

        private void MergeValuesWithProfile(RuleInfo ruleInfo)
        {
            ProfileSettings settings = null;
            if (ruleInfo._ruleSettings.ElementInformation.Properties["profile"].ValueOrigin != PropertyValueOrigin.Default)
            {
                settings = this._section.Profiles[ruleInfo._ruleSettings.Profile];
            }
            if ((settings != null) && (ruleInfo._ruleSettings.ElementInformation.Properties["minInstances"].ValueOrigin == PropertyValueOrigin.Default))
            {
                ruleInfo._minInstances = settings.MinInstances;
            }
            else
            {
                ruleInfo._minInstances = ruleInfo._ruleSettings.MinInstances;
            }
            if ((settings != null) && (ruleInfo._ruleSettings.ElementInformation.Properties["maxLimit"].ValueOrigin == PropertyValueOrigin.Default))
            {
                ruleInfo._maxLimit = settings.MaxLimit;
            }
            else
            {
                ruleInfo._maxLimit = ruleInfo._ruleSettings.MaxLimit;
            }
            if ((settings != null) && (ruleInfo._ruleSettings.ElementInformation.Properties["minInterval"].ValueOrigin == PropertyValueOrigin.Default))
            {
                ruleInfo._minInterval = settings.MinInterval;
            }
            else
            {
                ruleInfo._minInterval = ruleInfo._ruleSettings.MinInterval;
            }
            if ((settings != null) && (ruleInfo._ruleSettings.ElementInformation.Properties["custom"].ValueOrigin == PropertyValueOrigin.Default))
            {
                ruleInfo._customEvaluator = settings.Custom;
                ruleInfo._customEvaluatorConfig = settings;
            }
            else
            {
                ruleInfo._customEvaluator = ruleInfo._ruleSettings.Custom;
                ruleInfo._customEvaluatorConfig = ruleInfo._ruleSettings;
            }
        }

        internal bool Enabled
        {
            get
            {
                return this._enabled;
            }
        }

        internal System.Web.Configuration.HealthMonitoringSection HealthMonitoringSection
        {
            get
            {
                return this._section;
            }
        }

        internal class FiringRuleInfo
        {
            internal int _indexOfFirstRuleInfoWithSameProvider;
            internal HealthMonitoringSectionHelper.RuleInfo _ruleInfo;

            internal FiringRuleInfo(HealthMonitoringSectionHelper.RuleInfo ruleInfo)
            {
                this._ruleInfo = ruleInfo;
                this._indexOfFirstRuleInfoWithSameProvider = -1;
            }
        }

        internal class ProviderInstances
        {
            internal Hashtable _instances;

            [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
            internal ProviderInstances(HealthMonitoringSection section)
            {
                this._instances = CollectionsUtil.CreateCaseInsensitiveHashtable(section.Providers.Count);
                foreach (object obj2 in section.Providers)
                {
                    ProviderSettings settings = (ProviderSettings) obj2;
                    this._instances.Add(settings.Name, settings);
                }
            }

            internal void CleanupUninitProviders()
            {
                ArrayList list = new ArrayList();
                foreach (DictionaryEntry entry in this._instances)
                {
                    if (entry.Value is ProviderSettings)
                    {
                        list.Add(entry.Key);
                    }
                }
                foreach (object obj2 in list)
                {
                    this._instances.Remove(obj2);
                }
            }

            internal bool ContainsKey(string name)
            {
                return this._instances.ContainsKey(name);
            }

            public IDictionaryEnumerator GetEnumerator()
            {
                return this._instances.GetEnumerator();
            }

            private WebEventProvider GetProviderInstance(string providerName)
            {
                object obj2 = this._instances[providerName];
                if (obj2 == null)
                {
                    return null;
                }
                ProviderSettings settings = obj2 as ProviderSettings;
                if (settings != null)
                {
                    WebEventProvider provider;
                    Type c = BuildManager.GetType(settings.Type, false);
                    if (typeof(IInternalWebEventProvider).IsAssignableFrom(c))
                    {
                        provider = (WebEventProvider) HttpRuntime.CreateNonPublicInstance(c);
                    }
                    else
                    {
                        provider = (WebEventProvider) HttpRuntime.CreatePublicInstance(c);
                    }
                    ProcessImpersonationContext context = new ProcessImpersonationContext();
                    try
                    {
                        provider.Initialize(settings.Name, settings.Parameters);
                    }
                    catch (ConfigurationErrorsException)
                    {
                        throw;
                    }
                    catch (ConfigurationException exception)
                    {
                        throw new ConfigurationErrorsException(exception.Message, settings.ElementInformation.Properties["type"].Source, settings.ElementInformation.Properties["type"].LineNumber);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (context != null)
                        {
                            ((IDisposable) context).Dispose();
                        }
                    }
                    this._instances[providerName] = provider;
                    return provider;
                }
                return (obj2 as WebEventProvider);
            }

            internal WebEventProvider this[string name]
            {
                get
                {
                    return this.GetProviderInstance(name);
                }
            }
        }

        internal class RuleInfo
        {
            internal string _customEvaluator;
            internal ConfigurationElement _customEvaluatorConfig;
            internal Type _customEvaluatorType;
            internal EventMappingSettings _eventMappingSettings;
            internal int _maxLimit;
            internal int _minInstances;
            internal TimeSpan _minInterval;
            internal WebEventProvider _referencedProvider;
            internal RuleFiringRecord _ruleFiringRecord;
            internal RuleSettings _ruleSettings;

            internal RuleInfo(RuleSettings ruleSettings, HealthMonitoringSection section)
            {
                this._eventMappingSettings = section.EventMappings[ruleSettings.EventName];
                this._ruleSettings = ruleSettings;
                this._ruleFiringRecord = new RuleFiringRecord(this);
            }

            internal bool Match(Type eventType, int eventCode)
            {
                if (!eventType.Equals(this._eventMappingSettings.RealType) && !eventType.IsSubclassOf(this._eventMappingSettings.RealType))
                {
                    return false;
                }
                return ((this._eventMappingSettings.StartEventCode <= eventCode) && (eventCode <= this._eventMappingSettings.EndEventCode));
            }
        }
    }
}

