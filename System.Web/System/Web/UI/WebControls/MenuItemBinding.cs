namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("TextField")]
    public sealed class MenuItemBinding : IStateManager, ICloneable, IDataSourceViewSchemaAccessor
    {
        private bool _isTrackingViewState;
        private StateBag _viewState;

        internal void SetDirty()
        {
            this.ViewState.SetDirty(true);
        }

        object ICloneable.Clone()
        {
            return new MenuItemBinding { 
                DataMember = this.DataMember, Depth = this.Depth, Enabled = this.Enabled, EnabledField = this.EnabledField, FormatString = this.FormatString, ImageUrl = this.ImageUrl, ImageUrlField = this.ImageUrlField, NavigateUrl = this.NavigateUrl, NavigateUrlField = this.NavigateUrlField, PopOutImageUrl = this.PopOutImageUrl, PopOutImageUrlField = this.PopOutImageUrlField, Selectable = this.Selectable, SelectableField = this.SelectableField, SeparatorImageUrl = this.SeparatorImageUrl, SeparatorImageUrlField = this.SeparatorImageUrlField, Target = this.Target, 
                TargetField = this.TargetField, Text = this.Text, TextField = this.TextField, ToolTip = this.ToolTip, ToolTipField = this.ToolTipField, Value = this.Value, ValueField = this.ValueField
             };
        }

        void IStateManager.LoadViewState(object state)
        {
            if (state != null)
            {
                ((IStateManager) this.ViewState).LoadViewState(state);
            }
        }

        object IStateManager.SaveViewState()
        {
            if (this._viewState != null)
            {
                return ((IStateManager) this._viewState).SaveViewState();
            }
            return null;
        }

        void IStateManager.TrackViewState()
        {
            this._isTrackingViewState = true;
            if (this._viewState != null)
            {
                ((IStateManager) this._viewState).TrackViewState();
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.DataMember))
            {
                return this.DataMember;
            }
            return System.Web.SR.GetString("TreeNodeBinding_EmptyBindingText");
        }

        [DefaultValue(""), WebSysDescription("Binding_DataMember"), WebCategory("Data")]
        public string DataMember
        {
            get
            {
                object obj2 = this.ViewState["DataMember"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["DataMember"] = value;
            }
        }

        [DefaultValue(-1), WebSysDescription("MenuItemBinding_Depth"), TypeConverter("System.Web.UI.Design.WebControls.TreeNodeBindingDepthConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Data")]
        public int Depth
        {
            get
            {
                object obj2 = this.ViewState["Depth"];
                if (obj2 == null)
                {
                    return -1;
                }
                return (int) obj2;
            }
            set
            {
                this.ViewState["Depth"] = value;
            }
        }

        [DefaultValue(true), WebSysDescription("MenuItemBinding_Enabled"), WebCategory("DefaultProperties")]
        public bool Enabled
        {
            get
            {
                object obj2 = this.ViewState["Enabled"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["Enabled"] = value;
            }
        }

        [TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), WebSysDescription("MenuItemBinding_EnabledField"), WebCategory("Databindings")]
        public string EnabledField
        {
            get
            {
                object obj2 = this.ViewState["EnabledField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["EnabledField"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("MenuItemBinding_FormatString"), Localizable(true), WebCategory("Databindings")]
        public string FormatString
        {
            get
            {
                object obj2 = this.ViewState["FormatString"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["FormatString"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("MenuItemBinding_ImageUrl"), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("DefaultProperties")]
        public string ImageUrl
        {
            get
            {
                object obj2 = this.ViewState["ImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ImageUrl"] = value;
            }
        }

        [TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), WebSysDescription("MenuItemBinding_ImageUrlField"), WebCategory("Databindings")]
        public string ImageUrlField
        {
            get
            {
                object obj2 = this.ViewState["ImageUrlField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ImageUrlField"] = value;
            }
        }

        [WebSysDescription("MenuItemBinding_NavigateUrl"), DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("DefaultProperties")]
        public string NavigateUrl
        {
            get
            {
                object obj2 = this.ViewState["NavigateUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["NavigateUrl"] = value;
            }
        }

        [WebCategory("Databindings"), DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDescription("MenuItemBinding_NavigateUrlField")]
        public string NavigateUrlField
        {
            get
            {
                object obj2 = this.ViewState["NavigateUrlField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["NavigateUrlField"] = value;
            }
        }

        [UrlProperty, Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebCategory("DefaultProperties"), WebSysDescription("MenuItemBinding_PopOutImageUrl")]
        public string PopOutImageUrl
        {
            get
            {
                object obj2 = this.ViewState["PopOutImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["PopOutImageUrl"] = value;
            }
        }

        [DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings"), WebSysDescription("MenuItemBinding_PopOutImageUrlField")]
        public string PopOutImageUrlField
        {
            get
            {
                object obj2 = this.ViewState["PopOutImageUrlField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["PopOutImageUrlField"] = value;
            }
        }

        [WebSysDescription("MenuItemBinding_Selectable"), DefaultValue(true), WebCategory("DefaultProperties")]
        public bool Selectable
        {
            get
            {
                object obj2 = this.ViewState["Selectable"];
                if (obj2 != null)
                {
                    return (bool) obj2;
                }
                return true;
            }
            set
            {
                this.ViewState["Selectable"] = value;
            }
        }

        [DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings"), WebSysDescription("MenuItemBinding_SelectableField")]
        public string SelectableField
        {
            get
            {
                object obj2 = this.ViewState["SelectableField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["SelectableField"] = value;
            }
        }

        [Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), UrlProperty, WebCategory("DefaultProperties"), WebSysDescription("MenuItemBinding_SeparatorImageUrl")]
        public string SeparatorImageUrl
        {
            get
            {
                object obj2 = this.ViewState["SeparatorImageUrl"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["SeparatorImageUrl"] = value;
            }
        }

        [DefaultValue(""), WebCategory("Databindings"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDescription("MenuItemBinding_SeparatorImageUrlField")]
        public string SeparatorImageUrlField
        {
            get
            {
                object obj2 = this.ViewState["SeparatorImageUrlField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["SeparatorImageUrlField"] = value;
            }
        }

        object IDataSourceViewSchemaAccessor.DataSourceViewSchema
        {
            get
            {
                return this.ViewState["IDataSourceViewSchemaAccessor.DataSourceViewSchema"];
            }
            set
            {
                this.ViewState["IDataSourceViewSchemaAccessor.DataSourceViewSchema"] = value;
            }
        }

        bool IStateManager.IsTrackingViewState
        {
            get
            {
                return this._isTrackingViewState;
            }
        }

        [WebSysDescription("MenuItemBinding_Target"), DefaultValue(""), WebCategory("DefaultProperties")]
        public string Target
        {
            get
            {
                object obj2 = this.ViewState["Target"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        [WebSysDescription("MenuItemBinding_TargetField"), DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings")]
        public string TargetField
        {
            get
            {
                string str = (string) this.ViewState["TargetField"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["TargetField"] = value;
            }
        }

        [DefaultValue(""), Localizable(true), WebCategory("DefaultProperties"), WebSysDescription("MenuItemBinding_Text")]
        public string Text
        {
            get
            {
                object obj2 = this.ViewState["Text"];
                if (obj2 == null)
                {
                    obj2 = this.ViewState["Value"];
                    if (obj2 == null)
                    {
                        return string.Empty;
                    }
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["Text"] = value;
            }
        }

        [DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings"), WebSysDescription("MenuItemBinding_TextField")]
        public string TextField
        {
            get
            {
                object obj2 = this.ViewState["TextField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["TextField"] = value;
            }
        }

        [DefaultValue(""), Localizable(true), WebCategory("DefaultProperties"), WebSysDescription("MenuItemBinding_ToolTip")]
        public string ToolTip
        {
            get
            {
                object obj2 = this.ViewState["ToolTip"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ToolTip"] = value;
            }
        }

        [DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings"), WebSysDescription("MenuItemBinding_ToolTipField")]
        public string ToolTipField
        {
            get
            {
                object obj2 = this.ViewState["ToolTipField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ToolTipField"] = value;
            }
        }

        [DefaultValue(""), Localizable(true), WebCategory("DefaultProperties"), WebSysDescription("MenuItemBinding_Value")]
        public string Value
        {
            get
            {
                object obj2 = this.ViewState["Value"];
                if (obj2 == null)
                {
                    obj2 = this.ViewState["Text"];
                    if (obj2 == null)
                    {
                        return string.Empty;
                    }
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["Value"] = value;
            }
        }

        [DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings"), WebSysDescription("MenuItemBinding_ValueField")]
        public string ValueField
        {
            get
            {
                object obj2 = this.ViewState["ValueField"];
                if (obj2 == null)
                {
                    return string.Empty;
                }
                return (string) obj2;
            }
            set
            {
                this.ViewState["ValueField"] = value;
            }
        }

        private StateBag ViewState
        {
            get
            {
                if (this._viewState == null)
                {
                    this._viewState = new StateBag();
                    if (this._isTrackingViewState)
                    {
                        ((IStateManager) this._viewState).TrackViewState();
                    }
                }
                return this._viewState;
            }
        }
    }
}

