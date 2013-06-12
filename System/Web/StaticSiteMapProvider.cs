namespace System.Web
{
    using System;
    using System.Collections;
    using System.Web.Util;

    public abstract class StaticSiteMapProvider : SiteMapProvider
    {
        private Hashtable _childNodeCollectionTable;
        private Hashtable _keyTable;
        private Hashtable _parentNodeTable;
        private Hashtable _urlTable;

        protected StaticSiteMapProvider()
        {
        }

        protected internal override void AddNode(SiteMapNode node, SiteMapNode parentNode)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            lock (base._lock)
            {
                bool flag = false;
                string url = node.Url;
                if (!string.IsNullOrEmpty(url))
                {
                    if (HttpRuntime.AppDomainAppVirtualPath != null)
                    {
                        if (!UrlPath.IsAbsolutePhysicalPath(url))
                        {
                            url = UrlPath.MakeVirtualPathAppAbsolute(UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, url));
                        }
                        if (this.UrlTable[url] != null)
                        {
                            throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_Multiple_Nodes_With_Identical_Url", new object[] { url }));
                        }
                    }
                    flag = true;
                }
                string key = node.Key;
                if (this.KeyTable.Contains(key))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("XmlSiteMapProvider_Multiple_Nodes_With_Identical_Key", new object[] { key }));
                }
                this.KeyTable[key] = node;
                if (flag)
                {
                    this.UrlTable[url] = node;
                }
                if (parentNode != null)
                {
                    this.ParentNodeTable[node] = parentNode;
                    if (this.ChildNodeCollectionTable[parentNode] == null)
                    {
                        this.ChildNodeCollectionTable[parentNode] = new SiteMapNodeCollection();
                    }
                    ((SiteMapNodeCollection) this.ChildNodeCollectionTable[parentNode]).Add(node);
                }
            }
        }

        public abstract SiteMapNode BuildSiteMap();
        protected virtual void Clear()
        {
            lock (base._lock)
            {
                if (this._childNodeCollectionTable != null)
                {
                    this._childNodeCollectionTable.Clear();
                }
                if (this._urlTable != null)
                {
                    this._urlTable.Clear();
                }
                if (this._parentNodeTable != null)
                {
                    this._parentNodeTable.Clear();
                }
                if (this._keyTable != null)
                {
                    this._keyTable.Clear();
                }
            }
        }

        public override SiteMapNode FindSiteMapNode(string rawUrl)
        {
            if (rawUrl == null)
            {
                throw new ArgumentNullException("rawUrl");
            }
            rawUrl = rawUrl.Trim();
            if (rawUrl.Length == 0)
            {
                return null;
            }
            if (UrlPath.IsAppRelativePath(rawUrl))
            {
                rawUrl = UrlPath.MakeVirtualPathAppAbsolute(rawUrl);
            }
            this.BuildSiteMap();
            return base.ReturnNodeIfAccessible((SiteMapNode) this.UrlTable[rawUrl]);
        }

        public override SiteMapNode FindSiteMapNodeFromKey(string key)
        {
            SiteMapNode node = base.FindSiteMapNodeFromKey(key);
            if (node == null)
            {
                node = (SiteMapNode) this.KeyTable[key];
            }
            return base.ReturnNodeIfAccessible(node);
        }

        public override SiteMapNodeCollection GetChildNodes(SiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this.BuildSiteMap();
            SiteMapNodeCollection collection = (SiteMapNodeCollection) this.ChildNodeCollectionTable[node];
            if (collection == null)
            {
                SiteMapNode node2 = (SiteMapNode) this.KeyTable[node.Key];
                if (node2 != null)
                {
                    collection = (SiteMapNodeCollection) this.ChildNodeCollectionTable[node2];
                }
            }
            if (collection == null)
            {
                return SiteMapNodeCollection.Empty;
            }
            if (!base.SecurityTrimmingEnabled)
            {
                return SiteMapNodeCollection.ReadOnly(collection);
            }
            HttpContext current = HttpContext.Current;
            SiteMapNodeCollection nodes2 = new SiteMapNodeCollection(collection.Count);
            foreach (SiteMapNode node3 in collection)
            {
                if (node3.IsAccessibleToUser(current))
                {
                    nodes2.Add(node3);
                }
            }
            return SiteMapNodeCollection.ReadOnly(nodes2);
        }

        public override SiteMapNode GetParentNode(SiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this.BuildSiteMap();
            SiteMapNode parentNode = (SiteMapNode) this.ParentNodeTable[node];
            if (parentNode == null)
            {
                SiteMapNode node3 = (SiteMapNode) this.KeyTable[node.Key];
                if (node3 != null)
                {
                    parentNode = (SiteMapNode) this.ParentNodeTable[node3];
                }
            }
            if ((parentNode == null) && (this.ParentProvider != null))
            {
                parentNode = this.ParentProvider.GetParentNode(node);
            }
            return base.ReturnNodeIfAccessible(parentNode);
        }

        protected internal override void RemoveNode(SiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            lock (base._lock)
            {
                SiteMapNode node2 = (SiteMapNode) this.ParentNodeTable[node];
                if (this.ParentNodeTable.Contains(node))
                {
                    this.ParentNodeTable.Remove(node);
                }
                if (node2 != null)
                {
                    SiteMapNodeCollection nodes = (SiteMapNodeCollection) this.ChildNodeCollectionTable[node2];
                    if ((nodes != null) && nodes.Contains(node))
                    {
                        nodes.Remove(node);
                    }
                }
                string url = node.Url;
                if (((url != null) && (url.Length > 0)) && this.UrlTable.Contains(url))
                {
                    this.UrlTable.Remove(url);
                }
                string key = node.Key;
                if (this.KeyTable.Contains(key))
                {
                    this.KeyTable.Remove(key);
                }
            }
        }

        internal IDictionary ChildNodeCollectionTable
        {
            get
            {
                if (this._childNodeCollectionTable == null)
                {
                    lock (base._lock)
                    {
                        if (this._childNodeCollectionTable == null)
                        {
                            this._childNodeCollectionTable = new Hashtable();
                        }
                    }
                }
                return this._childNodeCollectionTable;
            }
        }

        internal IDictionary KeyTable
        {
            get
            {
                if (this._keyTable == null)
                {
                    lock (base._lock)
                    {
                        if (this._keyTable == null)
                        {
                            this._keyTable = new Hashtable();
                        }
                    }
                }
                return this._keyTable;
            }
        }

        internal IDictionary ParentNodeTable
        {
            get
            {
                if (this._parentNodeTable == null)
                {
                    lock (base._lock)
                    {
                        if (this._parentNodeTable == null)
                        {
                            this._parentNodeTable = new Hashtable();
                        }
                    }
                }
                return this._parentNodeTable;
            }
        }

        internal IDictionary UrlTable
        {
            get
            {
                if (this._urlTable == null)
                {
                    lock (base._lock)
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

