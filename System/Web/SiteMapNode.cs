namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Reflection;
    using System.Resources;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class SiteMapNode : ICloneable, IHierarchyData, INavigateUIData
    {
        private NameValueCollection _attributes;
        private SiteMapNodeCollection _childNodes;
        private bool _childNodesSet;
        private string _description;
        private string _key;
        private SiteMapNode _parentNode;
        private bool _parentNodeSet;
        private SiteMapProvider _provider;
        private bool _readonly;
        private string _resourceKey;
        private NameValueCollection _resourceKeys;
        private IList _roles;
        private static readonly string _siteMapNodeType = typeof(SiteMapNode).Name;
        private string _title;
        private string _url;
        private System.Web.VirtualPath _virtualPath;

        public SiteMapNode(SiteMapProvider provider, string key) : this(provider, key, null, null, null, null, null, null, null)
        {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url) : this(provider, key, url, null, null, null, null, null, null)
        {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url, string title) : this(provider, key, url, title, null, null, null, null, null)
        {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url, string title, string description) : this(provider, key, url, title, description, null, null, null, null)
        {
        }

        public SiteMapNode(SiteMapProvider provider, string key, string url, string title, string description, IList roles, NameValueCollection attributes, NameValueCollection explicitResourceKeys, string implicitResourceKey)
        {
            this._provider = provider;
            this._title = title;
            this._description = description;
            this._roles = roles;
            this._attributes = attributes;
            this._key = key;
            this._resourceKeys = explicitResourceKeys;
            this._resourceKey = implicitResourceKey;
            if (url != null)
            {
                this._url = url.Trim();
            }
            this._virtualPath = this.CreateVirtualPathFromUrl(this._url);
            if (this._key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (this._provider == null)
            {
                throw new ArgumentNullException("provider");
            }
        }

        public virtual SiteMapNode Clone()
        {
            ArrayList roles = null;
            NameValueCollection attributes = null;
            NameValueCollection explicitResourceKeys = null;
            if (this._roles != null)
            {
                roles = new ArrayList(this._roles);
            }
            if (this._attributes != null)
            {
                attributes = new NameValueCollection(this._attributes);
            }
            if (this._resourceKeys != null)
            {
                explicitResourceKeys = new NameValueCollection(this._resourceKeys);
            }
            return new SiteMapNode(this._provider, this.Key, this.Url, this.Title, this.Description, roles, attributes, explicitResourceKeys, this._resourceKey);
        }

        public virtual SiteMapNode Clone(bool cloneParentNodes)
        {
            SiteMapNode node = this.Clone();
            if (cloneParentNodes)
            {
                SiteMapNode node2 = node;
                SiteMapNode parentNode = this.ParentNode;
                while (parentNode != null)
                {
                    SiteMapNode node4 = parentNode.Clone();
                    node2.ParentNode = node4;
                    node4.ChildNodes = new SiteMapNodeCollection(node2);
                    parentNode = parentNode.ParentNode;
                    node2 = node4;
                }
            }
            return node;
        }

        private System.Web.VirtualPath CreateVirtualPathFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            if (!UrlPath.IsValidVirtualPathWithoutProtocol(url))
            {
                return null;
            }
            if (UrlPath.IsAbsolutePhysicalPath(url))
            {
                return null;
            }
            if (HttpRuntime.AppDomainAppVirtualPath == null)
            {
                return null;
            }
            if (UrlPath.IsRelativeUrl(url) && !UrlPath.IsAppRelativePath(url))
            {
                url = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, url);
            }
            int index = url.IndexOf('?');
            if (index != -1)
            {
                url = url.Substring(0, index);
            }
            return System.Web.VirtualPath.Create(url, VirtualPathOptions.AllowAppRelativePath | VirtualPathOptions.AllowAbsolutePath);
        }

        public override bool Equals(object obj)
        {
            SiteMapNode node = obj as SiteMapNode;
            return (((node != null) && (this._key == node.Key)) && string.Equals(this._url, node._url, StringComparison.OrdinalIgnoreCase));
        }

        public SiteMapNodeCollection GetAllNodes()
        {
            SiteMapNodeCollection collection = new SiteMapNodeCollection();
            this.GetAllNodesRecursive(collection);
            return SiteMapNodeCollection.ReadOnly(collection);
        }

        private void GetAllNodesRecursive(SiteMapNodeCollection collection)
        {
            SiteMapNodeCollection childNodes = this.ChildNodes;
            if ((childNodes != null) && (childNodes.Count > 0))
            {
                collection.AddRange(childNodes);
                foreach (SiteMapNode node in childNodes)
                {
                    node.GetAllNodesRecursive(collection);
                }
            }
        }

        public SiteMapDataSourceView GetDataSourceView(SiteMapDataSource owner, string viewName)
        {
            return new SiteMapDataSourceView(owner, viewName, this);
        }

        protected string GetExplicitResourceString(string attributeName, string defaultValue, bool throwIfNotFound)
        {
            if (attributeName == null)
            {
                throw new ArgumentNullException("attributeName");
            }
            string globalResourceObject = null;
            if (this._resourceKeys != null)
            {
                string[] values = this._resourceKeys.GetValues(attributeName);
                if ((values == null) || (values.Length <= 1))
                {
                    return globalResourceObject;
                }
                try
                {
                    globalResourceObject = ResourceExpressionBuilder.GetGlobalResourceObject(values[0], values[1]) as string;
                }
                catch (MissingManifestResourceException)
                {
                    if (defaultValue != null)
                    {
                        return defaultValue;
                    }
                }
                if ((globalResourceObject == null) && throwIfNotFound)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Res_not_found_with_class_and_key", new object[] { values[0], values[1] }));
                }
            }
            return globalResourceObject;
        }

        public override int GetHashCode()
        {
            return this._key.GetHashCode();
        }

        public SiteMapHierarchicalDataSourceView GetHierarchicalDataSourceView()
        {
            return new SiteMapHierarchicalDataSourceView(this);
        }

        protected string GetImplicitResourceString(string attributeName)
        {
            if (attributeName == null)
            {
                throw new ArgumentNullException("attributeName");
            }
            string globalResourceObject = null;
            if (!string.IsNullOrEmpty(this._resourceKey))
            {
                try
                {
                    globalResourceObject = ResourceExpressionBuilder.GetGlobalResourceObject(this.Provider.ResourceKey, this.ResourceKey + "." + attributeName) as string;
                }
                catch
                {
                }
            }
            return globalResourceObject;
        }

        public virtual bool IsAccessibleToUser(HttpContext context)
        {
            return this._provider.IsAccessibleToUser(context, this);
        }

        public virtual bool IsDescendantOf(SiteMapNode node)
        {
            for (SiteMapNode node2 = this.ParentNode; node2 != null; node2 = node2.ParentNode)
            {
                if (node2.Equals(node))
                {
                    return true;
                }
            }
            return false;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        IHierarchicalEnumerable IHierarchyData.GetChildren()
        {
            return this.ChildNodes;
        }

        IHierarchyData IHierarchyData.GetParent()
        {
            SiteMapNode parentNode = this.ParentNode;
            if (parentNode == null)
            {
                return null;
            }
            return parentNode;
        }

        public override string ToString()
        {
            return this.Title;
        }

        protected NameValueCollection Attributes
        {
            get
            {
                return this._attributes;
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "Attributes" }));
                }
                this._attributes = value;
            }
        }

        public virtual SiteMapNodeCollection ChildNodes
        {
            get
            {
                if (this._childNodesSet)
                {
                    return this._childNodes;
                }
                return this._provider.GetChildNodes(this);
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "ChildNodes" }));
                }
                this._childNodes = value;
                this._childNodesSet = true;
            }
        }

        [Localizable(true)]
        public virtual string Description
        {
            get
            {
                if (this._provider.EnableLocalization)
                {
                    string implicitResourceString = this.GetImplicitResourceString("description");
                    if (implicitResourceString != null)
                    {
                        return implicitResourceString;
                    }
                    implicitResourceString = this.GetExplicitResourceString("description", this._description, true);
                    if (implicitResourceString != null)
                    {
                        return implicitResourceString;
                    }
                }
                if (this._description != null)
                {
                    return this._description;
                }
                return string.Empty;
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "Description" }));
                }
                this._description = value;
            }
        }

        public virtual bool HasChildNodes
        {
            get
            {
                IList childNodes = this.ChildNodes;
                return ((childNodes != null) && (childNodes.Count > 0));
            }
        }

        public virtual string this[string key]
        {
            get
            {
                string defaultValue = null;
                if (this._attributes != null)
                {
                    defaultValue = this._attributes[key];
                }
                if (this._provider.EnableLocalization)
                {
                    string implicitResourceString = this.GetImplicitResourceString(key);
                    if (implicitResourceString != null)
                    {
                        return implicitResourceString;
                    }
                    implicitResourceString = this.GetExplicitResourceString(key, defaultValue, true);
                    if (implicitResourceString != null)
                    {
                        return implicitResourceString;
                    }
                }
                return defaultValue;
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "Item" }));
                }
                if (this._attributes == null)
                {
                    this._attributes = new NameValueCollection();
                }
                this._attributes[key] = value;
            }
        }

        public string Key
        {
            get
            {
                return this._key;
            }
        }

        public virtual SiteMapNode NextSibling
        {
            get
            {
                IList siblingNodes = this.SiblingNodes;
                if (siblingNodes != null)
                {
                    int index = siblingNodes.IndexOf(this);
                    if ((index >= 0) && (index < (siblingNodes.Count - 1)))
                    {
                        return (SiteMapNode) siblingNodes[index + 1];
                    }
                }
                return null;
            }
        }

        public virtual SiteMapNode ParentNode
        {
            get
            {
                if (this._parentNodeSet)
                {
                    return this._parentNode;
                }
                return this._provider.GetParentNode(this);
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "ParentNode" }));
                }
                this._parentNode = value;
                this._parentNodeSet = true;
            }
        }

        public virtual SiteMapNode PreviousSibling
        {
            get
            {
                IList siblingNodes = this.SiblingNodes;
                if (siblingNodes != null)
                {
                    int index = siblingNodes.IndexOf(this);
                    if ((index > 0) && (index <= (siblingNodes.Count - 1)))
                    {
                        return (SiteMapNode) siblingNodes[index - 1];
                    }
                }
                return null;
            }
        }

        public SiteMapProvider Provider
        {
            get
            {
                return this._provider;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return this._readonly;
            }
            set
            {
                this._readonly = value;
            }
        }

        public string ResourceKey
        {
            get
            {
                return this._resourceKey;
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "ResourceKey" }));
                }
                this._resourceKey = value;
            }
        }

        public IList Roles
        {
            get
            {
                return this._roles;
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "Roles" }));
                }
                this._roles = value;
            }
        }

        public virtual SiteMapNode RootNode
        {
            get
            {
                SiteMapNode rootNode = this._provider.RootProvider.RootNode;
                if (rootNode == null)
                {
                    string name = this._provider.RootProvider.Name;
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapProvider_Invalid_RootNode", new object[] { name }));
                }
                return rootNode;
            }
        }

        private SiteMapNodeCollection SiblingNodes
        {
            get
            {
                SiteMapNode parentNode = this.ParentNode;
                if (parentNode != null)
                {
                    return parentNode.ChildNodes;
                }
                return null;
            }
        }

        bool IHierarchyData.HasChildren
        {
            get
            {
                return this.HasChildNodes;
            }
        }

        object IHierarchyData.Item
        {
            get
            {
                return this;
            }
        }

        string IHierarchyData.Path
        {
            get
            {
                return this.Key;
            }
        }

        string IHierarchyData.Type
        {
            get
            {
                return _siteMapNodeType;
            }
        }

        string INavigateUIData.Description
        {
            get
            {
                return this.Description;
            }
        }

        string INavigateUIData.Name
        {
            get
            {
                return this.Title;
            }
        }

        string INavigateUIData.NavigateUrl
        {
            get
            {
                return this.Url;
            }
        }

        string INavigateUIData.Value
        {
            get
            {
                return this.Title;
            }
        }

        [Localizable(true)]
        public virtual string Title
        {
            get
            {
                if (this._provider.EnableLocalization)
                {
                    string implicitResourceString = this.GetImplicitResourceString("title");
                    if (implicitResourceString != null)
                    {
                        return implicitResourceString;
                    }
                    implicitResourceString = this.GetExplicitResourceString("title", this._title, true);
                    if (implicitResourceString != null)
                    {
                        return implicitResourceString;
                    }
                }
                if (this._title != null)
                {
                    return this._title;
                }
                return string.Empty;
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "Title" }));
                }
                this._title = value;
            }
        }

        public virtual string Url
        {
            get
            {
                if (this._url != null)
                {
                    return this._url;
                }
                return string.Empty;
            }
            set
            {
                if (this._readonly)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("SiteMapNode_readonly", new object[] { "Url" }));
                }
                if (value != null)
                {
                    this._url = value.Trim();
                }
                this._virtualPath = this.CreateVirtualPathFromUrl(this._url);
            }
        }

        internal System.Web.VirtualPath VirtualPath
        {
            get
            {
                return this._virtualPath;
            }
        }
    }
}

