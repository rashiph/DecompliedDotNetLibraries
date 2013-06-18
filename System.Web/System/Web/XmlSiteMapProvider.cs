namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;

    public class XmlSiteMapProvider : StaticSiteMapProvider, IDisposable
    {
        private ArrayList _childProviderList;
        private Hashtable _childProviderTable;
        private XmlDocument _document;
        private string _filename;
        private FileChangeEventHandler _handler;
        private bool _initialized;
        private VirtualPath _normalizedVirtualPath;
        private StringCollection _parentSiteMapFileCollection;
        private const string _providerAttribute = "provider";
        private const char _resourceKeySeparator = ',';
        private const string _resourcePrefix = "$resources:";
        private const int _resourcePrefixLength = 10;
        private static readonly char[] _seperators = new char[] { ';', ',' };
        private const string _siteMapFileAttribute = "siteMapFile";
        private SiteMapNode _siteMapNode;
        private const string _siteMapNodeName = "siteMapNode";
        private VirtualPath _virtualPath;
        private const string _xmlSiteMapFileExtension = ".sitemap";

        protected internal override void AddNode(SiteMapNode node, SiteMapNode parentNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (parentNode == null)
            {
                throw new ArgumentNullException("parentNode");
            }
            SiteMapProvider provider = node.Provider;
            SiteMapProvider provider2 = parentNode.Provider;
            if (provider != this)
            {
                throw new ArgumentException(System.Web.SR.GetString("XmlSiteMapProvider_cannot_add_node", new object[] { node.ToString() }), "node");
            }
            if (provider2 != this)
            {
                throw new ArgumentException(System.Web.SR.GetString("XmlSiteMapProvider_cannot_add_node", new object[] { parentNode.ToString() }), "parentNode");
            }
            lock (base._lock)
            {
                this.RemoveNode(node);
                this.AddNodeInternal(node, parentNode, null);
            }
        }

        private void AddNodeInternal(SiteMapNode node, SiteMapNode parentNode, XmlNode xmlNode)
        {
            lock (base._lock)
            {
                string url = node.Url;
                string key = node.Key;
                bool flag = false;
                if (!string.IsNullOrEmpty(url))
                {
                    if (base.UrlTable[url] != null)
                    {
                        if (xmlNode != null)
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Multiple_Nodes_With_Identical_Url", new object[] { url }), xmlNode);
                        }
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_Multiple_Nodes_With_Identical_Url", new object[] { url }));
                    }
                    flag = true;
                }
                if (base.KeyTable.Contains(key))
                {
                    if (xmlNode != null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Multiple_Nodes_With_Identical_Key", new object[] { key }), xmlNode);
                    }
                    throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_Multiple_Nodes_With_Identical_Key", new object[] { key }));
                }
                if (flag)
                {
                    base.UrlTable[url] = node;
                }
                base.KeyTable[key] = node;
                if (parentNode != null)
                {
                    base.ParentNodeTable[node] = parentNode;
                    if (base.ChildNodeCollectionTable[parentNode] == null)
                    {
                        base.ChildNodeCollectionTable[parentNode] = new SiteMapNodeCollection();
                    }
                    ((SiteMapNodeCollection) base.ChildNodeCollectionTable[parentNode]).Add(node);
                }
            }
        }

        protected virtual void AddProvider(string providerName, SiteMapNode parentNode)
        {
            if (parentNode == null)
            {
                throw new ArgumentNullException("parentNode");
            }
            if (parentNode.Provider != this)
            {
                throw new ArgumentException(System.Web.SR.GetString("XmlSiteMapProvider_cannot_add_node", new object[] { parentNode.ToString() }), "parentNode");
            }
            SiteMapNode nodeFromProvider = this.GetNodeFromProvider(providerName);
            this.AddNodeInternal(nodeFromProvider, parentNode, null);
        }

        public override SiteMapNode BuildSiteMap()
        {
            SiteMapNode node = this._siteMapNode;
            if (node != null)
            {
                return node;
            }
            XmlDocument configDocument = this.GetConfigDocument();
            lock (base._lock)
            {
                if (this._siteMapNode == null)
                {
                    this.Clear();
                    this.CheckSiteMapFileExists();
                    try
                    {
                        using (Stream stream = this._normalizedVirtualPath.OpenFile())
                        {
                            XmlReader reader = new XmlTextReader(stream);
                            configDocument.Load(reader);
                        }
                    }
                    catch (XmlException exception)
                    {
                        string virtualPathString = this._virtualPath.VirtualPathString;
                        string path = this._normalizedVirtualPath.MapPathInternal();
                        if ((path != null) && HttpRuntime.HasPathDiscoveryPermission(path))
                        {
                            virtualPathString = path;
                        }
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Error_loading_Config_file", new object[] { this._virtualPath, exception.Message }), exception, virtualPathString, exception.LineNumber);
                    }
                    catch (Exception exception2)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Error_loading_Config_file", new object[] { this._virtualPath, exception2.Message }), exception2);
                    }
                    XmlNode node2 = null;
                    foreach (XmlNode node3 in configDocument.ChildNodes)
                    {
                        if (string.Equals(node3.Name, "siteMap", StringComparison.Ordinal))
                        {
                            node2 = node3;
                            break;
                        }
                    }
                    if (node2 == null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Top_Element_Must_Be_SiteMap"), configDocument);
                    }
                    bool val = false;
                    System.Web.Configuration.HandlerBase.GetAndRemoveBooleanAttribute(node2, "enableLocalization", ref val);
                    base.EnableLocalization = val;
                    XmlNode node4 = null;
                    foreach (XmlNode node5 in node2.ChildNodes)
                    {
                        if (node5.NodeType == XmlNodeType.Element)
                        {
                            if (!"siteMapNode".Equals(node5.Name))
                            {
                                throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Only_SiteMapNode_Allowed"), node5);
                            }
                            if (node4 != null)
                            {
                                throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Only_One_SiteMapNode_Required_At_Top"), node5);
                            }
                            node4 = node5;
                        }
                    }
                    if (node4 == null)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Only_One_SiteMapNode_Required_At_Top"), node2);
                    }
                    Queue queue = new Queue(50);
                    queue.Enqueue(null);
                    queue.Enqueue(node4);
                    this._siteMapNode = this.ConvertFromXmlNode(queue);
                }
                return this._siteMapNode;
            }
        }

        private void CheckSiteMapFileExists()
        {
            if (!Util.VirtualFileExistsWithAssert(this._normalizedVirtualPath))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_FileName_does_not_exist", new object[] { this._virtualPath }));
            }
        }

        protected override void Clear()
        {
            lock (base._lock)
            {
                this.ChildProviderTable.Clear();
                this._siteMapNode = null;
                this._childProviderList = null;
                base.Clear();
            }
        }

        private SiteMapNode ConvertFromXmlNode(Queue queue)
        {
            SiteMapNode node = null;
            while (queue.Count != 0)
            {
                SiteMapNode parentNode = (SiteMapNode) queue.Dequeue();
                XmlNode node3 = (XmlNode) queue.Dequeue();
                SiteMapNode nodeFromProvider = null;
                if (!"siteMapNode".Equals(node3.Name))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_Only_SiteMapNode_Allowed"), node3);
                }
                string val = null;
                System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node3, "provider", ref val);
                if (val != null)
                {
                    nodeFromProvider = this.GetNodeFromProvider(val);
                    System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(node3);
                    System.Web.Configuration.HandlerBase.CheckForNonCommentChildNodes(node3);
                }
                else
                {
                    string str2 = null;
                    System.Web.Configuration.HandlerBase.GetAndRemoveNonEmptyStringAttribute(node3, "siteMapFile", ref str2);
                    if (str2 != null)
                    {
                        nodeFromProvider = this.GetNodeFromSiteMapFile(node3, VirtualPath.Create(str2));
                    }
                    else
                    {
                        nodeFromProvider = this.GetNodeFromXmlNode(node3, queue);
                    }
                }
                this.AddNodeInternal(nodeFromProvider, parentNode, node3);
                if (node == null)
                {
                    node = nodeFromProvider;
                }
            }
            return node;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._handler != null)
            {
                HttpRuntime.FileChangesMonitor.StopMonitoringFile(this._filename, this._handler);
            }
        }

        private void EnsureChildSiteMapProviderUpToDate(SiteMapProvider childProvider)
        {
            SiteMapNode node = (SiteMapNode) this.ChildProviderTable[childProvider];
            SiteMapNode rootNodeCore = childProvider.GetRootNodeCore();
            if (rootNodeCore == null)
            {
                throw new ProviderException(System.Web.SR.GetString("XmlSiteMapProvider_invalid_sitemapnode_returned", new object[] { childProvider.Name }));
            }
            if (!node.Equals(rootNodeCore) && (node != null))
            {
                lock (base._lock)
                {
                    node = (SiteMapNode) this.ChildProviderTable[childProvider];
                    if (node != null)
                    {
                        rootNodeCore = childProvider.GetRootNodeCore();
                        if (rootNodeCore == null)
                        {
                            throw new ProviderException(System.Web.SR.GetString("XmlSiteMapProvider_invalid_sitemapnode_returned", new object[] { childProvider.Name }));
                        }
                        if (!node.Equals(rootNodeCore))
                        {
                            if (this._siteMapNode.Equals(node))
                            {
                                base.UrlTable.Remove(node.Url);
                                base.KeyTable.Remove(node.Key);
                                base.UrlTable.Add(rootNodeCore.Url, rootNodeCore);
                                base.KeyTable.Add(rootNodeCore.Key, rootNodeCore);
                                this._siteMapNode = rootNodeCore;
                            }
                            SiteMapNode node3 = (SiteMapNode) base.ParentNodeTable[node];
                            if (node3 != null)
                            {
                                SiteMapNodeCollection nodes = (SiteMapNodeCollection) base.ChildNodeCollectionTable[node3];
                                int index = nodes.IndexOf(node);
                                if (index != -1)
                                {
                                    nodes.Remove(node);
                                    nodes.Insert(index, rootNodeCore);
                                }
                                else
                                {
                                    nodes.Add(rootNodeCore);
                                }
                                base.ParentNodeTable[rootNodeCore] = node3;
                                base.ParentNodeTable.Remove(node);
                                base.UrlTable.Remove(node.Url);
                                base.KeyTable.Remove(node.Key);
                                base.UrlTable.Add(rootNodeCore.Url, rootNodeCore);
                                base.KeyTable.Add(rootNodeCore.Key, rootNodeCore);
                            }
                            else
                            {
                                XmlSiteMapProvider parentProvider = this.ParentProvider as XmlSiteMapProvider;
                                if (parentProvider != null)
                                {
                                    parentProvider.EnsureChildSiteMapProviderUpToDate(this);
                                }
                            }
                            this.ChildProviderTable[childProvider] = rootNodeCore;
                            this._childProviderList = null;
                        }
                    }
                }
            }
        }

        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            SiteMapNode node = base.FindSiteMapNode(rawUrl);
            if (node == null)
            {
                foreach (SiteMapProvider provider in this.ChildProviderList)
                {
                    this.EnsureChildSiteMapProviderUpToDate(provider);
                    node = provider.FindSiteMapNode(rawUrl);
                    if (node != null)
                    {
                        return node;
                    }
                }
            }
            return node;
        }

        public override SiteMapNode FindSiteMapNodeFromKey(string key)
        {
            SiteMapNode node = base.FindSiteMapNodeFromKey(key);
            if (node == null)
            {
                foreach (SiteMapProvider provider in this.ChildProviderList)
                {
                    this.EnsureChildSiteMapProviderUpToDate(provider);
                    node = provider.FindSiteMapNodeFromKey(key);
                    if (node != null)
                    {
                        return node;
                    }
                }
            }
            return node;
        }

        private XmlDocument GetConfigDocument()
        {
            if (this._document == null)
            {
                if (!this._initialized)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_Not_Initialized"));
                }
                if (this._virtualPath == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("XmlSiteMapProvider_missing_siteMapFile", new object[] { "siteMapFile" }));
                }
                if (!this._virtualPath.Extension.Equals(".sitemap", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_Invalid_Extension", new object[] { this._virtualPath }));
                }
                this._normalizedVirtualPath = this._virtualPath.CombineWithAppRoot();
                this._normalizedVirtualPath.FailIfNotWithinAppRoot();
                this.CheckSiteMapFileExists();
                this._parentSiteMapFileCollection = new StringCollection();
                XmlSiteMapProvider parentProvider = this.ParentProvider as XmlSiteMapProvider;
                if ((parentProvider != null) && (parentProvider._parentSiteMapFileCollection != null))
                {
                    if (parentProvider._parentSiteMapFileCollection.Contains(this._normalizedVirtualPath.VirtualPathString))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_FileName_already_in_use", new object[] { this._virtualPath }));
                    }
                    foreach (string str in parentProvider._parentSiteMapFileCollection)
                    {
                        this._parentSiteMapFileCollection.Add(str);
                    }
                }
                this._parentSiteMapFileCollection.Add(this._normalizedVirtualPath.VirtualPathString);
                this._filename = HostingEnvironment.MapPathInternal(this._normalizedVirtualPath);
                if (!string.IsNullOrEmpty(this._filename))
                {
                    this._handler = new FileChangeEventHandler(this.OnConfigFileChange);
                    HttpRuntime.FileChangesMonitor.StartMonitoringFile(this._filename, this._handler);
                    base.ResourceKey = new FileInfo(this._filename).Name;
                }
                this._document = new ConfigXmlDocument();
            }
            return this._document;
        }

        private SiteMapNode GetNodeFromProvider(string providerName)
        {
            SiteMapProvider providerFromName = this.GetProviderFromName(providerName);
            SiteMapNode rootNodeCore = null;
            if (providerFromName is XmlSiteMapProvider)
            {
                XmlSiteMapProvider provider2 = (XmlSiteMapProvider) providerFromName;
                StringCollection strings = new StringCollection();
                if (this._parentSiteMapFileCollection != null)
                {
                    foreach (string str in this._parentSiteMapFileCollection)
                    {
                        strings.Add(str);
                    }
                }
                provider2.BuildSiteMap();
                strings.Add(this._normalizedVirtualPath.VirtualPathString);
                if (strings.Contains(VirtualPath.GetVirtualPathString(provider2._normalizedVirtualPath)))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_FileName_already_in_use", new object[] { provider2._virtualPath }));
                }
                provider2._parentSiteMapFileCollection = strings;
            }
            rootNodeCore = providerFromName.GetRootNodeCore();
            if (rootNodeCore == null)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_invalid_GetRootNodeCore", new object[] { providerFromName.Name }));
            }
            this.ChildProviderTable.Add(providerFromName, rootNodeCore);
            this._childProviderList = null;
            providerFromName.ParentProvider = this;
            return rootNodeCore;
        }

        private SiteMapNode GetNodeFromSiteMapFile(XmlNode xmlNode, VirtualPath siteMapFile)
        {
            SiteMapNode node = null;
            bool securityTrimmingEnabled = base.SecurityTrimmingEnabled;
            System.Web.Configuration.HandlerBase.GetAndRemoveBooleanAttribute(xmlNode, "securityTrimmingEnabled", ref securityTrimmingEnabled);
            System.Web.Configuration.HandlerBase.CheckForUnrecognizedAttributes(xmlNode);
            System.Web.Configuration.HandlerBase.CheckForNonCommentChildNodes(xmlNode);
            XmlSiteMapProvider key = new XmlSiteMapProvider();
            siteMapFile = this._normalizedVirtualPath.Parent.Combine(siteMapFile);
            key.ParentProvider = this;
            key.Initialize(siteMapFile, securityTrimmingEnabled);
            key.BuildSiteMap();
            node = key._siteMapNode;
            this.ChildProviderTable.Add(key, node);
            this._childProviderList = null;
            return node;
        }

        private SiteMapNode GetNodeFromXmlNode(XmlNode xmlNode, Queue queue)
        {
            SiteMapNode node = null;
            string val = null;
            string str2 = null;
            string str3 = null;
            string str4 = null;
            string str5 = null;
            System.Web.Configuration.HandlerBase.GetAndRemoveStringAttribute(xmlNode, "url", ref str2);
            System.Web.Configuration.HandlerBase.GetAndRemoveStringAttribute(xmlNode, "title", ref val);
            System.Web.Configuration.HandlerBase.GetAndRemoveStringAttribute(xmlNode, "description", ref str3);
            System.Web.Configuration.HandlerBase.GetAndRemoveStringAttribute(xmlNode, "roles", ref str4);
            System.Web.Configuration.HandlerBase.GetAndRemoveStringAttribute(xmlNode, "resourceKey", ref str5);
            if (!string.IsNullOrEmpty(str5) && !this.ValidateResource(base.ResourceKey, str5 + ".title"))
            {
                str5 = null;
            }
            System.Web.Configuration.HandlerBase.CheckForbiddenAttribute(xmlNode, "securityTrimmingEnabled");
            NameValueCollection collection = null;
            bool allowImplicitResource = string.IsNullOrEmpty(str5);
            this.HandleResourceAttribute(xmlNode, ref collection, "title", ref val, allowImplicitResource);
            this.HandleResourceAttribute(xmlNode, ref collection, "description", ref str3, allowImplicitResource);
            ArrayList list = new ArrayList();
            if (str4 != null)
            {
                int index = str4.IndexOf('?');
                if (index != -1)
                {
                    object[] args = new object[] { str4[index].ToString(CultureInfo.InvariantCulture) };
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Auth_rule_names_cant_contain_char", args), xmlNode);
                }
                foreach (string str6 in str4.Split(_seperators))
                {
                    string str7 = str6.Trim();
                    if (str7.Length > 0)
                    {
                        list.Add(str7);
                    }
                }
            }
            list = ArrayList.ReadOnly(list);
            string key = null;
            if (!string.IsNullOrEmpty(str2))
            {
                str2 = str2.Trim();
                if (!System.Web.Util.UrlPath.IsAbsolutePhysicalPath(str2))
                {
                    if (System.Web.Util.UrlPath.IsRelativeUrl(str2))
                    {
                        str2 = System.Web.Util.UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, str2);
                    }
                    if (HttpContext.Current != null)
                    {
                        str2 = HttpContext.Current.Response.ApplyAppPathModifier(str2);
                    }
                }
                string b = HttpUtility.UrlDecode(str2);
                if (!string.Equals(str2, b, StringComparison.Ordinal))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Property_Had_Malformed_Url", new object[] { "url", str2 }), xmlNode);
                }
                key = str2.ToLowerInvariant();
            }
            else
            {
                key = Guid.NewGuid().ToString();
            }
            ReadOnlyNameValueCollection attributes = new ReadOnlyNameValueCollection();
            attributes.SetReadOnly(false);
            foreach (XmlAttribute attribute in xmlNode.Attributes)
            {
                string text = attribute.Value;
                this.HandleResourceAttribute(xmlNode, ref collection, attribute.Name, ref text, allowImplicitResource);
                attributes[attribute.Name] = text;
            }
            attributes.SetReadOnly(true);
            node = new SiteMapNode(this, key, str2, val, str3, list, attributes, collection, str5) {
                ReadOnly = true
            };
            foreach (XmlNode node2 in xmlNode.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    queue.Enqueue(node);
                    queue.Enqueue(node2);
                }
            }
            return node;
        }

        private SiteMapProvider GetProviderFromName(string providerName)
        {
            SiteMapProvider provider = SiteMap.Providers[providerName];
            if (provider == null)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_Not_Found", new object[] { providerName }));
            }
            return provider;
        }

        protected internal override SiteMapNode GetRootNodeCore()
        {
            this.BuildSiteMap();
            return this._siteMapNode;
        }

        private void HandleResourceAttribute(XmlNode xmlNode, ref NameValueCollection collection, string attrName, ref string text, bool allowImplicitResource)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string str = null;
                string str2 = text.TrimStart(new char[] { ' ' });
                if (((str2 != null) && (str2.Length > 10)) && str2.ToLower(CultureInfo.InvariantCulture).StartsWith("$resources:", StringComparison.Ordinal))
                {
                    if (!allowImplicitResource)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_multiple_resource_definition", new object[] { attrName }), xmlNode);
                    }
                    str = str2.Substring(11);
                    if (str.Length == 0)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_resourceKey_cannot_be_empty"), xmlNode);
                    }
                    string str3 = null;
                    string str4 = null;
                    int index = str.IndexOf(',');
                    if (index == -1)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("XmlSiteMapProvider_invalid_resource_key", new object[] { str }), xmlNode);
                    }
                    str3 = str.Substring(0, index);
                    str4 = str.Substring(index + 1);
                    int length = str4.IndexOf(',');
                    if (length != -1)
                    {
                        text = str4.Substring(length + 1);
                        str4 = str4.Substring(0, length);
                    }
                    else
                    {
                        text = null;
                    }
                    if (collection == null)
                    {
                        collection = new NameValueCollection();
                    }
                    collection.Add(attrName, str3.Trim());
                    collection.Add(attrName, str4.Trim());
                }
            }
        }

        public override void Initialize(string name, NameValueCollection attributes)
        {
            if (this._initialized)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_Cannot_Be_Inited_Twice"));
            }
            if (attributes != null)
            {
                if (string.IsNullOrEmpty(attributes["description"]))
                {
                    attributes.Remove("description");
                    attributes.Add("description", System.Web.SR.GetString("XmlSiteMapProvider_Description"));
                }
                string val = null;
                ProviderUtil.GetAndRemoveStringAttribute(attributes, "siteMapFile", name, ref val);
                this._virtualPath = VirtualPath.CreateAllowNull(val);
            }
            base.Initialize(name, attributes);
            if (attributes != null)
            {
                ProviderUtil.CheckUnrecognizedAttributes(attributes, name);
            }
            this._initialized = true;
        }

        private void Initialize(VirtualPath virtualPath, bool secuityTrimmingEnabled)
        {
            NameValueCollection config = new NameValueCollection();
            config.Add("siteMapFile", virtualPath.VirtualPathString);
            config.Add("securityTrimmingEnabled", Util.GetStringFromBool(secuityTrimmingEnabled));
            this.Initialize(virtualPath.VirtualPathString, config);
        }

        private void OnConfigFileChange(object sender, FileChangeEvent e)
        {
            XmlSiteMapProvider parentProvider = this.ParentProvider as XmlSiteMapProvider;
            if (parentProvider != null)
            {
                parentProvider.OnConfigFileChange(sender, e);
            }
            this.Clear();
        }

        protected internal override void RemoveNode(SiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            SiteMapProvider provider = node.Provider;
            if (provider != this)
            {
                for (SiteMapProvider provider2 = provider.ParentProvider; provider2 != this; provider2 = provider2.ParentProvider)
                {
                    if (provider2 == null)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_cannot_remove_node", new object[] { node.ToString(), this.Name, provider.Name }));
                    }
                }
            }
            if (node.Equals(provider.GetRootNodeCore()))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("SiteMapProvider_cannot_remove_root_node"));
            }
            if (provider != this)
            {
                provider.RemoveNode(node);
            }
            base.RemoveNode(node);
        }

        protected virtual void RemoveProvider(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }
            lock (base._lock)
            {
                SiteMapProvider providerFromName = this.GetProviderFromName(providerName);
                SiteMapNode node = (SiteMapNode) this.ChildProviderTable[providerFromName];
                if (node == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_cannot_find_provider", new object[] { providerFromName.Name, this.Name }));
                }
                providerFromName.ParentProvider = null;
                this.ChildProviderTable.Remove(providerFromName);
                this._childProviderList = null;
                base.RemoveNode(node);
            }
        }

        private bool ValidateResource(string classKey, string resourceKey)
        {
            try
            {
                HttpContext.GetGlobalResourceObject(classKey, resourceKey);
            }
            catch (MissingManifestResourceException)
            {
                return false;
            }
            return true;
        }

        private ArrayList ChildProviderList
        {
            get
            {
                ArrayList list = this._childProviderList;
                if (list != null)
                {
                    return list;
                }
                lock (base._lock)
                {
                    if (this._childProviderList == null)
                    {
                        list = ArrayList.ReadOnly(new ArrayList(this.ChildProviderTable.Keys));
                        this._childProviderList = list;
                        return list;
                    }
                    return this._childProviderList;
                }
            }
        }

        private Hashtable ChildProviderTable
        {
            get
            {
                if (this._childProviderTable == null)
                {
                    lock (base._lock)
                    {
                        if (this._childProviderTable == null)
                        {
                            this._childProviderTable = new Hashtable();
                        }
                    }
                }
                return this._childProviderTable;
            }
        }

        public override SiteMapNode RootNode
        {
            get
            {
                this.BuildSiteMap();
                return base.ReturnNodeIfAccessible(this._siteMapNode);
            }
        }

        private class ReadOnlyNameValueCollection : NameValueCollection
        {
            public ReadOnlyNameValueCollection()
            {
                base.IsReadOnly = true;
            }

            internal void SetReadOnly(bool isReadonly)
            {
                base.IsReadOnly = isReadonly;
            }
        }
    }
}

