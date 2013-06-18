namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    [ParseChildren(true), WebSysDescription("SiteMapDataSource_Description"), WebSysDisplayName("SiteMapDataSource_DisplayName"), Designer("System.Web.UI.Design.WebControls.SiteMapDataSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ToolboxBitmap(typeof(SiteMapDataSource)), PersistChildren(false)]
    public class SiteMapDataSource : HierarchicalDataSourceControl, IDataSource, IListSource
    {
        private SiteMapDataSourceView _dataSourceView;
        private System.Web.SiteMapProvider _provider;
        private ICollection _viewNames;
        private const string DefaultViewName = "DefaultView";

        event EventHandler IDataSource.DataSourceChanged
        {
            add
            {
                this.DataSourceChanged += value;
            }
            remove
            {
                this.DataSourceChanged -= value;
            }
        }

        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
            if (this.Provider == null)
            {
                throw new HttpException(System.Web.SR.GetString("SiteMapDataSource_ProviderNotFound", new object[] { this.SiteMapProvider }));
            }
            return this.GetTreeView(viewPath);
        }

        public virtual IList GetList()
        {
            return ListSourceHelper.GetList(this);
        }

        private SiteMapNodeCollection GetNodes()
        {
            SiteMapNode currentNode = null;
            int startingNodeOffset = this.StartingNodeOffset;
            if (!string.IsNullOrEmpty(this.StartingNodeUrl) && this.StartFromCurrentNode)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("SiteMapDataSource_StartingNodeUrlAndStartFromcurrentNode_Defined"));
            }
            if (this.StartFromCurrentNode)
            {
                currentNode = this.Provider.CurrentNode;
            }
            else if (!string.IsNullOrEmpty(this.StartingNodeUrl))
            {
                currentNode = this.Provider.FindSiteMapNode(this.MakeUrlAbsolute(this.StartingNodeUrl));
                if (currentNode == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("SiteMapPath_CannotFindUrl", new object[] { this.StartingNodeUrl }));
                }
            }
            else
            {
                currentNode = this.Provider.RootNode;
            }
            if (currentNode == null)
            {
                return null;
            }
            if (startingNodeOffset <= 0)
            {
                if (startingNodeOffset != 0)
                {
                    this.Provider.HintNeighborhoodNodes(currentNode, Math.Abs(startingNodeOffset), 0);
                    SiteMapNode parentNode = currentNode.ParentNode;
                    while ((startingNodeOffset < 0) && (parentNode != null))
                    {
                        currentNode = currentNode.ParentNode;
                        parentNode = currentNode.ParentNode;
                        startingNodeOffset++;
                    }
                }
                return this.GetNodes(currentNode);
            }
            SiteMapNode currentNodeAndHintAncestorNodes = this.Provider.GetCurrentNodeAndHintAncestorNodes(-1);
            if (((currentNodeAndHintAncestorNodes == null) || !currentNodeAndHintAncestorNodes.IsDescendantOf(currentNode)) || currentNodeAndHintAncestorNodes.Equals(currentNode))
            {
                return null;
            }
            SiteMapNode node4 = currentNodeAndHintAncestorNodes;
            for (int i = 0; i < startingNodeOffset; i++)
            {
                node4 = node4.ParentNode;
                if ((node4 == null) || node4.Equals(currentNode))
                {
                    return this.GetNodes(currentNodeAndHintAncestorNodes);
                }
            }
            SiteMapNode node5 = currentNodeAndHintAncestorNodes;
            while ((node4 != null) && !node4.Equals(currentNode))
            {
                node5 = node5.ParentNode;
                node4 = node4.ParentNode;
            }
            return this.GetNodes(node5);
        }

        private SiteMapNodeCollection GetNodes(SiteMapNode node)
        {
            if (this.ShowStartingNode)
            {
                return new SiteMapNodeCollection(node);
            }
            return node.ChildNodes;
        }

        internal SiteMapNodeCollection GetPathNodeCollection(string viewPath)
        {
            SiteMapNodeCollection childNodes = null;
            if (string.IsNullOrEmpty(viewPath))
            {
                childNodes = this.GetNodes();
            }
            else
            {
                SiteMapNode node = this.Provider.FindSiteMapNodeFromKey(viewPath);
                if (node != null)
                {
                    childNodes = node.ChildNodes;
                }
            }
            if (childNodes == null)
            {
                childNodes = SiteMapNodeCollection.Empty;
            }
            return childNodes;
        }

        private HierarchicalDataSourceView GetTreeView(string viewPath)
        {
            SiteMapNode node = null;
            if (string.IsNullOrEmpty(viewPath))
            {
                SiteMapNodeCollection nodes = this.GetNodes();
                if (nodes != null)
                {
                    return nodes.GetHierarchicalDataSourceView();
                }
            }
            else
            {
                node = this.Provider.FindSiteMapNodeFromKey(viewPath);
                if (node != null)
                {
                    return node.ChildNodes.GetHierarchicalDataSourceView();
                }
            }
            return SiteMapNodeCollection.Empty.GetHierarchicalDataSourceView();
        }

        public virtual DataSourceView GetView(string viewName)
        {
            if (this.Provider == null)
            {
                throw new HttpException(System.Web.SR.GetString("SiteMapDataSource_ProviderNotFound", new object[] { this.SiteMapProvider }));
            }
            if (this._dataSourceView == null)
            {
                this._dataSourceView = SiteMapNodeCollection.ReadOnly(this.GetPathNodeCollection(viewName)).GetDataSourceView(this, string.Empty);
            }
            return this._dataSourceView;
        }

        public virtual ICollection GetViewNames()
        {
            if (this._viewNames == null)
            {
                this._viewNames = new string[] { "DefaultView" };
            }
            return this._viewNames;
        }

        private string MakeUrlAbsolute(string url)
        {
            if ((url.Length == 0) || !UrlPath.IsRelativeUrl(url))
            {
                return url;
            }
            string appRelativeTemplateSourceDirectory = base.AppRelativeTemplateSourceDirectory;
            if (appRelativeTemplateSourceDirectory.Length == 0)
            {
                return url;
            }
            return UrlPath.Combine(appRelativeTemplateSourceDirectory, url);
        }

        IList IListSource.GetList()
        {
            if (base.DesignMode)
            {
                return null;
            }
            return this.GetList();
        }

        DataSourceView IDataSource.GetView(string viewName)
        {
            return this.GetView(viewName);
        }

        ICollection IDataSource.GetViewNames()
        {
            return this.GetViewNames();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebSysDescription("SiteMapDataSource_ContainsListCollection")]
        public virtual bool ContainsListCollection
        {
            get
            {
                return ListSourceHelper.ContainsListCollection(this);
            }
        }

        [WebSysDescription("SiteMapDataSource_Provider"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.SiteMapProvider Provider
        {
            get
            {
                if (this._provider == null)
                {
                    if (string.IsNullOrEmpty(this.SiteMapProvider))
                    {
                        this._provider = SiteMap.Provider;
                        if (this._provider == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("SiteMapDataSource_DefaultProviderNotFound"));
                        }
                    }
                    else
                    {
                        this._provider = SiteMap.Providers[this.SiteMapProvider];
                        if (this._provider == null)
                        {
                            throw new HttpException(System.Web.SR.GetString("SiteMapDataSource_ProviderNotFound", new object[] { this.SiteMapProvider }));
                        }
                    }
                }
                return this._provider;
            }
            set
            {
                if (this._provider != value)
                {
                    this._provider = value;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [WebSysDescription("SiteMapDataSource_ShowStartingNode"), DefaultValue(true), WebCategory("Behavior")]
        public virtual bool ShowStartingNode
        {
            get
            {
                object obj2 = this.ViewState["ShowStartingNode"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                if (value != this.ShowStartingNode)
                {
                    this.ViewState["ShowStartingNode"] = value;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(""), WebSysDescription("SiteMapDataSource_SiteMapProvider"), WebCategory("Behavior")]
        public virtual string SiteMapProvider
        {
            get
            {
                string str = this.ViewState["SiteMapProvider"] as string;
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                if (value != this.SiteMapProvider)
                {
                    this._provider = null;
                    this.ViewState["SiteMapProvider"] = value;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [WebCategory("Behavior"), DefaultValue(false), WebSysDescription("SiteMapDataSource_StartFromCurrentNode")]
        public virtual bool StartFromCurrentNode
        {
            get
            {
                object obj2 = this.ViewState["StartFromCurrentNode "];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                if (value != this.StartFromCurrentNode)
                {
                    this.ViewState["StartFromCurrentNode "] = value;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [WebSysDescription("SiteMapDataSource_StartingNodeOffset"), DefaultValue(0), WebCategory("Behavior")]
        public virtual int StartingNodeOffset
        {
            get
            {
                object obj2 = this.ViewState["StartingNodeOffset"];
                if (obj2 == null)
                {
                    return 0;
                }
                return (int) obj2;
            }
            set
            {
                if (value != this.StartingNodeOffset)
                {
                    this.ViewState["StartingNodeOffset"] = value;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(""), WebSysDescription("SiteMapDataSource_StartingNodeUrl"), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("Behavior")]
        public virtual string StartingNodeUrl
        {
            get
            {
                string str = this.ViewState["StartingNodeUrl"] as string;
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                if (value != this.StartingNodeUrl)
                {
                    this.ViewState["StartingNodeUrl"] = value;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                if (base.DesignMode)
                {
                    return false;
                }
                return this.ContainsListCollection;
            }
        }
    }
}

