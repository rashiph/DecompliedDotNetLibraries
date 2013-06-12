namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Threading;
    using System.Web;
    using System.Web.UI;

    [TypeConverter(typeof(ExpandableObjectConverter)), DefaultProperty("HeaderText")]
    public abstract class DataControlField : IStateManager, IDataSourceViewSchemaAccessor
    {
        private System.Web.UI.Control _control;
        private Style _controlStyle;
        private object _dataSourceViewSchema = null;
        private TableItemStyle _footerStyle;
        private TableItemStyle _headerStyle;
        private TableItemStyle _itemStyle;
        private bool _sortingEnabled;
        private StateBag _statebag = new StateBag();
        private bool _trackViewState;

        internal event EventHandler FieldChanged;

        protected DataControlField()
        {
        }

        protected internal DataControlField CloneField()
        {
            DataControlField newField = this.CreateField();
            this.CopyProperties(newField);
            return newField;
        }

        protected virtual void CopyProperties(DataControlField newField)
        {
            newField.AccessibleHeaderText = this.AccessibleHeaderText;
            newField.ControlStyle.CopyFrom(this.ControlStyle);
            newField.FooterStyle.CopyFrom(this.FooterStyle);
            newField.HeaderStyle.CopyFrom(this.HeaderStyle);
            newField.ItemStyle.CopyFrom(this.ItemStyle);
            newField.FooterText = this.FooterText;
            newField.HeaderImageUrl = this.HeaderImageUrl;
            newField.HeaderText = this.HeaderText;
            newField.InsertVisible = this.InsertVisible;
            newField.ShowHeader = this.ShowHeader;
            newField.SortExpression = this.SortExpression;
            newField.Visible = this.Visible;
        }

        protected abstract DataControlField CreateField();
        public virtual void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
        {
        }

        public virtual bool Initialize(bool sortingEnabled, System.Web.UI.Control control)
        {
            this._sortingEnabled = sortingEnabled;
            this._control = control;
            return false;
        }

        public virtual void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            WebControl control;
            string sortExpression;
            string headerText;
            ImageButton button;
            switch (cellType)
            {
                case DataControlCellType.Header:
                {
                    control = null;
                    sortExpression = this.SortExpression;
                    bool flag = this._sortingEnabled && (sortExpression.Length > 0);
                    string headerImageUrl = this.HeaderImageUrl;
                    headerText = this.HeaderText;
                    if (headerImageUrl.Length == 0)
                    {
                        if (flag)
                        {
                            LinkButton button2;
                            IPostBackContainer container2 = this._control as IPostBackContainer;
                            if (container2 != null)
                            {
                                button2 = new DataControlLinkButton(container2);
                                ((DataControlLinkButton) button2).EnableCallback(null);
                            }
                            else
                            {
                                button2 = new LinkButton();
                            }
                            button2.Text = headerText;
                            button2.CommandName = "Sort";
                            button2.CommandArgument = sortExpression;
                            if (!(button2 is DataControlLinkButton))
                            {
                                button2.CausesValidation = false;
                            }
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
                        goto Label_015C;
                    }
                    if (!flag)
                    {
                        Image image = new Image {
                            ImageUrl = headerImageUrl
                        };
                        control = image;
                        image.AlternateText = headerText;
                        goto Label_015C;
                    }
                    IPostBackContainer container = this._control as IPostBackContainer;
                    if (container == null)
                    {
                        button = new ImageButton();
                        break;
                    }
                    button = new DataControlImageButton(container);
                    ((DataControlImageButton) button).EnableCallback(null);
                    break;
                }
                case DataControlCellType.Footer:
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
            button.ImageUrl = this.HeaderImageUrl;
            button.CommandName = "Sort";
            button.CommandArgument = sortExpression;
            if (!(button is DataControlImageButton))
            {
                button.CausesValidation = false;
            }
            button.AlternateText = headerText;
            control = button;
        Label_015C:
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

        protected virtual void OnFieldChanged()
        {
            if (this.FieldChanged != null)
            {
                this.FieldChanged(this, EventArgs.Empty);
            }
        }

        protected virtual object SaveViewState()
        {
            object obj2 = ((IStateManager) this.ViewState).SaveViewState();
            object obj3 = (this._itemStyle != null) ? ((IStateManager) this._itemStyle).SaveViewState() : null;
            object obj4 = (this._headerStyle != null) ? ((IStateManager) this._headerStyle).SaveViewState() : null;
            object obj5 = (this._footerStyle != null) ? ((IStateManager) this._footerStyle).SaveViewState() : null;
            object obj6 = (this._controlStyle != null) ? ((IStateManager) this._controlStyle).SaveViewState() : null;
            if (((obj2 == null) && (obj3 == null)) && (((obj4 == null) && (obj5 == null)) && (obj6 == null)))
            {
                return null;
            }
            return new object[] { obj2, obj3, obj4, obj5, obj6 };
        }

        internal void SetDirty()
        {
            this._statebag.SetDirty(true);
            if (this._itemStyle != null)
            {
                this._itemStyle.SetDirty();
            }
            if (this._headerStyle != null)
            {
                this._headerStyle.SetDirty();
            }
            if (this._footerStyle != null)
            {
                this._footerStyle.SetDirty();
            }
            if (this._controlStyle != null)
            {
                this._controlStyle.SetDirty();
            }
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
            string str = this.HeaderText.Trim();
            if (str.Length <= 0)
            {
                return base.GetType().Name;
            }
            return str;
        }

        protected virtual void TrackViewState()
        {
            this._trackViewState = true;
            ((IStateManager) this.ViewState).TrackViewState();
            if (this._itemStyle != null)
            {
                ((IStateManager) this._itemStyle).TrackViewState();
            }
            if (this._headerStyle != null)
            {
                ((IStateManager) this._headerStyle).TrackViewState();
            }
            if (this._footerStyle != null)
            {
                ((IStateManager) this._footerStyle).TrackViewState();
            }
            if (this._controlStyle != null)
            {
                ((IStateManager) this._controlStyle).TrackViewState();
            }
        }

        public virtual void ValidateSupportsCallback()
        {
            throw new NotSupportedException(System.Web.SR.GetString("DataControlField_CallbacksNotSupported", new object[] { this.Control.ID }));
        }

        [DefaultValue(""), WebSysDescription("DataControlField_AccessibleHeaderText"), WebCategory("Accessibility"), Localizable(true)]
        public virtual string AccessibleHeaderText
        {
            get
            {
                object obj2 = this.ViewState["AccessibleHeaderText"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                if (!object.Equals(value, this.ViewState["AccessibleHeaderText"]))
                {
                    this.ViewState["AccessibleHeaderText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        protected System.Web.UI.Control Control
        {
            get
            {
                return this._control;
            }
        }

        [DefaultValue((string) null), WebSysDescription("DataControlField_ControlStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles")]
        public Style ControlStyle
        {
            get
            {
                if (this._controlStyle == null)
                {
                    this._controlStyle = new Style();
                    if (this.IsTrackingViewState)
                    {
                        ((IStateManager) this._controlStyle).TrackViewState();
                    }
                }
                return this._controlStyle;
            }
        }

        internal Style ControlStyleInternal
        {
            get
            {
                return this._controlStyle;
            }
        }

        protected bool DesignMode
        {
            get
            {
                return ((this._control != null) && this._control.DesignMode);
            }
        }

        [WebCategory("Styles"), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), WebSysDescription("DataControlField_FooterStyle")]
        public TableItemStyle FooterStyle
        {
            get
            {
                if (this._footerStyle == null)
                {
                    this._footerStyle = new TableItemStyle();
                    if (this.IsTrackingViewState)
                    {
                        ((IStateManager) this._footerStyle).TrackViewState();
                    }
                }
                return this._footerStyle;
            }
        }

        internal TableItemStyle FooterStyleInternal
        {
            get
            {
                return this._footerStyle;
            }
        }

        [WebSysDescription("DataControlField_FooterText"), Localizable(true), WebCategory("Appearance"), DefaultValue("")]
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
                if (!object.Equals(value, this.ViewState["FooterText"]))
                {
                    this.ViewState["FooterText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [UrlProperty, WebSysDescription("DataControlField_HeaderImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("Appearance"), DefaultValue("")]
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
                if (!object.Equals(value, this.ViewState["HeaderImageUrl"]))
                {
                    this.ViewState["HeaderImageUrl"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [DefaultValue((string) null), PersistenceMode(PersistenceMode.InnerProperty), WebCategory("Styles"), WebSysDescription("DataControlField_HeaderStyle"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TableItemStyle HeaderStyle
        {
            get
            {
                if (this._headerStyle == null)
                {
                    this._headerStyle = new TableItemStyle();
                    if (this.IsTrackingViewState)
                    {
                        ((IStateManager) this._headerStyle).TrackViewState();
                    }
                }
                return this._headerStyle;
            }
        }

        internal TableItemStyle HeaderStyleInternal
        {
            get
            {
                return this._headerStyle;
            }
        }

        [Localizable(true), WebCategory("Appearance"), WebSysDescription("DataControlField_HeaderText"), DefaultValue("")]
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
                if (!object.Equals(value, this.ViewState["HeaderText"]))
                {
                    this.ViewState["HeaderText"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [WebCategory("Behavior"), WebSysDescription("DataControlField_InsertVisible"), DefaultValue(true)]
        public virtual bool InsertVisible
        {
            get
            {
                object obj2 = this.ViewState["InsertVisible"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                object obj2 = this.ViewState["InsertVisible"];
                if ((obj2 == null) || (value != ((bool) obj2)))
                {
                    this.ViewState["InsertVisible"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        protected bool IsTrackingViewState
        {
            get
            {
                return this._trackViewState;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty), DefaultValue((string) null), WebSysDescription("DataControlField_ItemStyle"), WebCategory("Styles")]
        public TableItemStyle ItemStyle
        {
            get
            {
                if (this._itemStyle == null)
                {
                    this._itemStyle = new TableItemStyle();
                    if (this.IsTrackingViewState)
                    {
                        ((IStateManager) this._itemStyle).TrackViewState();
                    }
                }
                return this._itemStyle;
            }
        }

        internal TableItemStyle ItemStyleInternal
        {
            get
            {
                return this._itemStyle;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("DataControlField_ShowHeader"), DefaultValue(true)]
        public virtual bool ShowHeader
        {
            get
            {
                object obj2 = this.ViewState["ShowHeader"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                object obj2 = this.ViewState["ShowHeader"];
                if ((obj2 == null) || (((bool) obj2) != value))
                {
                    this.ViewState["ShowHeader"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        [TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDescription("DataControlField_SortExpression"), WebCategory("Behavior"), DefaultValue("")]
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
                if (!object.Equals(value, this.ViewState["SortExpression"]))
                {
                    this.ViewState["SortExpression"] = value;
                    this.OnFieldChanged();
                }
            }
        }

        object IDataSourceViewSchemaAccessor.DataSourceViewSchema
        {
            get
            {
                return this._dataSourceViewSchema;
            }
            set
            {
                this._dataSourceViewSchema = value;
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
                return this._statebag;
            }
        }

        [WebSysDescription("DataControlField_Visible"), WebCategory("Behavior"), DefaultValue(true)]
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
                object obj2 = this.ViewState["Visible"];
                if ((obj2 == null) || (value != ((bool) obj2)))
                {
                    this.ViewState["Visible"] = value;
                    this.OnFieldChanged();
                }
            }
        }
    }
}

