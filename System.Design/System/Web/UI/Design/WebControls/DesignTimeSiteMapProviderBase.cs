namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class DesignTimeSiteMapProviderBase : StaticSiteMapProvider
    {
        private static readonly string _childNodeText = System.Design.SR.GetString("DesignTimeSiteMapProvider_ChildNodeText");
        private static readonly string _childNodeText1 = (_childNodeText + " 1");
        private static readonly string _childNodeText2 = (_childNodeText + " 2");
        private static readonly string _childNodeText3 = (_childNodeText + " 3");
        private SiteMapNode _currentNode;
        private static readonly string _currentNodeText = System.Design.SR.GetString("DesignTimeSiteMapProvider_CurrentNodeText");
        protected IDesignerHost _host;
        private static readonly string _parentNodeText = System.Design.SR.GetString("DesignTimeSiteMapProvider_ParentNodeText");
        private SiteMapNode _rootNode;
        private static readonly string _rootNodeText = System.Design.SR.GetString("DesignTimeSiteMapProvider_RootNodeText");
        private static readonly string _siblingNodeText = System.Design.SR.GetString("DesignTimeSiteMapProvider_SiblingNodeText");
        private static readonly string _siblingNodeText1 = (_siblingNodeText + " 1");
        private static readonly string _siblingNodeText2 = (_siblingNodeText + " 2");
        private static readonly string _siblingNodeText3 = (_siblingNodeText + " 3");

        internal DesignTimeSiteMapProviderBase(IDesignerHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException("host");
            }
            this._host = host;
        }

        private SiteMapNode BuildDesignTimeSiteMapInternal()
        {
            if (this._rootNode == null)
            {
                this._rootNode = new SiteMapNode(this, _rootNodeText + " url", _rootNodeText + " url", _rootNodeText, _rootNodeText);
                this._currentNode = new SiteMapNode(this, _currentNodeText + " url", _currentNodeText + " url", _currentNodeText, _currentNodeText);
                SiteMapNode node = this.CreateNewSiteMapNode(_parentNodeText);
                SiteMapNode node2 = this.CreateNewSiteMapNode(_siblingNodeText1);
                SiteMapNode node3 = this.CreateNewSiteMapNode(_siblingNodeText2);
                SiteMapNode node4 = this.CreateNewSiteMapNode(_siblingNodeText3);
                SiteMapNode node5 = this.CreateNewSiteMapNode(_childNodeText1);
                SiteMapNode node6 = this.CreateNewSiteMapNode(_childNodeText2);
                SiteMapNode node7 = this.CreateNewSiteMapNode(_childNodeText3);
                this.AddNode(this._rootNode);
                this.AddNode(node, this._rootNode);
                this.AddNode(node2, node);
                this.AddNode(this._currentNode, node);
                this.AddNode(node3, node);
                this.AddNode(node4, node);
                this.AddNode(node5, this._currentNode);
                this.AddNode(node6, this._currentNode);
                this.AddNode(node7, this._currentNode);
            }
            return this._rootNode;
        }

        public override SiteMapNode BuildSiteMap()
        {
            return this.BuildDesignTimeSiteMapInternal();
        }

        private SiteMapNode CreateNewSiteMapNode(string text)
        {
            string key = text + "url";
            return new SiteMapNode(this, key, key, text, text);
        }

        protected internal override SiteMapNode GetRootNodeCore()
        {
            this.BuildDesignTimeSiteMapInternal();
            return this._rootNode;
        }

        public override SiteMapNode CurrentNode
        {
            get
            {
                this.BuildDesignTimeSiteMapInternal();
                return this._currentNode;
            }
        }

        internal string DocumentAppRelativeUrl
        {
            get
            {
                if (this._host != null)
                {
                    IComponent rootComponent = this._host.RootComponent;
                    if (rootComponent != null)
                    {
                        WebFormsRootDesigner designer = this._host.GetDesigner(rootComponent) as WebFormsRootDesigner;
                        if (designer != null)
                        {
                            return designer.DocumentUrl;
                        }
                    }
                }
                return string.Empty;
            }
        }
    }
}

