namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI.Design;
    using System.Xml;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class DesignTimeSiteMapProvider : DesignTimeSiteMapProviderBase
    {
        private const char _appRelativeCharacter = '~';
        private const string _providerAttribute = "provider";
        private const string _resourcePrefix = "$resources:";
        private const int _resourcePrefixLength = 10;
        private SiteMapNode _rootNode;
        private static readonly char[] _seperators = new char[] { ';', ',' };
        private const string _siteMapFileAttribute = "siteMapFile";
        private const string _siteMapNodeName = "siteMapNode";
        private Hashtable _urlTable;

        internal DesignTimeSiteMapProvider(IDesignerHost host) : base(host)
        {
        }

        public override SiteMapNode BuildSiteMap()
        {
            if (this._rootNode == null)
            {
                string physicalPath = null;
                Stream siteMapFileStream = this.GetSiteMapFileStream(out physicalPath);
                XmlDocument document = new XmlDocument();
                if (siteMapFileStream == null)
                {
                    if (physicalPath.Length == 0)
                    {
                        this._rootNode = base.BuildSiteMap();
                        return this._rootNode;
                    }
                    document.Load(physicalPath);
                }
                else
                {
                    using (StreamReader reader = new StreamReader(siteMapFileStream))
                    {
                        document.LoadXml(reader.ReadToEnd());
                    }
                }
                XmlNode node = null;
                foreach (XmlNode node2 in document.ChildNodes)
                {
                    if (string.Equals(node2.Name, "siteMap", StringComparison.Ordinal))
                    {
                        node = node2;
                        break;
                    }
                }
                if (node == null)
                {
                    this._rootNode = base.BuildSiteMap();
                    return this._rootNode;
                }
                try
                {
                    this._rootNode = this.ConvertFromXmlNode(node.FirstChild);
                }
                catch (Exception)
                {
                    this.Clear();
                    this._rootNode = base.BuildSiteMap();
                }
            }
            return this._rootNode;
        }

        private SiteMapNode ConvertFromXmlNode(XmlNode xmlNode)
        {
            if ((xmlNode.Attributes.GetNamedItem("provider") != null) || (xmlNode.Attributes.GetNamedItem("siteMapFile") != null))
            {
                return null;
            }
            string text = null;
            string path = null;
            string attributeFromXmlNode = null;
            string str4 = null;
            text = this.GetAttributeFromXmlNode(xmlNode, "title");
            attributeFromXmlNode = this.GetAttributeFromXmlNode(xmlNode, "description");
            path = this.GetAttributeFromXmlNode(xmlNode, "url");
            str4 = this.GetAttributeFromXmlNode(xmlNode, "roles");
            text = this.HandleResourceAttribute(text);
            attributeFromXmlNode = this.HandleResourceAttribute(attributeFromXmlNode);
            ArrayList list = new ArrayList();
            if (str4 != null)
            {
                foreach (string str5 in str4.Split(_seperators))
                {
                    string str6 = str5.Trim();
                    if (str6.Length > 0)
                    {
                        list.Add(str6);
                    }
                }
            }
            list = ArrayList.ReadOnly(list);
            if (path == null)
            {
                path = string.Empty;
            }
            if ((path.Length != 0) && !IsAppRelativePath(path))
            {
                path = "~/" + path;
            }
            string key = path;
            if (key.Length == 0)
            {
                key = Guid.NewGuid().ToString();
            }
            SiteMapNode parentNode = new SiteMapNode(this, key, path, text, attributeFromXmlNode, list, null, null, null);
            SiteMapNodeCollection nodes = new SiteMapNodeCollection();
            foreach (XmlNode node2 in xmlNode.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    SiteMapNode node3 = this.ConvertFromXmlNode(node2);
                    if (node3 != null)
                    {
                        nodes.Add(node3);
                        this.AddNode(node3, parentNode);
                    }
                }
            }
            if (path.Length != 0)
            {
                if (this.UrlTable.Contains(path))
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("DesignTimeSiteMapProvider_Duplicate_Url", new object[] { path }));
                }
                this.UrlTable[path] = parentNode;
            }
            return parentNode;
        }

        private string GetAttributeFromXmlNode(XmlNode xmlNode, string attributeName)
        {
            XmlNode namedItem = xmlNode.Attributes.GetNamedItem(attributeName);
            if (namedItem != null)
            {
                return namedItem.Value;
            }
            return null;
        }

        private SiteMapNode GetCurrentNodeFromLiveData(out SiteMapNode rootNode)
        {
            rootNode = this.BuildSiteMap();
            if ((rootNode != null) && (base.DocumentAppRelativeUrl != null))
            {
                return (SiteMapNode) this.UrlTable[base.DocumentAppRelativeUrl];
            }
            return null;
        }

        private Stream GetSiteMapFileStream(out string physicalPath)
        {
            physicalPath = string.Empty;
            if (base._host != null)
            {
                IWebApplication service = (IWebApplication) base._host.GetService(typeof(IWebApplication));
                if (service != null)
                {
                    IProjectItem projectItemFromUrl = service.GetProjectItemFromUrl("~/web.sitemap");
                    if (projectItemFromUrl != null)
                    {
                        physicalPath = projectItemFromUrl.PhysicalPath;
                        IDocumentProjectItem item2 = projectItemFromUrl as IDocumentProjectItem;
                        if (item2 != null)
                        {
                            return item2.GetContents();
                        }
                    }
                }
            }
            return null;
        }

        private string HandleResourceAttribute(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            string str = text.TrimStart(new char[] { ' ' });
            if ((str.Length <= 10) || !str.ToLower(CultureInfo.InvariantCulture).StartsWith("$resources:", StringComparison.Ordinal))
            {
                return text;
            }
            int index = str.IndexOf(',');
            if (index != -1)
            {
                index = str.IndexOf(',', index + 1);
                if (index != -1)
                {
                    return str.Substring(index + 1);
                }
            }
            return string.Empty;
        }

        public override bool IsAccessibleToUser(HttpContext context, SiteMapNode node)
        {
            return true;
        }

        private static bool IsAppRelativePath(string path)
        {
            if ((path.Length < 2) || (path[0] != '~'))
            {
                return false;
            }
            if (path[1] != '/')
            {
                return (path[1] == '\\');
            }
            return true;
        }

        public override SiteMapNode CurrentNode
        {
            get
            {
                SiteMapNode node;
                SiteMapNode currentNodeFromLiveData = this.GetCurrentNodeFromLiveData(out node);
                if (currentNodeFromLiveData != null)
                {
                    return currentNodeFromLiveData;
                }
                return base.CurrentNode;
            }
        }

        public override SiteMapNode RootNode
        {
            get
            {
                SiteMapNode node;
                this.GetCurrentNodeFromLiveData(out node);
                if (node != null)
                {
                    return node;
                }
                return base.RootNode;
            }
        }

        internal IDictionary UrlTable
        {
            get
            {
                if (this._urlTable == null)
                {
                    lock (this)
                    {
                        if (this._urlTable == null)
                        {
                            this._urlTable = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                }
                return this._urlTable;
            }
        }
    }
}

