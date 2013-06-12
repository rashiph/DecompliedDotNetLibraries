namespace System.Web.Configuration
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;
    using System.Xml;

    internal class BrowserDefinition
    {
        private AdapterDictionary _adapters;
        private BrowserDefinitionCollection _browsers;
        private NameValueCollection _capabilities;
        private ArrayList _captureCapabilityChecks;
        private ArrayList _captureHeaderChecks;
        private int _depth;
        private BrowserDefinitionCollection _gateways;
        private string _htmlTextWriterString;
        private string _id;
        private ArrayList _idCapabilityChecks;
        private ArrayList _idHeaderChecks;
        private bool _isDefaultBrowser;
        private bool _isDeviceNode;
        private bool _isRefID;
        private string _name;
        private System.Xml.XmlNode _node;
        private string _parentID;
        private string _parentName;
        private BrowserDefinitionCollection _refBrowsers;
        private BrowserDefinitionCollection _refGateways;

        internal BrowserDefinition(System.Xml.XmlNode node) : this(node, false)
        {
        }

        internal BrowserDefinition(System.Xml.XmlNode node, bool isDefaultBrowser)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this._capabilities = new NameValueCollection();
            this._idHeaderChecks = new ArrayList();
            this._idCapabilityChecks = new ArrayList();
            this._captureHeaderChecks = new ArrayList();
            this._captureCapabilityChecks = new ArrayList();
            this._adapters = new AdapterDictionary();
            this._browsers = new BrowserDefinitionCollection();
            this._gateways = new BrowserDefinitionCollection();
            this._refBrowsers = new BrowserDefinitionCollection();
            this._refGateways = new BrowserDefinitionCollection();
            this._node = node;
            this._isDefaultBrowser = isDefaultBrowser;
            string val = null;
            System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "id", ref this._id);
            System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "refID", ref val);
            if ((val != null) && (this._id != null))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_mutually_exclusive_attributes", new object[] { "id", "refID" }), node);
            }
            if (this._id != null)
            {
                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(this._id))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_InvalidID", new object[] { "id", this._id }), node);
                }
            }
            else
            {
                if (val == null)
                {
                    if (this is GatewayDefinition)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_attributes_required", new object[] { "gateway", "refID", "id" }), node);
                    }
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_attributes_required", new object[] { "browser", "refID", "id" }), node);
                }
                if (!CodeGenerator.IsValidLanguageIndependentIdentifier(val))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_InvalidID", new object[] { "refID", val }), node);
                }
                this._parentID = val;
                this._isRefID = true;
                this._id = val;
                if (this is GatewayDefinition)
                {
                    this._name = "refgatewayid$";
                }
                else
                {
                    this._name = "refbrowserid$";
                }
                string str2 = null;
                System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "parentID", ref str2);
                if ((str2 != null) && (str2.Length != 0))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_mutually_exclusive_attributes", new object[] { "parentID", "refID" }), node);
                }
            }
            this._name = MakeValidTypeNameFromString(this._id + this._name);
            if (!this._isRefID)
            {
                if (!"Default".Equals(this._id))
                {
                    System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "parentID", ref this._parentID);
                }
                else
                {
                    System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node, "parentID", ref this._parentID);
                    if (this._parentID != null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_parentID_applied_to_default"), node);
                    }
                }
            }
            this._parentName = MakeValidTypeNameFromString(this._parentID);
            if (this._id.IndexOf(" ", StringComparison.Ordinal) != -1)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Space_attribute", new object[] { "id " + this._id }), node);
            }
            foreach (System.Xml.XmlNode node2 in node.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    string name = node2.Name;
                    if (name == null)
                    {
                        goto Label_03DA;
                    }
                    if (!(name == "identification"))
                    {
                        if (name == "capture")
                        {
                            goto Label_03BE;
                        }
                        if (name == "capabilities")
                        {
                            goto Label_03C8;
                        }
                        if (name == "controlAdapters")
                        {
                            goto Label_03D1;
                        }
                        if (name == "sampleHeaders")
                        {
                            continue;
                        }
                        goto Label_03DA;
                    }
                    if (this._isRefID)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_refid_prohibits_identification"), node);
                    }
                    this.ProcessIdentificationNode(node2, BrowserCapsElementType.Identification);
                }
                continue;
            Label_03BE:
                this.ProcessCaptureNode(node2, BrowserCapsElementType.Capture);
                continue;
            Label_03C8:
                this.ProcessCapabilitiesNode(node2);
                continue;
            Label_03D1:
                this.ProcessControlAdaptersNode(node2);
                continue;
            Label_03DA:;
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_invalid_element", new object[] { node2.Name }), node);
            }
        }

        private static Type CheckType(string typeName, Type baseType, System.Xml.XmlNode child)
        {
            Type c = ConfigUtil.GetType(typeName, child, true);
            if (!baseType.IsAssignableFrom(c))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_doesnt_inherit_from_type", new object[] { typeName, baseType.FullName }), child);
            }
            if (!HttpRuntime.IsTypeAllowedInConfig(c))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_from_untrusted_assembly", new object[] { typeName }), child);
            }
            return c;
        }

        private void DisallowNonMatchAttribute(System.Xml.XmlNode node)
        {
            string val = null;
            System.Web.Configuration.HandlerBase.GetAndRemoveStringAttribute(node, "nonMatch", ref val);
            if (val != null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_mutually_exclusive_attributes", new object[] { "match", "nonMatch" }), node);
            }
        }

        private void HandleMissingMatchAndNonMatchError(System.Xml.XmlNode node)
        {
            throw new ConfigurationErrorsException(System.Web.SR.GetString("Missing_required_attributes", new object[] { "match", "nonMatch", node.Name }), node);
        }

        internal static string MakeValidTypeNameFromString(string s)
        {
            if (s == null)
            {
                return s;
            }
            s = s.ToLower(CultureInfo.InvariantCulture);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0)
                {
                    if (char.IsDigit(s[0]))
                    {
                        builder.Append("N");
                    }
                    else if (char.IsLetter(s[0]))
                    {
                        builder.Append(s.Substring(0, 1).ToUpper(CultureInfo.InvariantCulture));
                        continue;
                    }
                }
                if (char.IsLetterOrDigit(s[i]) || (s[i] == '_'))
                {
                    builder.Append(s[i]);
                }
                else
                {
                    builder.Append('A');
                }
            }
            return builder.ToString();
        }

        internal void MergeWithDefinition(BrowserDefinition definition)
        {
            foreach (string str in definition.Capabilities.Keys)
            {
                this._capabilities[str] = definition.Capabilities[str];
            }
            foreach (string str2 in definition.Adapters.Keys)
            {
                this._adapters[str2] = definition.Adapters[str2];
            }
            this._htmlTextWriterString = definition.HtmlTextWriterString;
        }

        internal void ProcessCapabilitiesNode(System.Xml.XmlNode node)
        {
            foreach (System.Xml.XmlNode node2 in node.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    if (node2.Name != "capability")
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_unrecognized_element"), node2);
                    }
                    string val = null;
                    string str2 = null;
                    System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "name", ref val);
                    System.Web.Configuration.HandlerBase.GetAndRemoveRequiredStringAttribute(node2, "value", ref str2);
                    this._capabilities[val] = str2;
                }
            }
        }

        internal void ProcessCaptureNode(System.Xml.XmlNode node, BrowserCapsElementType elementType)
        {
            string val = null;
            string str2 = null;
            foreach (System.Xml.XmlNode node2 in node.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    string name = node2.Name;
                    if (name == null)
                    {
                        goto Label_00F6;
                    }
                    if (!(name == "userAgent"))
                    {
                        if (name == "header")
                        {
                            goto Label_0094;
                        }
                        if (name == "capability")
                        {
                            goto Label_00C5;
                        }
                        goto Label_00F6;
                    }
                    System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "match", ref val);
                    this._captureHeaderChecks.Add(new CheckPair("User-Agent", val));
                }
                continue;
            Label_0094:
                System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "name", ref str2);
                System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "match", ref val);
                this._captureHeaderChecks.Add(new CheckPair(str2, val));
                continue;
            Label_00C5:
                System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "name", ref str2);
                System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "match", ref val);
                this._captureCapabilityChecks.Add(new CheckPair(str2, val));
                continue;
            Label_00F6:;
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_invalid_element", new object[] { node2.ToString() }), node2);
            }
        }

        internal void ProcessControlAdaptersNode(System.Xml.XmlNode node)
        {
            System.Web.Configuration.HandlerBase.GetAndRemoveStringAttribute(node, "markupTextWriterType", ref this._htmlTextWriterString);
            foreach (System.Xml.XmlNode node2 in node.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    if (node2.Name != "adapter")
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_base_unrecognized_element"), node2);
                    }
                    XmlAttributeCollection attributes = node2.Attributes;
                    string val = null;
                    string str2 = null;
                    System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "controlType", ref val);
                    System.Web.Configuration.HandlerBase.GetAndRemoveRequiredStringAttribute(node2, "adapterType", ref str2);
                    val = CheckType(val, typeof(Control), node2).AssemblyQualifiedName;
                    if (!string.IsNullOrEmpty(str2))
                    {
                        CheckType(str2, typeof(ControlAdapter), node2);
                    }
                    this._adapters[val] = str2;
                }
            }
        }

        internal void ProcessIdentificationNode(System.Xml.XmlNode node, BrowserCapsElementType elementType)
        {
            string val = null;
            string str2 = null;
            bool flag2 = true;
            foreach (System.Xml.XmlNode node2 in node.ChildNodes)
            {
                val = string.Empty;
                bool nonMatch = false;
                if (node2.NodeType == XmlNodeType.Element)
                {
                    string name = node2.Name;
                    if (name == null)
                    {
                        goto Label_01BB;
                    }
                    if (!(name == "userAgent"))
                    {
                        if (name == "header")
                        {
                            goto Label_00E1;
                        }
                        if (name == "capability")
                        {
                            goto Label_0151;
                        }
                        goto Label_01BB;
                    }
                    flag2 = false;
                    System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node2, "match", ref val);
                    if (string.IsNullOrEmpty(val))
                    {
                        System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node2, "nonMatch", ref val);
                        if (string.IsNullOrEmpty(val))
                        {
                            this.HandleMissingMatchAndNonMatchError(node2);
                        }
                        nonMatch = true;
                    }
                    this._idHeaderChecks.Add(new CheckPair("User-Agent", val, nonMatch));
                    if (!nonMatch)
                    {
                        this.DisallowNonMatchAttribute(node2);
                    }
                }
                continue;
            Label_00E1:
                flag2 = false;
                System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "name", ref str2);
                System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node2, "match", ref val);
                if (string.IsNullOrEmpty(val))
                {
                    System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node2, "nonMatch", ref val);
                    if (string.IsNullOrEmpty(val))
                    {
                        this.HandleMissingMatchAndNonMatchError(node2);
                    }
                    nonMatch = true;
                }
                this._idHeaderChecks.Add(new CheckPair(str2, val, nonMatch));
                if (!nonMatch)
                {
                    this.DisallowNonMatchAttribute(node2);
                }
                continue;
            Label_0151:
                flag2 = false;
                System.Web.Configuration.HandlerBase.GetAndRemoveRequiredNonEmptyStringAttribute(node2, "name", ref str2);
                System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node2, "match", ref val);
                if (string.IsNullOrEmpty(val))
                {
                    System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node2, "nonMatch", ref val);
                    if (string.IsNullOrEmpty(val))
                    {
                        this.HandleMissingMatchAndNonMatchError(node2);
                    }
                    nonMatch = true;
                }
                this._idCapabilityChecks.Add(new CheckPair(str2, val, nonMatch));
                if (!nonMatch)
                {
                    this.DisallowNonMatchAttribute(node2);
                }
                continue;
            Label_01BB:;
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Config_invalid_element", new object[] { node2.ToString() }), node2);
            }
            if (flag2)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Browser_empty_identification"), node);
            }
        }

        public AdapterDictionary Adapters
        {
            get
            {
                return this._adapters;
            }
        }

        public BrowserDefinitionCollection Browsers
        {
            get
            {
                return this._browsers;
            }
        }

        public NameValueCollection Capabilities
        {
            get
            {
                return this._capabilities;
            }
        }

        public ArrayList CaptureCapabilityChecks
        {
            get
            {
                return this._captureCapabilityChecks;
            }
        }

        public ArrayList CaptureHeaderChecks
        {
            get
            {
                return this._captureHeaderChecks;
            }
        }

        internal int Depth
        {
            get
            {
                return this._depth;
            }
            set
            {
                this._depth = value;
            }
        }

        public BrowserDefinitionCollection Gateways
        {
            get
            {
                return this._gateways;
            }
        }

        public string HtmlTextWriterString
        {
            get
            {
                return this._htmlTextWriterString;
            }
        }

        public string ID
        {
            get
            {
                return this._id;
            }
        }

        public ArrayList IdCapabilityChecks
        {
            get
            {
                return this._idCapabilityChecks;
            }
        }

        public ArrayList IdHeaderChecks
        {
            get
            {
                return this._idHeaderChecks;
            }
        }

        public bool IsDefaultBrowser
        {
            get
            {
                return this._isDefaultBrowser;
            }
        }

        internal bool IsDeviceNode
        {
            get
            {
                return this._isDeviceNode;
            }
            set
            {
                this._isDeviceNode = value;
            }
        }

        internal bool IsRefID
        {
            get
            {
                return this._isRefID;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string ParentID
        {
            get
            {
                return this._parentID;
            }
        }

        public string ParentName
        {
            get
            {
                return this._parentName;
            }
        }

        public BrowserDefinitionCollection RefBrowsers
        {
            get
            {
                return this._refBrowsers;
            }
        }

        public BrowserDefinitionCollection RefGateways
        {
            get
            {
                return this._refGateways;
            }
        }

        internal System.Xml.XmlNode XmlNode
        {
            get
            {
                return this._node;
            }
        }
    }
}

