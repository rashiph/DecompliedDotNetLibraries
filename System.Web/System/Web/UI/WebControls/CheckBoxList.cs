namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    public class CheckBoxList : ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler
    {
        private bool _cachedIsEnabled;
        private bool _cachedRegisterEnabled;
        private CheckBox _controlToRepeat = new CheckBox();
        private bool _hasNotifiedOfChange;
        private string _oldAccessKey;

        public CheckBoxList()
        {
            this._controlToRepeat.EnableViewState = false;
            this._controlToRepeat.ID = "0";
            this.Controls.Add(this._controlToRepeat);
        }

        protected override Style CreateControlStyle()
        {
            return new TableStyle(this.ViewState);
        }

        protected override Control FindControl(string id, int pathOffset)
        {
            return this;
        }

        protected virtual Style GetItemStyle(ListItemType itemType, int repeatIndex)
        {
            return null;
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            if (base.IsEnabled)
            {
                string s = postDataKey.Substring(this.UniqueID.Length + 1);
                int num = s.LastIndexOf('_');
                if (num != -1)
                {
                    s = s.Substring(num + 1);
                }
                int num2 = int.Parse(s, CultureInfo.InvariantCulture);
                this.EnsureDataBound();
                if ((num2 >= 0) && (num2 < this.Items.Count))
                {
                    ListItem item = this.Items[num2];
                    if (!item.Enabled)
                    {
                        return false;
                    }
                    bool flag = postCollection[postDataKey] != null;
                    if (item.Selected != flag)
                    {
                        item.Selected = flag;
                        if (!this._hasNotifiedOfChange)
                        {
                            this._hasNotifiedOfChange = true;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this._controlToRepeat.AutoPostBack = this.AutoPostBack;
            this._controlToRepeat.CausesValidation = this.CausesValidation;
            this._controlToRepeat.ValidationGroup = this.ValidationGroup;
            if (this.Page != null)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    ListControl.SetControlToRepeatID(this, this._controlToRepeat, i);
                    this.Page.RegisterRequiresPostBack(this._controlToRepeat);
                }
            }
        }

        protected virtual void RaisePostDataChangedEvent()
        {
            if (this.AutoPostBack && !this.Page.IsPostBackEventControlRegistered)
            {
                this.Page.AutoPostBackControl = this;
                if (this.CausesValidation)
                {
                    this.Page.Validate(this.ValidationGroup);
                }
            }
            this.OnSelectedIndexChanged(EventArgs.Empty);
        }

        protected internal override void Render(HtmlTextWriter writer)
        {
            if ((this.Items.Count != 0) || base.EnableLegacyRendering)
            {
                RepeatInfo info = new RepeatInfo();
                Style controlStyle = base.ControlStyleCreated ? base.ControlStyle : null;
                short tabIndex = this.TabIndex;
                bool flag = false;
                this._controlToRepeat.TextAlign = this.TextAlign;
                this._controlToRepeat.TabIndex = tabIndex;
                if (tabIndex != 0)
                {
                    if (!this.ViewState.IsItemDirty("TabIndex"))
                    {
                        flag = true;
                    }
                    this.TabIndex = 0;
                }
                info.RepeatColumns = this.RepeatColumns;
                info.RepeatDirection = this.RepeatDirection;
                if (!base.DesignMode && !this.Context.Request.Browser.Tables)
                {
                    info.RepeatLayout = System.Web.UI.WebControls.RepeatLayout.Flow;
                }
                else
                {
                    info.RepeatLayout = this.RepeatLayout;
                }
                if (info.RepeatLayout == System.Web.UI.WebControls.RepeatLayout.Flow)
                {
                    info.EnableLegacyRendering = base.EnableLegacyRendering;
                }
                this._oldAccessKey = this.AccessKey;
                this.AccessKey = string.Empty;
                info.RenderRepeater(writer, this, controlStyle, this);
                this.AccessKey = this._oldAccessKey;
                if (tabIndex != 0)
                {
                    this.TabIndex = tabIndex;
                }
                if (flag)
                {
                    this.ViewState.SetItemDirty("TabIndex", false);
                }
            }
        }

        protected virtual void RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
        {
            if (repeatIndex == 0)
            {
                this._cachedIsEnabled = base.IsEnabled;
                this._cachedRegisterEnabled = ((this.Page != null) && base.IsEnabled) && !base.SaveSelectedIndicesViewState;
            }
            int index = repeatIndex;
            ListItem item = this.Items[index];
            this._controlToRepeat.Attributes.Clear();
            if (item.HasAttributes)
            {
                foreach (string str in item.Attributes.Keys)
                {
                    this._controlToRepeat.Attributes[str] = item.Attributes[str];
                }
            }
            if (!string.IsNullOrEmpty(this._controlToRepeat.CssClass))
            {
                this._controlToRepeat.CssClass = "";
            }
            if (this.RenderingCompatibility >= VersionUtil.Framework40)
            {
                this._controlToRepeat.InputAttributes.Add("value", item.Value);
            }
            ListControl.SetControlToRepeatID(this, this._controlToRepeat, index);
            this._controlToRepeat.Text = item.Text;
            this._controlToRepeat.Checked = item.Selected;
            this._controlToRepeat.Enabled = this._cachedIsEnabled && item.Enabled;
            this._controlToRepeat.AccessKey = this._oldAccessKey;
            if (this._cachedRegisterEnabled && this._controlToRepeat.Enabled)
            {
                this.Page.RegisterEnabledControl(this._controlToRepeat);
            }
            this._controlToRepeat.RenderControl(writer);
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
        {
            return this.GetItemStyle(itemType, repeatIndex);
        }

        void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
        {
            this.RenderItem(itemType, repeatIndex, repeatInfo, writer);
        }

        [WebCategory("Layout"), WebSysDescription("CheckBoxList_CellPadding"), DefaultValue(-1)]
        public virtual int CellPadding
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return -1;
                }
                return ((TableStyle) base.ControlStyle).CellPadding;
            }
            set
            {
                ((TableStyle) base.ControlStyle).CellPadding = value;
            }
        }

        [WebSysDescription("CheckBoxList_CellSpacing"), WebCategory("Layout"), DefaultValue(-1)]
        public virtual int CellSpacing
        {
            get
            {
                if (!base.ControlStyleCreated)
                {
                    return -1;
                }
                return ((TableStyle) base.ControlStyle).CellSpacing;
            }
            set
            {
                ((TableStyle) base.ControlStyle).CellSpacing = value;
            }
        }

        protected virtual bool HasFooter
        {
            get
            {
                return false;
            }
        }

        protected virtual bool HasHeader
        {
            get
            {
                return false;
            }
        }

        protected virtual bool HasSeparators
        {
            get
            {
                return false;
            }
        }

        internal override bool IsMultiSelectInternal
        {
            get
            {
                return true;
            }
        }

        [WebCategory("Layout"), WebSysDescription("CheckBoxList_RepeatColumns"), DefaultValue(0)]
        public virtual int RepeatColumns
        {
            get
            {
                object obj2 = this.ViewState["RepeatColumns"];
                if (obj2 != null)
                {
                    return (int) obj2;
                }
                return 0;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["RepeatColumns"] = value;
            }
        }

        [WebSysDescription("Item_RepeatDirection"), WebCategory("Layout"), DefaultValue(1)]
        public virtual System.Web.UI.WebControls.RepeatDirection RepeatDirection
        {
            get
            {
                object obj2 = this.ViewState["RepeatDirection"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.RepeatDirection) obj2;
                }
                return System.Web.UI.WebControls.RepeatDirection.Vertical;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.RepeatDirection.Horizontal) || (value > System.Web.UI.WebControls.RepeatDirection.Vertical))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["RepeatDirection"] = value;
            }
        }

        protected virtual int RepeatedItemCount
        {
            get
            {
                if (this.Items == null)
                {
                    return 0;
                }
                return this.Items.Count;
            }
        }

        [WebCategory("Layout"), WebSysDescription("WebControl_RepeatLayout"), DefaultValue(0)]
        public virtual System.Web.UI.WebControls.RepeatLayout RepeatLayout
        {
            get
            {
                object obj2 = this.ViewState["RepeatLayout"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.RepeatLayout) obj2;
                }
                return System.Web.UI.WebControls.RepeatLayout.Table;
            }
            set
            {
                EnumerationRangeValidationUtil.ValidateRepeatLayout(value);
                this.ViewState["RepeatLayout"] = value;
            }
        }

        bool IRepeatInfoUser.HasFooter
        {
            get
            {
                return this.HasFooter;
            }
        }

        bool IRepeatInfoUser.HasHeader
        {
            get
            {
                return this.HasHeader;
            }
        }

        bool IRepeatInfoUser.HasSeparators
        {
            get
            {
                return this.HasSeparators;
            }
        }

        int IRepeatInfoUser.RepeatedItemCount
        {
            get
            {
                return this.RepeatedItemCount;
            }
        }

        [WebSysDescription("WebControl_TextAlign"), WebCategory("Appearance"), DefaultValue(2)]
        public virtual System.Web.UI.WebControls.TextAlign TextAlign
        {
            get
            {
                object obj2 = this.ViewState["TextAlign"];
                if (obj2 != null)
                {
                    return (System.Web.UI.WebControls.TextAlign) obj2;
                }
                return System.Web.UI.WebControls.TextAlign.Right;
            }
            set
            {
                if ((value < System.Web.UI.WebControls.TextAlign.Left) || (value > System.Web.UI.WebControls.TextAlign.Right))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.ViewState["TextAlign"] = value;
            }
        }
    }
}

