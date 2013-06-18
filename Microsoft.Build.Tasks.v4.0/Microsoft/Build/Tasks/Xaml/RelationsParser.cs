namespace Microsoft.Build.Tasks.Xaml
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    internal class RelationsParser
    {
        private const string argumentProperty = "ARGUMENTPARAMETER";
        private const string argumentRequiredProperty = "REQUIRED";
        private const string argumentType = "ARGUMENT";
        private const string argumentValueName = "ARGUMENTVALUE";
        private string baseClass = "DataDrivenToolTask";
        private const string baseClassAttribute = "BASECLASS";
        private const string categoryProperty = "CATEGORY";
        private const string conflictsType = "CONFLICTS";
        private string defaultPrefix = "/";
        private const string defaultProperty = "DEFAULT";
        private LinkedList<Property> defaultSet = new LinkedList<Property>();
        private const string descriptionProperty = "DESCRIPTION";
        private const string displayNameProperty = "DISPLAYNAME";
        private const string enumType = "VALUE";
        private int errorCount;
        private LinkedList<string> errorLog = new LinkedList<string>();
        private const string excludedPlatformType = "EXCLUDEDPLATFORM";
        private const string externalConflictsType = "EXTERNALCONFLICTS";
        private const string externalOverridesType = "EXTERNALOVERRIDES";
        private const string externalRequiresType = "EXTERNALREQUIRES";
        private const string fallbackProperty = "FALLBACKARGUMENTPARAMETER";
        private Dictionary<string, string> fallbackSet = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private const string falseProperty = "FALSE";
        private const string importType = "IMPORT";
        private const string includedPlatformType = "INCLUDEDPLATFORM";
        private bool isImport;
        private const string maxProperty = "MAX";
        private const string minProperty = "MIN";
        private string name;
        private const string nameProperty = "NAME";
        private const string namespaceAttribute = "NAMESPACE";
        private string namespaceValue = "MyDataDrivenTasks";
        private const string oldName = "OLDNAME";
        private const string outputProperty = "OUTPUT";
        private const string overridesType = "OVERRIDES";
        private const string parameterGroupType = "PARAMETERGROUP";
        private const string parameterType = "PARAMETER";
        private const string prefixString = "PREFIX";
        private LinkedList<Property> properties = new LinkedList<Property>();
        private const string propertyRequiredProperty = "REQUIRED";
        private const string relations = "RELATIONS";
        private const string requiresType = "REQUIRES";
        private const string resourceNamespaceAttribute = "RESOURCENAMESPACE";
        private string resourceNamespaceValue;
        private const string reverseSwitchName = "REVERSESWITCH";
        private const string reversibleProperty = "REVERSIBLE";
        private const string separatorProperty = "SEPARATOR";
        private const string status = "STATUS";
        private const string switchAttribute = "SWITCH";
        private const string switchGroupType = "SWITCHGROUP";
        private const string switchName = "SWITCH";
        private Dictionary<string, SwitchRelations> switchRelationsList = new Dictionary<string, SwitchRelations>(StringComparer.OrdinalIgnoreCase);
        private const string switchType = "SWITCH";
        private const string task = "TASK";
        private const string tasksAttribute = "TASKS";
        private const string toolAttribute = "TOOL";
        private string toolName;
        private const string toolNameString = "TOOLNAME";
        private const string trueProperty = "TRUE";
        private const string typeAlways = "ALWAYS";
        private const string typeProperty = "TYPE";
        private const string xmlNamespace = "http://schemas.microsoft.com/developer/msbuild/tasks/2005";

        private static bool IsXmlRootElement(XmlNode node)
        {
            return ((((node.NodeType != XmlNodeType.Comment) && (node.NodeType != XmlNodeType.Whitespace)) && ((node.NodeType != XmlNodeType.XmlDeclaration) && (node.NodeType != XmlNodeType.ProcessingInstruction))) && (node.NodeType != XmlNodeType.DocumentType));
        }

        private XmlDocument LoadFile(string fileName)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(fileName);
                return document;
            }
            catch (FileNotFoundException exception)
            {
                this.LogError("LoadFailed", new object[] { exception.ToString() });
                return null;
            }
            catch (XmlException exception2)
            {
                this.LogError("XmlError", new object[] { exception2.ToString() });
                return null;
            }
        }

        internal XmlDocument LoadXml(string xml)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);
                return document;
            }
            catch (XmlException exception)
            {
                this.LogError("XmlError", new object[] { exception.ToString() });
                return null;
            }
        }

        private void LogError(string messageResourceName, params object[] messageArgs)
        {
            this.errorLog.AddLast(Microsoft.Build.Shared.ResourceUtilities.FormatResourceString(messageResourceName, messageArgs));
            this.errorCount++;
        }

        private SwitchRelations ObtainAttributes(XmlNode node, SwitchRelations switchGroup)
        {
            SwitchRelations relations;
            if (switchGroup != null)
            {
                relations = switchGroup.Clone();
            }
            else
            {
                relations = new SwitchRelations();
            }
            foreach (XmlAttribute attribute in node.Attributes)
            {
                string str = attribute.Name.ToUpperInvariant();
                if (str != null)
                {
                    if (!(str == "NAME"))
                    {
                        if (str == "STATUS")
                        {
                            goto Label_0065;
                        }
                    }
                    else
                    {
                        relations.SwitchValue = attribute.InnerText;
                    }
                }
                continue;
            Label_0065:
                relations.Status = attribute.InnerText;
            }
            return relations;
        }

        private bool ParseImportOption(XmlNode node)
        {
            if (!VerifyAttributeExists(node, "TASKS"))
            {
                this.LogError("MissingAttribute", new object[] { "IMPORT", "TASKS" });
                return false;
            }
            string[] strArray = null;
            foreach (XmlAttribute attribute in node.Attributes)
            {
                if (string.Equals(attribute.Name, "TASKS", StringComparison.OrdinalIgnoreCase))
                {
                    strArray = attribute.InnerText.Split(new char[] { ';' });
                }
            }
            this.isImport = true;
            foreach (string str in strArray)
            {
                if (!this.ParseXmlDocument(str))
                {
                    return false;
                }
            }
            this.isImport = false;
            return true;
        }

        private bool ParseSwitch(XmlNode node, Dictionary<string, SwitchRelations> switchRelationsList, SwitchRelations switchRelations)
        {
            SwitchRelations relations = this.ObtainAttributes(node, switchRelations);
            if ((relations.SwitchValue == null) || (relations.SwitchValue == string.Empty))
            {
                return false;
            }
            if (!switchRelationsList.ContainsKey(relations.SwitchValue))
            {
                switchRelationsList.Remove(relations.SwitchValue);
            }
            for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    if (string.Equals(node2.Name, "REQUIRES", StringComparison.OrdinalIgnoreCase))
                    {
                        string key = string.Empty;
                        string item = string.Empty;
                        bool flag = false;
                        foreach (XmlAttribute attribute in node2.Attributes)
                        {
                            string str3 = attribute.Name.ToUpperInvariant();
                            if (str3 == null)
                            {
                                goto Label_00FE;
                            }
                            if (!(str3 == "NAME"))
                            {
                                if (str3 == "TOOL")
                                {
                                    goto Label_00E7;
                                }
                                if (str3 == "SWITCH")
                                {
                                    goto Label_00F4;
                                }
                                goto Label_00FE;
                            }
                            string innerText = attribute.InnerText;
                            continue;
                        Label_00E7:
                            flag = true;
                            key = attribute.InnerText;
                            continue;
                        Label_00F4:
                            item = attribute.InnerText;
                            continue;
                        Label_00FE:
                            return false;
                        }
                        if (!flag)
                        {
                            if (item == string.Empty)
                            {
                                return false;
                            }
                            relations.Requires.Add(item);
                        }
                        else if (!relations.ExternalRequires.ContainsKey(key))
                        {
                            List<string> list = new List<string> {
                                item
                            };
                            relations.ExternalRequires.Add(key, list);
                        }
                        else
                        {
                            relations.ExternalRequires[key].Add(item);
                        }
                    }
                    else if (string.Equals(node2.Name, "INCLUDEDPLATFORM", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlAttribute attribute2 in node2.Attributes)
                        {
                            string str4;
                            if (((str4 = attribute2.Name.ToUpperInvariant()) != null) && (str4 == "NAME"))
                            {
                                relations.IncludedPlatforms.Add(attribute2.InnerText);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else if (string.Equals(node2.Name, "EXCLUDEDPLATFORM", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlAttribute attribute3 in node2.Attributes)
                        {
                            string str5;
                            if (((str5 = attribute3.Name.ToUpperInvariant()) != null) && (str5 == "NAME"))
                            {
                                relations.ExcludedPlatforms.Add(attribute3.InnerText);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else if (string.Equals(node2.Name, "OVERRIDES", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlAttribute attribute4 in node2.Attributes)
                        {
                            string str6 = attribute4.Name.ToUpperInvariant();
                            if (str6 != null)
                            {
                                if (!(str6 == "SWITCH"))
                                {
                                    if (str6 == "ARGUMENTVALUE")
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    relations.Overrides.Add(attribute4.InnerText);
                                    continue;
                                }
                            }
                            return false;
                        }
                    }
                }
            }
            switchRelationsList.Add(relations.SwitchValue, relations);
            return true;
        }

        private bool ParseSwitchGroupOrSwitch(XmlNode node, Dictionary<string, SwitchRelations> switchRelationsList, SwitchRelations switchRelations)
        {
            while (node != null)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    if (string.Equals(node.Name, "SWITCHGROUP", StringComparison.OrdinalIgnoreCase))
                    {
                        SwitchRelations relations = this.ObtainAttributes(node, switchRelations);
                        if (!this.ParseSwitchGroupOrSwitch(node.FirstChild, switchRelationsList, relations))
                        {
                            return false;
                        }
                    }
                    else if (string.Equals(node.Name, "SWITCH", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!this.ParseSwitch(node, switchRelationsList, switchRelations))
                        {
                            return false;
                        }
                    }
                    else if (string.Equals(node.Name, "IMPORT", StringComparison.OrdinalIgnoreCase) && !this.ParseImportOption(node))
                    {
                        return false;
                    }
                }
                node = node.NextSibling;
            }
            return true;
        }

        public bool ParseXmlDocument(string fileName)
        {
            XmlDocument xmlDocument = this.LoadFile(fileName);
            return ((xmlDocument != null) && this.ParseXmlDocument(xmlDocument));
        }

        internal bool ParseXmlDocument(XmlDocument xmlDocument)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(xmlDocument != null, "NoXml");
            XmlNode firstChild = xmlDocument.FirstChild;
            while (!IsXmlRootElement(firstChild))
            {
                firstChild = firstChild.NextSibling;
            }
            if (string.IsNullOrEmpty(firstChild.NamespaceURI) || !string.Equals(firstChild.NamespaceURI, "http://schemas.microsoft.com/developer/msbuild/tasks/2005", StringComparison.OrdinalIgnoreCase))
            {
                this.LogError("InvalidNamespace", new object[] { "http://schemas.microsoft.com/developer/msbuild/tasks/2005" });
                return false;
            }
            if (!VerifyNodeName(firstChild))
            {
                this.LogError("MissingRootElement", new object[] { "RELATIONS" });
                return false;
            }
            if (!VerifyAttributeExists(firstChild, "NAME") && !this.isImport)
            {
                this.LogError("MissingAttribute", new object[] { "TASK", "NAME" });
                return false;
            }
            foreach (XmlAttribute attribute in firstChild.Attributes)
            {
                if (string.Equals(attribute.Name, "PREFIX", StringComparison.OrdinalIgnoreCase))
                {
                    this.defaultPrefix = attribute.InnerText;
                }
                else if (string.Equals(attribute.Name, "TOOLNAME", StringComparison.OrdinalIgnoreCase))
                {
                    this.toolName = attribute.InnerText;
                }
                else if (string.Equals(attribute.Name, "NAME", StringComparison.OrdinalIgnoreCase))
                {
                    this.name = attribute.InnerText;
                }
                else if (string.Equals(attribute.Name, "BASECLASS", StringComparison.OrdinalIgnoreCase))
                {
                    this.baseClass = attribute.InnerText;
                }
                else if (string.Equals(attribute.Name, "NAMESPACE", StringComparison.OrdinalIgnoreCase))
                {
                    this.namespaceValue = attribute.InnerText;
                }
                else if (string.Equals(attribute.Name, "RESOURCENAMESPACE", StringComparison.OrdinalIgnoreCase))
                {
                    this.resourceNamespaceValue = attribute.InnerText;
                }
            }
            if (firstChild.HasChildNodes)
            {
                return this.ParseSwitchGroupOrSwitch(firstChild.FirstChild, this.switchRelationsList, null);
            }
            this.LogError("NoChildren", new object[0]);
            return false;
        }

        private static bool VerifyAttributeExists(XmlNode node, string attributeName)
        {
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool VerifyNodeName(XmlNode node)
        {
            return string.Equals(node.Name, "RELATIONS", StringComparison.OrdinalIgnoreCase);
        }

        public string BaseClass
        {
            get
            {
                return this.baseClass;
            }
        }

        public string DefaultPrefix
        {
            get
            {
                return this.defaultPrefix;
            }
        }

        public LinkedList<Property> DefaultSet
        {
            get
            {
                return this.defaultSet;
            }
        }

        public int ErrorCount
        {
            get
            {
                return this.errorCount;
            }
        }

        public LinkedList<string> ErrorLog
        {
            get
            {
                return this.errorLog;
            }
        }

        public Dictionary<string, string> FallbackSet
        {
            get
            {
                return this.fallbackSet;
            }
        }

        public string GeneratedTaskName
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Namespace
        {
            get
            {
                return this.namespaceValue;
            }
        }

        public LinkedList<Property> Properties
        {
            get
            {
                return this.properties;
            }
        }

        public string ResourceNamespace
        {
            get
            {
                return this.resourceNamespaceValue;
            }
        }

        public Dictionary<string, SwitchRelations> SwitchRelationsList
        {
            get
            {
                return this.switchRelationsList;
            }
        }

        public string ToolName
        {
            get
            {
                return this.toolName;
            }
        }
    }
}

