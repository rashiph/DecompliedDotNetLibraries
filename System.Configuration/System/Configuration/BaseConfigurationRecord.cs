namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration.Internal;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Xml;

    [DebuggerDisplay("ConfigPath = {ConfigPath}")]
    internal abstract class BaseConfigurationRecord : IInternalConfigRecord
    {
        protected Hashtable _children;
        private object _configContext;
        protected string _configName;
        protected string _configPath;
        protected InternalConfigRoot _configRoot;
        private ConfigRecordStreamInfo _configStreamInfo;
        protected Hashtable _factoryRecords;
        protected SafeBitVector32 _flags = new SafeBitVector32();
        private BaseConfigurationRecord _initDelayedRoot;
        private ConfigurationSchemaErrors _initErrors;
        protected ArrayList _locationSections;
        protected string _locationSubPath;
        protected BaseConfigurationRecord _parent;
        private ProtectedConfigurationSection _protectedConfig;
        private PermissionSet _restrictedPermissions;
        protected Hashtable _sectionRecords;
        protected const int ClassIgnoreLocalErrors = 0x40;
        protected const int ClassSupportsChangeNotifications = 1;
        protected const int ClassSupportsDelayedInit = 0x20;
        protected const int ClassSupportsImpersonation = 4;
        protected const int ClassSupportsKeepInputs = 0x10;
        protected const int ClassSupportsRefresh = 2;
        protected const int ClassSupportsRestrictedPermissions = 8;
        protected const int Closed = 2;
        internal const char ConfigPathSeparatorChar = '/';
        internal static readonly char[] ConfigPathSeparatorParams = new char[] { '/' };
        internal const string ConfigPathSeparatorString = "/";
        private const int ContextEvaluated = 0x80;
        protected const int ForceLocationWritten = 0x1000000;
        protected const string FORMAT_CONFIGSOURCE_FILE = "<?xml version=\"1.0\" encoding=\"{0}\"?>\r\n";
        protected const string FORMAT_CONFIGURATION = "<configuration>\r\n";
        protected const string FORMAT_CONFIGURATION_ENDELEMENT = "</configuration>";
        protected const string FORMAT_CONFIGURATION_NAMESPACE = "<configuration xmlns=\"{0}\">\r\n";
        protected const string FORMAT_LOCATION_ENDELEMENT = "</location>";
        protected const string FORMAT_LOCATION_NOPATH = "<location {0} inheritInChildApplications=\"{1}\">\r\n";
        protected const string FORMAT_LOCATION_PATH = "<location path=\"{2}\" {0} inheritInChildApplications=\"{1}\">\r\n";
        protected const string FORMAT_NEWCONFIGFILE = "<?xml version=\"1.0\" encoding=\"{0}\"?>\r\n";
        protected const string FORMAT_SECTION_CONFIGSOURCE = "<{0} configSource=\"{1}\" />";
        protected const string FORMAT_SECTIONGROUP_ENDELEMENT = "</sectionGroup>";
        private const string invalidFirstSubPathCharacters = @"\./";
        private const string invalidLastSubPathCharacters = @"\./";
        private const string invalidSubPathCharactersString = "\\?:*\"<>|";
        protected const int IsAboveApplication = 0x20;
        private const int IsLocationListResolved = 0x100;
        protected const int IsTrusted = 0x2000;
        protected const string KEYWORD_CLEAR = "clear";
        protected const string KEYWORD_CONFIGSECTIONS = "configSections";
        protected const string KEYWORD_CONFIGSOURCE = "configSource";
        protected const string KEYWORD_CONFIGURATION = "configuration";
        protected const string KEYWORD_CONFIGURATION_NAMESPACE = "http://schemas.microsoft.com/.NetConfiguration/v2.0";
        internal const string KEYWORD_FALSE = "false";
        protected const string KEYWORD_LOCATION = "location";
        internal const string KEYWORD_LOCATION_ALLOWOVERRIDE = "allowOverride";
        protected const string KEYWORD_LOCATION_INHERITINCHILDAPPLICATIONS = "inheritInChildApplications";
        internal const string KEYWORD_LOCATION_OVERRIDEMODE = "overrideMode";
        internal const string KEYWORD_LOCATION_OVERRIDEMODE_STRING = "{0}=\"{1}\"";
        protected const string KEYWORD_LOCATION_PATH = "path";
        internal const string KEYWORD_OVERRIDEMODE_ALLOW = "Allow";
        internal const string KEYWORD_OVERRIDEMODE_DENY = "Deny";
        internal const string KEYWORD_OVERRIDEMODE_INHERIT = "Inherit";
        internal const string KEYWORD_PROTECTION_PROVIDER = "configProtectionProvider";
        protected const string KEYWORD_REMOVE = "remove";
        protected const string KEYWORD_SECTION = "section";
        protected const string KEYWORD_SECTION_ALLOWDEFINITION = "allowDefinition";
        protected const string KEYWORD_SECTION_ALLOWDEFINITION_EVERYWHERE = "Everywhere";
        protected const string KEYWORD_SECTION_ALLOWDEFINITION_MACHINEONLY = "MachineOnly";
        protected const string KEYWORD_SECTION_ALLOWDEFINITION_MACHINETOAPPLICATION = "MachineToApplication";
        protected const string KEYWORD_SECTION_ALLOWDEFINITION_MACHINETOWEBROOT = "MachineToWebRoot";
        protected const string KEYWORD_SECTION_ALLOWEXEDEFINITION = "allowExeDefinition";
        protected const string KEYWORD_SECTION_ALLOWEXEDEFINITION_MACHTOLOCAL = "MachineToLocalUser";
        protected const string KEYWORD_SECTION_ALLOWEXEDEFINITION_MACHTOROAMING = "MachineToRoamingUser";
        protected const string KEYWORD_SECTION_ALLOWLOCATION = "allowLocation";
        protected const string KEYWORD_SECTION_NAME = "name";
        internal const string KEYWORD_SECTION_OVERRIDEMODEDEFAULT = "overrideModeDefault";
        protected const string KEYWORD_SECTION_REQUIREPERMISSION = "requirePermission";
        protected const string KEYWORD_SECTION_RESTARTONEXTERNALCHANGES = "restartOnExternalChanges";
        protected const string KEYWORD_SECTION_TYPE = "type";
        protected const string KEYWORD_SECTIONGROUP = "sectionGroup";
        protected const string KEYWORD_SECTIONGROUP_NAME = "name";
        protected const string KEYWORD_SECTIONGROUP_TYPE = "type";
        internal const string KEYWORD_TRUE = "true";
        protected const string KEYWORD_XMLNS = "xmlns";
        protected const int NamespacePresentCurrent = 0x4000000;
        protected const int NamespacePresentInFile = 0x200;
        protected const string NL = "\r\n";
        protected const int PrefetchAll = 8;
        private const string ProtectedConfigurationSectionTypeName = "System.Configuration.ProtectedConfigurationSection, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        protected const int ProtectedDataInitialized = 1;
        internal const string RESERVED_SECTION_PROTECTED_CONFIGURATION = "configProtectedData";
        private const int RestrictedPermissionsResolved = 0x800;
        private static string s_appConfigPath;
        private static IComparer<SectionInput> s_indirectInputsComparer = new IndirectLocationInputComparer();
        private static char[] s_invalidSubPathCharactersArray = "\\?:*\"<>|".ToCharArray();
        private static ConfigurationPermission s_unrestrictedConfigPermission;
        protected const int SuggestLocationRemoval = 0x2000000;
        protected const int SupportsChangeNotifications = 0x10000;
        protected const int SupportsKeepInputs = 0x80000;
        protected const int SupportsLocation = 0x100000;
        protected const int SupportsPath = 0x40000;
        protected const int SupportsRefresh = 0x20000;

        internal BaseConfigurationRecord()
        {
        }

        private void AddImplicitSections(Hashtable factoryList)
        {
            if (this._parent.IsRootConfig)
            {
                if (factoryList == null)
                {
                    factoryList = this.EnsureFactories();
                }
                if (((FactoryRecord) factoryList["configProtectedData"]) == null)
                {
                    factoryList["configProtectedData"] = new FactoryRecord("configProtectedData", string.Empty, "configProtectedData", "System.Configuration.ProtectedConfigurationSection, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true, ConfigurationAllowDefinition.Everywhere, ConfigurationAllowExeDefinition.MachineToApplication, OverrideModeSetting.SectionDefault, true, true, true, true, null, -1);
                }
            }
        }

        protected virtual void AddLocation(string LocationSubPath)
        {
        }

        internal static ConfigurationAllowDefinition AllowDefinitionToEnum(string allowDefinition, XmlUtil xmlUtil)
        {
            switch (xmlUtil.Reader.Value)
            {
                case "Everywhere":
                    return ConfigurationAllowDefinition.Everywhere;

                case "MachineOnly":
                    return ConfigurationAllowDefinition.MachineOnly;

                case "MachineToApplication":
                    return ConfigurationAllowDefinition.MachineToApplication;

                case "MachineToWebRoot":
                    return ConfigurationAllowDefinition.MachineToWebRoot;
            }
            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_section_allow_definition_attribute_invalid"), xmlUtil);
        }

        internal static ConfigurationAllowExeDefinition AllowExeDefinitionToEnum(string allowExeDefinition, XmlUtil xmlUtil)
        {
            switch (allowExeDefinition)
            {
                case "MachineOnly":
                    return ConfigurationAllowExeDefinition.MachineOnly;

                case "MachineToApplication":
                    return ConfigurationAllowExeDefinition.MachineToApplication;

                case "MachineToRoamingUser":
                    return ConfigurationAllowExeDefinition.MachineToRoamingUser;

                case "MachineToLocalUser":
                    return ConfigurationAllowExeDefinition.MachineToLocalUser;
            }
            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_section_allow_exe_definition_attribute_invalid"), xmlUtil);
        }

        protected object CallCreateSection(bool inputIsTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader, string filename, int line)
        {
            object obj2;
            try
            {
                using (this.Impersonate())
                {
                    obj2 = this.CreateSection(inputIsTrusted, factoryRecord, sectionRecord, parentConfig, reader);
                    if ((obj2 == null) && (parentConfig != null))
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_object_is_null"), filename, line);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_exception_creating_section_handler", new object[] { factoryRecord.ConfigKey }), exception, filename, line);
            }
            return obj2;
        }

        protected virtual string CallHostDecryptSection(string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedConfig)
        {
            return this.Host.DecryptSection(encryptedXml, protectionProvider, protectedConfig);
        }

        private void CheckPermissionAllowed(string configKey, bool requirePermission, bool isTrustedWithoutAptca)
        {
            if (requirePermission)
            {
                try
                {
                    UnrestrictedConfigPermission.Demand();
                }
                catch (SecurityException exception)
                {
                    throw new SecurityException(System.Configuration.SR.GetString("ConfigurationPermission_Denied", new object[] { configKey }), exception);
                }
            }
            if (isTrustedWithoutAptca && !this.Host.IsFullTrustSectionWithoutAptcaAllowed(this))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Section_from_untrusted_assembly", new object[] { configKey }));
            }
        }

        internal void CloseRecursive()
        {
            if (!this._flags[2])
            {
                bool flag = false;
                HybridDictionary streamInfos = null;
                StreamChangeCallback callbackDelegate = null;
                lock (this)
                {
                    if (!this._flags[2])
                    {
                        this._flags[2] = true;
                        flag = true;
                        if (!this.IsLocationConfig && this.ConfigStreamInfo.HasStreamInfos)
                        {
                            callbackDelegate = this.ConfigStreamInfo.CallbackDelegate;
                            streamInfos = this.ConfigStreamInfo.StreamInfos;
                            this.ConfigStreamInfo.CallbackDelegate = null;
                            this.ConfigStreamInfo.ClearStreamInfos();
                        }
                    }
                }
                if (flag)
                {
                    if (this._children != null)
                    {
                        foreach (BaseConfigurationRecord record in this._children.Values)
                        {
                            record.CloseRecursive();
                        }
                    }
                    if (streamInfos != null)
                    {
                        foreach (StreamInfo info in streamInfos.Values)
                        {
                            if (info.IsMonitored)
                            {
                                this.Host.StopMonitoringStreamForChanges(info.StreamName, callbackDelegate);
                                info.IsMonitored = false;
                            }
                        }
                    }
                }
            }
        }

        internal static string CombineConfigKey(string parentConfigKey, string tagName)
        {
            if (string.IsNullOrEmpty(parentConfigKey))
            {
                return tagName;
            }
            if (string.IsNullOrEmpty(tagName))
            {
                return parentConfigKey;
            }
            return (parentConfigKey + "/" + tagName);
        }

        protected abstract object CreateSection(bool inputIsTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader);
        protected void CreateSectionDefault(string configKey, bool getRuntimeObject, FactoryRecord factoryRecord, SectionRecord sectionRecord, out object result, out object resultRuntimeObject)
        {
            SectionRecord record;
            object runtimeObject;
            result = null;
            resultRuntimeObject = null;
            if (sectionRecord != null)
            {
                record = sectionRecord;
            }
            else
            {
                record = new SectionRecord(configKey);
            }
            object obj2 = this.CallCreateSection(true, factoryRecord, record, null, null, null, -1);
            if (getRuntimeObject)
            {
                runtimeObject = this.GetRuntimeObject(obj2);
            }
            else
            {
                runtimeObject = null;
            }
            result = obj2;
            resultRuntimeObject = runtimeObject;
        }

        protected abstract object CreateSectionFactory(FactoryRecord factoryRecord);
        [Conditional("DBG")]
        private void DebugValidateIndirectInputs(SectionRecord sectionRecord)
        {
            if (!this._parent.IsRootConfig)
            {
                for (int i = sectionRecord.IndirectLocationInputs.Count - 1; i >= 0; i--)
                {
                    SectionInput local1 = sectionRecord.IndirectLocationInputs[i];
                }
            }
        }

        private ConfigXmlReader DecryptConfigSection(ConfigXmlReader reader, ProtectedConfigurationProvider protectionProvider)
        {
            XmlNodeType nodeType;
            ConfigXmlReader reader2 = reader.Clone();
            IConfigErrorInfo info = reader2;
            string encryptedXml = null;
            string rawXml = null;
            reader2.Read();
            string filename = info.Filename;
            int lineNumber = info.LineNumber;
            int lineOffset = lineNumber;
            if (reader2.IsEmptyElement)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("EncryptedNode_not_found"), filename, lineNumber);
            }
            do
            {
                reader2.Read();
                nodeType = reader2.NodeType;
                if ((nodeType == XmlNodeType.Element) && (reader2.Name == "EncryptedData"))
                {
                    lineNumber = info.LineNumber;
                    encryptedXml = reader2.ReadOuterXml();
                    try
                    {
                        rawXml = this.CallHostDecryptSection(encryptedXml, protectionProvider, this.ProtectedConfig);
                    }
                    catch (Exception exception)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Decryption_failed", new object[] { protectionProvider.Name, exception.Message }), exception, filename, lineNumber);
                    }
                    do
                    {
                        nodeType = reader2.NodeType;
                        if (nodeType == XmlNodeType.EndElement)
                        {
                            break;
                        }
                        if ((nodeType != XmlNodeType.Comment) && (nodeType != XmlNodeType.Whitespace))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("EncryptedNode_is_in_invalid_format"), filename, lineNumber);
                        }
                    }
                    while (reader2.Read());
                    return new ConfigXmlReader(rawXml, filename, lineOffset, true);
                }
                if (nodeType == XmlNodeType.EndElement)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("EncryptedNode_not_found"), filename, lineNumber);
                }
            }
            while ((nodeType == XmlNodeType.Comment) || (nodeType == XmlNodeType.Whitespace));
            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("EncryptedNode_is_in_invalid_format"), filename, lineNumber);
        }

        protected Hashtable EnsureFactories()
        {
            if (this._factoryRecords == null)
            {
                this._factoryRecords = new Hashtable();
            }
            return this._factoryRecords;
        }

        private ArrayList EnsureLocationSections()
        {
            if (this._locationSections == null)
            {
                this._locationSections = new ArrayList();
            }
            return this._locationSections;
        }

        protected SectionRecord EnsureSectionRecord(string configKey, bool permitErrors)
        {
            return this.EnsureSectionRecordImpl(configKey, permitErrors, true);
        }

        private SectionRecord EnsureSectionRecordImpl(string configKey, bool permitErrors, bool setLockSettings)
        {
            SectionRecord sectionRecord = this.GetSectionRecord(configKey, permitErrors);
            if (sectionRecord == null)
            {
                lock (this)
                {
                    if (this._sectionRecords == null)
                    {
                        this._sectionRecords = new Hashtable();
                    }
                    else
                    {
                        sectionRecord = this.GetSectionRecord(configKey, permitErrors);
                    }
                    if (sectionRecord == null)
                    {
                        sectionRecord = new SectionRecord(configKey);
                        this._sectionRecords.Add(configKey, sectionRecord);
                    }
                }
                if (setLockSettings)
                {
                    OverrideMode inherit = OverrideMode.Inherit;
                    OverrideMode childLockMode = OverrideMode.Inherit;
                    inherit = this.ResolveOverrideModeFromParent(configKey, out childLockMode);
                    sectionRecord.ChangeLockSettings(inherit, childLockMode);
                }
            }
            return sectionRecord;
        }

        protected SectionRecord EnsureSectionRecordUnsafe(string configKey, bool permitErrors)
        {
            return this.EnsureSectionRecordImpl(configKey, permitErrors, false);
        }

        private bool Evaluate(FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentResult, bool getLkg, bool getRuntimeObject, out object result, out object resultRuntimeObject)
        {
            result = null;
            resultRuntimeObject = null;
            object obj2 = null;
            object runtimeObject = null;
            List<SectionInput> locationInputs = sectionRecord.LocationInputs;
            List<SectionInput> indirectLocationInputs = sectionRecord.IndirectLocationInputs;
            SectionInput fileInput = sectionRecord.FileInput;
            bool flag = false;
            if (sectionRecord.HasResult)
            {
                if (getRuntimeObject && !sectionRecord.HasResultRuntimeObject)
                {
                    try
                    {
                        sectionRecord.ResultRuntimeObject = this.GetRuntimeObject(sectionRecord.Result);
                    }
                    catch
                    {
                        if (!getLkg)
                        {
                            throw;
                        }
                    }
                }
                if (!getRuntimeObject || sectionRecord.HasResultRuntimeObject)
                {
                    obj2 = sectionRecord.Result;
                    if (getRuntimeObject)
                    {
                        runtimeObject = sectionRecord.ResultRuntimeObject;
                    }
                    flag = true;
                }
            }
            if (!flag)
            {
                Exception exception = null;
                try
                {
                    string configKey = factoryRecord.ConfigKey;
                    string[] keys = configKey.Split(ConfigPathSeparatorParams);
                    object obj4 = parentResult;
                    if (indirectLocationInputs != null)
                    {
                        foreach (SectionInput input2 in indirectLocationInputs)
                        {
                            if (!input2.HasResult)
                            {
                                input2.ThrowOnErrors();
                                bool isTrusted = this.Host.IsTrustedConfigPath(input2.SectionXmlInfo.DefinitionConfigPath);
                                input2.Result = this.EvaluateOne(keys, input2, isTrusted, factoryRecord, sectionRecord, obj4);
                            }
                            obj4 = input2.Result;
                        }
                    }
                    if (locationInputs != null)
                    {
                        foreach (SectionInput input3 in locationInputs)
                        {
                            if (!input3.HasResult)
                            {
                                input3.ThrowOnErrors();
                                bool flag3 = this.Host.IsTrustedConfigPath(input3.SectionXmlInfo.DefinitionConfigPath);
                                input3.Result = this.EvaluateOne(keys, input3, flag3, factoryRecord, sectionRecord, obj4);
                            }
                            obj4 = input3.Result;
                        }
                    }
                    if (fileInput != null)
                    {
                        if (!fileInput.HasResult)
                        {
                            fileInput.ThrowOnErrors();
                            bool flag4 = this._flags[0x2000];
                            fileInput.Result = this.EvaluateOne(keys, fileInput, flag4, factoryRecord, sectionRecord, obj4);
                        }
                        obj4 = fileInput.Result;
                    }
                    else
                    {
                        obj4 = this.UseParentResult(configKey, obj4, sectionRecord);
                    }
                    if (getRuntimeObject)
                    {
                        runtimeObject = this.GetRuntimeObject(obj4);
                    }
                    obj2 = obj4;
                    flag = true;
                }
                catch (Exception exception2)
                {
                    if (!getLkg || (locationInputs == null))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (!flag)
                {
                    int count = locationInputs.Count;
                    while (--count >= 0)
                    {
                        SectionInput input4 = locationInputs[count];
                        if (input4.HasResult)
                        {
                            if (getRuntimeObject && !input4.HasResultRuntimeObject)
                            {
                                try
                                {
                                    input4.ResultRuntimeObject = this.GetRuntimeObject(input4.Result);
                                }
                                catch
                                {
                                }
                            }
                            if (!getRuntimeObject || input4.HasResultRuntimeObject)
                            {
                                obj2 = input4.Result;
                                if (getRuntimeObject)
                                {
                                    runtimeObject = input4.ResultRuntimeObject;
                                }
                                break;
                            }
                        }
                    }
                    if (count < 0)
                    {
                        throw exception;
                    }
                }
            }
            if (flag && !this._flags[0x80000])
            {
                sectionRecord.ClearRawXml();
            }
            result = obj2;
            if (getRuntimeObject)
            {
                resultRuntimeObject = runtimeObject;
            }
            return flag;
        }

        private object EvaluateOne(string[] keys, SectionInput input, bool isTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentResult)
        {
            object obj2;
            try
            {
                ConfigXmlReader sectionXmlReader = this.GetSectionXmlReader(keys, input);
                if (sectionXmlReader == null)
                {
                    return this.UseParentResult(factoryRecord.ConfigKey, parentResult, sectionRecord);
                }
                obj2 = this.CallCreateSection(isTrusted, factoryRecord, sectionRecord, parentResult, sectionXmlReader, input.SectionXmlInfo.Filename, input.SectionXmlInfo.LineNumber);
            }
            catch (Exception exception)
            {
                throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_exception_creating_section", new object[] { factoryRecord.ConfigKey }), exception, input.SectionXmlInfo);
            }
            return obj2;
        }

        private FactoryRecord FindAndEnsureFactoryRecord(string configKey, out bool isRootDeclaredHere)
        {
            BaseConfigurationRecord record;
            isRootDeclaredHere = false;
            FactoryRecord errorInfo = this.FindFactoryRecord(configKey, false, out record);
            if ((errorInfo != null) && !errorInfo.IsGroup)
            {
                BaseConfigurationRecord record6;
                FactoryRecord factoryRecord = errorInfo;
                BaseConfigurationRecord objB = record;
                for (BaseConfigurationRecord record5 = record._parent; !record5.IsRootConfig; record5 = record6.Parent)
                {
                    FactoryRecord record7 = record5.FindFactoryRecord(configKey, false, out record6);
                    if (record7 == null)
                    {
                        break;
                    }
                    factoryRecord = record7;
                    objB = record6;
                }
                if (factoryRecord.Factory == null)
                {
                    try
                    {
                        object obj2 = objB.CreateSectionFactory(factoryRecord);
                        bool flag = System.Configuration.TypeUtil.IsTypeFromTrustedAssemblyWithoutAptca(obj2.GetType());
                        factoryRecord.Factory = obj2;
                        factoryRecord.IsFactoryTrustedWithoutAptca = flag;
                    }
                    catch (Exception exception)
                    {
                        throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_exception_creating_section_handler", new object[] { errorInfo.ConfigKey }), exception, errorInfo);
                    }
                }
                if (errorInfo.Factory == null)
                {
                    errorInfo.Factory = factoryRecord.Factory;
                    errorInfo.IsFactoryTrustedWithoutAptca = factoryRecord.IsFactoryTrustedWithoutAptca;
                }
                isRootDeclaredHere = object.ReferenceEquals(this, objB);
            }
            return errorInfo;
        }

        internal string FindChangedConfigurationStream()
        {
            for (BaseConfigurationRecord record = this; !record.IsRootConfig; record = record._parent)
            {
                lock (record)
                {
                    if (record.ConfigStreamInfo.HasStreamInfos)
                    {
                        foreach (StreamInfo info in record.ConfigStreamInfo.StreamInfos.Values)
                        {
                            if (info.IsMonitored && this.HasStreamChanged(info.StreamName, info.Version))
                            {
                                return info.StreamName;
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal FactoryRecord FindFactoryRecord(string configKey, bool permitErrors)
        {
            BaseConfigurationRecord record;
            return this.FindFactoryRecord(configKey, permitErrors, out record);
        }

        internal FactoryRecord FindFactoryRecord(string configKey, bool permitErrors, out BaseConfigurationRecord configRecord)
        {
            configRecord = null;
            for (BaseConfigurationRecord record = this; !record.IsRootConfig; record = record._parent)
            {
                FactoryRecord factoryRecord = record.GetFactoryRecord(configKey, permitErrors);
                if (factoryRecord != null)
                {
                    configRecord = record;
                    return factoryRecord;
                }
            }
            return null;
        }

        private ConfigXmlReader FindSection(string[] keys, SectionXmlInfo sectionXmlInfo, out int lineNumber)
        {
            lineNumber = 0;
            ConfigXmlReader reader = null;
            try
            {
                using (this.Impersonate())
                {
                    using (Stream stream = this.Host.OpenStreamForRead(sectionXmlInfo.Filename))
                    {
                        if (!this._flags[0x20000] && ((stream == null) || this.HasStreamChanged(sectionXmlInfo.Filename, sectionXmlInfo.StreamVersion)))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_file_has_changed"), sectionXmlInfo.Filename, 0);
                        }
                        if (stream != null)
                        {
                            using (XmlUtil util = new XmlUtil(stream, sectionXmlInfo.Filename, true))
                            {
                                if (sectionXmlInfo.SubPath == null)
                                {
                                    reader = this.FindSectionRecursive(keys, 0, util, ref lineNumber);
                                }
                                else
                                {
                                    util.ReadToNextElement();
                                    while (util.Reader.Depth > 0)
                                    {
                                        if (util.Reader.Name == "location")
                                        {
                                            bool flag = false;
                                            string attribute = util.Reader.GetAttribute("path");
                                            try
                                            {
                                                attribute = NormalizeLocationSubPath(attribute, util);
                                                flag = true;
                                            }
                                            catch (ConfigurationException exception)
                                            {
                                                util.SchemaErrors.AddError(exception, ExceptionAction.NonSpecific);
                                            }
                                            if (flag && StringUtil.EqualsIgnoreCase(sectionXmlInfo.SubPath, attribute))
                                            {
                                                reader = this.FindSectionRecursive(keys, 0, util, ref lineNumber);
                                                if (reader != null)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        util.SkipToNextElement();
                                    }
                                }
                                this.ThrowIfParseErrors(util.SchemaErrors);
                            }
                        }
                        return reader;
                    }
                }
            }
            catch
            {
                throw;
            }
            return reader;
        }

        private ConfigXmlReader FindSectionRecursive(string[] keys, int iKey, XmlUtil xmlUtil, ref int lineNumber)
        {
            string str = keys[iKey];
            ConfigXmlReader reader = null;
            int depth = xmlUtil.Reader.Depth;
            xmlUtil.ReadToNextElement();
            while (xmlUtil.Reader.Depth > depth)
            {
                if (xmlUtil.Reader.Name == str)
                {
                    if (iKey >= (keys.Length - 1))
                    {
                        string filename = xmlUtil.Filename;
                        return new ConfigXmlReader(xmlUtil.CopySection(), filename, xmlUtil.Reader.LineNumber);
                    }
                    reader = this.FindSectionRecursive(keys, iKey + 1, xmlUtil, ref lineNumber);
                    if (reader != null)
                    {
                        return reader;
                    }
                }
                else
                {
                    if ((iKey == 0) && (xmlUtil.Reader.Name == "location"))
                    {
                        string attribute = xmlUtil.Reader.GetAttribute("path");
                        bool flag = false;
                        try
                        {
                            attribute = NormalizeLocationSubPath(attribute, xmlUtil);
                            flag = true;
                        }
                        catch (ConfigurationException exception)
                        {
                            xmlUtil.SchemaErrors.AddError(exception, ExceptionAction.NonSpecific);
                        }
                        if (flag && (attribute == null))
                        {
                            reader = this.FindSectionRecursive(keys, iKey, xmlUtil, ref lineNumber);
                            if (reader == null)
                            {
                                continue;
                            }
                            return reader;
                        }
                    }
                    xmlUtil.SkipToNextElement();
                }
            }
            return reader;
        }

        internal FactoryRecord GetFactoryRecord(string configKey, bool permitErrors)
        {
            if (this._factoryRecords == null)
            {
                return null;
            }
            FactoryRecord record = (FactoryRecord) this._factoryRecords[configKey];
            if ((record != null) && !permitErrors)
            {
                record.ThrowOnErrors();
            }
            return record;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public object GetLkgSection(string configKey)
        {
            return this.GetSection(configKey, true, true);
        }

        internal ProtectedConfigurationProvider GetProtectionProviderFromName(string providerName, bool throwIfNotFound)
        {
            if (!string.IsNullOrEmpty(providerName))
            {
                return this.ProtectedConfig.GetProviderFromName(providerName);
            }
            if (throwIfNotFound)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("ProtectedConfigurationProvider_not_found", new object[] { providerName }));
            }
            return null;
        }

        internal PermissionSet GetRestrictedPermissions()
        {
            if (!this._flags[0x800])
            {
                PermissionSet set;
                bool flag;
                this.Host.GetRestrictedPermissions(this, out set, out flag);
                if (flag)
                {
                    this._restrictedPermissions = set;
                    this._flags[0x800] = true;
                }
            }
            return this._restrictedPermissions;
        }

        protected abstract object GetRuntimeObject(object result);
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public object GetSection(string configKey)
        {
            return this.GetSection(configKey, false, true);
        }

        private object GetSection(string configKey, bool getLkg, bool checkPermission)
        {
            object obj2;
            object obj3;
            this.GetSectionRecursive(configKey, getLkg, checkPermission, true, true, out obj2, out obj3);
            return obj3;
        }

        protected OverrideMode GetSectionLockedMode(string configKey)
        {
            OverrideMode inherit = OverrideMode.Inherit;
            return this.GetSectionLockedMode(configKey, out inherit);
        }

        protected OverrideMode GetSectionLockedMode(string configKey, out OverrideMode childLockMode)
        {
            OverrideMode inherit = OverrideMode.Inherit;
            SectionRecord sectionRecord = this.GetSectionRecord(configKey, true);
            if (sectionRecord != null)
            {
                inherit = sectionRecord.Locked ? OverrideMode.Deny : OverrideMode.Allow;
                childLockMode = sectionRecord.LockChildren ? OverrideMode.Deny : OverrideMode.Allow;
                return inherit;
            }
            return this.ResolveOverrideModeFromParent(configKey, out childLockMode);
        }

        protected SectionRecord GetSectionRecord(string configKey, bool permitErrors)
        {
            SectionRecord record;
            if (this._sectionRecords != null)
            {
                record = (SectionRecord) this._sectionRecords[configKey];
            }
            else
            {
                record = null;
            }
            if ((record != null) && !permitErrors)
            {
                record.ThrowOnErrors();
            }
            return record;
        }

        private void GetSectionRecursive(string configKey, bool getLkg, bool checkPermission, bool getRuntimeObject, bool requestIsHere, out object result, out object resultRuntimeObject)
        {
            result = null;
            resultRuntimeObject = null;
            object obj2 = null;
            object obj3 = null;
            bool requirePermission = true;
            bool isTrustedWithoutAptca = true;
            if (!getLkg)
            {
                this.ThrowIfInitErrors();
            }
            bool flag3 = false;
            SectionRecord sectionRecord = this.GetSectionRecord(configKey, getLkg);
            if ((sectionRecord != null) && sectionRecord.HasResult)
            {
                if (getRuntimeObject && !sectionRecord.HasResultRuntimeObject)
                {
                    try
                    {
                        sectionRecord.ResultRuntimeObject = this.GetRuntimeObject(sectionRecord.Result);
                    }
                    catch
                    {
                        if (!getLkg)
                        {
                            throw;
                        }
                    }
                }
                if (!getRuntimeObject || sectionRecord.HasResultRuntimeObject)
                {
                    requirePermission = sectionRecord.RequirePermission;
                    isTrustedWithoutAptca = sectionRecord.IsResultTrustedWithoutAptca;
                    obj2 = sectionRecord.Result;
                    if (getRuntimeObject)
                    {
                        obj3 = sectionRecord.ResultRuntimeObject;
                    }
                    flag3 = true;
                }
            }
            if (!flag3)
            {
                FactoryRecord factoryRecord = null;
                bool flag4 = (sectionRecord != null) && sectionRecord.HasInput;
                bool flag5 = requestIsHere || flag4;
                try
                {
                    bool flag6;
                    if (requestIsHere)
                    {
                        factoryRecord = this.FindAndEnsureFactoryRecord(configKey, out flag6);
                        if (this.IsInitDelayed && ((factoryRecord == null) || this._initDelayedRoot.IsDefinitionAllowed(factoryRecord.AllowDefinition, factoryRecord.AllowExeDefinition)))
                        {
                            string configPath = this._configPath;
                            InternalConfigRoot root = this._configRoot;
                            this.Host.RequireCompleteInit(this._initDelayedRoot);
                            this._initDelayedRoot.Remove();
                            ((BaseConfigurationRecord) root.GetConfigRecord(configPath)).GetSectionRecursive(configKey, getLkg, checkPermission, getRuntimeObject, requestIsHere, out result, out resultRuntimeObject);
                            return;
                        }
                        if ((factoryRecord == null) || factoryRecord.IsGroup)
                        {
                            return;
                        }
                        configKey = factoryRecord.ConfigKey;
                    }
                    else if (flag4)
                    {
                        factoryRecord = this.FindAndEnsureFactoryRecord(configKey, out flag6);
                    }
                    else
                    {
                        factoryRecord = this.GetFactoryRecord(configKey, false);
                        if (factoryRecord == null)
                        {
                            flag6 = false;
                        }
                        else
                        {
                            factoryRecord = this.FindAndEnsureFactoryRecord(configKey, out flag6);
                        }
                    }
                    if (flag6)
                    {
                        flag5 = true;
                    }
                    if ((sectionRecord == null) && flag5)
                    {
                        sectionRecord = this.EnsureSectionRecord(configKey, true);
                    }
                    bool flag7 = getRuntimeObject && !flag4;
                    object obj4 = null;
                    object obj5 = null;
                    if (flag6)
                    {
                        SectionRecord record4 = flag4 ? null : sectionRecord;
                        this.CreateSectionDefault(configKey, flag7, factoryRecord, record4, out obj4, out obj5);
                    }
                    else
                    {
                        this._parent.GetSectionRecursive(configKey, false, false, flag7, false, out obj4, out obj5);
                    }
                    if (flag4)
                    {
                        if (!this.Evaluate(factoryRecord, sectionRecord, obj4, getLkg, getRuntimeObject, out obj2, out obj3))
                        {
                            flag5 = false;
                        }
                    }
                    else if (sectionRecord != null)
                    {
                        obj2 = this.UseParentResult(configKey, obj4, sectionRecord);
                        if (getRuntimeObject)
                        {
                            if (object.ReferenceEquals(obj4, obj5))
                            {
                                obj3 = obj2;
                            }
                            else
                            {
                                obj3 = this.UseParentResult(configKey, obj5, sectionRecord);
                            }
                        }
                    }
                    else
                    {
                        obj2 = obj4;
                        obj3 = obj5;
                    }
                    if (flag5 || checkPermission)
                    {
                        requirePermission = factoryRecord.RequirePermission;
                        isTrustedWithoutAptca = factoryRecord.IsFactoryTrustedWithoutAptca;
                        if (flag5)
                        {
                            if (sectionRecord == null)
                            {
                                sectionRecord = this.EnsureSectionRecord(configKey, true);
                            }
                            sectionRecord.Result = obj2;
                            if (getRuntimeObject)
                            {
                                sectionRecord.ResultRuntimeObject = obj3;
                            }
                            sectionRecord.RequirePermission = requirePermission;
                            sectionRecord.IsResultTrustedWithoutAptca = isTrustedWithoutAptca;
                        }
                    }
                    flag3 = true;
                }
                catch
                {
                    if (!getLkg)
                    {
                        throw;
                    }
                }
                if (!flag3)
                {
                    this._parent.GetSectionRecursive(configKey, true, checkPermission, true, true, out result, out resultRuntimeObject);
                    return;
                }
            }
            if (checkPermission)
            {
                this.CheckPermissionAllowed(configKey, requirePermission, isTrustedWithoutAptca);
            }
            result = obj2;
            if (getRuntimeObject)
            {
                resultRuntimeObject = obj3;
            }
        }

        protected ConfigXmlReader GetSectionXmlReader(string[] keys, SectionInput input)
        {
            ConfigXmlReader reader = null;
            string filename = input.SectionXmlInfo.Filename;
            int lineNumber = input.SectionXmlInfo.LineNumber;
            try
            {
                string name = keys[keys.Length - 1];
                string rawXml = input.SectionXmlInfo.RawXml;
                if (rawXml != null)
                {
                    reader = new ConfigXmlReader(rawXml, input.SectionXmlInfo.Filename, input.SectionXmlInfo.LineNumber);
                }
                else if (!string.IsNullOrEmpty(input.SectionXmlInfo.ConfigSource))
                {
                    filename = input.SectionXmlInfo.ConfigSourceStreamName;
                    lineNumber = 0;
                    reader = this.LoadConfigSource(name, input.SectionXmlInfo);
                }
                else
                {
                    lineNumber = 0;
                    reader = this.FindSection(keys, input.SectionXmlInfo, out lineNumber);
                }
                if (reader == null)
                {
                    return reader;
                }
                if (!input.IsProtectionProviderDetermined)
                {
                    input.ProtectionProvider = this.GetProtectionProviderFromName(input.SectionXmlInfo.ProtectionProviderName, false);
                }
                if (input.ProtectionProvider != null)
                {
                    reader = this.DecryptConfigSection(reader, input.ProtectionProvider);
                }
            }
            catch (Exception exception)
            {
                throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_error_loading_XML_file"), exception, filename, lineNumber);
            }
            return reader;
        }

        private bool HasStreamChanged(string streamname, object lastVersion)
        {
            object streamVersion = this.Host.GetStreamVersion(streamname);
            if (lastVersion == null)
            {
                return (streamVersion != null);
            }
            if (streamVersion != null)
            {
                return !lastVersion.Equals(streamVersion);
            }
            return true;
        }

        internal void hlAddChild(string configName, BaseConfigurationRecord child)
        {
            if (this._children == null)
            {
                this._children = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }
            this._children.Add(configName, child);
        }

        internal void hlClearResultRecursive(string configKey, bool forceEvaluatation)
        {
            this.RefreshFactoryRecord(configKey);
            SectionRecord sectionRecord = this.GetSectionRecord(configKey, false);
            if (sectionRecord != null)
            {
                sectionRecord.ClearResult();
                sectionRecord.ClearRawXml();
            }
            if ((forceEvaluatation && !this.IsInitDelayed) && !string.IsNullOrEmpty(this.ConfigStreamInfo.StreamName))
            {
                if (this._flags[0x40000])
                {
                    throw ExceptionUtil.UnexpectedError("BaseConfigurationRecord::hlClearResultRecursive");
                }
                FactoryRecord record2 = this.FindFactoryRecord(configKey, false);
                if ((record2 != null) && !record2.IsGroup)
                {
                    configKey = record2.ConfigKey;
                    sectionRecord = this.EnsureSectionRecord(configKey, false);
                    if (!sectionRecord.HasFileInput)
                    {
                        SectionXmlInfo sectionXmlInfo = new SectionXmlInfo(configKey, this._configPath, this._configPath, null, this.ConfigStreamInfo.StreamName, 0, null, null, null, null, null, null, OverrideModeSetting.LocationDefault, false);
                        SectionInput sectionInput = new SectionInput(sectionXmlInfo, null);
                        sectionRecord.AddFileInput(sectionInput);
                    }
                }
            }
            if (this._children != null)
            {
                foreach (BaseConfigurationRecord record3 in this._children.Values)
                {
                    record3.hlClearResultRecursive(configKey, forceEvaluatation);
                }
            }
        }

        internal BaseConfigurationRecord hlGetChild(string configName)
        {
            if (this._children == null)
            {
                return null;
            }
            return (BaseConfigurationRecord) this._children[configName];
        }

        internal bool hlNeedsChildFor(string configName)
        {
            if (this.IsRootConfig)
            {
                return true;
            }
            if (!this.HasInitErrors)
            {
                string configPath = ConfigPathUtility.Combine(this._configPath, configName);
                try
                {
                    using (this.Impersonate())
                    {
                        if (this.Host.IsConfigRecordRequired(configPath))
                        {
                            return true;
                        }
                    }
                }
                catch
                {
                    throw;
                }
                if (this._flags[0x100000])
                {
                    for (BaseConfigurationRecord record = this; !record.IsRootConfig; record = record._parent)
                    {
                        if (record._locationSections != null)
                        {
                            record.ResolveLocationSections();
                            foreach (LocationSectionRecord record2 in record._locationSections)
                            {
                                if (UrlPath.IsEqualOrSubpath(configPath, record2.SectionXmlInfo.TargetConfigPath))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal void hlRemoveChild(string configName)
        {
            if (this._children != null)
            {
                this._children.Remove(configName);
            }
        }

        protected IDisposable Impersonate()
        {
            IDisposable staticInstance = null;
            if (this.ClassFlags[4])
            {
                staticInstance = this.Host.Impersonate();
            }
            if (staticInstance == null)
            {
                staticInstance = EmptyImpersonationContext.GetStaticInstance();
            }
            return staticInstance;
        }

        internal void Init(IInternalConfigRoot configRoot, BaseConfigurationRecord parent, string configPath, string locationSubPath)
        {
            this._initErrors = new ConfigurationSchemaErrors();
            try
            {
                this._configRoot = (InternalConfigRoot) configRoot;
                this._parent = parent;
                this._configPath = configPath;
                this._locationSubPath = locationSubPath;
                this._configName = ConfigPathUtility.GetName(configPath);
                if (this.IsLocationConfig)
                {
                    this._configStreamInfo = this._parent.ConfigStreamInfo;
                }
                else
                {
                    this._configStreamInfo = new ConfigRecordStreamInfo();
                }
                if (!this.IsRootConfig)
                {
                    this._flags[0x10000] = this.ClassFlags[1] && this.Host.SupportsChangeNotifications;
                    this._flags[0x20000] = this.ClassFlags[2] && this.Host.SupportsRefresh;
                    this._flags[0x80000] = this.ClassFlags[0x10] || this._flags[0x20000];
                    this._flags[0x40000] = this.Host.SupportsPath;
                    this._flags[0x100000] = this.Host.SupportsLocation;
                    if (this._flags[0x100000])
                    {
                        this._flags[0x20] = this.Host.IsAboveApplication(this._configPath);
                    }
                    this._flags[0x2000] = this.Host.IsTrustedConfigPath(this._configPath);
                    ArrayList list = null;
                    if (this._flags[0x100000])
                    {
                        if (this.IsLocationConfig && (this._parent._locationSections != null))
                        {
                            this._parent.ResolveLocationSections();
                            int index = 0;
                            while (index < this._parent._locationSections.Count)
                            {
                                LocationSectionRecord record = (LocationSectionRecord) this._parent._locationSections[index];
                                if (!StringUtil.EqualsIgnoreCase(record.SectionXmlInfo.TargetConfigPath, this.ConfigPath))
                                {
                                    index++;
                                }
                                else
                                {
                                    this._parent._locationSections.RemoveAt(index);
                                    if (list == null)
                                    {
                                        list = new ArrayList();
                                    }
                                    list.Add(record);
                                }
                            }
                        }
                        if (this.IsLocationConfig && this.Host.IsLocationApplicable(this._configPath))
                        {
                            Dictionary<string, List<SectionInput>> dictionary = null;
                            for (BaseConfigurationRecord record2 = this._parent; !record2.IsRootConfig; record2 = record2._parent)
                            {
                                if (record2._locationSections != null)
                                {
                                    record2.ResolveLocationSections();
                                    foreach (LocationSectionRecord record3 in record2._locationSections)
                                    {
                                        if ((this.IsLocationConfig && UrlPath.IsSubpath(record3.SectionXmlInfo.TargetConfigPath, this.ConfigPath)) && (UrlPath.IsSubpath(parent.ConfigPath, record3.SectionXmlInfo.TargetConfigPath) && !this.ShouldSkipDueToInheritInChildApplications(record3.SectionXmlInfo.SkipInChildApps, record3.SectionXmlInfo.TargetConfigPath)))
                                        {
                                            if (dictionary == null)
                                            {
                                                dictionary = new Dictionary<string, List<SectionInput>>(1);
                                            }
                                            string configKey = record3.SectionXmlInfo.ConfigKey;
                                            if (!dictionary.Contains(configKey))
                                            {
                                                dictionary.Add(configKey, new List<SectionInput>(1));
                                            }
                                            dictionary[configKey].Add(new SectionInput(record3.SectionXmlInfo, record3.ErrorsList));
                                            if (record3.HasErrors)
                                            {
                                                this._initErrors.AddSavedLocalErrors(record3.Errors);
                                            }
                                        }
                                    }
                                }
                            }
                            if (dictionary != null)
                            {
                                foreach (KeyValuePair<string, List<SectionInput>> pair in dictionary)
                                {
                                    List<SectionInput> list2 = pair.Value;
                                    string key = pair.Key;
                                    list2.Sort(s_indirectInputsComparer);
                                    SectionRecord record4 = this.EnsureSectionRecord(key, true);
                                    foreach (SectionInput input in list2)
                                    {
                                        record4.AddIndirectLocationInput(input);
                                    }
                                }
                            }
                        }
                        if (this.Host.IsLocationApplicable(this._configPath))
                        {
                            for (BaseConfigurationRecord record5 = this._parent; !record5.IsRootConfig; record5 = record5._parent)
                            {
                                if (record5._locationSections != null)
                                {
                                    record5.ResolveLocationSections();
                                    foreach (LocationSectionRecord record6 in record5._locationSections)
                                    {
                                        if (StringUtil.EqualsIgnoreCase(record6.SectionXmlInfo.TargetConfigPath, this._configPath) && !this.ShouldSkipDueToInheritInChildApplications(record6.SectionXmlInfo.SkipInChildApps))
                                        {
                                            SectionRecord record7 = this.EnsureSectionRecord(record6.ConfigKey, true);
                                            SectionInput sectionInput = new SectionInput(record6.SectionXmlInfo, record6.ErrorsList);
                                            record7.AddLocationInput(sectionInput);
                                            if (record6.HasErrors)
                                            {
                                                this._initErrors.AddSavedLocalErrors(record6.Errors);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!this.IsLocationConfig)
                    {
                        this.InitConfigFromFile();
                    }
                    else if (list != null)
                    {
                        foreach (LocationSectionRecord record8 in list)
                        {
                            SectionRecord record9 = this.EnsureSectionRecord(record8.ConfigKey, true);
                            SectionInput input3 = new SectionInput(record8.SectionXmlInfo, record8.ErrorsList);
                            record9.AddFileInput(input3);
                            if (record8.HasErrors)
                            {
                                this._initErrors.AddSavedLocalErrors(record8.Errors);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                string filename = (this.ConfigStreamInfo != null) ? this.ConfigStreamInfo.StreamName : null;
                this._initErrors.AddError(ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_error_loading_XML_file"), exception, filename, 0), ExceptionAction.Global);
            }
        }

        private void InitConfigFromFile()
        {
            bool flag = false;
            try
            {
                if (this.ClassFlags[0x20] && this.Host.IsInitDelayed(this))
                {
                    if (this._parent._initDelayedRoot == null)
                    {
                        this._initDelayedRoot = this;
                    }
                    else
                    {
                        this._initDelayedRoot = this._parent._initDelayedRoot;
                    }
                }
                else
                {
                    using (this.Impersonate())
                    {
                        this.ConfigStreamInfo.StreamName = this.Host.GetStreamName(this._configPath);
                        if (!string.IsNullOrEmpty(this.ConfigStreamInfo.StreamName))
                        {
                            this.ConfigStreamInfo.StreamVersion = this.MonitorStream(null, null, this.ConfigStreamInfo.StreamName);
                            using (Stream stream = this.Host.OpenStreamForRead(this.ConfigStreamInfo.StreamName))
                            {
                                if (stream == null)
                                {
                                    return;
                                }
                                this.ConfigStreamInfo.HasStream = true;
                                this._flags[8] = this.Host.PrefetchAll(this._configPath, this.ConfigStreamInfo.StreamName);
                                using (XmlUtil util = new XmlUtil(stream, this.ConfigStreamInfo.StreamName, true, this._initErrors))
                                {
                                    this.ConfigStreamInfo.StreamEncoding = util.Reader.Encoding;
                                    Hashtable hashtable = this.ScanFactories(util);
                                    this._factoryRecords = hashtable;
                                    this.AddImplicitSections(null);
                                    flag = true;
                                    if (util.Reader.Depth == 1)
                                    {
                                        this.ScanSections(util);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (XmlException exception)
            {
                this._initErrors.SetSingleGlobalError(ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_error_loading_XML_file"), exception, this.ConfigStreamInfo.StreamName, 0));
            }
            catch (Exception exception2)
            {
                this._initErrors.AddError(ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_error_loading_XML_file"), exception2, this.ConfigStreamInfo.StreamName, 0), ExceptionAction.Global);
            }
            if (this._initErrors.HasGlobalErrors)
            {
                this._initErrors.ResetLocalErrors();
                HybridDictionary streamInfos = null;
                lock (this)
                {
                    if (this.ConfigStreamInfo.HasStreamInfos)
                    {
                        streamInfos = this.ConfigStreamInfo.StreamInfos;
                        this.ConfigStreamInfo.ClearStreamInfos();
                        if (!string.IsNullOrEmpty(this.ConfigStreamInfo.StreamName))
                        {
                            StreamInfo info = (StreamInfo) streamInfos[this.ConfigStreamInfo.StreamName];
                            if (info != null)
                            {
                                streamInfos.Remove(this.ConfigStreamInfo.StreamName);
                                this.ConfigStreamInfo.StreamInfos.Add(this.ConfigStreamInfo.StreamName, info);
                            }
                        }
                    }
                }
                if (streamInfos != null)
                {
                    foreach (StreamInfo info2 in streamInfos.Values)
                    {
                        if (info2.IsMonitored)
                        {
                            this.Host.StopMonitoringStreamForChanges(info2.StreamName, this.ConfigStreamInfo.CallbackDelegate);
                        }
                    }
                }
                if (this._sectionRecords != null)
                {
                    List<SectionRecord> list = null;
                    foreach (SectionRecord record in this._sectionRecords.Values)
                    {
                        if (record.HasLocationInputs)
                        {
                            record.RemoveFileInput();
                        }
                        else
                        {
                            if (list == null)
                            {
                                list = new List<SectionRecord>();
                            }
                            list.Add(record);
                        }
                    }
                    if (list != null)
                    {
                        foreach (SectionRecord record2 in list)
                        {
                            this._sectionRecords.Remove(record2.ConfigKey);
                        }
                    }
                }
                if (this._locationSections != null)
                {
                    this._locationSections.Clear();
                }
                if (this._factoryRecords != null)
                {
                    this._factoryRecords.Clear();
                }
            }
            if (!flag)
            {
                this.AddImplicitSections(null);
            }
        }

        internal void InitProtectedConfigurationSection()
        {
            if (!this._flags[1])
            {
                this._protectedConfig = this.GetSection("configProtectedData", false, false) as ProtectedConfigurationSection;
                this._flags[1] = true;
            }
        }

        internal bool IsDefinitionAllowed(ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
        {
            return this.Host.IsDefinitionAllowed(this._configPath, allowDefinition, allowExeDefinition);
        }

        internal static bool IsImplicitSection(string configKey)
        {
            return (configKey == "configProtectedData");
        }

        internal static bool IsReservedAttributeName(string name)
        {
            if (!StringUtil.StartsWith(name, "config") && !StringUtil.StartsWith(name, "lock"))
            {
                return false;
            }
            return true;
        }

        internal bool IsRootDeclaration(string configKey, bool implicitIsRooted)
        {
            if (!implicitIsRooted && IsImplicitSection(configKey))
            {
                return false;
            }
            if (!this._parent.IsRootConfig)
            {
                return (this._parent.FindFactoryRecord(configKey, true) == null);
            }
            return true;
        }

        private ConfigXmlReader LoadConfigSource(string name, SectionXmlInfo sectionXmlInfo)
        {
            ConfigXmlReader reader2;
            string configSourceStreamName = sectionXmlInfo.ConfigSourceStreamName;
            try
            {
                using (this.Impersonate())
                {
                    using (Stream stream = this.Host.OpenStreamForRead(configSourceStreamName))
                    {
                        if (stream == null)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_cannot_open_config_source", new object[] { sectionXmlInfo.ConfigSource }), sectionXmlInfo);
                        }
                        using (XmlUtil util = new XmlUtil(stream, configSourceStreamName, true))
                        {
                            if (util.Reader.Name != name)
                            {
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_file_format"), util);
                            }
                            string attribute = util.Reader.GetAttribute("configProtectionProvider");
                            if (attribute != null)
                            {
                                if (util.Reader.AttributeCount != 1)
                                {
                                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Protection_provider_syntax_error"), util);
                                }
                                sectionXmlInfo.ProtectionProviderName = ValidateProtectionProviderAttribute(attribute, util);
                            }
                            int lineNumber = util.Reader.LineNumber;
                            string rawXml = util.CopySection();
                            while (!util.Reader.EOF)
                            {
                                if (util.Reader.NodeType != XmlNodeType.Comment)
                                {
                                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_file_format"), util);
                                }
                                util.Reader.Read();
                            }
                            reader2 = new ConfigXmlReader(rawXml, configSourceStreamName, lineNumber);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
            return reader2;
        }

        protected object MonitorStream(string configKey, string configSource, string streamname)
        {
            lock (this)
            {
                if (this._flags[2])
                {
                    return null;
                }
                StreamInfo info = (StreamInfo) this.ConfigStreamInfo.StreamInfos[streamname];
                if (info != null)
                {
                    if (info.SectionName != configKey)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_cannot_be_shared", new object[] { streamname }));
                    }
                    if (info.IsMonitored)
                    {
                        return info.Version;
                    }
                }
                else
                {
                    info = new StreamInfo(configKey, configSource, streamname);
                    this.ConfigStreamInfo.StreamInfos.Add(streamname, info);
                }
            }
            object streamVersion = this.Host.GetStreamVersion(streamname);
            StreamChangeCallback callbackDelegate = null;
            lock (this)
            {
                if (this._flags[2])
                {
                    return null;
                }
                StreamInfo info2 = (StreamInfo) this.ConfigStreamInfo.StreamInfos[streamname];
                if (info2.IsMonitored)
                {
                    return info2.Version;
                }
                info2.IsMonitored = true;
                info2.Version = streamVersion;
                if (this._flags[0x10000])
                {
                    if (this.ConfigStreamInfo.CallbackDelegate == null)
                    {
                        this.ConfigStreamInfo.CallbackDelegate = new StreamChangeCallback(this.OnStreamChanged);
                    }
                    callbackDelegate = this.ConfigStreamInfo.CallbackDelegate;
                }
            }
            if (this._flags[0x10000])
            {
                this.Host.StartMonitoringStreamForChanges(streamname, callbackDelegate);
            }
            return streamVersion;
        }

        internal static string NormalizeConfigSource(string configSource, IConfigErrorInfo errorInfo)
        {
            if (string.IsNullOrEmpty(configSource))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_invalid_format"), errorInfo);
            }
            if (configSource.Trim().Length != configSource.Length)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_invalid_format"), errorInfo);
            }
            if (configSource.IndexOf('/') != -1)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_invalid_chars"), errorInfo);
            }
            if (string.IsNullOrEmpty(configSource) || Path.IsPathRooted(configSource))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_invalid_format"), errorInfo);
            }
            return configSource;
        }

        internal static string NormalizeLocationSubPath(string subPath, IConfigErrorInfo errorInfo)
        {
            if (string.IsNullOrEmpty(subPath))
            {
                return null;
            }
            if (subPath == ".")
            {
                return null;
            }
            if (subPath.TrimStart(new char[0]).Length != subPath.Length)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_location_path_invalid_first_character"), errorInfo);
            }
            if (@"\./".IndexOf(subPath[0]) != -1)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_location_path_invalid_first_character"), errorInfo);
            }
            if (subPath.TrimEnd(new char[0]).Length != subPath.Length)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_location_path_invalid_last_character"), errorInfo);
            }
            if (@"\./".IndexOf(subPath[subPath.Length - 1]) != -1)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_location_path_invalid_last_character"), errorInfo);
            }
            if (subPath.IndexOfAny(s_invalidSubPathCharactersArray) != -1)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_location_path_invalid_character"), errorInfo);
            }
            return subPath;
        }

        private void OnStreamChanged(string streamname)
        {
            bool restartOnExternalChanges;
            string sectionName;
            lock (this)
            {
                if (this._flags[2])
                {
                    return;
                }
                StreamInfo info = (StreamInfo) this.ConfigStreamInfo.StreamInfos[streamname];
                if ((info == null) || !info.IsMonitored)
                {
                    return;
                }
                sectionName = info.SectionName;
            }
            if (sectionName == null)
            {
                restartOnExternalChanges = true;
            }
            else
            {
                restartOnExternalChanges = this.FindFactoryRecord(sectionName, false).RestartOnExternalChanges;
            }
            if (restartOnExternalChanges)
            {
                this._configRoot.FireConfigChanged(this._configPath);
            }
            else
            {
                this._configRoot.ClearResult(this, sectionName, false);
            }
        }

        private void RefreshFactoryRecord(string configKey)
        {
            Hashtable factoryList = null;
            FactoryRecord record = null;
            ConfigurationSchemaErrors schemaErrors = new ConfigurationSchemaErrors();
            int line = 0;
            try
            {
                using (this.Impersonate())
                {
                    using (Stream stream = this.Host.OpenStreamForRead(this.ConfigStreamInfo.StreamName))
                    {
                        if (stream != null)
                        {
                            this.ConfigStreamInfo.HasStream = true;
                            using (XmlUtil util = new XmlUtil(stream, this.ConfigStreamInfo.StreamName, true, schemaErrors))
                            {
                                try
                                {
                                    factoryList = this.ScanFactories(util);
                                    this.ThrowIfParseErrors(util.SchemaErrors);
                                }
                                catch
                                {
                                    line = util.LineNumber;
                                    throw;
                                }
                            }
                        }
                    }
                }
                if (factoryList == null)
                {
                    factoryList = new Hashtable();
                }
                this.AddImplicitSections(factoryList);
                if (factoryList != null)
                {
                    record = (FactoryRecord) factoryList[configKey];
                }
            }
            catch (Exception exception)
            {
                schemaErrors.AddError(ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_error_loading_XML_file"), exception, this.ConfigStreamInfo.StreamName, line), ExceptionAction.Global);
            }
            if ((record != null) || this.HasFactoryRecords)
            {
                this.EnsureFactories()[configKey] = record;
            }
            this.ThrowIfParseErrors(schemaErrors);
        }

        public void RefreshSection(string configKey)
        {
            this._configRoot.ClearResult(this, configKey, true);
        }

        public void Remove()
        {
            this._configRoot.RemoveConfigRecord(this);
        }

        private void ResolveLocationSections()
        {
            if (!this._flags[0x100])
            {
                if (!this._parent.IsRootConfig)
                {
                    this._parent.ResolveLocationSections();
                }
                lock (this)
                {
                    if (!this._flags[0x100] && (this._locationSections != null))
                    {
                        HybridDictionary dictionary = new HybridDictionary(true);
                        foreach (LocationSectionRecord record in this._locationSections)
                        {
                            string configPathFromLocationSubPath = this.Host.GetConfigPathFromLocationSubPath(this._configPath, record.SectionXmlInfo.SubPath);
                            record.SectionXmlInfo.TargetConfigPath = configPathFromLocationSubPath;
                            HybridDictionary dictionary2 = (HybridDictionary) dictionary[configPathFromLocationSubPath];
                            if (dictionary2 == null)
                            {
                                dictionary2 = new HybridDictionary(false);
                                dictionary.Add(configPathFromLocationSubPath, dictionary2);
                            }
                            LocationSectionRecord record2 = (LocationSectionRecord) dictionary2[record.ConfigKey];
                            FactoryRecord factoryRecord = null;
                            if (record2 == null)
                            {
                                dictionary2.Add(record.ConfigKey, record);
                            }
                            else
                            {
                                factoryRecord = this.FindFactoryRecord(record.ConfigKey, true);
                                if ((factoryRecord == null) || !factoryRecord.IsIgnorable())
                                {
                                    if (!record2.HasErrors)
                                    {
                                        record2.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_sections_must_be_unique"), record2.SectionXmlInfo));
                                    }
                                    record.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_sections_must_be_unique"), record.SectionXmlInfo));
                                }
                            }
                            if (factoryRecord == null)
                            {
                                factoryRecord = this.FindFactoryRecord(record.ConfigKey, true);
                            }
                            if (!factoryRecord.HasErrors)
                            {
                                try
                                {
                                    this.VerifyDefinitionAllowed(factoryRecord, configPathFromLocationSubPath, record.SectionXmlInfo);
                                }
                                catch (ConfigurationException exception)
                                {
                                    record.AddError(exception);
                                }
                            }
                        }
                        for (BaseConfigurationRecord record4 = this._parent; !record4.IsRootConfig; record4 = record4._parent)
                        {
                            foreach (LocationSectionRecord record5 in this._locationSections)
                            {
                                bool flag = false;
                                SectionRecord sectionRecord = record4.GetSectionRecord(record5.ConfigKey, true);
                                if ((sectionRecord != null) && (sectionRecord.LockChildren || sectionRecord.Locked))
                                {
                                    flag = true;
                                }
                                else if (record4._locationSections != null)
                                {
                                    string targetConfigPath = record5.SectionXmlInfo.TargetConfigPath;
                                    foreach (LocationSectionRecord record7 in record4._locationSections)
                                    {
                                        string subpath = record7.SectionXmlInfo.TargetConfigPath;
                                        if ((record7.SectionXmlInfo.OverrideModeSetting.IsLocked && (record5.ConfigKey == record7.ConfigKey)) && UrlPath.IsEqualOrSubpath(targetConfigPath, subpath))
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                                if (flag)
                                {
                                    record5.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_section_locked"), record5.SectionXmlInfo));
                                }
                            }
                        }
                    }
                    this._flags[0x100] = true;
                }
            }
        }

        private OverrideMode ResolveOverrideModeFromParent(string configKey, out OverrideMode childLockMode)
        {
            OverrideMode inherit = OverrideMode.Inherit;
            BaseConfigurationRecord parent = this.Parent;
            BaseConfigurationRecord objA = this.Parent;
            childLockMode = OverrideMode.Inherit;
            while (!parent.IsRootConfig && (inherit == OverrideMode.Inherit))
            {
                SectionRecord sectionRecord = parent.GetSectionRecord(configKey, true);
                if (sectionRecord != null)
                {
                    if (this.IsLocationConfig && object.ReferenceEquals(objA, parent))
                    {
                        inherit = sectionRecord.Locked ? OverrideMode.Deny : OverrideMode.Allow;
                        childLockMode = sectionRecord.LockChildren ? OverrideMode.Deny : OverrideMode.Allow;
                    }
                    else
                    {
                        inherit = sectionRecord.LockChildren ? OverrideMode.Deny : OverrideMode.Allow;
                        childLockMode = inherit;
                    }
                }
                parent = parent._parent;
            }
            if (inherit == OverrideMode.Inherit)
            {
                bool flag = false;
                OverrideMode overrideMode = this.FindFactoryRecord(configKey, true).OverrideModeDefault.OverrideMode;
                if (this.IsLocationConfig)
                {
                    flag = this.Parent.GetFactoryRecord(configKey, true) != null;
                }
                else
                {
                    flag = this.GetFactoryRecord(configKey, true) != null;
                }
                if (!flag)
                {
                    childLockMode = inherit = overrideMode;
                    return inherit;
                }
                inherit = OverrideMode.Allow;
                childLockMode = overrideMode;
            }
            return inherit;
        }

        private Hashtable ScanFactories(XmlUtil xmlUtil)
        {
            Hashtable factoryList = new Hashtable();
            if ((xmlUtil.Reader.NodeType == XmlNodeType.Element) && !(xmlUtil.Reader.Name != "configuration"))
            {
                while (xmlUtil.Reader.MoveToNextAttribute())
                {
                    string str2;
                    if (((str2 = xmlUtil.Reader.Name) != null) && (str2 == "xmlns"))
                    {
                        if (xmlUtil.Reader.Value == "http://schemas.microsoft.com/.NetConfiguration/v2.0")
                        {
                            this._flags[0x200] = true;
                            this._flags[0x4000000] = true;
                        }
                        else
                        {
                            ConfigurationErrorsException ce = new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_namespace_invalid", new object[] { xmlUtil.Reader.Value, "http://schemas.microsoft.com/.NetConfiguration/v2.0" }), xmlUtil);
                            xmlUtil.SchemaErrors.AddError(ce, ExceptionAction.Global);
                        }
                    }
                    else
                    {
                        xmlUtil.AddErrorUnrecognizedAttribute(ExceptionAction.NonSpecific);
                    }
                }
                xmlUtil.StrictReadToNextElement(ExceptionAction.NonSpecific);
                if ((xmlUtil.Reader.Depth == 1) && (xmlUtil.Reader.Name == "configSections"))
                {
                    xmlUtil.VerifyNoUnrecognizedAttributes(ExceptionAction.NonSpecific);
                    this.ScanFactoriesRecursive(xmlUtil, string.Empty, factoryList);
                }
                return factoryList;
            }
            string str = ConfigurationErrorsException.AlwaysSafeFilename(xmlUtil.Filename);
            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_file_doesnt_have_root_configuration", new object[] { str }), xmlUtil);
        }

        private void ScanFactoriesRecursive(XmlUtil xmlUtil, string parentConfigKey, Hashtable factoryList)
        {
            xmlUtil.SchemaErrors.ResetLocalErrors();
            int depth = xmlUtil.Reader.Depth;
            xmlUtil.StrictReadToNextElement(ExceptionAction.NonSpecific);
            while (xmlUtil.Reader.Depth == (depth + 1))
            {
                string str4;
                string str7;
                bool flag = false;
                string name = xmlUtil.Reader.Name;
                if (name == null)
                {
                    goto Label_0647;
                }
                if (!(name == "sectionGroup"))
                {
                    if (name == "section")
                    {
                        goto Label_0235;
                    }
                    if (name == "remove")
                    {
                        goto Label_05CD;
                    }
                    if (name == "clear")
                    {
                        goto Label_063E;
                    }
                    goto Label_0647;
                }
                string str = null;
                string newValue = null;
                int lineNumber = xmlUtil.Reader.LineNumber;
                while (xmlUtil.Reader.MoveToNextAttribute())
                {
                    string str9 = xmlUtil.Reader.Name;
                    if (str9 == null)
                    {
                        goto Label_00E1;
                    }
                    if (!(str9 == "name"))
                    {
                        if (str9 == "type")
                        {
                            goto Label_00D6;
                        }
                        goto Label_00E1;
                    }
                    str = xmlUtil.Reader.Value;
                    VerifySectionName(str, xmlUtil, ExceptionAction.Local, false);
                    continue;
                Label_00D6:
                    xmlUtil.VerifyAndGetNonEmptyStringAttribute(ExceptionAction.Local, out newValue);
                    continue;
                Label_00E1:
                    xmlUtil.AddErrorUnrecognizedAttribute(ExceptionAction.Local);
                }
                xmlUtil.Reader.MoveToElement();
                if (!xmlUtil.VerifyRequiredAttribute(str, "name", ExceptionAction.NonSpecific))
                {
                    xmlUtil.SchemaErrors.RetrieveAndResetLocalErrors(true);
                    xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                }
                else
                {
                    string configKey = CombineConfigKey(parentConfigKey, str);
                    FactoryRecord record = (FactoryRecord) factoryList[configKey];
                    if (record != null)
                    {
                        xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined_at_this_level", new object[] { str }), xmlUtil), ExceptionAction.Local);
                    }
                    else
                    {
                        FactoryRecord record2 = this._parent.FindFactoryRecord(configKey, true);
                        if (record2 != null)
                        {
                            configKey = record2.ConfigKey;
                            if ((record2 != null) && (!record2.IsGroup || !record2.IsEquivalentSectionGroupFactory(this.Host, newValue)))
                            {
                                xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { str }), xmlUtil), ExceptionAction.Local);
                                record2 = null;
                            }
                        }
                        if (record2 != null)
                        {
                            record = record2.CloneSectionGroup(newValue, xmlUtil.Filename, lineNumber);
                        }
                        else
                        {
                            record = new FactoryRecord(configKey, parentConfigKey, str, newValue, xmlUtil.Filename, lineNumber);
                        }
                        factoryList[configKey] = record;
                    }
                    record.AddErrors(xmlUtil.SchemaErrors.RetrieveAndResetLocalErrors(true));
                    this.ScanFactoriesRecursive(xmlUtil, configKey, factoryList);
                }
                continue;
            Label_0235:
                str4 = null;
                string str5 = null;
                ConfigurationAllowDefinition everywhere = ConfigurationAllowDefinition.Everywhere;
                ConfigurationAllowExeDefinition machineToApplication = ConfigurationAllowExeDefinition.MachineToApplication;
                OverrideModeSetting sectionDefault = OverrideModeSetting.SectionDefault;
                bool flag2 = true;
                bool flag3 = true;
                bool flag4 = true;
                bool flag5 = false;
                int num3 = xmlUtil.Reader.LineNumber;
                while (xmlUtil.Reader.MoveToNextAttribute())
                {
                    switch (xmlUtil.Reader.Name)
                    {
                        case "name":
                        {
                            str4 = xmlUtil.Reader.Value;
                            VerifySectionName(str4, xmlUtil, ExceptionAction.Local, false);
                            continue;
                        }
                        case "type":
                        {
                            xmlUtil.VerifyAndGetNonEmptyStringAttribute(ExceptionAction.Local, out str5);
                            flag5 = true;
                            continue;
                        }
                        case "allowLocation":
                        {
                            xmlUtil.VerifyAndGetBooleanAttribute(ExceptionAction.Local, true, out flag2);
                            continue;
                        }
                        case "allowExeDefinition":
                        {
                            try
                            {
                                machineToApplication = AllowExeDefinitionToEnum(xmlUtil.Reader.Value, xmlUtil);
                            }
                            catch (ConfigurationException exception)
                            {
                                xmlUtil.SchemaErrors.AddError(exception, ExceptionAction.Local);
                            }
                            continue;
                        }
                        case "allowDefinition":
                        {
                            try
                            {
                                everywhere = AllowDefinitionToEnum(xmlUtil.Reader.Value, xmlUtil);
                            }
                            catch (ConfigurationException exception2)
                            {
                                xmlUtil.SchemaErrors.AddError(exception2, ExceptionAction.Local);
                            }
                            continue;
                        }
                        case "restartOnExternalChanges":
                        {
                            xmlUtil.VerifyAndGetBooleanAttribute(ExceptionAction.Local, true, out flag3);
                            continue;
                        }
                        case "requirePermission":
                        {
                            xmlUtil.VerifyAndGetBooleanAttribute(ExceptionAction.Local, true, out flag4);
                            continue;
                        }
                        case "overrideModeDefault":
                        {
                            try
                            {
                                sectionDefault = OverrideModeSetting.CreateFromXmlReadValue(OverrideModeSetting.ParseOverrideModeXmlValue(xmlUtil.Reader.Value, xmlUtil));
                                if (sectionDefault.OverrideMode == OverrideMode.Inherit)
                                {
                                    sectionDefault.ChangeModeInternal(OverrideMode.Allow);
                                }
                            }
                            catch (ConfigurationException exception3)
                            {
                                xmlUtil.SchemaErrors.AddError(exception3, ExceptionAction.Local);
                            }
                            continue;
                        }
                    }
                    xmlUtil.AddErrorUnrecognizedAttribute(ExceptionAction.Local);
                }
                xmlUtil.Reader.MoveToElement();
                if (!xmlUtil.VerifyRequiredAttribute(str4, "name", ExceptionAction.NonSpecific))
                {
                    xmlUtil.SchemaErrors.RetrieveAndResetLocalErrors(true);
                }
                else
                {
                    if (!flag5)
                    {
                        xmlUtil.AddErrorRequiredAttribute("type", ExceptionAction.Local);
                    }
                    string str6 = CombineConfigKey(parentConfigKey, str4);
                    FactoryRecord record3 = (FactoryRecord) factoryList[str6];
                    if (record3 != null)
                    {
                        xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined_at_this_level", new object[] { str4 }), xmlUtil), ExceptionAction.Local);
                    }
                    else
                    {
                        FactoryRecord record4 = this._parent.FindFactoryRecord(str6, true);
                        if (record4 != null)
                        {
                            str6 = record4.ConfigKey;
                            if (record4.IsGroup)
                            {
                                xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { str4 }), xmlUtil), ExceptionAction.Local);
                                record4 = null;
                            }
                            else if (!record4.IsEquivalentSectionFactory(this.Host, str5, flag2, everywhere, machineToApplication, flag3, flag4))
                            {
                                xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_already_defined", new object[] { str4 }), xmlUtil), ExceptionAction.Local);
                                record4 = null;
                            }
                        }
                        if (record4 != null)
                        {
                            record3 = record4.CloneSection(xmlUtil.Filename, num3);
                        }
                        else
                        {
                            record3 = new FactoryRecord(str6, parentConfigKey, str4, str5, flag2, everywhere, machineToApplication, sectionDefault, flag3, flag4, this._flags[0x2000], false, xmlUtil.Filename, num3);
                        }
                        factoryList[str6] = record3;
                    }
                    record3.AddErrors(xmlUtil.SchemaErrors.RetrieveAndResetLocalErrors(true));
                }
                goto Label_0657;
            Label_05CD:
                str7 = null;
                while (xmlUtil.Reader.MoveToNextAttribute())
                {
                    if (xmlUtil.Reader.Name != "name")
                    {
                        xmlUtil.AddErrorUnrecognizedAttribute(ExceptionAction.NonSpecific);
                    }
                    str7 = xmlUtil.Reader.Value;
                    int num1 = xmlUtil.Reader.LineNumber;
                }
                xmlUtil.Reader.MoveToElement();
                if (xmlUtil.VerifyRequiredAttribute(str7, "name", ExceptionAction.NonSpecific))
                {
                    VerifySectionName(str7, xmlUtil, ExceptionAction.NonSpecific, false);
                }
                goto Label_0657;
            Label_063E:
                xmlUtil.VerifyNoUnrecognizedAttributes(ExceptionAction.NonSpecific);
                goto Label_0657;
            Label_0647:
                xmlUtil.AddErrorUnrecognizedElement(ExceptionAction.NonSpecific);
                xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                flag = true;
            Label_0657:
                if (!flag)
                {
                    xmlUtil.StrictReadToNextElement(ExceptionAction.NonSpecific);
                    if (xmlUtil.Reader.Depth <= (depth + 1))
                    {
                        continue;
                    }
                    xmlUtil.AddErrorUnrecognizedElement(ExceptionAction.NonSpecific);
                    while (xmlUtil.Reader.Depth > (depth + 1))
                    {
                        xmlUtil.ReadToNextElement();
                    }
                }
            }
        }

        private void ScanLocationSection(XmlUtil xmlUtil)
        {
            string subPath = null;
            bool newValue = true;
            int globalErrorCount = xmlUtil.SchemaErrors.GlobalErrorCount;
            OverrideModeSetting locationDefault = OverrideModeSetting.LocationDefault;
            bool flag2 = false;
            while (xmlUtil.Reader.MoveToNextAttribute())
            {
                string name = xmlUtil.Reader.Name;
                if (name == null)
                {
                    goto Label_0108;
                }
                if (!(name == "path"))
                {
                    if (name == "allowOverride")
                    {
                        goto Label_0082;
                    }
                    if (name == "overrideMode")
                    {
                        goto Label_00BE;
                    }
                    if (name == "inheritInChildApplications")
                    {
                        goto Label_00FC;
                    }
                    goto Label_0108;
                }
                subPath = xmlUtil.Reader.Value;
                continue;
            Label_0082:
                if (flag2)
                {
                    xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Invalid_override_mode_declaration"), xmlUtil), ExceptionAction.Global);
                }
                else
                {
                    bool flag3 = true;
                    xmlUtil.VerifyAndGetBooleanAttribute(ExceptionAction.Global, true, out flag3);
                    locationDefault = OverrideModeSetting.CreateFromXmlReadValue(flag3);
                    flag2 = true;
                }
                continue;
            Label_00BE:
                if (flag2)
                {
                    xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Invalid_override_mode_declaration"), xmlUtil), ExceptionAction.Global);
                }
                else
                {
                    locationDefault = OverrideModeSetting.CreateFromXmlReadValue(OverrideModeSetting.ParseOverrideModeXmlValue(xmlUtil.Reader.Value, xmlUtil));
                    flag2 = true;
                }
                continue;
            Label_00FC:
                xmlUtil.VerifyAndGetBooleanAttribute(ExceptionAction.Global, true, out newValue);
                continue;
            Label_0108:
                xmlUtil.AddErrorUnrecognizedAttribute(ExceptionAction.Global);
            }
            xmlUtil.Reader.MoveToElement();
            try
            {
                subPath = NormalizeLocationSubPath(subPath, xmlUtil);
                if (((subPath == null) && !newValue) && this.Host.IsDefinitionAllowed(this._configPath, ConfigurationAllowDefinition.MachineToWebRoot, ConfigurationAllowExeDefinition.MachineOnly))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Location_invalid_inheritInChildApplications_in_machine_or_root_web_config"), xmlUtil);
                }
            }
            catch (ConfigurationErrorsException exception)
            {
                xmlUtil.SchemaErrors.AddError(exception, ExceptionAction.Global);
            }
            if (xmlUtil.SchemaErrors.GlobalErrorCount > globalErrorCount)
            {
                xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
            }
            else if (subPath == null)
            {
                this.ScanSectionsRecursive(xmlUtil, string.Empty, true, null, locationDefault, !newValue);
            }
            else if (!this._flags[0x100000])
            {
                xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
            }
            else
            {
                IInternalConfigHost host = this.Host;
                if (((this is RuntimeConfigurationRecord) && (host != null)) && ((subPath.Length != 0) && (subPath[0] != '.')))
                {
                    if (s_appConfigPath == null)
                    {
                        object configContext = this.ConfigContext;
                        if (configContext != null)
                        {
                            string str2 = configContext.ToString();
                            Interlocked.CompareExchange<string>(ref s_appConfigPath, str2, null);
                        }
                    }
                    string configPathFromLocationSubPath = host.GetConfigPathFromLocationSubPath(this._configPath, subPath);
                    if (!StringUtil.StartsWithIgnoreCase(s_appConfigPath, configPathFromLocationSubPath) && !StringUtil.StartsWithIgnoreCase(configPathFromLocationSubPath, s_appConfigPath))
                    {
                        xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                        return;
                    }
                }
                this.AddLocation(subPath);
                this.ScanSectionsRecursive(xmlUtil, string.Empty, true, subPath, locationDefault, !newValue);
            }
        }

        private void ScanSections(XmlUtil xmlUtil)
        {
            this.ScanSectionsRecursive(xmlUtil, string.Empty, false, null, OverrideModeSetting.LocationDefault, false);
        }

        private void ScanSectionsRecursive(XmlUtil xmlUtil, string parentConfigKey, bool inLocation, string locationSubPath, OverrideModeSetting overrideMode, bool skipInChildApps)
        {
            int depth;
            xmlUtil.SchemaErrors.ResetLocalErrors();
            if ((parentConfigKey.Length == 0) && !inLocation)
            {
                depth = 0;
            }
            else
            {
                depth = xmlUtil.Reader.Depth;
                xmlUtil.StrictReadToNextElement(ExceptionAction.NonSpecific);
            }
            while (xmlUtil.Reader.Depth == (depth + 1))
            {
                string configKey;
                FactoryRecord record;
                string filename;
                int lineNumber;
                string str4;
                string str5;
                string streamNameForConfigSource;
                object obj2;
                string str7;
                OverrideMode inherit;
                OverrideMode mode2;
                bool flag;
                bool flag2;
                List<ConfigurationException> list;
                string name = xmlUtil.Reader.Name;
                switch (name)
                {
                    case "configSections":
                    {
                        xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_client_config_too_many_configsections_elements", new object[] { name }), xmlUtil), ExceptionAction.NonSpecific);
                        xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                        continue;
                    }
                    case "location":
                    {
                        if ((parentConfigKey.Length > 0) || inLocation)
                        {
                            xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_location_location_not_allowed"), xmlUtil), ExceptionAction.Global);
                            xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                        }
                        else
                        {
                            this.ScanLocationSection(xmlUtil);
                        }
                        continue;
                    }
                    default:
                        configKey = CombineConfigKey(parentConfigKey, name);
                        record = this.FindFactoryRecord(configKey, true);
                        if (record == null)
                        {
                            if (!this.ClassFlags[0x40])
                            {
                                xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_unrecognized_configuration_section", new object[] { configKey }), xmlUtil), ExceptionAction.Local);
                            }
                            VerifySectionName(name, xmlUtil, ExceptionAction.Local, false);
                            record = new FactoryRecord(configKey, parentConfigKey, name, typeof(DefaultSection).AssemblyQualifiedName, true, ConfigurationAllowDefinition.Everywhere, ConfigurationAllowExeDefinition.MachineToRoamingUser, OverrideModeSetting.SectionDefault, true, true, this._flags[0x2000], true, null, -1);
                            record.AddErrors(xmlUtil.SchemaErrors.RetrieveAndResetLocalErrors(true));
                            this.EnsureFactories()[configKey] = record;
                        }
                        if (record.IsGroup)
                        {
                            if (record.HasErrors)
                            {
                                xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                            }
                            else
                            {
                                if (xmlUtil.Reader.AttributeCount > 0)
                                {
                                    while (xmlUtil.Reader.MoveToNextAttribute())
                                    {
                                        if (IsReservedAttributeName(xmlUtil.Reader.Name))
                                        {
                                            xmlUtil.AddErrorReservedAttribute(ExceptionAction.NonSpecific);
                                        }
                                    }
                                    xmlUtil.Reader.MoveToElement();
                                }
                                this.ScanSectionsRecursive(xmlUtil, configKey, inLocation, locationSubPath, overrideMode, skipInChildApps);
                            }
                            continue;
                        }
                        configKey = record.ConfigKey;
                        filename = xmlUtil.Filename;
                        lineNumber = xmlUtil.LineNumber;
                        str4 = null;
                        str5 = null;
                        streamNameForConfigSource = null;
                        obj2 = null;
                        str7 = null;
                        inherit = OverrideMode.Inherit;
                        mode2 = OverrideMode.Inherit;
                        flag = false;
                        flag2 = locationSubPath == null;
                        if (record.HasErrors)
                        {
                            goto Label_04D1;
                        }
                        if (inLocation && !record.AllowLocation)
                        {
                            xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_section_cannot_be_used_in_location"), xmlUtil), ExceptionAction.Local);
                        }
                        if (flag2)
                        {
                            SectionRecord sectionRecord = this.GetSectionRecord(configKey, true);
                            if (((sectionRecord != null) && sectionRecord.HasFileInput) && !record.IsIgnorable())
                            {
                                xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_sections_must_be_unique"), xmlUtil), ExceptionAction.Local);
                            }
                            try
                            {
                                this.VerifyDefinitionAllowed(record, this._configPath, xmlUtil);
                            }
                            catch (ConfigurationException exception)
                            {
                                xmlUtil.SchemaErrors.AddError(exception, ExceptionAction.Local);
                            }
                        }
                        inherit = this.GetSectionLockedMode(configKey, out mode2);
                        if (inherit == OverrideMode.Deny)
                        {
                            xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_section_locked"), xmlUtil), ExceptionAction.Local);
                        }
                        if (xmlUtil.Reader.AttributeCount >= 1)
                        {
                            string attribute = xmlUtil.Reader.GetAttribute("configSource");
                            if (attribute != null)
                            {
                                try
                                {
                                    str5 = NormalizeConfigSource(attribute, xmlUtil);
                                }
                                catch (ConfigurationException exception2)
                                {
                                    xmlUtil.SchemaErrors.AddError(exception2, ExceptionAction.Local);
                                }
                                if (xmlUtil.Reader.AttributeCount != 1)
                                {
                                    xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_syntax_error"), xmlUtil), ExceptionAction.Local);
                                }
                            }
                            string protectionProvider = xmlUtil.Reader.GetAttribute("configProtectionProvider");
                            if (protectionProvider != null)
                            {
                                try
                                {
                                    str7 = ValidateProtectionProviderAttribute(protectionProvider, xmlUtil);
                                }
                                catch (ConfigurationException exception3)
                                {
                                    xmlUtil.SchemaErrors.AddError(exception3, ExceptionAction.Local);
                                }
                                if (xmlUtil.Reader.AttributeCount != 1)
                                {
                                    xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Protection_provider_syntax_error"), xmlUtil), ExceptionAction.Local);
                                }
                            }
                            if ((attribute != null) && !xmlUtil.Reader.IsEmptyElement)
                            {
                                while (xmlUtil.Reader.Read())
                                {
                                    XmlNodeType nodeType = xmlUtil.Reader.NodeType;
                                    if (nodeType == XmlNodeType.EndElement)
                                    {
                                        break;
                                    }
                                    if (nodeType != XmlNodeType.Comment)
                                    {
                                        xmlUtil.SchemaErrors.AddError(new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_syntax_error"), xmlUtil), ExceptionAction.Local);
                                        if (nodeType == XmlNodeType.Element)
                                        {
                                            xmlUtil.StrictSkipToOurParentsEndElement(ExceptionAction.NonSpecific);
                                        }
                                        else
                                        {
                                            xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                                        }
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }
                if (str5 != null)
                {
                    try
                    {
                        try
                        {
                            streamNameForConfigSource = this.Host.GetStreamNameForConfigSource(this.ConfigStreamInfo.StreamName, str5);
                        }
                        catch (Exception exception4)
                        {
                            throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_source_invalid"), exception4, xmlUtil);
                        }
                        this.ValidateUniqueConfigSource(configKey, streamNameForConfigSource, str5, xmlUtil);
                        obj2 = this.MonitorStream(configKey, str5, streamNameForConfigSource);
                    }
                    catch (ConfigurationException exception5)
                    {
                        xmlUtil.SchemaErrors.AddError(exception5, ExceptionAction.Local);
                    }
                }
                if ((!xmlUtil.SchemaErrors.HasLocalErrors && (str5 == null)) && this.ShouldPrefetchRawXml(record))
                {
                    str4 = xmlUtil.CopySection();
                    if (xmlUtil.Reader.NodeType != XmlNodeType.Element)
                    {
                        xmlUtil.VerifyIgnorableNodeType(ExceptionAction.NonSpecific);
                        xmlUtil.StrictReadToNextElement(ExceptionAction.NonSpecific);
                    }
                    flag = true;
                }
            Label_04D1:
                list = xmlUtil.SchemaErrors.RetrieveAndResetLocalErrors(flag2);
                if (!flag)
                {
                    xmlUtil.StrictSkipToNextElement(ExceptionAction.NonSpecific);
                }
                bool flag3 = true;
                if (flag2)
                {
                    if (this.ShouldSkipDueToInheritInChildApplications(skipInChildApps))
                    {
                        flag3 = false;
                    }
                }
                else if (!this._flags[0x100000])
                {
                    flag3 = false;
                }
                if (flag3)
                {
                    string targetConfigPath = (locationSubPath == null) ? this._configPath : null;
                    SectionXmlInfo sectionXmlInfo = new SectionXmlInfo(configKey, this._configPath, targetConfigPath, locationSubPath, filename, lineNumber, this.ConfigStreamInfo.StreamVersion, str4, str5, streamNameForConfigSource, obj2, str7, overrideMode, skipInChildApps);
                    if (locationSubPath == null)
                    {
                        SectionRecord record3 = this.EnsureSectionRecordUnsafe(configKey, true);
                        record3.ChangeLockSettings(inherit, mode2);
                        SectionInput sectionInput = new SectionInput(sectionXmlInfo, list);
                        record3.AddFileInput(sectionInput);
                    }
                    else
                    {
                        LocationSectionRecord record4 = new LocationSectionRecord(sectionXmlInfo, list);
                        this.EnsureLocationSections().Add(record4);
                    }
                }
            }
        }

        private bool ShouldPrefetchRawXml(FactoryRecord factoryRecord)
        {
            string str;
            if (!this._flags[8] && (((str = factoryRecord.ConfigKey) == null) || ((!(str == "configProtectedData") && !(str == "system.diagnostics")) && (!(str == "appSettings") && !(str == "connectionStrings")))))
            {
                return this.Host.PrefetchSection(factoryRecord.Group, factoryRecord.Name);
            }
            return true;
        }

        private bool ShouldSkipDueToInheritInChildApplications(bool skipInChildApps)
        {
            return (skipInChildApps && this._flags[0x20]);
        }

        private bool ShouldSkipDueToInheritInChildApplications(bool skipInChildApps, string configPath)
        {
            return (skipInChildApps && this.Host.IsAboveApplication(configPath));
        }

        internal static void SplitConfigKey(string configKey, out string group, out string name)
        {
            int length = configKey.LastIndexOf('/');
            if (length == -1)
            {
                group = string.Empty;
                name = configKey;
            }
            else
            {
                group = configKey.Substring(0, length);
                name = configKey.Substring(length + 1);
            }
        }

        public void ThrowIfInitErrors()
        {
            this.ThrowIfParseErrors(this._initErrors);
        }

        private void ThrowIfParseErrors(ConfigurationSchemaErrors schemaErrors)
        {
            schemaErrors.ThrowIfErrors(this.ClassFlags[0x40]);
        }

        protected abstract object UseParentResult(string configKey, object parentResult, SectionRecord sectionRecord);
        internal static string ValidateProtectionProviderAttribute(string protectionProvider, IConfigErrorInfo errorInfo)
        {
            if (string.IsNullOrEmpty(protectionProvider))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Protection_provider_invalid_format"), errorInfo);
            }
            return protectionProvider;
        }

        protected void ValidateUniqueChildConfigSource(string configKey, string configSourceStreamName, string configSourceArg, IConfigErrorInfo errorInfo)
        {
            BaseConfigurationRecord parent;
            if (this.IsLocationConfig)
            {
                parent = this._parent._parent;
            }
            else
            {
                parent = this._parent;
            }
            while (!parent.IsRootConfig)
            {
                lock (parent)
                {
                    if (parent.ConfigStreamInfo.HasStreamInfos && (((StreamInfo) parent.ConfigStreamInfo.StreamInfos[configSourceStreamName]) != null))
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_parent_conflict", new object[] { configSourceArg }), errorInfo);
                    }
                }
                parent = parent.Parent;
            }
        }

        private void ValidateUniqueConfigSource(string configKey, string configSourceStreamName, string configSourceArg, IConfigErrorInfo errorInfo)
        {
            lock (this)
            {
                if (this.ConfigStreamInfo.HasStreamInfos)
                {
                    StreamInfo info = (StreamInfo) this.ConfigStreamInfo.StreamInfos[configSourceStreamName];
                    if ((info != null) && (info.SectionName != configKey))
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_cannot_be_shared", new object[] { configSourceArg }), errorInfo);
                    }
                }
            }
            this.ValidateUniqueChildConfigSource(configKey, configSourceStreamName, configSourceArg, errorInfo);
        }

        private void VerifyDefinitionAllowed(FactoryRecord factoryRecord, string configPath, IConfigErrorInfo errorInfo)
        {
            this.Host.VerifyDefinitionAllowed(configPath, factoryRecord.AllowDefinition, factoryRecord.AllowExeDefinition, errorInfo);
        }

        protected static void VerifySectionName(string name, IConfigErrorInfo errorInfo, bool allowImplicit)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_invalid"), errorInfo);
            }
            try
            {
                XmlConvert.VerifyName(name);
            }
            catch (Exception exception)
            {
                throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_tag_name_invalid"), exception, errorInfo);
            }
            if (IsImplicitSection(name))
            {
                if (!allowImplicit)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Cannot_declare_or_remove_implicit_section", new object[] { name }), errorInfo);
                }
            }
            else
            {
                if (StringUtil.StartsWith(name, "config"))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_cannot_begin_with_config"), errorInfo);
                }
                if (name == "location")
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_tag_name_cannot_be_location"), errorInfo);
                }
            }
        }

        protected static void VerifySectionName(string name, XmlUtil xmlUtil, ExceptionAction action, bool allowImplicit)
        {
            try
            {
                VerifySectionName(name, xmlUtil, allowImplicit);
            }
            catch (ConfigurationErrorsException exception)
            {
                xmlUtil.SchemaErrors.AddError(exception, action);
            }
        }

        internal Func<string, string> AssemblyStringTransformer
        {
            get
            {
                if (this.CurrentConfiguration != null)
                {
                    return this.CurrentConfiguration.AssemblyStringTransformer;
                }
                return null;
            }
        }

        internal bool AssemblyStringTransformerIsSet
        {
            get
            {
                return ((this.CurrentConfiguration != null) && this.CurrentConfiguration.AssemblyStringTransformerIsSet);
            }
        }

        protected abstract SimpleBitVector32 ClassFlags { get; }

        internal object ConfigContext
        {
            get
            {
                if (!this._flags[0x80])
                {
                    this._configContext = this.Host.CreateConfigurationContext(this.ConfigPath, this.LocationSubPath);
                    this._flags[0x80] = true;
                }
                return this._configContext;
            }
        }

        public string ConfigPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._configPath;
            }
        }

        protected ConfigRecordStreamInfo ConfigStreamInfo
        {
            get
            {
                if (this.IsLocationConfig)
                {
                    return this._parent._configStreamInfo;
                }
                return this._configStreamInfo;
            }
        }

        internal System.Configuration.Configuration CurrentConfiguration
        {
            get
            {
                return this._configRoot.CurrentConfiguration;
            }
        }

        internal string DefaultProviderName
        {
            get
            {
                return this.ProtectedConfig.DefaultProvider;
            }
        }

        private bool HasFactoryRecords
        {
            get
            {
                return (this._factoryRecords != null);
            }
        }

        public bool HasInitErrors
        {
            get
            {
                return this._initErrors.HasErrors(this.ClassFlags[0x40]);
            }
        }

        internal bool HasStream
        {
            get
            {
                return this.ConfigStreamInfo.HasStream;
            }
        }

        internal IInternalConfigHost Host
        {
            get
            {
                return this._configRoot.Host;
            }
        }

        internal bool IsEmpty
        {
            get
            {
                if ((((this._parent != null) && !this._initErrors.HasErrors(false)) && ((this._sectionRecords == null) || (this._sectionRecords.Count == 0))) && ((this._factoryRecords == null) || (this._factoryRecords.Count == 0)))
                {
                    if (this._locationSections != null)
                    {
                        return (this._locationSections.Count == 0);
                    }
                    return true;
                }
                return false;
            }
        }

        private bool IsInitDelayed
        {
            get
            {
                return (this._initDelayedRoot != null);
            }
        }

        internal bool IsLocationConfig
        {
            get
            {
                return (this._locationSubPath != null);
            }
        }

        internal bool IsMachineConfig
        {
            get
            {
                return (this._parent == this._configRoot.RootConfigRecord);
            }
        }

        internal bool IsRootConfig
        {
            get
            {
                return (this._parent == null);
            }
        }

        internal string LocationSubPath
        {
            get
            {
                return this._locationSubPath;
            }
        }

        internal BaseConfigurationRecord Parent
        {
            get
            {
                return this._parent;
            }
        }

        private ProtectedConfigurationSection ProtectedConfig
        {
            get
            {
                if (!this._flags[1])
                {
                    this.InitProtectedConfigurationSection();
                }
                return this._protectedConfig;
            }
        }

        internal bool RecordSupportsLocation
        {
            get
            {
                if (!this._flags[0x100000])
                {
                    return this.IsMachineConfig;
                }
                return true;
            }
        }

        internal Stack SectionsStack
        {
            get
            {
                if (this.CurrentConfiguration != null)
                {
                    return this.CurrentConfiguration.SectionsStack;
                }
                return new Stack();
            }
        }

        public string StreamName
        {
            get
            {
                return this.ConfigStreamInfo.StreamName;
            }
        }

        internal FrameworkName TargetFramework
        {
            get
            {
                if (this.CurrentConfiguration != null)
                {
                    return this.CurrentConfiguration.TargetFramework;
                }
                return null;
            }
        }

        internal Func<string, string> TypeStringTransformer
        {
            get
            {
                if (this.CurrentConfiguration != null)
                {
                    return this.CurrentConfiguration.TypeStringTransformer;
                }
                return null;
            }
        }

        internal bool TypeStringTransformerIsSet
        {
            get
            {
                return ((this.CurrentConfiguration != null) && this.CurrentConfiguration.TypeStringTransformerIsSet);
            }
        }

        private static ConfigurationPermission UnrestrictedConfigPermission
        {
            get
            {
                if (s_unrestrictedConfigPermission == null)
                {
                    s_unrestrictedConfigPermission = new ConfigurationPermission(PermissionState.Unrestricted);
                }
                return s_unrestrictedConfigPermission;
            }
        }

        protected class ConfigRecordStreamInfo
        {
            private StreamChangeCallback _callbackDelegate;
            private Encoding _encoding = Encoding.UTF8;
            private bool _hasStream;
            private HybridDictionary _streamInfos;
            private string _streamname;
            private object _streamVersion;

            internal ConfigRecordStreamInfo()
            {
            }

            internal void ClearStreamInfos()
            {
                this._streamInfos = null;
            }

            internal StreamChangeCallback CallbackDelegate
            {
                get
                {
                    return this._callbackDelegate;
                }
                set
                {
                    this._callbackDelegate = value;
                }
            }

            internal bool HasStream
            {
                get
                {
                    return this._hasStream;
                }
                set
                {
                    this._hasStream = value;
                }
            }

            internal bool HasStreamInfos
            {
                get
                {
                    return (this._streamInfos != null);
                }
            }

            internal Encoding StreamEncoding
            {
                get
                {
                    return this._encoding;
                }
                set
                {
                    this._encoding = value;
                }
            }

            internal HybridDictionary StreamInfos
            {
                get
                {
                    if (this._streamInfos == null)
                    {
                        this._streamInfos = new HybridDictionary(true);
                    }
                    return this._streamInfos;
                }
            }

            internal string StreamName
            {
                get
                {
                    return this._streamname;
                }
                set
                {
                    this._streamname = value;
                }
            }

            internal object StreamVersion
            {
                get
                {
                    return this._streamVersion;
                }
                set
                {
                    this._streamVersion = value;
                }
            }
        }

        private class IndirectLocationInputComparer : IComparer<SectionInput>
        {
            public int Compare(SectionInput x, SectionInput y)
            {
                if (!object.ReferenceEquals(x, y))
                {
                    string targetConfigPath = x.SectionXmlInfo.TargetConfigPath;
                    string subpath = y.SectionXmlInfo.TargetConfigPath;
                    if (UrlPath.IsSubpath(targetConfigPath, subpath))
                    {
                        return 1;
                    }
                    if (UrlPath.IsSubpath(subpath, targetConfigPath))
                    {
                        return -1;
                    }
                    string definitionConfigPath = x.SectionXmlInfo.DefinitionConfigPath;
                    string str4 = y.SectionXmlInfo.DefinitionConfigPath;
                    if (UrlPath.IsSubpath(definitionConfigPath, str4))
                    {
                        return 1;
                    }
                    if (UrlPath.IsSubpath(str4, definitionConfigPath))
                    {
                        return -1;
                    }
                }
                return 0;
            }
        }
    }
}

