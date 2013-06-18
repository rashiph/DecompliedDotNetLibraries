namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration.Internal;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;

    internal sealed class MgmtConfigurationRecord : BaseConfigurationRecord
    {
        private Hashtable _locationTags;
        private Hashtable _removedSectionGroups;
        private Hashtable _removedSections;
        private Hashtable _sectionFactories;
        private Hashtable _sectionGroupFactories;
        private Hashtable _sectionGroups;
        private HybridDictionary _streamInfoUpdates;
        private const int DEFAULT_INDENT = 4;
        private const int MAX_INDENT = 10;
        private static readonly SimpleBitVector32 MgmtClassFlags = new SimpleBitVector32(80);

        private MgmtConfigurationRecord()
        {
        }

        internal void AddConfigurationSection(string group, string name, ConfigurationSection configSection)
        {
            if (base.IsLocationConfig)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_add_configurationsection_in_location_config"));
            }
            BaseConfigurationRecord.VerifySectionName(name, null, false);
            if (configSection == null)
            {
                throw new ArgumentNullException("configSection");
            }
            if (configSection.SectionInformation.Attached)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_add_configurationsection_already_added"));
            }
            string configKey = BaseConfigurationRecord.CombineConfigKey(group, name);
            if (base.FindFactoryRecord(configKey, true) != null)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Config_add_configurationsection_already_exists"));
            }
            if (!string.IsNullOrEmpty(configSection.SectionInformation.ConfigSource))
            {
                this.ChangeConfigSource(configSection.SectionInformation, null, null, configSection.SectionInformation.ConfigSource);
            }
            if (this._sectionFactories != null)
            {
                this._sectionFactories.Add(configKey, new FactoryId(configKey, group, name));
            }
            string type = configSection.SectionInformation.Type;
            if (type == null)
            {
                type = base.Host.GetConfigTypeName(configSection.GetType());
            }
            FactoryRecord factoryRecord = new FactoryRecord(configKey, group, name, type, configSection.SectionInformation.AllowLocation, configSection.SectionInformation.AllowDefinition, configSection.SectionInformation.AllowExeDefinition, configSection.SectionInformation.OverrideModeDefaultSetting, configSection.SectionInformation.RestartOnExternalChanges, configSection.SectionInformation.RequirePermission, this._flags[0x2000], false, base.ConfigStreamInfo.StreamName, -1) {
                Factory = System.Configuration.TypeUtil.GetConstructorWithReflectionPermission(configSection.GetType(), typeof(ConfigurationSection), true),
                IsFactoryTrustedWithoutAptca = System.Configuration.TypeUtil.IsTypeFromTrustedAssemblyWithoutAptca(configSection.GetType())
            };
            base.EnsureFactories()[configKey] = factoryRecord;
            SectionRecord sectionRecord = base.EnsureSectionRecordUnsafe(configKey, false);
            sectionRecord.Result = configSection;
            sectionRecord.ResultRuntimeObject = configSection;
            if (this._removedSections != null)
            {
                this._removedSections.Remove(configKey);
            }
            configSection.SectionInformation.AttachToConfigurationRecord(this, factoryRecord, sectionRecord);
            string rawXml = configSection.SectionInformation.RawXml;
            if (!string.IsNullOrEmpty(rawXml))
            {
                configSection.SectionInformation.RawXml = null;
                configSection.SectionInformation.SetRawXml(rawXml);
            }
        }

        internal void AddConfigurationSectionGroup(string group, string name, ConfigurationSectionGroup configSectionGroup)
        {
            if (base.IsLocationConfig)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_add_configurationsectiongroup_in_location_config"));
            }
            BaseConfigurationRecord.VerifySectionName(name, null, false);
            if (configSectionGroup == null)
            {
                throw ExceptionUtil.ParameterInvalid("name");
            }
            if (configSectionGroup.Attached)
            {
                throw new InvalidOperationException(System.Configuration.SR.GetString("Config_add_configurationsectiongroup_already_added"));
            }
            string configKey = BaseConfigurationRecord.CombineConfigKey(group, name);
            if (base.FindFactoryRecord(configKey, true) != null)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Config_add_configurationsectiongroup_already_exists"));
            }
            if (this._sectionGroupFactories != null)
            {
                this._sectionGroupFactories.Add(configKey, new FactoryId(configKey, group, name));
            }
            string type = configSectionGroup.Type;
            if (type == null)
            {
                type = base.Host.GetConfigTypeName(configSectionGroup.GetType());
            }
            FactoryRecord factoryRecord = new FactoryRecord(configKey, group, name, type, base.ConfigStreamInfo.StreamName, -1);
            base.EnsureFactories()[configKey] = factoryRecord;
            this.SectionGroups[configKey] = configSectionGroup;
            if (this._removedSectionGroups != null)
            {
                this._removedSectionGroups.Remove(configKey);
            }
            configSectionGroup.AttachToConfigurationRecord(this, factoryRecord);
        }

        protected override void AddLocation(string locationSubPath)
        {
            if (this._locationTags == null)
            {
                this._locationTags = new Hashtable(StringComparer.OrdinalIgnoreCase);
            }
            this._locationTags[locationSubPath] = locationSubPath;
        }

        private void AppendAttribute(StringBuilder sb, string key, string value)
        {
            sb.Append(key);
            sb.Append("=\"");
            sb.Append(value);
            sb.Append("\" ");
        }

        private bool AreDeclarationAttributesModified(FactoryRecord factoryRecord, ConfigurationSection configSection)
        {
            if (((!(factoryRecord.FactoryTypeName != configSection.SectionInformation.Type) && (factoryRecord.AllowLocation == configSection.SectionInformation.AllowLocation)) && ((factoryRecord.RestartOnExternalChanges == configSection.SectionInformation.RestartOnExternalChanges) && (factoryRecord.RequirePermission == configSection.SectionInformation.RequirePermission))) && (((factoryRecord.AllowDefinition == configSection.SectionInformation.AllowDefinition) && (factoryRecord.AllowExeDefinition == configSection.SectionInformation.AllowExeDefinition)) && (factoryRecord.OverrideModeDefault.OverrideMode == configSection.SectionInformation.OverrideModeDefaultSetting.OverrideMode)))
            {
                return configSection.SectionInformation.IsModifiedFlags();
            }
            return true;
        }

        private bool AreLocationAttributesModified(SectionRecord sectionRecord, ConfigurationSection configSection)
        {
            OverrideModeSetting locationDefault = OverrideModeSetting.LocationDefault;
            bool flag = true;
            if (sectionRecord.HasFileInput)
            {
                SectionXmlInfo sectionXmlInfo = sectionRecord.FileInput.SectionXmlInfo;
                locationDefault = sectionXmlInfo.OverrideModeSetting;
                flag = !sectionXmlInfo.SkipInChildApps;
            }
            if (OverrideModeSetting.CanUseSameLocationTag(locationDefault, configSection.SectionInformation.OverrideModeSetting))
            {
                return (flag != configSection.SectionInformation.InheritInChildApplications);
            }
            return true;
        }

        private bool AreSectionAttributesModified(SectionRecord sectionRecord, ConfigurationSection configSection)
        {
            string configSource;
            string protectionProviderName;
            if (sectionRecord.HasFileInput)
            {
                SectionXmlInfo sectionXmlInfo = sectionRecord.FileInput.SectionXmlInfo;
                configSource = sectionXmlInfo.ConfigSource;
                protectionProviderName = sectionXmlInfo.ProtectionProviderName;
            }
            else
            {
                configSource = null;
                protectionProviderName = null;
            }
            if (StringUtil.EqualsNE(configSource, configSection.SectionInformation.ConfigSource) && StringUtil.EqualsNE(protectionProviderName, configSection.SectionInformation.ProtectionProviderName))
            {
                return this.AreLocationAttributesModified(sectionRecord, configSection);
            }
            return true;
        }

        private static string BoolToString(bool v)
        {
            if (!v)
            {
                return "false";
            }
            return "true";
        }

        internal void ChangeConfigSource(SectionInformation sectionInformation, string oldConfigSource, string oldConfigSourceStreamName, string newConfigSource)
        {
            if (string.IsNullOrEmpty(oldConfigSource))
            {
                oldConfigSource = null;
            }
            if (string.IsNullOrEmpty(newConfigSource))
            {
                newConfigSource = null;
            }
            if (!StringUtil.EqualsIgnoreCase(oldConfigSource, newConfigSource))
            {
                if (string.IsNullOrEmpty(base.ConfigStreamInfo.StreamName))
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_requires_file"));
                }
                string configSourceStreamName = null;
                if (newConfigSource != null)
                {
                    configSourceStreamName = base.Host.GetStreamNameForConfigSource(base.ConfigStreamInfo.StreamName, newConfigSource);
                }
                if (configSourceStreamName != null)
                {
                    base.ValidateUniqueChildConfigSource(sectionInformation.ConfigKey, configSourceStreamName, newConfigSource, null);
                    StreamInfo info = (StreamInfo) this._streamInfoUpdates[configSourceStreamName];
                    if (info != null)
                    {
                        if (info.SectionName != sectionInformation.ConfigKey)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_source_cannot_be_shared", new object[] { newConfigSource }));
                        }
                    }
                    else
                    {
                        info = new StreamInfo(sectionInformation.ConfigKey, newConfigSource, configSourceStreamName);
                        this._streamInfoUpdates.Add(configSourceStreamName, info);
                    }
                }
                if ((oldConfigSourceStreamName != null) && !this.IsStreamUsed(oldConfigSourceStreamName))
                {
                    this._streamInfoUpdates.Remove(oldConfigSourceStreamName);
                }
                sectionInformation.ConfigSourceStreamName = configSourceStreamName;
            }
        }

        private void CheckPreamble(byte[] preamble, XmlUtilWriter utilWriter, byte[] buffer)
        {
            bool flag = false;
            using (Stream stream = new MemoryStream(buffer))
            {
                byte[] buffer2 = new byte[preamble.Length];
                if (stream.Read(buffer2, 0, buffer2.Length) == buffer2.Length)
                {
                    flag = true;
                    for (int i = 0; i < buffer2.Length; i++)
                    {
                        if (buffer2[i] != preamble[i])
                        {
                            flag = false;
                            goto Label_004A;
                        }
                    }
                }
            }
        Label_004A:
            if (!flag)
            {
                object o = utilWriter.CreateStreamCheckpoint();
                utilWriter.Write('x');
                utilWriter.RestoreStreamCheckpoint(o);
            }
        }

        private void CopyConfig(SectionUpdates declarationUpdates, ConfigDefinitionUpdates definitionUpdates, byte[] buffer, string filename, NamespaceChange namespaceChange, XmlUtilWriter utilWriter)
        {
            this.CheckPreamble(base.ConfigStreamInfo.StreamEncoding.GetPreamble(), utilWriter, buffer);
            using (Stream stream = new MemoryStream(buffer))
            {
                using (XmlUtil util = new XmlUtil(stream, filename, false))
                {
                    string str;
                    XmlTextReader reader = util.Reader;
                    reader.WhitespaceHandling = WhitespaceHandling.All;
                    reader.Read();
                    util.CopyReaderToNextElement(utilWriter, false);
                    int indent = 4;
                    int trueLinePosition = util.TrueLinePosition;
                    bool isEmptyElement = reader.IsEmptyElement;
                    if (namespaceChange == NamespaceChange.Add)
                    {
                        str = string.Format(CultureInfo.InvariantCulture, "<configuration xmlns=\"{0}\">\r\n", new object[] { "http://schemas.microsoft.com/.NetConfiguration/v2.0" });
                    }
                    else if (namespaceChange == NamespaceChange.Remove)
                    {
                        str = "<configuration>\r\n";
                    }
                    else
                    {
                        str = null;
                    }
                    bool needsChildren = (declarationUpdates != null) || (definitionUpdates != null);
                    string s = util.UpdateStartElement(utilWriter, str, needsChildren, trueLinePosition, indent);
                    bool flag3 = false;
                    if (!isEmptyElement)
                    {
                        util.CopyReaderToNextElement(utilWriter, true);
                        indent = this.UpdateIndent(indent, util, utilWriter, trueLinePosition);
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "configSections"))
                        {
                            flag3 = true;
                            int linePosition = util.TrueLinePosition;
                            bool flag4 = reader.IsEmptyElement;
                            if (declarationUpdates == null)
                            {
                                util.CopyOuterXmlToNextElement(utilWriter, true);
                            }
                            else
                            {
                                string str3 = util.UpdateStartElement(utilWriter, null, true, linePosition, indent);
                                if (!flag4)
                                {
                                    util.CopyReaderToNextElement(utilWriter, true);
                                    this.CopyConfigDeclarationsRecursive(declarationUpdates, util, utilWriter, string.Empty, linePosition, indent);
                                }
                                if (declarationUpdates.HasUnretrievedSections())
                                {
                                    int num4 = 0;
                                    if (str3 == null)
                                    {
                                        num4 = util.TrueLinePosition;
                                    }
                                    if (!utilWriter.IsLastLineBlank)
                                    {
                                        utilWriter.AppendNewLine();
                                    }
                                    this.WriteUnwrittenConfigDeclarations(declarationUpdates, utilWriter, linePosition + indent, indent, false);
                                    if (str3 == null)
                                    {
                                        utilWriter.AppendSpacesToLinePosition(num4);
                                    }
                                }
                                if (str3 == null)
                                {
                                    util.CopyXmlNode(utilWriter);
                                }
                                else
                                {
                                    utilWriter.Write(str3);
                                }
                                util.CopyReaderToNextElement(utilWriter, true);
                            }
                        }
                    }
                    if (!flag3 && (declarationUpdates != null))
                    {
                        int num5;
                        bool skipFirstIndent = (reader.Depth > 0) && (reader.NodeType == XmlNodeType.Element);
                        if (skipFirstIndent)
                        {
                            num5 = util.TrueLinePosition;
                        }
                        else
                        {
                            num5 = trueLinePosition + indent;
                        }
                        this.WriteNewConfigDeclarations(declarationUpdates, utilWriter, num5, indent, skipFirstIndent);
                    }
                    if (definitionUpdates != null)
                    {
                        bool locationPathApplies = false;
                        LocationUpdates locationUpdates = null;
                        SectionUpdates sectionUpdates = null;
                        if (!base.IsLocationConfig)
                        {
                            locationPathApplies = true;
                            locationUpdates = definitionUpdates.FindLocationUpdates(OverrideModeSetting.LocationDefault, true);
                            if (locationUpdates != null)
                            {
                                sectionUpdates = locationUpdates.SectionUpdates;
                            }
                        }
                        this.CopyConfigDefinitionsRecursive(definitionUpdates, util, utilWriter, locationPathApplies, locationUpdates, sectionUpdates, true, string.Empty, trueLinePosition, indent);
                        this.WriteNewConfigDefinitions(definitionUpdates, utilWriter, trueLinePosition + indent, indent);
                    }
                    if (s != null)
                    {
                        if (!utilWriter.IsLastLineBlank)
                        {
                            utilWriter.AppendNewLine();
                        }
                        utilWriter.Write(s);
                    }
                    while (util.CopyXmlNode(utilWriter))
                    {
                    }
                }
            }
        }

        private bool CopyConfigDeclarationsRecursive(SectionUpdates declarationUpdates, XmlUtil xmlUtil, XmlUtilWriter utilWriter, string group, int parentLinePosition, int parentIndent)
        {
            int trueLinePosition;
            int num3;
            bool flag = false;
            XmlTextReader reader = xmlUtil.Reader;
            int oldIndent = this.UpdateIndent(parentIndent, xmlUtil, utilWriter, parentLinePosition);
            if (reader.NodeType == XmlNodeType.Element)
            {
                trueLinePosition = xmlUtil.TrueLinePosition;
                num3 = trueLinePosition;
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                trueLinePosition = parentLinePosition + oldIndent;
                if (utilWriter.IsLastLineBlank)
                {
                    num3 = xmlUtil.TrueLinePosition;
                }
                else
                {
                    num3 = parentLinePosition;
                }
            }
            else
            {
                trueLinePosition = parentLinePosition + oldIndent;
                num3 = 0;
            }
            if (declarationUpdates != null)
            {
                string[] movedSectionNames = declarationUpdates.GetMovedSectionNames();
                if (movedSectionNames != null)
                {
                    if (!utilWriter.IsLastLineBlank)
                    {
                        utilWriter.AppendNewLine();
                    }
                    foreach (string str in movedSectionNames)
                    {
                        DeclarationUpdate declarationUpdate = declarationUpdates.GetDeclarationUpdate(str);
                        utilWriter.AppendSpacesToLinePosition(trueLinePosition);
                        utilWriter.Write(declarationUpdate.UpdatedXml);
                        utilWriter.AppendNewLine();
                        flag = true;
                    }
                    utilWriter.AppendSpacesToLinePosition(num3);
                }
            }
            if (reader.NodeType == XmlNodeType.Element)
            {
                int depth = reader.Depth;
                while (reader.Depth == depth)
                {
                    bool flag2 = false;
                    DeclarationUpdate update2 = null;
                    DeclarationUpdate sectionGroupUpdate = null;
                    SectionUpdates sectionUpdatesForGroup = null;
                    SectionUpdates updates2 = declarationUpdates;
                    string str2 = group;
                    oldIndent = this.UpdateIndent(oldIndent, xmlUtil, utilWriter, parentLinePosition);
                    trueLinePosition = xmlUtil.TrueLinePosition;
                    string name = reader.Name;
                    string attribute = reader.GetAttribute("name");
                    string configKey = BaseConfigurationRecord.CombineConfigKey(group, attribute);
                    if (name == "sectionGroup")
                    {
                        sectionUpdatesForGroup = declarationUpdates.GetSectionUpdatesForGroup(attribute);
                        if (sectionUpdatesForGroup != null)
                        {
                            sectionGroupUpdate = sectionUpdatesForGroup.GetSectionGroupUpdate();
                            if (sectionUpdatesForGroup.HasUnretrievedSections())
                            {
                                flag2 = true;
                                str2 = configKey;
                                updates2 = sectionUpdatesForGroup;
                            }
                        }
                    }
                    else
                    {
                        update2 = declarationUpdates.GetDeclarationUpdate(configKey);
                    }
                    bool flag3 = (sectionGroupUpdate != null) && (sectionGroupUpdate.UpdatedXml != null);
                    if (flag2)
                    {
                        object o = utilWriter.CreateStreamCheckpoint();
                        string s = null;
                        if (flag3)
                        {
                            utilWriter.Write(sectionGroupUpdate.UpdatedXml);
                            reader.Read();
                        }
                        else
                        {
                            s = xmlUtil.UpdateStartElement(utilWriter, null, true, trueLinePosition, oldIndent);
                        }
                        if (s == null)
                        {
                            xmlUtil.CopyReaderToNextElement(utilWriter, true);
                        }
                        bool flag4 = this.CopyConfigDeclarationsRecursive(updates2, xmlUtil, utilWriter, str2, trueLinePosition, oldIndent);
                        if (s != null)
                        {
                            utilWriter.AppendSpacesToLinePosition(trueLinePosition);
                            utilWriter.Write(s);
                            utilWriter.AppendSpacesToLinePosition(parentLinePosition);
                        }
                        else
                        {
                            xmlUtil.CopyXmlNode(utilWriter);
                        }
                        if (flag4 || flag3)
                        {
                            flag = true;
                        }
                        else
                        {
                            utilWriter.RestoreStreamCheckpoint(o);
                        }
                        xmlUtil.CopyReaderToNextElement(utilWriter, true);
                    }
                    else
                    {
                        bool flag5;
                        bool flag6 = false;
                        if (update2 == null)
                        {
                            flag5 = true;
                            if (flag3)
                            {
                                flag = true;
                                utilWriter.Write(sectionGroupUpdate.UpdatedXml);
                                utilWriter.AppendNewLine();
                                utilWriter.AppendSpacesToLinePosition(trueLinePosition);
                                utilWriter.Write("</sectionGroup>");
                                utilWriter.AppendNewLine();
                                utilWriter.AppendSpacesToLinePosition(trueLinePosition);
                            }
                            else if (sectionGroupUpdate != null)
                            {
                                flag = true;
                                flag5 = false;
                                flag6 = true;
                            }
                        }
                        else
                        {
                            flag = true;
                            if (update2.UpdatedXml == null)
                            {
                                flag5 = false;
                            }
                            else
                            {
                                flag5 = true;
                                utilWriter.Write(update2.UpdatedXml);
                            }
                        }
                        if (flag5)
                        {
                            xmlUtil.SkipAndCopyReaderToNextElement(utilWriter, true);
                        }
                        else
                        {
                            if (flag6)
                            {
                                xmlUtil.SkipChildElementsAndCopyOuterXmlToNextElement(utilWriter);
                                continue;
                            }
                            xmlUtil.CopyOuterXmlToNextElement(utilWriter, true);
                        }
                    }
                }
            }
            return flag;
        }

        private bool CopyConfigDefinitionsRecursive(ConfigDefinitionUpdates configDefinitionUpdates, XmlUtil xmlUtil, XmlUtilWriter utilWriter, bool locationPathApplies, LocationUpdates locationUpdates, SectionUpdates sectionUpdates, bool addNewSections, string group, int parentLinePosition, int parentIndent)
        {
            int trueLinePosition;
            int num3;
            bool flag = false;
            XmlTextReader reader = xmlUtil.Reader;
            int indent = this.UpdateIndent(parentIndent, xmlUtil, utilWriter, parentLinePosition);
            if (reader.NodeType == XmlNodeType.Element)
            {
                trueLinePosition = xmlUtil.TrueLinePosition;
                num3 = trueLinePosition;
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                trueLinePosition = parentLinePosition + indent;
                if (utilWriter.IsLastLineBlank)
                {
                    num3 = xmlUtil.TrueLinePosition;
                }
                else
                {
                    num3 = parentLinePosition;
                }
            }
            else
            {
                trueLinePosition = parentLinePosition + indent;
                num3 = 0;
            }
            if ((sectionUpdates != null) && addNewSections)
            {
                sectionUpdates.IsNew = false;
                string[] movedSectionNames = sectionUpdates.GetMovedSectionNames();
                if (movedSectionNames != null)
                {
                    if (!utilWriter.IsLastLineBlank)
                    {
                        utilWriter.AppendNewLine();
                    }
                    utilWriter.AppendSpacesToLinePosition(trueLinePosition);
                    bool skipFirstIndent = true;
                    foreach (string str in movedSectionNames)
                    {
                        DefinitionUpdate definitionUpdate = sectionUpdates.GetDefinitionUpdate(str);
                        this.WriteSectionUpdate(utilWriter, definitionUpdate, trueLinePosition, indent, skipFirstIndent);
                        skipFirstIndent = false;
                        utilWriter.AppendNewLine();
                        flag = true;
                    }
                    utilWriter.AppendSpacesToLinePosition(num3);
                }
            }
            if (reader.NodeType == XmlNodeType.Element)
            {
                int depth = reader.Depth;
                while (reader.Depth == depth)
                {
                    bool flag3 = false;
                    DefinitionUpdate update = null;
                    bool flag4 = locationPathApplies;
                    LocationUpdates updates = locationUpdates;
                    SectionUpdates updates2 = sectionUpdates;
                    bool flag5 = addNewSections;
                    string str2 = group;
                    bool flag6 = false;
                    indent = this.UpdateIndent(indent, xmlUtil, utilWriter, parentLinePosition);
                    trueLinePosition = xmlUtil.TrueLinePosition;
                    string name = reader.Name;
                    if (name == "location")
                    {
                        string locationSubPath = BaseConfigurationRecord.NormalizeLocationSubPath(reader.GetAttribute("path"), xmlUtil);
                        flag4 = false;
                        OverrideModeSetting locationDefault = OverrideModeSetting.LocationDefault;
                        bool inheritInChildApps = true;
                        if (base.IsLocationConfig)
                        {
                            if (locationSubPath == null)
                            {
                                flag4 = false;
                            }
                            else
                            {
                                flag4 = StringUtil.EqualsIgnoreCase(base.ConfigPath, base.Host.GetConfigPathFromLocationSubPath(base.Parent.ConfigPath, locationSubPath));
                            }
                        }
                        else
                        {
                            flag4 = locationSubPath == null;
                        }
                        if (flag4)
                        {
                            string attribute = reader.GetAttribute("allowOverride");
                            if (attribute != null)
                            {
                                locationDefault = OverrideModeSetting.CreateFromXmlReadValue(bool.Parse(attribute));
                            }
                            string str6 = reader.GetAttribute("overrideMode");
                            if (str6 != null)
                            {
                                locationDefault = OverrideModeSetting.CreateFromXmlReadValue(OverrideModeSetting.ParseOverrideModeXmlValue(str6, null));
                            }
                            string str7 = reader.GetAttribute("inheritInChildApplications");
                            if (str7 != null)
                            {
                                inheritInChildApps = bool.Parse(str7);
                            }
                            configDefinitionUpdates.FlagLocationWritten();
                        }
                        if (reader.IsEmptyElement)
                        {
                            if (flag4 && (configDefinitionUpdates.FindLocationUpdates(locationDefault, inheritInChildApps) != null))
                            {
                                flag4 = true;
                            }
                            else
                            {
                                flag4 = false;
                            }
                        }
                        else if (flag4)
                        {
                            if (configDefinitionUpdates != null)
                            {
                                updates = configDefinitionUpdates.FindLocationUpdates(locationDefault, inheritInChildApps);
                                if (updates != null)
                                {
                                    flag3 = true;
                                    updates2 = updates.SectionUpdates;
                                    if ((base._locationSubPath == null) && updates.IsDefault)
                                    {
                                        flag5 = false;
                                    }
                                }
                            }
                        }
                        else if ((this.HasRemovedSectionsOrGroups && !base.IsLocationConfig) && base.Host.SupportsLocation)
                        {
                            flag3 = true;
                            updates = null;
                            updates2 = null;
                            flag5 = false;
                        }
                    }
                    else
                    {
                        string configKey = BaseConfigurationRecord.CombineConfigKey(group, name);
                        FactoryRecord record = base.FindFactoryRecord(configKey, false);
                        if (record == null)
                        {
                            if (!flag4 && !base.IsLocationConfig)
                            {
                                flag6 = true;
                            }
                        }
                        else if (record.IsGroup)
                        {
                            if (reader.IsEmptyElement)
                            {
                                if (!flag4 && !base.IsLocationConfig)
                                {
                                    flag6 = true;
                                }
                            }
                            else if (sectionUpdates != null)
                            {
                                SectionUpdates sectionUpdatesForGroup = sectionUpdates.GetSectionUpdatesForGroup(name);
                                if (sectionUpdatesForGroup != null)
                                {
                                    flag3 = true;
                                    str2 = configKey;
                                    updates2 = sectionUpdatesForGroup;
                                }
                            }
                            else if (!flag4 && !base.IsLocationConfig)
                            {
                                if ((this._removedSectionGroups != null) && this._removedSectionGroups.Contains(configKey))
                                {
                                    flag6 = true;
                                }
                                else
                                {
                                    flag3 = true;
                                    str2 = configKey;
                                    updates = null;
                                    updates2 = null;
                                    flag5 = false;
                                }
                            }
                        }
                        else if (sectionUpdates != null)
                        {
                            update = sectionUpdates.GetDefinitionUpdate(configKey);
                        }
                        else if ((!flag4 && !base.IsLocationConfig) && ((this._removedSections != null) && this._removedSections.Contains(configKey)))
                        {
                            flag6 = true;
                        }
                    }
                    if (flag3)
                    {
                        object o = utilWriter.CreateStreamCheckpoint();
                        xmlUtil.CopyXmlNode(utilWriter);
                        xmlUtil.CopyReaderToNextElement(utilWriter, true);
                        bool flag8 = this.CopyConfigDefinitionsRecursive(configDefinitionUpdates, xmlUtil, utilWriter, flag4, updates, updates2, flag5, str2, trueLinePosition, indent);
                        xmlUtil.CopyXmlNode(utilWriter);
                        if (flag8)
                        {
                            flag = true;
                        }
                        else
                        {
                            utilWriter.RestoreStreamCheckpoint(o);
                        }
                        xmlUtil.CopyReaderToNextElement(utilWriter, true);
                    }
                    else
                    {
                        bool flag9;
                        if (update == null)
                        {
                            flag9 = flag4 || flag6;
                        }
                        else
                        {
                            flag9 = false;
                            if (update.UpdatedXml != null)
                            {
                                ConfigurationSection result = (ConfigurationSection) update.SectionRecord.Result;
                                if (string.IsNullOrEmpty(result.SectionInformation.ConfigSource) || result.SectionInformation.ConfigSourceModified)
                                {
                                    flag9 = true;
                                    this.WriteSectionUpdate(utilWriter, update, trueLinePosition, indent, true);
                                    flag = true;
                                }
                            }
                        }
                        if (flag9)
                        {
                            xmlUtil.SkipAndCopyReaderToNextElement(utilWriter, true);
                        }
                        else
                        {
                            xmlUtil.CopyOuterXmlToNextElement(utilWriter, true);
                            flag = true;
                        }
                    }
                }
            }
            if (((sectionUpdates != null) && addNewSections) && sectionUpdates.HasNewSectionGroups())
            {
                trueLinePosition = parentLinePosition + indent;
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (utilWriter.IsLastLineBlank)
                    {
                        num3 = xmlUtil.TrueLinePosition;
                    }
                    else
                    {
                        num3 = parentLinePosition;
                    }
                }
                else
                {
                    num3 = 0;
                }
                utilWriter.AppendSpacesToLinePosition(trueLinePosition);
                if (this.WriteNewConfigDefinitionsRecursive(utilWriter, sectionUpdates, trueLinePosition, indent, true))
                {
                    flag = true;
                }
                utilWriter.AppendSpacesToLinePosition(num3);
            }
            return flag;
        }

        private void CopyConfigSource(XmlUtilWriter utilWriter, string updatedXml, string configSourceStreamName, byte[] buffer)
        {
            byte[] preamble;
            using (Stream stream = new MemoryStream(buffer))
            {
                using (new XmlUtil(stream, configSourceStreamName, true))
                {
                    preamble = base.ConfigStreamInfo.StreamEncoding.GetPreamble();
                }
            }
            this.CheckPreamble(preamble, utilWriter, buffer);
            using (Stream stream2 = new MemoryStream(buffer))
            {
                using (XmlUtil util2 = new XmlUtil(stream2, configSourceStreamName, false))
                {
                    XmlTextReader reader = util2.Reader;
                    reader.WhitespaceHandling = WhitespaceHandling.All;
                    reader.Read();
                    int indent = 4;
                    int linePosition = 1;
                    bool flag = util2.CopyReaderToNextElement(utilWriter, false);
                    if (flag)
                    {
                        int lineNumber = reader.LineNumber;
                        linePosition = reader.LinePosition - 1;
                        int num4 = 0;
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.LineNumber > lineNumber)
                            {
                                num4 = reader.LinePosition - linePosition;
                                break;
                            }
                        }
                        int num5 = 0;
                        reader.Read();
                        while (reader.Depth >= 1)
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                num5 = (reader.LinePosition - 1) - linePosition;
                                break;
                            }
                            reader.Read();
                        }
                        if (num5 > 0)
                        {
                            indent = num5;
                        }
                        else if (num4 > 0)
                        {
                            indent = num4;
                        }
                    }
                    string s = XmlUtil.FormatXmlElement(updatedXml, linePosition, indent, true);
                    utilWriter.Write(s);
                    if (flag)
                    {
                        while (reader.Depth > 0)
                        {
                            reader.Read();
                        }
                        if (reader.IsEmptyElement || (reader.NodeType == XmlNodeType.EndElement))
                        {
                            reader.Read();
                        }
                        while (util2.CopyXmlNode(utilWriter))
                        {
                        }
                    }
                }
            }
        }

        internal static MgmtConfigurationRecord Create(IInternalConfigRoot configRoot, IInternalConfigRecord parent, string configPath, string locationSubPath)
        {
            MgmtConfigurationRecord record = new MgmtConfigurationRecord();
            record.Init(configRoot, parent, configPath, locationSubPath);
            return record;
        }

        private void CreateNewConfig(SectionUpdates declarationUpdates, ConfigDefinitionUpdates definitionUpdates, NamespaceChange namespaceChange, XmlUtilWriter utilWriter)
        {
            int linePosition = 5;
            int indent = 4;
            utilWriter.Write(string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"{0}\"?>\r\n", new object[] { base.ConfigStreamInfo.StreamEncoding.WebName }));
            if (namespaceChange == NamespaceChange.Add)
            {
                utilWriter.Write(string.Format(CultureInfo.InvariantCulture, "<configuration xmlns=\"{0}\">\r\n", new object[] { "http://schemas.microsoft.com/.NetConfiguration/v2.0" }));
            }
            else
            {
                utilWriter.Write("<configuration>\r\n");
            }
            if (declarationUpdates != null)
            {
                this.WriteNewConfigDeclarations(declarationUpdates, utilWriter, linePosition, indent, false);
            }
            this.WriteNewConfigDefinitions(definitionUpdates, utilWriter, linePosition, indent);
            utilWriter.Write("</configuration>");
        }

        private void CreateNewConfigSource(XmlUtilWriter utilWriter, string updatedXml, int indent)
        {
            string str = XmlUtil.FormatXmlElement(updatedXml, 0, indent, true);
            utilWriter.Write(string.Format(CultureInfo.InvariantCulture, "<?xml version=\"1.0\" encoding=\"{0}\"?>\r\n", new object[] { base.ConfigStreamInfo.StreamEncoding.WebName }));
            utilWriter.Write(str + "\r\n");
        }

        protected override object CreateSection(bool inputIsTrusted, FactoryRecord factoryRecord, SectionRecord sectionRecord, object parentConfig, ConfigXmlReader reader)
        {
            ConstructorInfo factory = (ConstructorInfo) factoryRecord.Factory;
            ConfigurationSection section = (ConfigurationSection) System.Configuration.TypeUtil.InvokeCtorWithReflectionPermission(factory);
            section.SectionInformation.AttachToConfigurationRecord(this, factoryRecord, sectionRecord);
            section.CallInit();
            ConfigurationSection parentElement = (ConfigurationSection) parentConfig;
            section.Reset(parentElement);
            if (reader != null)
            {
                section.DeserializeSection(reader);
            }
            section.ResetModified();
            return section;
        }

        protected override object CreateSectionFactory(FactoryRecord factoryRecord)
        {
            Type c = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(base.Host, factoryRecord.FactoryTypeName, true);
            if (!typeof(ConfigurationSection).IsAssignableFrom(c))
            {
                System.Configuration.TypeUtil.VerifyAssignableType(typeof(IConfigurationSectionHandler), c, true);
                c = typeof(DefaultSection);
            }
            return System.Configuration.TypeUtil.GetConstructorWithReflectionPermission(c, typeof(ConfigurationSection), true);
        }

        private ConstructorInfo CreateSectionGroupFactory(FactoryRecord factoryRecord)
        {
            Type type;
            if (string.IsNullOrEmpty(factoryRecord.FactoryTypeName))
            {
                type = typeof(ConfigurationSectionGroup);
            }
            else
            {
                type = System.Configuration.TypeUtil.GetTypeWithReflectionPermission(base.Host, factoryRecord.FactoryTypeName, true);
            }
            return System.Configuration.TypeUtil.GetConstructorWithReflectionPermission(type, typeof(ConfigurationSectionGroup), true);
        }

        private ConstructorInfo EnsureSectionGroupFactory(FactoryRecord factoryRecord)
        {
            ConstructorInfo factory = (ConstructorInfo) factoryRecord.Factory;
            if (factory == null)
            {
                factory = this.CreateSectionGroupFactory(factoryRecord);
                factoryRecord.Factory = factory;
            }
            return factory;
        }

        private string ExeDefinitionToString(ConfigurationAllowExeDefinition allowDefinition)
        {
            switch (allowDefinition)
            {
                case ConfigurationAllowExeDefinition.MachineOnly:
                    return "MachineOnly";

                case ConfigurationAllowExeDefinition.MachineToApplication:
                    return "MachineToApplication";

                case ConfigurationAllowExeDefinition.MachineToRoamingUser:
                    return "MachineToRoamingUser";

                case ConfigurationAllowExeDefinition.MachineToLocalUser:
                    return "MachineToLocalUser";
            }
            throw ExceptionUtil.PropertyInvalid("AllowExeDefinition");
        }

        internal ConfigurationSection FindAndCloneImmediateParentSection(ConfigurationSection configSection)
        {
            string configKey = configSection.SectionInformation.ConfigKey;
            ConfigurationSection parentResult = this.FindImmediateParentSection(configSection);
            SectionRecord sectionRecord = base.GetSectionRecord(configKey, false);
            return (ConfigurationSection) this.UseParentResult(configKey, parentResult, sectionRecord);
        }

        internal ConfigurationSection FindImmediateParentSection(ConfigurationSection section)
        {
            ConfigurationSection result = null;
            string sectionName = section.SectionInformation.SectionName;
            SectionRecord sectionRecord = base.GetSectionRecord(sectionName, false);
            if (sectionRecord.HasLocationInputs)
            {
                result = (ConfigurationSection) sectionRecord.LastLocationInput.Result;
            }
            else if (sectionRecord.HasIndirectLocationInputs)
            {
                result = (ConfigurationSection) sectionRecord.LastIndirectLocationInput.Result;
            }
            else if (base.IsRootDeclaration(sectionName, true))
            {
                object obj2;
                object obj3;
                FactoryRecord factoryRecord = base.GetFactoryRecord(sectionName, false);
                base.CreateSectionDefault(sectionName, false, factoryRecord, null, out obj2, out obj3);
                result = (ConfigurationSection) obj2;
            }
            else
            {
                for (MgmtConfigurationRecord record3 = this.MgmtParent; !record3.IsRootConfig; record3 = record3.MgmtParent)
                {
                    sectionRecord = record3.GetSectionRecord(sectionName, false);
                    if ((sectionRecord != null) && sectionRecord.HasResult)
                    {
                        result = (ConfigurationSection) sectionRecord.Result;
                        break;
                    }
                }
            }
            if (!result.IsReadOnly())
            {
                result.SetReadOnly();
            }
            return result;
        }

        private Hashtable GetAllFactories(bool isGroup)
        {
            Hashtable hashtable = new Hashtable();
            MgmtConfigurationRecord mgmtParent = this;
            do
            {
                if (mgmtParent._factoryRecords != null)
                {
                    foreach (FactoryRecord record2 in mgmtParent._factoryRecords.Values)
                    {
                        if (record2.IsGroup == isGroup)
                        {
                            string configKey = record2.ConfigKey;
                            hashtable[configKey] = new FactoryId(record2.ConfigKey, record2.Group, record2.Name);
                        }
                    }
                }
                mgmtParent = mgmtParent.MgmtParent;
            }
            while (!mgmtParent.IsRootConfig);
            return hashtable;
        }

        private SectionUpdates GetConfigDeclarationUpdates(ConfigurationSaveMode saveMode, bool forceUpdateAll)
        {
            if (!base.IsLocationConfig)
            {
                bool hasRemovedSectionsOrGroups = this.HasRemovedSectionsOrGroups;
                SectionUpdates updates = new SectionUpdates(string.Empty);
                if (base._factoryRecords != null)
                {
                    foreach (FactoryRecord record in base._factoryRecords.Values)
                    {
                        if (!record.IsGroup)
                        {
                            string str = null;
                            if (!record.IsUndeclared)
                            {
                                ConfigurationSection configSection = this.GetConfigSection(record.ConfigKey);
                                if (configSection != null)
                                {
                                    if ((!configSection.SectionInformation.IsDeclared && !this.MgmtParent.IsRootConfig) && (this.MgmtParent.FindFactoryRecord(record.ConfigKey, false) != null))
                                    {
                                        if (record.HasFile)
                                        {
                                            hasRemovedSectionsOrGroups = true;
                                        }
                                        continue;
                                    }
                                    if ((base.TargetFramework != null) && !configSection.ShouldSerializeSectionInTargetVersion(base.TargetFramework))
                                    {
                                        continue;
                                    }
                                    if (this.AreDeclarationAttributesModified(record, configSection) || !record.HasFile)
                                    {
                                        str = this.GetUpdatedSectionDeclarationXml(record, configSection, saveMode);
                                        if (!string.IsNullOrEmpty(str))
                                        {
                                            hasRemovedSectionsOrGroups = true;
                                        }
                                    }
                                }
                                DeclarationUpdate update = new DeclarationUpdate(record.ConfigKey, !record.HasFile, str);
                                updates.AddSection(update);
                            }
                        }
                        else
                        {
                            bool flag2 = false;
                            ConfigurationSectionGroup sectionGroup = this.LookupSectionGroup(record.ConfigKey);
                            if (!record.HasFile)
                            {
                                flag2 = true;
                            }
                            else if ((sectionGroup != null) && sectionGroup.IsDeclarationRequired)
                            {
                                flag2 = true;
                            }
                            else if ((record.FactoryTypeName != null) || (sectionGroup != null))
                            {
                                FactoryRecord record2 = null;
                                if (!this.MgmtParent.IsRootConfig)
                                {
                                    record2 = this.MgmtParent.FindFactoryRecord(record.ConfigKey, false);
                                }
                                flag2 = (record2 == null) || (record2.FactoryTypeName == null);
                            }
                            if (flag2)
                            {
                                string updatedSectionGroupDeclarationXml = null;
                                if (!record.HasFile || ((sectionGroup != null) && (sectionGroup.Type != record.FactoryTypeName)))
                                {
                                    updatedSectionGroupDeclarationXml = this.GetUpdatedSectionGroupDeclarationXml(record, sectionGroup);
                                    if (!string.IsNullOrEmpty(updatedSectionGroupDeclarationXml))
                                    {
                                        hasRemovedSectionsOrGroups = true;
                                    }
                                }
                                DeclarationUpdate update2 = new DeclarationUpdate(record.ConfigKey, !record.HasFile, updatedSectionGroupDeclarationXml);
                                updates.AddSectionGroup(update2);
                            }
                        }
                    }
                }
                if (base._sectionRecords != null)
                {
                    foreach (SectionRecord record3 in base._sectionRecords.Values)
                    {
                        if ((base.GetFactoryRecord(record3.ConfigKey, false) == null) && record3.HasResult)
                        {
                            ConfigurationSection result = (ConfigurationSection) record3.Result;
                            FactoryRecord factoryRecord = this.MgmtParent.FindFactoryRecord(record3.ConfigKey, false);
                            if (result.SectionInformation.IsDeclared)
                            {
                                string str3 = this.GetUpdatedSectionDeclarationXml(factoryRecord, result, saveMode);
                                if (!string.IsNullOrEmpty(str3))
                                {
                                    hasRemovedSectionsOrGroups = true;
                                    DeclarationUpdate update3 = new DeclarationUpdate(factoryRecord.ConfigKey, true, str3);
                                    updates.AddSection(update3);
                                }
                            }
                        }
                    }
                }
                if (this._sectionGroups != null)
                {
                    foreach (ConfigurationSectionGroup group2 in this._sectionGroups.Values)
                    {
                        if (base.GetFactoryRecord(group2.SectionGroupName, false) == null)
                        {
                            FactoryRecord record5 = this.MgmtParent.FindFactoryRecord(group2.SectionGroupName, false);
                            if (group2.IsDeclared || ((record5 != null) && (group2.Type != record5.FactoryTypeName)))
                            {
                                string str4 = this.GetUpdatedSectionGroupDeclarationXml(record5, group2);
                                if (!string.IsNullOrEmpty(str4))
                                {
                                    hasRemovedSectionsOrGroups = true;
                                    DeclarationUpdate update4 = new DeclarationUpdate(record5.ConfigKey, true, str4);
                                    updates.AddSectionGroup(update4);
                                }
                            }
                        }
                    }
                }
                if (hasRemovedSectionsOrGroups)
                {
                    return updates;
                }
            }
            return null;
        }

        private void GetConfigDefinitionUpdates(bool requireUpdates, ConfigurationSaveMode saveMode, bool forceSaveAll, out ConfigDefinitionUpdates definitionUpdates, out ArrayList configSourceUpdates)
        {
            definitionUpdates = new ConfigDefinitionUpdates();
            configSourceUpdates = null;
            bool hasRemovedSections = this.HasRemovedSections;
            if (base._sectionRecords != null)
            {
                base.InitProtectedConfigurationSection();
                foreach (DictionaryEntry entry in base._sectionRecords)
                {
                    string key = (string) entry.Key;
                    SectionRecord sectionRecord = (SectionRecord) entry.Value;
                    sectionRecord.AddUpdate = false;
                    bool hasFileInput = sectionRecord.HasFileInput;
                    OverrideModeSetting locationDefault = OverrideModeSetting.LocationDefault;
                    bool inheritInChildApps = true;
                    bool moved = false;
                    string xmlElement = null;
                    bool flag5 = false;
                    if (!sectionRecord.HasResult)
                    {
                        if (sectionRecord.HasFileInput)
                        {
                            SectionXmlInfo sectionXmlInfo = sectionRecord.FileInput.SectionXmlInfo;
                            locationDefault = sectionXmlInfo.OverrideModeSetting;
                            inheritInChildApps = !sectionXmlInfo.SkipInChildApps;
                            flag5 = requireUpdates && !string.IsNullOrEmpty(sectionXmlInfo.ConfigSource);
                        }
                    }
                    else
                    {
                        ConfigurationSection result = (ConfigurationSection) sectionRecord.Result;
                        if ((base.TargetFramework != null) && !result.ShouldSerializeSectionInTargetVersion(base.TargetFramework))
                        {
                            continue;
                        }
                        locationDefault = result.SectionInformation.OverrideModeSetting;
                        inheritInChildApps = result.SectionInformation.InheritInChildApplications;
                        if (!result.SectionInformation.AllowLocation && (!locationDefault.IsDefaultForLocationTag || !inheritInChildApps))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_inconsistent_location_attributes", new object[] { key }));
                        }
                        flag5 = requireUpdates && !string.IsNullOrEmpty(result.SectionInformation.ConfigSource);
                        try
                        {
                            bool flag6 = (result.SectionInformation.ForceSave || result.IsModified()) || (forceSaveAll && !result.SectionInformation.IsLocked);
                            bool flag7 = this.AreSectionAttributesModified(sectionRecord, result);
                            bool flag8 = flag6 || (result.SectionInformation.RawXml != null);
                            if (flag8 || flag7)
                            {
                                result.SectionInformation.VerifyIsEditable();
                                result.SectionInformation.Removed = false;
                                hasFileInput = true;
                                moved = this.IsConfigSectionMoved(sectionRecord, result);
                                if (!flag5)
                                {
                                    flag5 = !string.IsNullOrEmpty(result.SectionInformation.ConfigSource) && (flag8 || result.SectionInformation.ConfigSourceModified);
                                }
                                if ((flag6 || (result.SectionInformation.RawXml == null)) || (saveMode == ConfigurationSaveMode.Full))
                                {
                                    ConfigurationSection parentElement = this.FindImmediateParentSection(result);
                                    xmlElement = result.SerializeSection(parentElement, result.SectionInformation.Name, saveMode);
                                    this.ValidateSectionXml(xmlElement, key);
                                }
                                else
                                {
                                    xmlElement = result.SectionInformation.RawXml;
                                }
                                if (string.IsNullOrEmpty(xmlElement) && ((!string.IsNullOrEmpty(result.SectionInformation.ConfigSource) || !result.SectionInformation.LocationAttributesAreDefault) || (result.SectionInformation.ProtectionProvider != null)))
                                {
                                    xmlElement = this.WriteEmptyElement(result.SectionInformation.Name);
                                }
                                if (string.IsNullOrEmpty(xmlElement))
                                {
                                    result.SectionInformation.Removed = true;
                                    xmlElement = null;
                                    hasFileInput = false;
                                    if (sectionRecord.HasFileInput)
                                    {
                                        hasRemovedSections = true;
                                        sectionRecord.RemoveFileInput();
                                    }
                                    goto Label_0416;
                                }
                                if ((flag7 || moved) || string.IsNullOrEmpty(result.SectionInformation.ConfigSource))
                                {
                                    hasRemovedSections = true;
                                }
                                if (result.SectionInformation.ProtectionProvider == null)
                                {
                                    goto Label_0416;
                                }
                                ProtectedConfigurationSection section = base.GetSection("configProtectedData") as ProtectedConfigurationSection;
                                try
                                {
                                    xmlElement = ProtectedConfigurationSection.FormatEncryptedSection(base.Host.EncryptSection(xmlElement, result.SectionInformation.ProtectionProvider, section), result.SectionInformation.Name, result.SectionInformation.ProtectionProvider.Name);
                                    goto Label_0416;
                                }
                                catch (Exception exception)
                                {
                                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Encryption_failed", new object[] { result.SectionInformation.SectionName, result.SectionInformation.ProtectionProvider.Name, exception.Message }), exception);
                                }
                            }
                            if (result.SectionInformation.Removed)
                            {
                                hasFileInput = false;
                                if (sectionRecord.HasFileInput)
                                {
                                    hasRemovedSections = true;
                                }
                            }
                        }
                        catch (Exception exception2)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_exception_in_config_section_handler", new object[] { result.SectionInformation.SectionName }), exception2);
                        }
                    }
                Label_0416:
                    if (hasFileInput)
                    {
                        if (base.GetSectionLockedMode(sectionRecord.ConfigKey) == OverrideMode.Deny)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_section_locked"), null);
                        }
                        sectionRecord.AddUpdate = true;
                        DefinitionUpdate update = definitionUpdates.AddUpdate(locationDefault, inheritInChildApps, moved, xmlElement, sectionRecord);
                        if (flag5)
                        {
                            if (configSourceUpdates == null)
                            {
                                configSourceUpdates = new ArrayList();
                            }
                            configSourceUpdates.Add(update);
                        }
                    }
                }
            }
            if (this._flags[0x1000000])
            {
                hasRemovedSections = true;
                definitionUpdates.RequireLocation = true;
            }
            if (this._flags[0x2000000])
            {
                hasRemovedSections = true;
            }
            if (hasRemovedSections)
            {
                definitionUpdates.CompleteUpdates();
            }
            else
            {
                definitionUpdates = null;
            }
        }

        private ConfigurationSection GetConfigSection(string configKey)
        {
            SectionRecord sectionRecord = base.GetSectionRecord(configKey, false);
            if ((sectionRecord != null) && sectionRecord.HasResult)
            {
                return (ConfigurationSection) sectionRecord.Result;
            }
            return null;
        }

        private ArrayList GetDescendentSectionFactories(string configKey)
        {
            string str;
            ArrayList list = new ArrayList();
            if (configKey.Length == 0)
            {
                str = string.Empty;
            }
            else
            {
                str = configKey + "/";
            }
            foreach (FactoryId id in this.SectionFactories.Values)
            {
                if ((id.Group == configKey) || StringUtil.StartsWith(id.Group, str))
                {
                    list.Add(id);
                }
            }
            return list;
        }

        private ArrayList GetDescendentSectionGroupFactories(string configKey)
        {
            string str;
            ArrayList list = new ArrayList();
            if (configKey.Length == 0)
            {
                str = string.Empty;
            }
            else
            {
                str = configKey + "/";
            }
            foreach (FactoryId id in this.SectionGroupFactories.Values)
            {
                if ((id.ConfigKey == configKey) || StringUtil.StartsWith(id.ConfigKey, str))
                {
                    list.Add(id);
                }
            }
            return list;
        }

        internal ConfigurationLocationCollection GetLocationCollection(System.Configuration.Configuration config)
        {
            ArrayList col = new ArrayList();
            if (this._locationTags != null)
            {
                foreach (string str in this._locationTags.Values)
                {
                    col.Add(new ConfigurationLocation(config, str));
                }
            }
            return new ConfigurationLocationCollection(col);
        }

        internal string GetRawXml(string configKey)
        {
            SectionRecord sectionRecord = base.GetSectionRecord(configKey, false);
            if ((sectionRecord == null) || !sectionRecord.HasFileInput)
            {
                return null;
            }
            string[] keys = configKey.Split(BaseConfigurationRecord.ConfigPathSeparatorParams);
            return base.GetSectionXmlReader(keys, sectionRecord.FileInput).RawXml;
        }

        protected override object GetRuntimeObject(object result)
        {
            return result;
        }

        internal ConfigurationSectionGroup GetSectionGroup(string configKey)
        {
            ConfigurationSectionGroup sectionGroup = this.LookupSectionGroup(configKey);
            if (sectionGroup == null)
            {
                BaseConfigurationRecord record;
                FactoryRecord factoryRecord = base.FindFactoryRecord(configKey, false, out record);
                if (factoryRecord == null)
                {
                    return null;
                }
                if (!factoryRecord.IsGroup)
                {
                    throw ExceptionUtil.ParameterInvalid("sectionGroupName");
                }
                if (factoryRecord.FactoryTypeName == null)
                {
                    sectionGroup = new ConfigurationSectionGroup();
                }
                else
                {
                    ConstructorInfo ctor = this.EnsureSectionGroupFactory(factoryRecord);
                    try
                    {
                        sectionGroup = (ConfigurationSectionGroup) System.Configuration.TypeUtil.InvokeCtorWithReflectionPermission(ctor);
                    }
                    catch (Exception exception)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_exception_creating_section_handler", new object[] { factoryRecord.ConfigKey }), exception, factoryRecord);
                    }
                }
                sectionGroup.AttachToConfigurationRecord(this, factoryRecord);
                this.SectionGroups[configKey] = sectionGroup;
            }
            return sectionGroup;
        }

        private string GetUpdatedSectionDeclarationXml(FactoryRecord factoryRecord, ConfigurationSection configSection, ConfigurationSaveMode saveMode)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('<');
            sb.Append("section");
            sb.Append(' ');
            string arg = (configSection.SectionInformation.Type != null) ? configSection.SectionInformation.Type : factoryRecord.FactoryTypeName;
            if (base.TypeStringTransformerIsSet)
            {
                arg = base.TypeStringTransformer(arg);
            }
            this.AppendAttribute(sb, "name", configSection.SectionInformation.Name);
            this.AppendAttribute(sb, "type", arg);
            if ((!configSection.SectionInformation.AllowLocation || (saveMode == ConfigurationSaveMode.Full)) || ((saveMode == ConfigurationSaveMode.Modified) && configSection.SectionInformation.AllowLocationModified))
            {
                this.AppendAttribute(sb, "allowLocation", configSection.SectionInformation.AllowLocation ? "true" : "false");
            }
            if (((configSection.SectionInformation.AllowDefinition != ConfigurationAllowDefinition.Everywhere) || (saveMode == ConfigurationSaveMode.Full)) || ((saveMode == ConfigurationSaveMode.Modified) && configSection.SectionInformation.AllowDefinitionModified))
            {
                string str2 = null;
                switch (configSection.SectionInformation.AllowDefinition)
                {
                    case ConfigurationAllowDefinition.MachineToApplication:
                        str2 = "MachineToApplication";
                        break;

                    case ConfigurationAllowDefinition.Everywhere:
                        str2 = "Everywhere";
                        break;

                    case ConfigurationAllowDefinition.MachineOnly:
                        str2 = "MachineOnly";
                        break;

                    case ConfigurationAllowDefinition.MachineToWebRoot:
                        str2 = "MachineToWebRoot";
                        break;
                }
                this.AppendAttribute(sb, "allowDefinition", str2);
            }
            if (((configSection.SectionInformation.AllowExeDefinition != ConfigurationAllowExeDefinition.MachineToApplication) || (saveMode == ConfigurationSaveMode.Full)) || ((saveMode == ConfigurationSaveMode.Modified) && configSection.SectionInformation.AllowExeDefinitionModified))
            {
                this.AppendAttribute(sb, "allowExeDefinition", this.ExeDefinitionToString(configSection.SectionInformation.AllowExeDefinition));
            }
            if ((!configSection.SectionInformation.OverrideModeDefaultSetting.IsDefaultForSection || (saveMode == ConfigurationSaveMode.Full)) || ((saveMode == ConfigurationSaveMode.Modified) && configSection.SectionInformation.OverrideModeDefaultModified))
            {
                this.AppendAttribute(sb, "overrideModeDefault", configSection.SectionInformation.OverrideModeDefaultSetting.OverrideModeXmlValue);
            }
            if (!configSection.SectionInformation.RestartOnExternalChanges)
            {
                this.AppendAttribute(sb, "restartOnExternalChanges", "false");
            }
            else if ((saveMode == ConfigurationSaveMode.Full) || ((saveMode == ConfigurationSaveMode.Modified) && configSection.SectionInformation.RestartOnExternalChangesModified))
            {
                this.AppendAttribute(sb, "restartOnExternalChanges", "true");
            }
            if (!configSection.SectionInformation.RequirePermission)
            {
                this.AppendAttribute(sb, "requirePermission", "false");
            }
            else if ((saveMode == ConfigurationSaveMode.Full) || ((saveMode == ConfigurationSaveMode.Modified) && configSection.SectionInformation.RequirePermissionModified))
            {
                this.AppendAttribute(sb, "requirePermission", "true");
            }
            sb.Append("/>");
            return sb.ToString();
        }

        private string GetUpdatedSectionGroupDeclarationXml(FactoryRecord factoryRecord, ConfigurationSectionGroup configSectionGroup)
        {
            if ((base.TargetFramework != null) && !configSectionGroup.ShouldSerializeSectionGroupInTargetVersion(base.TargetFramework))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append('<');
            sb.Append("sectionGroup");
            sb.Append(' ');
            this.AppendAttribute(sb, "name", configSectionGroup.Name);
            string arg = (configSectionGroup.Type != null) ? configSectionGroup.Type : factoryRecord.FactoryTypeName;
            if (base.TypeStringTransformerIsSet)
            {
                arg = base.TypeStringTransformer(arg);
            }
            this.AppendAttribute(sb, "type", arg);
            sb.Append('>');
            return sb.ToString();
        }

        private void Init(IInternalConfigRoot configRoot, IInternalConfigRecord parent, string configPath, string locationSubPath)
        {
            base.Init(configRoot, (BaseConfigurationRecord) parent, configPath, locationSubPath);
            if (base.IsLocationConfig && ((this.MgmtParent._locationTags == null) || !this.MgmtParent._locationTags.Contains(base._locationSubPath)))
            {
                this._flags[0x1000000] = true;
            }
            this.InitStreamInfoUpdates();
        }

        private void InitStreamInfoUpdates()
        {
            this._streamInfoUpdates = new HybridDictionary(true);
            if (base.ConfigStreamInfo.HasStreamInfos)
            {
                foreach (StreamInfo info in base.ConfigStreamInfo.StreamInfos.Values)
                {
                    this._streamInfoUpdates.Add(info.StreamName, info.Clone());
                }
            }
        }

        private bool IsConfigSectionMoved(SectionRecord sectionRecord, ConfigurationSection configSection)
        {
            return (!sectionRecord.HasFileInput || this.AreLocationAttributesModified(sectionRecord, configSection));
        }

        private bool IsStreamUsed(string oldStreamName)
        {
            MgmtConfigurationRecord mgmtParent = this;
            if (base.IsLocationConfig)
            {
                mgmtParent = this.MgmtParent;
                if (mgmtParent._sectionRecords != null)
                {
                    foreach (SectionRecord record2 in mgmtParent._sectionRecords.Values)
                    {
                        if (record2.HasFileInput && StringUtil.EqualsIgnoreCase(record2.FileInput.SectionXmlInfo.ConfigSourceStreamName, oldStreamName))
                        {
                            return true;
                        }
                    }
                }
            }
            if (mgmtParent._locationSections != null)
            {
                foreach (LocationSectionRecord record3 in mgmtParent._locationSections)
                {
                    if (StringUtil.EqualsIgnoreCase(record3.SectionXmlInfo.ConfigSourceStreamName, oldStreamName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal ConfigurationSectionGroup LookupSectionGroup(string configKey)
        {
            ConfigurationSectionGroup group = null;
            if (this._sectionGroups != null)
            {
                group = (ConfigurationSectionGroup) this._sectionGroups[configKey];
            }
            return group;
        }

        internal void RemoveConfigurationSection(string group, string name)
        {
            bool flag = false;
            BaseConfigurationRecord.VerifySectionName(name, null, true);
            string key = BaseConfigurationRecord.CombineConfigKey(group, name);
            if (!this.RemovedSections.Contains(key) && (base.FindFactoryRecord(key, true) != null))
            {
                ConfigurationSection configSection = this.GetConfigSection(key);
                if (configSection != null)
                {
                    configSection.SectionInformation.DetachFromConfigurationRecord();
                }
                bool flag2 = base.IsRootDeclaration(key, false);
                if ((this._sectionFactories != null) && flag2)
                {
                    this._sectionFactories.Remove(key);
                }
                if ((!base.IsLocationConfig && (base._factoryRecords != null)) && base._factoryRecords.Contains(key))
                {
                    flag = true;
                    base._factoryRecords.Remove(key);
                }
                if ((base._sectionRecords != null) && base._sectionRecords.Contains(key))
                {
                    flag = true;
                    base._sectionRecords.Remove(key);
                }
                if (base._locationSections != null)
                {
                    int index = 0;
                    while (index < base._locationSections.Count)
                    {
                        LocationSectionRecord record = (LocationSectionRecord) base._locationSections[index];
                        if (record.ConfigKey != key)
                        {
                            index++;
                        }
                        else
                        {
                            flag = true;
                            base._locationSections.RemoveAt(index);
                        }
                    }
                }
                if (flag)
                {
                    this.RemovedSections.Add(key, key);
                }
                List<string> list = new List<string>();
                foreach (StreamInfo info in this._streamInfoUpdates.Values)
                {
                    if (info.SectionName == key)
                    {
                        list.Add(info.StreamName);
                    }
                }
                foreach (string str2 in list)
                {
                    this._streamInfoUpdates.Remove(str2);
                }
            }
        }

        internal void RemoveConfigurationSectionGroup(string group, string name)
        {
            BaseConfigurationRecord.VerifySectionName(name, null, false);
            string configKey = BaseConfigurationRecord.CombineConfigKey(group, name);
            if (base.FindFactoryRecord(configKey, true) != null)
            {
                foreach (FactoryId id in this.GetDescendentSectionFactories(configKey))
                {
                    this.RemoveConfigurationSection(id.Group, id.Name);
                }
                foreach (FactoryId id2 in this.GetDescendentSectionGroupFactories(configKey))
                {
                    if (!this.RemovedSectionGroups.Contains(id2.ConfigKey))
                    {
                        ConfigurationSectionGroup sectionGroup = this.LookupSectionGroup(id2.ConfigKey);
                        if (sectionGroup != null)
                        {
                            sectionGroup.DetachFromConfigurationRecord();
                        }
                        bool flag = base.IsRootDeclaration(id2.ConfigKey, false);
                        if ((this._sectionGroupFactories != null) && flag)
                        {
                            this._sectionGroupFactories.Remove(id2.ConfigKey);
                        }
                        if (!base.IsLocationConfig && (base._factoryRecords != null))
                        {
                            base._factoryRecords.Remove(id2.ConfigKey);
                        }
                        if (this._sectionGroups != null)
                        {
                            this._sectionGroups.Remove(id2.ConfigKey);
                        }
                        this.RemovedSectionGroups.Add(id2.ConfigKey, id2.ConfigKey);
                    }
                }
            }
        }

        internal void RemoveLocationWriteRequirement()
        {
            if (base.IsLocationConfig)
            {
                this._flags[0x1000000] = false;
                this._flags[0x2000000] = true;
            }
        }

        internal void RevertToParent(ConfigurationSection configSection)
        {
            configSection.SectionInformation.RawXml = null;
            try
            {
                ConfigurationSection parentElement = this.FindImmediateParentSection(configSection);
                configSection.Reset(parentElement);
                configSection.ResetModified();
            }
            catch (Exception exception)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_exception_in_config_section_handler", new object[] { configSection.SectionInformation.SectionName }), exception, base.ConfigStreamInfo.StreamName, 0);
            }
            configSection.SectionInformation.Removed = true;
        }

        internal void SaveAs(string filename, ConfigurationSaveMode saveMode, bool forceUpdateAll)
        {
            ConfigDefinitionUpdates updates2;
            ArrayList list;
            SectionUpdates configDeclarationUpdates = this.GetConfigDeclarationUpdates(saveMode, forceUpdateAll);
            bool flag = false;
            bool requireUpdates = filename != null;
            this.GetConfigDefinitionUpdates(requireUpdates, saveMode, forceUpdateAll, out updates2, out list);
            if (filename != null)
            {
                if (!base.Host.IsRemote && this._streamInfoUpdates.Contains(filename))
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Filename_in_SaveAs_is_used_already", new object[] { filename }));
                }
                if (string.IsNullOrEmpty(base.ConfigStreamInfo.StreamName))
                {
                    StreamInfo info = new StreamInfo(null, null, filename);
                    this._streamInfoUpdates.Add(filename, info);
                    base.ConfigStreamInfo.StreamName = filename;
                    base.ConfigStreamInfo.StreamVersion = base.MonitorStream(null, null, base.ConfigStreamInfo.StreamName);
                }
                this.UpdateConfigHost.AddStreamname(base.ConfigStreamInfo.StreamName, filename, base.Host.IsRemote);
                foreach (StreamInfo info2 in this._streamInfoUpdates.Values)
                {
                    if (!string.IsNullOrEmpty(info2.SectionName))
                    {
                        string newStreamname = InternalConfigHost.StaticGetStreamNameForConfigSource(filename, info2.ConfigSource);
                        this.UpdateConfigHost.AddStreamname(info2.StreamName, newStreamname, base.Host.IsRemote);
                    }
                }
            }
            if (!requireUpdates)
            {
                requireUpdates = this.RecordItselfRequiresUpdates;
            }
            if (((configDeclarationUpdates != null) || (updates2 != null)) || requireUpdates)
            {
                byte[] buffer = null;
                Encoding currentEncoding = null;
                if (base.ConfigStreamInfo.HasStream)
                {
                    using (Stream stream = base.Host.OpenStreamForRead(base.ConfigStreamInfo.StreamName))
                    {
                        if (stream == null)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_file_has_changed"), base.ConfigStreamInfo.StreamName, 0);
                        }
                        buffer = new byte[stream.Length];
                        if (stream.Read(buffer, 0, (int) stream.Length) != stream.Length)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_data_read_count_mismatch"));
                        }
                    }
                    try
                    {
                        using (StreamReader reader = new StreamReader(base.ConfigStreamInfo.StreamName))
                        {
                            if (reader.Peek() >= 0)
                            {
                                reader.Read();
                            }
                            if (reader.CurrentEncoding is UnicodeEncoding)
                            {
                                currentEncoding = reader.CurrentEncoding;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                string str2 = base.FindChangedConfigurationStream();
                if (str2 != null)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_file_has_changed"), str2, 0);
                }
                flag = true;
                object writeContext = null;
                bool flag3 = false;
                try
                {
                    try
                    {
                        using (Stream stream2 = base.Host.OpenStreamForWrite(base.ConfigStreamInfo.StreamName, null, ref writeContext))
                        {
                            flag3 = true;
                            using (StreamWriter writer = (currentEncoding == null) ? new StreamWriter(stream2) : new StreamWriter(stream2, currentEncoding))
                            {
                                XmlUtilWriter utilWriter = new XmlUtilWriter(writer, true);
                                if (base.ConfigStreamInfo.HasStream)
                                {
                                    this.CopyConfig(configDeclarationUpdates, updates2, buffer, base.ConfigStreamInfo.StreamName, this.NamespaceChangeNeeded, utilWriter);
                                }
                                else
                                {
                                    this.CreateNewConfig(configDeclarationUpdates, updates2, this.NamespaceChangeNeeded, utilWriter);
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (flag3)
                        {
                            base.Host.WriteCompleted(base.ConfigStreamInfo.StreamName, false, writeContext);
                        }
                        throw;
                    }
                }
                catch (Exception exception)
                {
                    throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_error_loading_XML_file"), exception, base.ConfigStreamInfo.StreamName, 0);
                }
                base.Host.WriteCompleted(base.ConfigStreamInfo.StreamName, true, writeContext);
                base.ConfigStreamInfo.HasStream = true;
                base.ConfigStreamInfo.ClearStreamInfos();
                base.ConfigStreamInfo.StreamVersion = base.MonitorStream(null, null, base.ConfigStreamInfo.StreamName);
            }
            if (list != null)
            {
                if (!flag)
                {
                    string str3 = base.FindChangedConfigurationStream();
                    if (str3 != null)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_file_has_changed"), str3, 0);
                    }
                }
                foreach (DefinitionUpdate update in list)
                {
                    this.SaveConfigSource(update);
                }
            }
            this.UpdateRecords();
        }

        private void SaveConfigSource(DefinitionUpdate update)
        {
            string configSourceStreamName;
            if (update.SectionRecord.HasResult)
            {
                ConfigurationSection result = (ConfigurationSection) update.SectionRecord.Result;
                configSourceStreamName = result.SectionInformation.ConfigSourceStreamName;
            }
            else
            {
                configSourceStreamName = update.SectionRecord.FileInput.SectionXmlInfo.ConfigSourceStreamName;
            }
            byte[] buffer = null;
            using (Stream stream = base.Host.OpenStreamForRead(configSourceStreamName))
            {
                if (stream != null)
                {
                    buffer = new byte[stream.Length];
                    if (stream.Read(buffer, 0, (int) stream.Length) != stream.Length)
                    {
                        throw new ConfigurationErrorsException();
                    }
                }
            }
            bool flag = buffer != null;
            object writeContext = null;
            bool flag2 = false;
            try
            {
                try
                {
                    string streamName;
                    if (base.Host.IsRemote)
                    {
                        streamName = null;
                    }
                    else
                    {
                        streamName = base.ConfigStreamInfo.StreamName;
                    }
                    using (Stream stream2 = base.Host.OpenStreamForWrite(configSourceStreamName, streamName, ref writeContext))
                    {
                        flag2 = true;
                        if (update.UpdatedXml == null)
                        {
                            if (flag)
                            {
                                stream2.Write(buffer, 0, buffer.Length);
                            }
                        }
                        else
                        {
                            using (StreamWriter writer = new StreamWriter(stream2))
                            {
                                XmlUtilWriter utilWriter = new XmlUtilWriter(writer, true);
                                if (flag)
                                {
                                    this.CopyConfigSource(utilWriter, update.UpdatedXml, configSourceStreamName, buffer);
                                }
                                else
                                {
                                    this.CreateNewConfigSource(utilWriter, update.UpdatedXml, 4);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    if (flag2)
                    {
                        base.Host.WriteCompleted(configSourceStreamName, false, writeContext);
                    }
                    throw;
                }
            }
            catch (Exception exception)
            {
                throw ExceptionUtil.WrapAsConfigException(System.Configuration.SR.GetString("Config_error_loading_XML_file"), exception, configSourceStreamName, 0);
            }
            base.Host.WriteCompleted(configSourceStreamName, true, writeContext);
        }

        internal void SetRawXml(ConfigurationSection configSection, string xmlElement)
        {
            if (string.IsNullOrEmpty(xmlElement))
            {
                this.RevertToParent(configSection);
            }
            else
            {
                this.ValidateSectionXml(xmlElement, configSection.SectionInformation.Name);
                ConfigurationSection parentElement = this.FindImmediateParentSection(configSection);
                ConfigXmlReader reader = new ConfigXmlReader(xmlElement, null, 0);
                configSection.SectionInformation.RawXml = xmlElement;
                try
                {
                    try
                    {
                        bool elementPresent = configSection.ElementPresent;
                        PropertySourceInfo sourceInformation = configSection.ElementInformation.PropertyInfoInternal();
                        configSection.Reset(parentElement);
                        configSection.DeserializeSection(reader);
                        configSection.ResetModified();
                        configSection.ElementPresent = elementPresent;
                        configSection.ElementInformation.ChangeSourceAndLineNumber(sourceInformation);
                    }
                    catch
                    {
                        configSection.SectionInformation.RawXml = null;
                        throw;
                    }
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_exception_in_config_section_handler", new object[] { configSection.SectionInformation.SectionName }), exception, null, 0);
                }
                configSection.SectionInformation.Removed = false;
            }
        }

        private int UpdateIndent(int oldIndent, XmlUtil xmlUtil, XmlUtilWriter utilWriter, int parentLinePosition)
        {
            int num = oldIndent;
            if ((xmlUtil.Reader.NodeType == XmlNodeType.Element) && utilWriter.IsLastLineBlank)
            {
                int trueLinePosition = xmlUtil.TrueLinePosition;
                if ((parentLinePosition < trueLinePosition) && (trueLinePosition <= (parentLinePosition + 10)))
                {
                    num = trueLinePosition - parentLinePosition;
                }
            }
            return num;
        }

        private void UpdateRecords()
        {
            if (base._factoryRecords != null)
            {
                foreach (FactoryRecord record in base._factoryRecords.Values)
                {
                    if (string.IsNullOrEmpty(record.Filename))
                    {
                        record.Filename = base.ConfigStreamInfo.StreamName;
                    }
                    record.LineNumber = 0;
                    ConfigurationSection configSection = this.GetConfigSection(record.ConfigKey);
                    if (configSection != null)
                    {
                        if (configSection.SectionInformation.Type != null)
                        {
                            record.FactoryTypeName = configSection.SectionInformation.Type;
                        }
                        record.AllowLocation = configSection.SectionInformation.AllowLocation;
                        record.RestartOnExternalChanges = configSection.SectionInformation.RestartOnExternalChanges;
                        record.RequirePermission = configSection.SectionInformation.RequirePermission;
                        record.AllowDefinition = configSection.SectionInformation.AllowDefinition;
                        record.AllowExeDefinition = configSection.SectionInformation.AllowExeDefinition;
                    }
                }
            }
            if (base._sectionRecords != null)
            {
                string definitionConfigPath = base.IsLocationConfig ? base._parent.ConfigPath : base.ConfigPath;
                foreach (SectionRecord record2 in base._sectionRecords.Values)
                {
                    string configSource;
                    string configSourceStreamName;
                    object obj2;
                    ConfigurationSection result;
                    if (record2.HasResult)
                    {
                        result = (ConfigurationSection) record2.Result;
                        configSource = result.SectionInformation.ConfigSource;
                        if (string.IsNullOrEmpty(configSource))
                        {
                            configSource = null;
                        }
                        configSourceStreamName = result.SectionInformation.ConfigSourceStreamName;
                        if (string.IsNullOrEmpty(configSourceStreamName))
                        {
                            configSourceStreamName = null;
                        }
                    }
                    else
                    {
                        result = null;
                        configSource = null;
                        configSourceStreamName = null;
                        if (record2.HasFileInput)
                        {
                            SectionXmlInfo sectionXmlInfo = record2.FileInput.SectionXmlInfo;
                            configSource = sectionXmlInfo.ConfigSource;
                            configSourceStreamName = sectionXmlInfo.ConfigSourceStreamName;
                        }
                    }
                    if (!string.IsNullOrEmpty(configSource))
                    {
                        obj2 = base.MonitorStream(record2.ConfigKey, configSource, configSourceStreamName);
                    }
                    else
                    {
                        obj2 = null;
                    }
                    if (!record2.HasResult)
                    {
                        if (record2.HasFileInput)
                        {
                            SectionXmlInfo info2 = record2.FileInput.SectionXmlInfo;
                            info2.StreamVersion = base.ConfigStreamInfo.StreamVersion;
                            info2.ConfigSourceStreamVersion = obj2;
                        }
                    }
                    else
                    {
                        result.SectionInformation.RawXml = null;
                        bool addUpdate = record2.AddUpdate;
                        record2.AddUpdate = false;
                        if (addUpdate)
                        {
                            SectionInput fileInput = record2.FileInput;
                            if (fileInput == null)
                            {
                                SectionXmlInfo info3 = new SectionXmlInfo(record2.ConfigKey, definitionConfigPath, base._configPath, base._locationSubPath, base.ConfigStreamInfo.StreamName, 0, base.ConfigStreamInfo.StreamVersion, null, configSource, configSourceStreamName, obj2, result.SectionInformation.ProtectionProviderName, result.SectionInformation.OverrideModeSetting, !result.SectionInformation.InheritInChildApplications);
                                fileInput = new SectionInput(info3, null) {
                                    Result = result,
                                    ResultRuntimeObject = result
                                };
                                record2.AddFileInput(fileInput);
                            }
                            else
                            {
                                SectionXmlInfo info4 = fileInput.SectionXmlInfo;
                                info4.LineNumber = 0;
                                info4.StreamVersion = base.ConfigStreamInfo.StreamVersion;
                                info4.RawXml = null;
                                info4.ConfigSource = configSource;
                                info4.ConfigSourceStreamName = configSourceStreamName;
                                info4.ConfigSourceStreamVersion = obj2;
                                info4.ProtectionProviderName = result.SectionInformation.ProtectionProviderName;
                                info4.OverrideModeSetting = result.SectionInformation.OverrideModeSetting;
                                info4.SkipInChildApps = !result.SectionInformation.InheritInChildApplications;
                            }
                            fileInput.ProtectionProvider = result.SectionInformation.ProtectionProvider;
                        }
                        try
                        {
                            result.ResetModified();
                        }
                        catch (Exception exception)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_exception_in_config_section_handler", new object[] { record2.ConfigKey }), exception, base.ConfigStreamInfo.StreamName, 0);
                        }
                    }
                }
            }
            foreach (StreamInfo info5 in this._streamInfoUpdates.Values)
            {
                if (!base.ConfigStreamInfo.StreamInfos.Contains(info5.StreamName))
                {
                    base.MonitorStream(info5.SectionName, info5.ConfigSource, info5.StreamName);
                }
            }
            this.InitStreamInfoUpdates();
            this._flags[0x200] = this._flags[0x4000000];
            this._flags[0x1000000] = false;
            this._flags[0x2000000] = false;
            if ((!base.IsLocationConfig && (base._locationSections != null)) && ((this._removedSections != null) && (this._removedSections.Count > 0)))
            {
                int index = 0;
                while (index < base._locationSections.Count)
                {
                    LocationSectionRecord record3 = (LocationSectionRecord) base._locationSections[index];
                    if (this._removedSections.Contains(record3.ConfigKey))
                    {
                        base._locationSections.RemoveAt(index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }
            this._removedSections = null;
            this._removedSectionGroups = null;
        }

        protected override object UseParentResult(string configKey, object parentResult, SectionRecord sectionRecord)
        {
            FactoryRecord factoryRecord = base.FindFactoryRecord(configKey, false);
            if (factoryRecord == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_unrecognized_configuration_section", new object[] { configKey }));
            }
            return base.CallCreateSection(false, factoryRecord, sectionRecord, parentResult, null, null, -1);
        }

        private void ValidateSectionXml(string xmlElement, string configKey)
        {
            if (!string.IsNullOrEmpty(xmlElement))
            {
                XmlTextReader reader = null;
                try
                {
                    string str;
                    string str2;
                    XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.Default, Encoding.Unicode);
                    reader = new XmlTextReader(xmlElement, XmlNodeType.Element, context);
                    reader.Read();
                    if (reader.NodeType != XmlNodeType.Element)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_unexpected_node_type", new object[] { reader.NodeType }));
                    }
                    BaseConfigurationRecord.SplitConfigKey(configKey, out str, out str2);
                    if (reader.Name != str2)
                    {
                        throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_unexpected_element_name", new object[] { reader.Name }));
                    }
                Label_0098:
                    if (!reader.Read())
                    {
                        if (reader.Depth != 0)
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_unexpected_element_end"), reader);
                        }
                    }
                    else
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.DocumentType:
                            case XmlNodeType.XmlDeclaration:
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_invalid_node_type"), reader);
                        }
                        if ((reader.Depth <= 0) && (reader.NodeType != XmlNodeType.EndElement))
                        {
                            throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_more_data_than_expected"), reader);
                        }
                        goto Label_0098;
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
        }

        private string WriteEmptyElement(string ElementName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<');
            builder.Append(ElementName);
            builder.Append(" />");
            return builder.ToString();
        }

        private void WriteNewConfigDeclarations(SectionUpdates declarationUpdates, XmlUtilWriter utilWriter, int linePosition, int indent, bool skipFirstIndent)
        {
            if (!skipFirstIndent)
            {
                utilWriter.AppendSpacesToLinePosition(linePosition);
            }
            utilWriter.Write("<configSections>\r\n");
            this.WriteUnwrittenConfigDeclarations(declarationUpdates, utilWriter, linePosition + indent, indent, false);
            utilWriter.AppendSpacesToLinePosition(linePosition);
            utilWriter.Write("</configSections>\r\n");
            if (skipFirstIndent)
            {
                utilWriter.AppendSpacesToLinePosition(linePosition);
            }
        }

        private void WriteNewConfigDefinitions(ConfigDefinitionUpdates configDefinitionUpdates, XmlUtilWriter utilWriter, int linePosition, int indent)
        {
            if (configDefinitionUpdates != null)
            {
                foreach (LocationUpdates updates in configDefinitionUpdates.LocationUpdatesList)
                {
                    SectionUpdates sectionUpdates = updates.SectionUpdates;
                    if (!sectionUpdates.IsEmpty && sectionUpdates.IsNew)
                    {
                        configDefinitionUpdates.FlagLocationWritten();
                        bool flag = (base._locationSubPath != null) || !updates.IsDefault;
                        int num = linePosition;
                        utilWriter.AppendSpacesToLinePosition(linePosition);
                        if (flag)
                        {
                            if (base._locationSubPath == null)
                            {
                                utilWriter.Write(string.Format(CultureInfo.InvariantCulture, "<location {0} inheritInChildApplications=\"{1}\">\r\n", new object[] { updates.OverrideMode.LocationTagXmlString, BoolToString(updates.InheritInChildApps) }));
                            }
                            else
                            {
                                utilWriter.Write(string.Format(CultureInfo.InvariantCulture, "<location path=\"{2}\" {0} inheritInChildApplications=\"{1}\">\r\n", new object[] { updates.OverrideMode.LocationTagXmlString, BoolToString(updates.InheritInChildApps), base._locationSubPath }));
                            }
                            num += indent;
                            utilWriter.AppendSpacesToLinePosition(num);
                        }
                        this.WriteNewConfigDefinitionsRecursive(utilWriter, updates.SectionUpdates, num, indent, true);
                        if (flag)
                        {
                            utilWriter.AppendSpacesToLinePosition(linePosition);
                            utilWriter.Write("</location>");
                            utilWriter.AppendNewLine();
                        }
                    }
                }
                if (configDefinitionUpdates.RequireLocation)
                {
                    configDefinitionUpdates.FlagLocationWritten();
                    utilWriter.AppendSpacesToLinePosition(linePosition);
                    utilWriter.Write(string.Format(CultureInfo.InvariantCulture, "<location path=\"{2}\" {0} inheritInChildApplications=\"{1}\">\r\n", new object[] { OverrideModeSetting.LocationDefault.LocationTagXmlString, "true", base._locationSubPath }));
                    utilWriter.AppendSpacesToLinePosition(linePosition);
                    utilWriter.Write("</location>");
                    utilWriter.AppendNewLine();
                }
            }
        }

        private bool WriteNewConfigDefinitionsRecursive(XmlUtilWriter utilWriter, SectionUpdates sectionUpdates, int linePosition, int indent, bool skipFirstIndent)
        {
            bool flag = false;
            string[] movedSectionNames = sectionUpdates.GetMovedSectionNames();
            if (movedSectionNames != null)
            {
                flag = true;
                foreach (string str in movedSectionNames)
                {
                    DefinitionUpdate definitionUpdate = sectionUpdates.GetDefinitionUpdate(str);
                    this.WriteSectionUpdate(utilWriter, definitionUpdate, linePosition, indent, skipFirstIndent);
                    utilWriter.AppendNewLine();
                    skipFirstIndent = false;
                }
            }
            string[] newGroupNames = sectionUpdates.GetNewGroupNames();
            if (newGroupNames != null)
            {
                foreach (string str2 in newGroupNames)
                {
                    if (base.TargetFramework != null)
                    {
                        ConfigurationSectionGroup sectionGroup = this.GetSectionGroup(str2);
                        if ((sectionGroup != null) && !sectionGroup.ShouldSerializeSectionGroupInTargetVersion(base.TargetFramework))
                        {
                            sectionUpdates.MarkGroupAsRetrieved(str2);
                            continue;
                        }
                    }
                    if (!skipFirstIndent)
                    {
                        utilWriter.AppendSpacesToLinePosition(linePosition);
                    }
                    skipFirstIndent = false;
                    utilWriter.Write("<" + str2 + ">\r\n");
                    if (this.WriteNewConfigDefinitionsRecursive(utilWriter, sectionUpdates.GetSectionUpdatesForGroup(str2), linePosition + indent, indent, false))
                    {
                        flag = true;
                    }
                    utilWriter.AppendSpacesToLinePosition(linePosition);
                    utilWriter.Write("</" + str2 + ">\r\n");
                }
            }
            sectionUpdates.IsNew = false;
            return flag;
        }

        private void WriteSectionUpdate(XmlUtilWriter utilWriter, DefinitionUpdate update, int linePosition, int indent, bool skipFirstIndent)
        {
            string updatedXml;
            ConfigurationSection result = (ConfigurationSection) update.SectionRecord.Result;
            if (!string.IsNullOrEmpty(result.SectionInformation.ConfigSource))
            {
                updatedXml = string.Format(CultureInfo.InvariantCulture, "<{0} configSource=\"{1}\" />", new object[] { result.SectionInformation.Name, result.SectionInformation.ConfigSource });
            }
            else
            {
                updatedXml = update.UpdatedXml;
            }
            string s = XmlUtil.FormatXmlElement(updatedXml, linePosition, indent, skipFirstIndent);
            utilWriter.Write(s);
        }

        private void WriteUnwrittenConfigDeclarations(SectionUpdates declarationUpdates, XmlUtilWriter utilWriter, int linePosition, int indent, bool skipFirstIndent)
        {
            this.WriteUnwrittenConfigDeclarationsRecursive(declarationUpdates, utilWriter, linePosition, indent, skipFirstIndent);
        }

        private void WriteUnwrittenConfigDeclarationsRecursive(SectionUpdates declarationUpdates, XmlUtilWriter utilWriter, int linePosition, int indent, bool skipFirstIndent)
        {
            string[] unretrievedSectionNames = declarationUpdates.GetUnretrievedSectionNames();
            if (unretrievedSectionNames != null)
            {
                foreach (string str in unretrievedSectionNames)
                {
                    if (!skipFirstIndent)
                    {
                        utilWriter.AppendSpacesToLinePosition(linePosition);
                    }
                    skipFirstIndent = false;
                    DeclarationUpdate declarationUpdate = declarationUpdates.GetDeclarationUpdate(str);
                    if ((declarationUpdate != null) && !string.IsNullOrEmpty(declarationUpdate.UpdatedXml))
                    {
                        utilWriter.Write(declarationUpdate.UpdatedXml);
                        utilWriter.AppendNewLine();
                    }
                }
            }
            string[] unretrievedGroupNames = declarationUpdates.GetUnretrievedGroupNames();
            if (unretrievedGroupNames != null)
            {
                foreach (string str2 in unretrievedGroupNames)
                {
                    if (base.TargetFramework != null)
                    {
                        ConfigurationSectionGroup sectionGroup = this.GetSectionGroup(str2);
                        if ((sectionGroup != null) && !sectionGroup.ShouldSerializeSectionGroupInTargetVersion(base.TargetFramework))
                        {
                            declarationUpdates.MarkGroupAsRetrieved(str2);
                            continue;
                        }
                    }
                    if (!skipFirstIndent)
                    {
                        utilWriter.AppendSpacesToLinePosition(linePosition);
                    }
                    skipFirstIndent = false;
                    SectionUpdates sectionUpdatesForGroup = declarationUpdates.GetSectionUpdatesForGroup(str2);
                    DeclarationUpdate sectionGroupUpdate = sectionUpdatesForGroup.GetSectionGroupUpdate();
                    if (sectionGroupUpdate == null)
                    {
                        utilWriter.Write("<sectionGroup name=\"" + str2 + "\">");
                    }
                    else
                    {
                        utilWriter.Write(sectionGroupUpdate.UpdatedXml);
                    }
                    utilWriter.AppendNewLine();
                    this.WriteUnwrittenConfigDeclarationsRecursive(sectionUpdatesForGroup, utilWriter, linePosition + indent, indent, false);
                    utilWriter.AppendSpacesToLinePosition(linePosition);
                    utilWriter.Write("</sectionGroup>\r\n");
                }
            }
        }

        protected override SimpleBitVector32 ClassFlags
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return MgmtClassFlags;
            }
        }

        internal string ConfigurationFilePath
        {
            get
            {
                string newStreamname = this.UpdateConfigHost.GetNewStreamname(base.ConfigStreamInfo.StreamName);
                if (newStreamname == null)
                {
                    newStreamname = string.Empty;
                }
                if (!string.IsNullOrEmpty(newStreamname))
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, newStreamname).Demand();
                }
                return newStreamname;
            }
        }

        private bool HasRemovedSections
        {
            get
            {
                return ((this._removedSections != null) && (this._removedSections.Count > 0));
            }
        }

        private bool HasRemovedSectionsOrGroups
        {
            get
            {
                return (((this._removedSections != null) && (this._removedSections.Count > 0)) || ((this._removedSectionGroups != null) && (this._removedSectionGroups.Count > 0)));
            }
        }

        private MgmtConfigurationRecord MgmtParent
        {
            get
            {
                return (MgmtConfigurationRecord) base._parent;
            }
        }

        private NamespaceChange NamespaceChangeNeeded
        {
            get
            {
                if (this._flags[0x4000000] == this._flags[0x200])
                {
                    return NamespaceChange.None;
                }
                if (this._flags[0x4000000])
                {
                    return NamespaceChange.Add;
                }
                return NamespaceChange.Remove;
            }
        }

        internal bool NamespacePresent
        {
            get
            {
                return this._flags[0x4000000];
            }
            set
            {
                this._flags[0x4000000] = value;
            }
        }

        private bool RecordItselfRequiresUpdates
        {
            get
            {
                return (this.NamespaceChangeNeeded != NamespaceChange.None);
            }
        }

        private Hashtable RemovedSectionGroups
        {
            get
            {
                if (this._removedSectionGroups == null)
                {
                    this._removedSectionGroups = new Hashtable();
                }
                return this._removedSectionGroups;
            }
        }

        private Hashtable RemovedSections
        {
            get
            {
                if (this._removedSections == null)
                {
                    this._removedSections = new Hashtable();
                }
                return this._removedSections;
            }
        }

        internal Hashtable SectionFactories
        {
            get
            {
                if (this._sectionFactories == null)
                {
                    this._sectionFactories = this.GetAllFactories(false);
                }
                return this._sectionFactories;
            }
        }

        internal Hashtable SectionGroupFactories
        {
            get
            {
                if (this._sectionGroupFactories == null)
                {
                    this._sectionGroupFactories = this.GetAllFactories(true);
                }
                return this._sectionGroupFactories;
            }
        }

        private Hashtable SectionGroups
        {
            get
            {
                if (this._sectionGroups == null)
                {
                    this._sectionGroups = new Hashtable();
                }
                return this._sectionGroups;
            }
        }

        private System.Configuration.UpdateConfigHost UpdateConfigHost
        {
            get
            {
                return (System.Configuration.UpdateConfigHost) base.Host;
            }
        }
    }
}

