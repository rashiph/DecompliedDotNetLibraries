namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class DataGridColumn : IStateManager
    {
        private TableItemStyle footerStyle;
        private TableItemStyle headerStyle;
        private TableItemStyle itemStyle;
        private bool marked;
        private DataGrid owner;
        private StateBag statebag = new StateBag();

        protected DataGridColumn()
        {
        }

        public virtual void Initialize()
        {
        }

        public virtual void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
        {
            WebControl control;
            switch (itemType)
            {
                case ListItemType.Header:
                {
                    control = null;
                    bool flag = true;
                    string sortExpression = null;
                    if ((this.owner != null) && !this.owner.AllowSorting)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        sortExpression = this.SortExpression;
                        if (sortExpression.Length == 0)
                        {
                            flag = false;
                        }
                    }
                    string headerImageUrl = this.HeaderImageUrl;
                    if (headerImageUrl.Length == 0)
                    {
                        string headerText = this.HeaderText;
                        if (flag)
                        {
                            LinkButton button2 = new DataGridLinkButton {
                                Text = headerText,
                                CommandName = "Sort",
                                CommandArgument = sortExpression,
                                CausesValidation = false
                            };
                            control = button2;
                        }
                        else
                        {
                            if (headerText.Length == 0)
                            {
                                headerText = "&nbsp;";
                            }
                            cell.Text = headerText;
                        }
                        break;
                    }
                    if (flag)
                    {
                        ImageButton button = new ImageButton {
                            ImageUrl = this.HeaderImageUrl,
                            CommandName = "Sort",
                            CommandArgument = sortExpression,
                            CausesValidation = false
                        };
                        control = button;
                    }
                    else
                    {
                        Image image = new Image {
                            ImageUrl = headerImageUrl
                        };
                        control = image;
                    }
                    break;
                }
                case ListItemType.Footer:
                {
                    string footerText = this.FooterText;
                    if (footerText.Length == 0)
                    {
                        footerText = "&nbsp;";
                    }
                    cell.Text = footerText;
                    return;
                }
                default:
                    return;
            }
            if (control != null)
            {
                cell.Controls.Add(control);
            }
        }

        protected virtual void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                object[] objArray = (object[]) savedState;
                if (objArray[0] != null)
                {
                    ((IStateManager) this.ViewState).LoadViewState(objArray[0]);
                }
                if (objArray[1] != null)
                {
                    ((IStateManager) this.ItemStyle).LoadViewState(objArray[1]);
                }
                if (objArray[2] != null)
                {
                    ((IStateManager) this.HeaderStyle).LoadViewState(objArray[2]);
                }
                if (objArray[3] != null)
                {
                    ((IStateManager) this.FooterStyle).LoadViewState(objArray[3]);
                }
            }
        }

        protected virtual void OnColumnChanged()
        {
            if (this.owner != null)
            {
                this.owner.OnColumnsChanged();
            }
        }

        protected virtual object SaveViewState()
        {
            object obj2 = ((IStateManager) this.ViewState).SaveViewState();
            object obj3 = (this.itemStyle != null) ? ((IStateManager) this.itemStyle).SaveViewState() : null;
            object obj4 = (this.headerStyle != null) ? ((IStateManager) this.headerStyle).SaveViewState() : null;
            object obj5 = (this.footerStyle != null) ? ((IStateManager) this.footerStyle).SaveViewState() : null;
            if (((obj2 == null) && (obj3 == null)) && ((obj4 == null) && (obj5 == null)))
            {
                return null;
            }
            return new object[] { obj2, obj3, obj4, obj5 };
        }

        internal void SetOwner(DataGrid owner)
        {
            this.owner = owner;
        }

        void IStateManager.LoadViewState(object state)
        {
            this.LoadViewState(state);
        }

        object IStateManager.SaveViewState()
        {
            return this.SaveViewState();
        }

        void IStateManager.TrackViewState()
        {
            this.TrackViewState();
        }

        public override string ToString()
        {
            return string.Empty;
        }

        protected virtual void TrackViewState()
        {
            this.marked = true;
            ((IStateManager) this.ViewState).TrackViewState();
            if (this.itemStyle != null)
            {
                ((IStateManager) this.itemStyle).TrackViewState();
            }
            if (this.headerStyle != null)
            {
                ((IStateManager) this.headerStyle).TrackViewState();
            }
            if (this.footerStyle != null)
            {
                ((IStateManager) this.footerStyle).TrackViewState();
            }
        }

        protected bool DesignMode
        {
            get
            {
                return ((this.owner != null) && this.owner.DesignMode);
            }
        }

        [WebSysDescription("DataGridColumn_FooterStyle"), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual TableItemStyle FooterStyle
        {
            get
            {
                if (this.footerStyle == null)
                {
                    this.footerStyle = new TableItemStyle();
                    if (this.IsTrackingViewState)
                    {
                        ((IStateManager) this.footerStyle).TrackViewState();
                    }
                }
                return this.footerStyle;
            }
        }

        internal TableItemStyle FooterStyleInternal
        {
            get
            {
                return this.footerStyle;
            }
        }

        [WebCategory("Appearance"), WebSysDescription("DataGridColumn_FooterText"), DefaultValue("")]
        public virtual string FooterText
        {
            get
            {
                object obj2 = this.ViewState["FooterText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["FooterText"] = value;
                this.OnColumnChanged();
            }
        }

        [WebCategory("Appearance"), WebSysDescription("DataGridColumn_HeaderImageUrl"), DefaultValue(""), UrlProperty]
        public virtual string HeaderImageUrl
        {
            get
            {
                object obj2 = this.ViewState["HeaderImageUrl"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HeaderImageUrl"] = value;
                this.OnColumnChanged();
            }
        }

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebSysDescription("DataGridColumn_HeaderStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual TableItemStyle HeaderStyle
        {
            get
            {
                if (this.headerStyle == null)
                {
                    this.headerStyle = new TableItemStyle();
                    if (this.IsTrackingViewState)
                    {
                        ((IStateManager) this.headerStyle).TrackViewState();
                    }
                }
                return this.headerStyle;
            }
        }

        internal TableItemStyle HeaderStyleInternal
        {
            get
            {
                return this.headerStyle;
            }
        }

        [DefaultValue(""), WebSysDescription("DataGridColumn_HeaderText"), WebCategory("Appearance")]
        public virtual string HeaderText
        {
            get
            {
                object obj2 = this.ViewState["HeaderText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["HeaderText"] = value;
                this.OnColumnChanged();
            }
        }

        protected bool IsTrackingViewState
        {
            get
            {
                return this.marked;
            }
        }

        [WebCategory("Styles"), WebSysDescription("DataGridColumn_ItemStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null)]
        public virtual TableItemStyle ItemStyle
        {
            get
            {
                if (this.itemStyle == null)
                {
                    this.itemStyle = new TableItemStyle();
                    if (this.IsTrackingViewState)
                    {
                        ((IStateManager) this.itemStyle).TrackViewState();
                    }
                }
                return this.itemStyle;
            }
        }

        internal TableItemStyle ItemStyleInternal
        {
            get
            {
                return this.itemStyle;
            }
        }

        protected DataGrid Owner
        {
            get
            {
                return this.owner;
            }
        }

        [WebSysDescription("DataGridColumn_SortExpression"), WebCategory("Behavior"), DefaultValue("")]
        public virtual string SortExpression
        {
            get
            {
                object obj2 = this.ViewState["SortExpression"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["SortExpression"] = value;
                this.OnColumnChanged();
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this.IsTrackingViewState;
            }
        }

        protected StateBag ViewState
        {
            get
            {
                return this.statebag;
            }
        }

        [WebSysDescription("DataGridColumn_Visible"), DefaultValue(true), WebCategory("Behavior")]
        public bool Visible
        {
            get
            {
                object obj2 = this.ViewState["Visible"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["Visible"] = value;
                this.OnColumnChanged();
            }
        }
    }
}

