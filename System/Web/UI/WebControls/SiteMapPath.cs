namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [Designer("System.Web.UI.Design.WebControls.SiteMapPathDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class SiteMapPath : CompositeControl
    {
        private const string _afterSiteMapPathMark = "_SkipLink";
        private Style _currentNodeStyle;
        private ITemplate _currentNodeTemplate;
        private const string _defaultSeparator = " > ";
        private static readonly object _eventItemCreated = new object();
        private static readonly object _eventItemDataBound = new object();
        private Style _mergedCurrentNodeStyle;
        private Style _mergedRootNodeStyle;
        private Style _nodeStyle;
        private ITemplate _nodeTemplate;
        private Style _pathSeparatorStyle;
        private ITemplate _pathSeparatorTemplate;
        private System.Web.SiteMapProvider _provider;
        private Style _rootNodeStyle;
        private ITemplate _rootNodeTemplate;

        [WebSysDescription("DataControls_OnItemCreated"), WebCategory("Action")]
        public event SiteMapNodeItemEventHandler ItemCreated
        {
            add
            {
                base.Events.AddHandler(_eventItemCreated, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventItemCreated, value);
            }
        }

        [WebSysDescription("SiteMapPath_OnItemDataBound"), WebCategory("Action")]
        public event SiteMapNodeItemEventHandler ItemDataBound
        {
            add
            {
                base.Events.AddHandler(_eventItemDataBound, value);
            }
            remove
            {
                base.Events.RemoveHandler(_eventItemDataBound, value);
            }
        }

        private void CopyStyle(Style toStyle, Style fromStyle)
        {
            if ((fromStyle != null) && fromStyle.IsSet(0x2000))
            {
                toStyle.Font.Underline = fromStyle.Font.Underline;
            }
            toStyle.CopyFrom(fromStyle);
        }

        protected internal override void CreateChildControls()
        {
            this.Controls.Clear();
            this.CreateControlHierarchy();
            base.ClearChildState();
        }

        protected virtual void CreateControlHierarchy()
        {
            if (this.Provider != null)
            {
                int index = 0;
                this.CreateMergedStyles();
                SiteMapNode currentNodeAndHintAncestorNodes = this.Provider.GetCurrentNodeAndHintAncestorNodes(-1);
                if (currentNodeAndHintAncestorNodes != null)
                {
                    SiteMapNode parentNode = currentNodeAndHintAncestorNodes.ParentNode;
                    if (parentNode != null)
                    {
                        this.CreateControlHierarchyRecursive(ref index, parentNode, this.ParentLevelsDisplayed);
                    }
                    this.CreateItem(index++, SiteMapNodeItemType.Current, currentNodeAndHintAncestorNodes);
                }
            }
        }

        private void CreateControlHierarchyRecursive(ref int index, SiteMapNode node, int parentLevels)
        {
            if (parentLevels != 0)
            {
                SiteMapNode parentNode = node.ParentNode;
                if (parentNode != null)
                {
                    this.CreateControlHierarchyRecursive(ref index, parentNode, parentLevels - 1);
                    this.CreateItem(index++, SiteMapNodeItemType.Parent, node);
                }
                else
                {
                    this.CreateItem(index++, SiteMapNodeItemType.Root, node);
                }
                this.CreateItem(index, SiteMapNodeItemType.PathSeparator, null);
            }
        }

        private SiteMapNodeItem CreateItem(int itemIndex, SiteMapNodeItemType itemType, SiteMapNode node)
        {
            SiteMapNodeItem item = new SiteMapNodeItem(itemIndex, itemType);
            int index = (this.PathDirection == System.Web.UI.WebControls.PathDirection.CurrentToRoot) ? 0 : -1;
            SiteMapNodeItemEventArgs e = new SiteMapNodeItemEventArgs(item);
            item.SiteMapNode = node;
            this.InitializeItem(item);
            this.OnItemCreated(e);
            this.Controls.AddAt(index, item);
            item.DataBind();
            this.OnItemDataBound(e);
            item.SiteMapNode = null;
            item.EnableViewState = false;
            return item;
        }

        private void CreateMergedStyles()
        {
            this._mergedCurrentNodeStyle = new Style();
            this.CopyStyle(this._mergedCurrentNodeStyle, this._nodeStyle);
            this.CopyStyle(this._mergedCurrentNodeStyle, this._currentNodeStyle);
            this._mergedRootNodeStyle = new Style();
            this.CopyStyle(this._mergedRootNodeStyle, this._nodeStyle);
            this.CopyStyle(this._mergedRootNodeStyle, this._rootNodeStyle);
        }

        public override void DataBind()
        {
            this.OnDataBinding(EventArgs.Empty);
        }

        protected virtual void InitializeItem(SiteMapNodeItem item)
        {
            ITemplate nodeTemplate = null;
            Style s = null;
            SiteMapNodeItemType itemType = item.ItemType;
            SiteMapNode siteMapNode = item.SiteMapNode;
            switch (itemType)
            {
                case SiteMapNodeItemType.Root:
                    nodeTemplate = (this.RootNodeTemplate != null) ? this.RootNodeTemplate : this.NodeTemplate;
                    s = this._mergedRootNodeStyle;
                    break;

                case SiteMapNodeItemType.Parent:
                    nodeTemplate = this.NodeTemplate;
                    s = this._nodeStyle;
                    break;

                case SiteMapNodeItemType.Current:
                    nodeTemplate = (this.CurrentNodeTemplate != null) ? this.CurrentNodeTemplate : this.NodeTemplate;
                    s = this._mergedCurrentNodeStyle;
                    break;

                case SiteMapNodeItemType.PathSeparator:
                    nodeTemplate = this.PathSeparatorTemplate;
                    s = this._pathSeparatorStyle;
                    break;
            }
            if (nodeTemplate == null)
            {
                if (itemType == SiteMapNodeItemType.PathSeparator)
                {
                    Literal child = new Literal {
                        Mode = LiteralMode.Encode,
                        Text = this.PathSeparator
                    };
                    item.Controls.Add(child);
                    item.ApplyStyle(s);
                }
                else if ((itemType == SiteMapNodeItemType.Current) && !this.RenderCurrentNodeAsLink)
                {
                    Literal literal2 = new Literal {
                        Mode = LiteralMode.Encode,
                        Text = siteMapNode.Title
                    };
                    item.Controls.Add(literal2);
                    item.ApplyStyle(s);
                }
                else
                {
                    HyperLink link = new HyperLink();
                    if ((s != null) && s.IsSet(0x2000))
                    {
                        link.Font.Underline = s.Font.Underline;
                    }
                    link.EnableTheming = false;
                    link.Enabled = this.Enabled;
                    if (siteMapNode.Url.StartsWith(@"\\", StringComparison.Ordinal))
                    {
                        link.NavigateUrl = base.ResolveClientUrl(HttpUtility.UrlPathEncode(siteMapNode.Url));
                    }
                    else
                    {
                        link.NavigateUrl = (this.Context != null) ? this.Context.Response.ApplyAppPathModifier(base.ResolveClientUrl(HttpUtility.UrlPathEncode(siteMapNode.Url))) : siteMapNode.Url;
                    }
                    link.Text = HttpUtility.HtmlEncode(siteMapNode.Title);
                    if (this.ShowToolTips)
                    {
                        link.ToolTip = siteMapNode.Description;
                    }
                    item.Controls.Add(link);
                    link.ApplyStyle(s);
                }
            }
            else
            {
                nodeTemplate.InstantiateIn(item);
                item.ApplyStyle(s);
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] objArray = (object[]) savedState;
                base.LoadViewState(objArray[0]);
                if (objArray[1] != null)
                {
                    ((IStateManager) this.CurrentNodeStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.NodeStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.RootNodeStyle).LoadViewState(objArray[3]);
                }
                if (objArray[4] != null)
                {
                    ((IStateManager) this.PathSeparatorStyle).LoadViewState(objArray[4]);
                }
            }
            else
            {
                base.LoadViewState(null);
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);
            this.Controls.Clear();
            base.ClearChildState();
            this.CreateControlHierarchy();
            base.ChildControlsCreated = true;
        }

        protected virtual void OnItemCreated(SiteMapNodeItemEventArgs e)
        {
            SiteMapNodeItemEventHandler handler = (SiteMapNodeItemEventHandler) base.Events[_eventItemCreated];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnItemDataBound(SiteMapNodeItemEventArgs e)
        {
            SiteMapNodeItemEventHandler handler = (SiteMapNodeItemEventHandler) base.Events[_eventItemDataBound];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if (base.DesignMode)
            {
                base.ChildControlsCreated = false;
                this.EnsureChildControls();
            }
            base.Render(writer);
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            bool flag = !string.IsNullOrEmpty(this.SkipLinkText) && !base.DesignMode;
            string str = this.ClientID + "_SkipLink";
            if (flag)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, "#" + str);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, this.SkipLinkText);
                writer.AddAttribute(HtmlTextWriterAttribute.Height, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "0");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
                writer.AddAttribute(HtmlTextWriterAttribute.Src, base.SpacerImageUrl);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            base.RenderContents(writer);
            if (flag)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, str);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.RenderEndTag();
            }
        }

        protected override object SaveViewState()
        {
            object[] objArray = new object[] { base.SaveViewState(), (this._currentNodeStyle != null) ? ((IStateManager) this._currentNodeStyle).SaveViewState() : null, (this._nodeStyle != null) ? ((IStateManager) this._nodeStyle).SaveViewState() : null, (this._rootNodeStyle != null) ? ((IStateManager) this._rootNodeStyle).SaveViewState() : null, (this._pathSeparatorStyle != null) ? ((IStateManager) this._pathSeparatorStyle).SaveViewState() : null };
            for (int i = 0; i < objArray.Length; i++)
            {
                if (objArray[i] != null)
                {
                    return objArray;
                }
            }
            return null;
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            if (this._currentNodeStyle != null)
            {
                ((IStateManager) this._currentNodeStyle).TrackViewState();
            }
            if (this._nodeStyle != null)
            {
                ((IStateManager) this._nodeStyle).TrackViewState();
            }
            if (this._rootNodeStyle != null)
            {
                ((IStateManager) this._rootNodeStyle).TrackViewState();
            }
            if (this._pathSeparatorStyle != null)
            {
                ((IStateManager) this._pathSeparatorStyle).TrackViewState();
            }
        }

        [DefaultValue((string) null), WebSysDescription("SiteMapPath_CurrentNodeStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style CurrentNodeStyle
        {
            get
            {
                if (this._currentNodeStyle == null)
                {
                    this._currentNodeStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._currentNodeStyle).TrackViewState();
                    }
                }
                return this._currentNodeStyle;
            }
        }

        [Browsable(false), WebSysDescription("SiteMapPath_CurrentNodeTemplate"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(SiteMapNodeItem))]
        public virtual ITemplate CurrentNodeTemplate
        {
            get
            {
                return this._currentNodeTemplate;
            }
            set
            {
                this._currentNodeTemplate = value;
            }
        }

        [WebSysDescription("SiteMapPath_NodeStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style NodeStyle
        {
            get
            {
                if (this._nodeStyle == null)
                {
                    this._nodeStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._nodeStyle).TrackViewState();
                    }
                }
                return this._nodeStyle;
            }
        }

        [PersistenceMode(PersistenceMode.InnerProperty), Browsable(false), DefaultValue((string) null), TemplateContainer(typeof(SiteMapNodeItem)), WebSysDescription("SiteMapPath_NodeTemplate")]
        public virtual ITemplate NodeTemplate
        {
            get
            {
                return this._nodeTemplate;
            }
            set
            {
                this._nodeTemplate = value;
            }
        }

        [DefaultValue(-1), WebCategory("Behavior"), Themeable(false), WebSysDescription("SiteMapPath_ParentLevelsDisplayed")]
        public virtual int ParentLevelsDisplayed
        {
            get
            {
                object obj2 = this.ViewState["ParentLevelsDisplayed"];
                if (obj2 == null)
                {
                    return -1;
                }
                return (int) obj2;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["ParentLevelsDisplayed"] = value;
            }
        }

        [WebSysDescription("SiteMapPath_PathDirection"), DefaultValue(0), WebCategory("Appearance")]
        public virtual System.Web.UI.WebControls.PathDirection PathDirection
        {
            get
            {
                object obj2 = this.ViewState["PathDirection"];
                if (obj2 == null)
                {
                    return System.Web.UI.WebControls.PathDirection.RootToCurrent;
                }
                return (System.Web.UI.WebControls.PathDirection) obj2;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.PathDirection.RootToCurrent) || (value > System.Web.UI.WebControls.PathDirection.CurrentToRoot))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["PathDirection"] = value;
            }
        }

        [WebSysDescription("SiteMapPath_PathSeparator"), WebCategory("Appearance"), DefaultValue(" > "), Localizable(true)]
        public virtual string PathSeparator
        {
            get
            {
                string str = (string) this.ViewState["PathSeparator"];
                if (str == null)
                {
                    return " > ";
                }
                return str;
            }
            set
            {
                this.ViewState["PathSeparator"] = value;
            }
        }

        [WebSysDescription("SiteMapPath_PathSeparatorStyle"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style PathSeparatorStyle
        {
            get
            {
                if (this._pathSeparatorStyle == null)
                {
                    this._pathSeparatorStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._pathSeparatorStyle).TrackViewState();
                    }
                }
                return this._pathSeparatorStyle;
            }
        }

        [DefaultValue((string) null), Browsable(false), WebSysDescription("SiteMapPath_PathSeparatorTemplate"), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(SiteMapNodeItem))]
        public virtual ITemplate PathSeparatorTemplate
        {
            get
            {
                return this._pathSeparatorTemplate;
            }
            set
            {
                this._pathSeparatorTemplate = value;
            }
        }

        [Browsable(false), WebSysDescription("SiteMapPath_Provider"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Web.SiteMapProvider Provider
        {
            get
            {
                if ((this._provider == null) && !base.DesignMode)
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
                this._provider = value;
            }
        }

        [WebSysDescription("SiteMapPath_RenderCurrentNodeAsLink"), DefaultValue(false), WebCategory("Appearance")]
        public virtual bool RenderCurrentNodeAsLink
        {
            get
            {
                object obj2 = this.ViewState["RenderCurrentNodeAsLink"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["RenderCurrentNodeAsLink"] = value;
            }
        }

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true), WebSysDescription("SiteMapPath_RootNodeStyle")]
        public Style RootNodeStyle
        {
            get
            {
                if (this._rootNodeStyle == null)
                {
                    this._rootNodeStyle = new Style();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this._rootNodeStyle).TrackViewState();
                    }
                }
                return this._rootNodeStyle;
            }
        }

        [Browsable(false), WebSysDescription("SiteMapPath_RootNodeTemplate"), DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), TemplateContainer(typeof(SiteMapNodeItem))]
        public virtual ITemplate RootNodeTemplate
        {
            get
            {
                return this._rootNodeTemplate;
            }
            set
            {
                this._rootNodeTemplate = value;
            }
        }

        [WebSysDescription("SiteMapPath_ShowToolTips"), DefaultValue(true), Themeable(false), WebCategory("Behavior")]
        public virtual bool ShowToolTips
        {
            get
            {
                object obj2 = this.ViewState["ShowToolTips"];
                return ((obj2 == null) || ((bool) obj2));
            }
            set
            {
                this.ViewState["ShowToolTips"] = value;
            }
        }

        [Themeable(false), WebSysDescription("SiteMapPath_SiteMapProvider"), DefaultValue(""), WebCategory("Behavior")]
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
                this.ViewState["SiteMapProvider"] = value;
                this._provider = null;
            }
        }

        [WebSysDescription("SiteMapPath_SkipToContentText"), WebSysDefaultValue("SiteMapPath_Default_SkipToContentText"), Localizable(true), WebCategory("Accessibility")]
        public virtual string SkipLinkText
        {
            get
            {
                string str = this.ViewState["SkipLinkText"] as string;
                if (str != null)
                {
                    return str;
                }
                return System.Web.SR.GetString("SiteMapPath_Default_SkipToContentText");
            }
            set
            {
                this.ViewState["SkipLinkText"] = value;
            }
        }
    }
}

