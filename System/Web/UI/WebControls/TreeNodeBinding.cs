namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.UI;

    [DefaultProperty("TextField")]
    public sealed class TreeNodeBinding : IStateManager, ICloneable, IDataSourceViewSchemaAccessor
    {
        private bool _isTrackingViewState;
        private StateBag _viewState;

        internal void SetDirty()
        {
            this.ViewState.SetDirty(true);
        }

        object ICloneable.Clone()
        {
            return new TreeNodeBinding { 
                DataMember = this.DataMember, Depth = this.Depth, FormatString = this.FormatString, ImageToolTip = this.ImageToolTip, ImageToolTipField = this.ImageToolTipField, ImageUrl = this.ImageUrl, ImageUrlField = this.ImageUrlField, NavigateUrl = this.NavigateUrl, NavigateUrlField = this.NavigateUrlField, PopulateOnDemand = this.PopulateOnDemand, SelectAction = this.SelectAction, ShowCheckBox = this.ShowCheckBox, Target = this.Target, TargetField = this.TargetField, Text = this.Text, TextField = this.TextField, 
                ToolTip = this.ToolTip, ToolTipField = this.ToolTipField, Value = this.Value, ValueField = this.ValueField
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

        [WebSysDescription("Binding_DataMember"), DefaultValue(""), WebCategory("Data")]
        public string DataMember
        {
            get
            {
                string str = (string) this.ViewState["DataMember"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["DataMember"] = value;
            }
        }

        [DefaultValue(-1), WebSysDescription("TreeNodeBinding_Depth"), TypeConverter("System.Web.UI.Design.WebControls.TreeNodeBindingDepthConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Data")]
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

        [DefaultValue(""), Localizable(true), WebCategory("Databindings"), WebSysDescription("TreeNodeBinding_FormatString")]
        public string FormatString
        {
            get
            {
                string str = (string) this.ViewState["FormatString"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["FormatString"] = value;
            }
        }

        [WebCategory("DefaultProperties"), WebSysDescription("TreeNodeBinding_ImageToolTip"), DefaultValue(""), Localizable(true)]
        public string ImageToolTip
        {
            get
            {
                string str = (string) this.ViewState["ImageToolTip"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ImageToolTip"] = value;
            }
        }

        [DefaultValue(""), WebCategory("Databindings"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDescription("TreeNodeBinding_ImageToolTipField")]
        public string ImageToolTipField
        {
            get
            {
                string str = (string) this.ViewState["ImageToolTipField"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ImageToolTipField"] = value;
            }
        }

        [UrlProperty, DefaultValue(""), Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebCategory("DefaultProperties"), WebSysDescription("TreeNodeBinding_ImageUrl")]
        public string ImageUrl
        {
            get
            {
                string str = (string) this.ViewState["ImageUrl"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ImageUrl"] = value;
            }
        }

        [TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), WebSysDescription("TreeNodeBinding_ImageUrlField"), WebCategory("Databindings")]
        public string ImageUrlField
        {
            get
            {
                string str = (string) this.ViewState["ImageUrlField"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ImageUrlField"] = value;
            }
        }

        [WebSysDescription("TreeNodeBinding_NavigateUrl"), DefaultValue(""), Editor("System.Web.UI.Design.UrlEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), UrlProperty, WebCategory("DefaultProperties")]
        public string NavigateUrl
        {
            get
            {
                string str = (string) this.ViewState["NavigateUrl"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["NavigateUrl"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("TreeNodeBinding_NavigateUrlField"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings")]
        public string NavigateUrlField
        {
            get
            {
                string str = (string) this.ViewState["NavigateUrlField"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["NavigateUrlField"] = value;
            }
        }

        [DefaultValue(false), WebSysDescription("TreeNodeBinding_PopulateOnDemand"), WebCategory("DefaultProperties")]
        public bool PopulateOnDemand
        {
            get
            {
                object obj2 = this.ViewState["PopulateOnDemand"];
                if (obj2 == null)
                {
                    return false;
                }
                return (bool) obj2;
            }
            set
            {
                this.ViewState["PopulateOnDemand"] = value;
            }
        }

        [WebSysDescription("TreeNodeBinding_SelectAction"), DefaultValue(0), WebCategory("DefaultProperties")]
        public TreeNodeSelectAction SelectAction
        {
            get
            {
                object obj2 = this.ViewState["SelectAction"];
                if (obj2 == null)
                {
                    return TreeNodeSelectAction.Select;
                }
                return (TreeNodeSelectAction) obj2;
            }
            set
            {
                this.ViewState["SelectAction"] = value;
            }
        }

        [WebCategory("DefaultProperties"), DefaultValue(typeof(bool?), ""), WebSysDescription("TreeNodeBinding_ShowCheckBox")]
        public bool? ShowCheckBox
        {
            get
            {
                object obj2 = this.ViewState["ShowCheckBox"];
                if (obj2 == null)
                {
                    return null;
                }
                return (bool?) obj2;
            }
            set
            {
                this.ViewState["ShowCheckBox"] = value;
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

        [DefaultValue(""), WebCategory("DefaultProperties"), WebSysDescription("TreeNodeBinding_Target")]
        public string Target
        {
            get
            {
                string str = (string) this.ViewState["Target"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["Target"] = value;
            }
        }

        [DefaultValue(""), WebSysDescription("TreeNodeBinding_TargetField"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings")]
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

        [WebSysDescription("TreeNodeBinding_Text"), DefaultValue(""), Localizable(true), WebCategory("DefaultProperties")]
        public string Text
        {
            get
            {
                string str = (string) this.ViewState["Text"];
                if (str == null)
                {
                    str = (string) this.ViewState["Value"];
                    if (str == null)
                    {
                        return string.Empty;
                    }
                }
                return str;
            }
            set
            {
                this.ViewState["Text"] = value;
            }
        }

        [WebCategory("Databindings"), DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDescription("TreeNodeBinding_TextField")]
        public string TextField
        {
            get
            {
                string str = (string) this.ViewState["TextField"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["TextField"] = value;
            }
        }

        [WebCategory("DefaultProperties"), Localizable(true), DefaultValue(""), WebSysDescription("TreeNodeBinding_ToolTip")]
        public string ToolTip
        {
            get
            {
                string str = (string) this.ViewState["ToolTip"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ToolTip"] = value;
            }
        }

        [DefaultValue(""), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebCategory("Databindings"), WebSysDescription("TreeNodeBinding_ToolTipField")]
        public string ToolTipField
        {
            get
            {
                string str = (string) this.ViewState["ToolTipField"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
            }
            set
            {
                this.ViewState["ToolTipField"] = value;
            }
        }

        [DefaultValue(""), Localizable(true), WebCategory("DefaultProperties"), WebSysDescription("TreeNodeBinding_Value")]
        public string Value
        {
            get
            {
                string str = (string) this.ViewState["Value"];
                if (str == null)
                {
                    str = (string) this.ViewState["Text"];
                    if (str == null)
                    {
                        return string.Empty;
                    }
                }
                return str;
            }
            set
            {
                this.ViewState["Value"] = value;
            }
        }

        [WebCategory("Databindings"), TypeConverter("System.Web.UI.Design.DataSourceViewSchemaConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), WebSysDescription("TreeNodeBinding_ValueField")]
        public string ValueField
        {
            get
            {
                string str = (string) this.ViewState["ValueField"];
                if (str == null)
                {
                    return string.Empty;
                }
                return str;
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

