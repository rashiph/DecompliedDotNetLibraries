namespace System.Web
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Threading;
    using System.Web.UI;
    using System.Web.Util;

    public abstract class SiteMapProvider : ProviderBase
    {
        private const string _allRoles = "*";
        private bool _enableLocalization;
        internal readonly object _lock = new object();
        private SiteMapProvider _parentProvider;
        private object _resolutionTicket = new object();
        private string _resourceKey;
        private SiteMapProvider _rootProvider;
        private bool _securityTrimmingEnabled;
        internal const string _securityTrimmingEnabledAttrName = "securityTrimmingEnabled";

        public event SiteMapResolveEventHandler SiteMapResolve;

        protected SiteMapProvider()
        {
        }

        protected virtual void AddNode(SiteMapNode node)
        {
            this.AddNode(node, null);
        }

        protected internal virtual void AddNode(SiteMapNode node, SiteMapNode parentNode)
        {
            throw new NotImplementedException();
        }

        public abstract SiteMapNode FindSiteMapNode(string rawUrl);
        public virtual SiteMapNode FindSiteMapNode(HttpContext context)
        {
            if (context == null)
            {
                return null;
            }
            string rawUrl = context.Request.RawUrl;
            SiteMapNode node = null;
            node = this.FindSiteMapNode(rawUrl);
            if (node == null)
            {
                int index = rawUrl.IndexOf("?", StringComparison.Ordinal);
                if (index != -1)
                {
                    node = this.FindSiteMapNode(rawUrl.Substring(0, index));
                }
                if (node != null)
                {
                    return node;
                }
                Page currentHandler = context.CurrentHandler as Page;
                if (currentHandler != null)
                {
                    string clientQueryString = currentHandler.ClientQueryString;
                    if (clientQueryString.Length > 0)
                    {
                        node = this.FindSiteMapNode(context.Request.Path + "?" + clientQueryString);
                    }
                }
                if (node == null)
                {
                    node = this.FindSiteMapNode(context.Request.Path);
                }
            }
            return node;
        }

        public virtual SiteMapNode FindSiteMapNodeFromKey(string key)
        {
            return this.FindSiteMapNode(key);
        }

        public abstract SiteMapNodeCollection GetChildNodes(SiteMapNode node);
        public virtual SiteMapNode GetCurrentNodeAndHintAncestorNodes(int upLevel)
        {
            if (upLevel < -1)
            {
                throw new ArgumentOutOfRangeException("upLevel");
            }
            return this.CurrentNode;
        }

        public virtual SiteMapNode GetCurrentNodeAndHintNeighborhoodNodes(int upLevel, int downLevel)
        {
            if (upLevel < -1)
            {
                throw new ArgumentOutOfRangeException("upLevel");
            }
            if (downLevel < -1)
            {
                throw new ArgumentOutOfRangeException("downLevel");
            }
            return this.CurrentNode;
        }

        public abstract SiteMapNode GetParentNode(SiteMapNode node);
        public virtual SiteMapNode GetParentNodeRelativeToCurrentNodeAndHintDownFromParent(int walkupLevels, int relativeDepthFromWalkup)
        {
            if (walkupLevels < 0)
            {
                throw new ArgumentOutOfRangeException("walkupLevels");
            }
            if (relativeDepthFromWalkup < 0)
            {
                throw new ArgumentOutOfRangeException("relativeDepthFromWalkup");
            }
            SiteMapNode currentNodeAndHintAncestorNodes = this.GetCurrentNodeAndHintAncestorNodes(walkupLevels);
            if (currentNodeAndHintAncestorNodes == null)
            {
                return null;
            }
            SiteMapNode parentNodesInternal = this.GetParentNodesInternal(currentNodeAndHintAncestorNodes, walkupLevels);
            if (parentNodesInternal == null)
            {
                return null;
            }
            this.HintNeighborhoodNodes(parentNodesInternal, 0, relativeDepthFromWalkup);
            return parentNodesInternal;
        }

        public virtual SiteMapNode GetParentNodeRelativeToNodeAndHintDownFromParent(SiteMapNode node, int walkupLevels, int relativeDepthFromWalkup)
        {
            if (walkupLevels < 0)
            {
                throw new ArgumentOutOfRangeException("walkupLevels");
            }
            if (relativeDepthFromWalkup < 0)
            {
                throw new ArgumentOutOfRangeException("relativeDepthFromWalkup");
            }
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            this.HintAncestorNodes(node, walkupLevels);
            SiteMapNode parentNodesInternal = this.GetParentNodesInternal(node, walkupLevels);
            if (parentNodesInternal == null)
            {
                return null;
            }
            this.HintNeighborhoodNodes(parentNodesInternal, 0, relativeDepthFromWalkup);
            return parentNodesInternal;
        }

        private SiteMapNode GetParentNodesInternal(SiteMapNode node, int walkupLevels)
        {
            if (walkupLevels > 0)
            {
                do
                {
                    node = node.ParentNode;
                    walkupLevels--;
                }
                while ((node != null) && (walkupLevels != 0));
            }
            return node;
        }

        protected internal abstract SiteMapNode GetRootNodeCore();
        protected static SiteMapNode GetRootNodeCoreFromProvider(SiteMapProvider provider)
        {
            return provider.GetRootNodeCore();
        }

        public virtual void HintAncestorNodes(SiteMapNode node, int upLevel)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (upLevel < -1)
            {
                throw new ArgumentOutOfRangeException("upLevel");
            }
        }

        public virtual void HintNeighborhoodNodes(SiteMapNode node, int upLevel, int downLevel)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (upLevel < -1)
            {
                throw new ArgumentOutOfRangeException("upLevel");
            }
            if (downLevel < -1)
            {
                throw new ArgumentOutOfRangeException("downLevel");
            }
        }

        public override void Initialize(string name, NameValueCollection attributes)
        {
            if (attributes != null)
            {
                if (string.IsNullOrEmpty(attributes["description"]))
                {
                    attributes.Remove("description");
                    attributes.Add("description", base.GetType().Name);
                }
                ProviderUtil.GetAndRemoveBooleanAttribute(attributes, "securityTrimmingEnabled", this.Name, ref this._securityTrimmingEnabled);
            }
            base.Initialize(name, attributes);
        }

        public virtual bool IsAccessibleToUser(HttpContext context, SiteMapNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (!this.SecurityTrimmingEnabled)
            {
                return true;
            }
            if (node.Roles != null)
            {
                foreach (string str in node.Roles)
                {
                    if ((str == "*") || ((context.User != null) && context.User.IsInRole(str)))
                    {
                        return true;
                    }
                }
            }
            VirtualPath virtualPath = node.VirtualPath;
            return (((virtualPath != null) && virtualPath.IsWithinAppRoot) && Util.IsUserAllowedToPath(context, virtualPath));
        }

        protected internal virtual void RemoveNode(SiteMapNode node)
        {
            throw new NotImplementedException();
        }

        protected SiteMapNode ResolveSiteMapNode(HttpContext context)
        {
            SiteMapResolveEventHandler siteMapResolve = this.SiteMapResolve;
            if ((siteMapResolve != null) && !context.Items.Contains(this._resolutionTicket))
            {
                context.Items.Add(this._resolutionTicket, true);
                try
                {
                    Delegate[] invocationList = siteMapResolve.GetInvocationList();
                    int length = invocationList.Length;
                    for (int i = 0; i < length; i++)
                    {
                        SiteMapNode node = ((SiteMapResolveEventHandler) invocationList[i])(this, new SiteMapResolveEventArgs(context, this));
                        if (node != null)
                        {
                            return node;
                        }
                    }
                }
                finally
                {
                    context.Items.Remove(this._resolutionTicket);
                }
            }
            return null;
        }

        internal SiteMapNode ReturnNodeIfAccessible(SiteMapNode node)
        {
            if ((node != null) && node.IsAccessibleToUser(HttpContext.Current))
            {
                return node;
            }
            return null;
        }

        public virtual SiteMapNode CurrentNode
        {
            get
            {
                HttpContext current = HttpContext.Current;
                SiteMapNode node = null;
                node = this.ResolveSiteMapNode(current);
                if (node == null)
                {
                    node = this.FindSiteMapNode(current);
                }
                return this.ReturnNodeIfAccessible(node);
            }
        }

        public bool EnableLocalization
        {
            get
            {
                return this._enableLocalization;
            }
            set
            {
                this._enableLocalization = value;
            }
        }

        public virtual SiteMapProvider ParentProvider
        {
            get
            {
                return this._parentProvider;
            }
            set
            {
                this._parentProvider = value;
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
                this._resourceKey = value;
            }
        }

        public virtual SiteMapNode RootNode
        {
            get
            {
                SiteMapNode rootNodeCore = this.GetRootNodeCore();
                return this.ReturnNodeIfAccessible(rootNodeCore);
            }
        }

        public virtual SiteMapProvider RootProvider
        {
            get
            {
                if (this._rootProvider == null)
                {
                    lock (this._lock)
                    {
                        if (this._rootProvider == null)
                        {
                            Hashtable hashtable = new Hashtable();
                            SiteMapProvider key = this;
                            hashtable.Add(key, null);
                            while (key.ParentProvider != null)
                            {
                                if (hashtable.Contains(key.ParentProvider))
                                {
                                    throw new ProviderException(System.Web.SR.GetString("SiteMapProvider_Circular_Provider"));
                                }
                                key = key.ParentProvider;
                                hashtable.Add(key, null);
                            }
                            this._rootProvider = key;
                        }
                    }
                }
                return this._rootProvider;
            }
        }

        public bool SecurityTrimmingEnabled
        {
            get
            {
                return this._securityTrimmingEnabled;
            }
        }
    }
}

