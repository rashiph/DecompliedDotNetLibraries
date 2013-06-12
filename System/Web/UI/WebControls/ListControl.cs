namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    [DataBindingHandler("System.Web.UI.Design.WebControls.ListControlDataBindingHandler, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ControlValueProperty("SelectedValue"), Designer("System.Web.UI.Design.WebControls.ListControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ParseChildren(true, "Items"), DefaultEvent("SelectedIndexChanged")]
    public abstract class ListControl : DataBoundControl, IEditableTextControl, ITextControl
    {
        private bool _stateLoaded;
        private int cachedSelectedIndex = -1;
        private ArrayList cachedSelectedIndices;
        private string cachedSelectedValue;
        private static readonly object EventSelectedIndexChanged = new object();
        private static readonly object EventTextChanged = new object();
        private ListItemCollection items;

        [WebSysDescription("ListControl_OnSelectedIndexChanged"), WebCategory("Action")]
        public event EventHandler SelectedIndexChanged
        {
            add
            {
                base.Events.AddHandler(EventSelectedIndexChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventSelectedIndexChanged, value);
            }
        }

        [WebCategory("Action"), WebSysDescription("ListControl_TextChanged")]
        public event EventHandler TextChanged
        {
            add
            {
                base.Events.AddHandler(EventTextChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTextChanged, value);
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.VerifyRenderingInServerForm(this);
            }
            if (this.IsMultiSelectInternal)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Multiple, "multiple");
            }
            if ((this.AutoPostBack && (this.Page != null)) && this.Page.ClientSupportsJavaScript)
            {
                string str = null;
                if (base.HasAttributes)
                {
                    str = base.Attributes["onchange"];
                    if (str != null)
                    {
                        str = Util.EnsureEndWithSemiColon(str);
                        base.Attributes.Remove("onchange");
                    }
                }
                PostBackOptions options = new PostBackOptions(this, string.Empty);
                if (this.CausesValidation)
                {
                    options.PerformValidation = true;
                    options.ValidationGroup = this.ValidationGroup;
                }
                if (this.Page.Form != null)
                {
                    options.AutoPostBack = true;
                }
                str = Util.MergeScript(str, this.Page.ClientScript.GetPostBackEventReference(options, true));
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, str);
                if (base.EnableLegacyRendering)
                {
                    writer.AddAttribute("language", "javascript", false);
                }
            }
            if (this.Enabled && (!base.IsEnabled & this.SupportsDisabledAttribute))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            }
            base.AddAttributesToRender(writer);
        }

        public virtual void ClearSelection()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].Selected = false;
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Triplet triplet = (Triplet) savedState;
                base.LoadViewState(triplet.First);
                this.Items.LoadViewState(triplet.Second);
                ArrayList third = triplet.Third as ArrayList;
                if (third != null)
                {
                    this.SelectInternal(third);
                }
            }
            else
            {
                base.LoadViewState(null);
            }
            this._stateLoaded = true;
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);
            IEnumerable data = this.GetData().ExecuteSelect(DataSourceSelectArguments.Empty);
            this.PerformDataBinding(data);
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Page != null) && base.IsEnabled)
            {
                if (this.AutoPostBack)
                {
                    this.Page.RegisterPostBackScript();
                    this.Page.RegisterFocusScript();
                    if (this.CausesValidation && (this.Page.GetValidators(this.ValidationGroup).Count > 0))
                    {
                        this.Page.RegisterWebFormsScript();
                    }
                }
                if (!this.SaveSelectedIndicesViewState)
                {
                    this.Page.RegisterEnabledControl(this);
                }
            }
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventSelectedIndexChanged];
            if (handler != null)
            {
                handler(this, e);
            }
            this.OnTextChanged(e);
        }

        protected virtual void OnTextChanged(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventTextChanged];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected internal override void PerformDataBinding(IEnumerable dataSource)
        {
            base.PerformDataBinding(dataSource);
            if (dataSource != null)
            {
                bool flag = false;
                bool flag2 = false;
                string dataTextField = this.DataTextField;
                string dataValueField = this.DataValueField;
                string dataTextFormatString = this.DataTextFormatString;
                if (!this.AppendDataBoundItems)
                {
                    this.Items.Clear();
                }
                ICollection is2 = dataSource as ICollection;
                if (is2 != null)
                {
                    this.Items.Capacity = is2.Count + this.Items.Count;
                }
                if ((dataTextField.Length != 0) || (dataValueField.Length != 0))
                {
                    flag = true;
                }
                if (dataTextFormatString.Length != 0)
                {
                    flag2 = true;
                }
                foreach (object obj2 in dataSource)
                {
                    ListItem item = new ListItem();
                    if (flag)
                    {
                        if (dataTextField.Length > 0)
                        {
                            item.Text = DataBinder.GetPropertyValue(obj2, dataTextField, dataTextFormatString);
                        }
                        if (dataValueField.Length > 0)
                        {
                            item.Value = DataBinder.GetPropertyValue(obj2, dataValueField, null);
                        }
                    }
                    else
                    {
                        if (flag2)
                        {
                            item.Text = string.Format(CultureInfo.CurrentCulture, dataTextFormatString, new object[] { obj2 });
                        }
                        else
                        {
                            item.Text = obj2.ToString();
                        }
                        item.Value = obj2.ToString();
                    }
                    this.Items.Add(item);
                }
            }
            if (this.cachedSelectedValue != null)
            {
                int num = -1;
                num = this.Items.FindByValueInternal(this.cachedSelectedValue, true);
                if (-1 == num)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("ListControl_SelectionOutOfRange", new object[] { this.ID, "SelectedValue" }));
                }
                if ((this.cachedSelectedIndex != -1) && (this.cachedSelectedIndex != num))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Attributes_mutually_exclusive", new object[] { "SelectedIndex", "SelectedValue" }));
                }
                this.SelectedIndex = num;
                this.cachedSelectedValue = null;
                this.cachedSelectedIndex = -1;
            }
            else if (this.cachedSelectedIndex != -1)
            {
                this.SelectedIndex = this.cachedSelectedIndex;
                this.cachedSelectedIndex = -1;
            }
        }

        protected override void PerformSelect()
        {
            this.OnDataBinding(EventArgs.Empty);
            base.RequiresDataBinding = false;
            base.MarkAsDataBound();
            this.OnDataBound(EventArgs.Empty);
        }

        protected internal override void RenderContents(HtmlTextWriter writer)
        {
            ListItemCollection items = this.Items;
            int count = items.Count;
            if (count > 0)
            {
                bool flag = false;
                for (int i = 0; i < count; i++)
                {
                    ListItem item = items[i];
                    if (item.Enabled)
                    {
                        writer.WriteBeginTag("option");
                        if (item.Selected)
                        {
                            if (flag)
                            {
                                this.VerifyMultiSelect();
                            }
                            flag = true;
                            writer.WriteAttribute("selected", "selected");
                        }
                        writer.WriteAttribute("value", item.Value, true);
                        if (item.HasAttributes)
                        {
                            item.Attributes.Render(writer);
                        }
                        if (this.Page != null)
                        {
                            this.Page.ClientScript.RegisterForEventValidation(this.UniqueID, item.Value);
                        }
                        writer.Write('>');
                        HttpUtility.HtmlEncode(item.Text, writer);
                        writer.WriteEndTag("option");
                        writer.WriteLine();
                    }
                }
            }
        }

        protected override object SaveViewState()
        {
            object x = base.SaveViewState();
            object y = this.Items.SaveViewState();
            object z = null;
            if (this.SaveSelectedIndicesViewState)
            {
                z = this.SelectedIndicesInternal;
            }
            if (((z == null) && (y == null)) && (x == null))
            {
                return null;
            }
            return new Triplet(x, y, z);
        }

        internal void SelectInternal(ArrayList selectedIndices)
        {
            this.ClearSelection();
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                int num2 = (int) selectedIndices[i];
                if ((num2 >= 0) && (num2 < this.Items.Count))
                {
                    this.Items[num2].Selected = true;
                }
            }
            this.cachedSelectedIndices = selectedIndices;
        }

        internal static void SetControlToRepeatID(Control owner, Control controlToRepeat, int index)
        {
            string str = index.ToString(NumberFormatInfo.InvariantInfo);
            if (owner.EffectiveClientIDMode == ClientIDMode.Static)
            {
                if (string.IsNullOrEmpty(owner.ID))
                {
                    controlToRepeat.ID = str;
                    controlToRepeat.ClientIDMode = ClientIDMode.AutoID;
                }
                else
                {
                    controlToRepeat.ID = owner.ID + "_" + str;
                    controlToRepeat.ClientIDMode = ClientIDMode.Inherit;
                }
            }
            else
            {
                controlToRepeat.ID = str;
                controlToRepeat.ClientIDMode = ClientIDMode.Inherit;
            }
        }

        protected void SetPostDataSelection(int selectedIndex)
        {
            if ((this.Items.Count != 0) && (selectedIndex < this.Items.Count))
            {
                this.ClearSelection();
                if (selectedIndex >= 0)
                {
                    this.Items[selectedIndex].Selected = true;
                }
            }
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            this.Items.TrackViewState();
        }

        protected internal virtual void VerifyMultiSelect()
        {
            if (!this.IsMultiSelectInternal)
            {
                throw new HttpException(System.Web.SR.GetString("Cant_Multiselect_In_Single_Mode"));
            }
        }

        [DefaultValue(false), WebSysDescription("ListControl_AppendDataBoundItems"), Themeable(false), WebCategory("Behavior")]
        public virtual bool AppendDataBoundItems
        {
            get
            {
                object obj2 = this.ViewState["AppendDataBoundItems"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["AppendDataBoundItems"] = value;
                if (base.Initialized)
                {
                    base.RequiresDataBinding = true;
                }
            }
        }

        [DefaultValue(false), WebCategory("Behavior"), WebSysDescription("ListControl_AutoPostBack"), Themeable(false)]
        public virtual bool AutoPostBack
        {
            get
            {
                object obj2 = this.ViewState["AutoPostBack"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["AutoPostBack"] = value;
            }
        }

        [WebCategory("Behavior"), DefaultValue(false), WebSysDescription("AutoPostBackControl_CausesValidation"), Themeable(false)]
        public virtual bool CausesValidation
        {
            get
            {
                object obj2 = this.ViewState["CausesValidation"];
                return ((obj2 != null) && ((bool) obj2));
            }
            set
            {
                this.ViewState["CausesValidation"] = value;
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("ListControl_DataTextField"), Themeable(false)]
        public virtual string DataTextField
        {
            get
            {
                object obj2 = this.ViewState["DataTextField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DataTextField"] = value;
                if (base.Initialized)
                {
                    base.RequiresDataBinding = true;
                }
            }
        }

        [DefaultValue(""), Themeable(false), WebCategory("Data"), WebSysDescription("ListControl_DataTextFormatString")]
        public virtual string DataTextFormatString
        {
            get
            {
                object obj2 = this.ViewState["DataTextFormatString"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DataTextFormatString"] = value;
                if (base.Initialized)
                {
                    base.RequiresDataBinding = true;
                }
            }
        }

        [DefaultValue(""), Themeable(false), WebCategory("Data"), WebSysDescription("ListControl_DataValueField")]
        public virtual string DataValueField
        {
            get
            {
                object obj2 = this.ViewState["DataValueField"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DataValueField"] = value;
                if (base.Initialized)
                {
                    base.RequiresDataBinding = true;
                }
            }
        }

        internal virtual bool IsMultiSelectInternal
        {
            get
            {
                return false;
            }
        }

        [MergableProperty(false), PersistenceMode(PersistenceMode.InnerDefaultProperty), WebCategory("Default"), DefaultValue((string) null), Editor("System.Web.UI.Design.WebControls.ListItemsCollectionEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), WebSysDescription("ListControl_Items")]
        public virtual ListItemCollection Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new ListItemCollection();
                    if (base.IsTrackingViewState)
                    {
                        this.items.TrackViewState();
                    }
                }
                return this.items;
            }
        }

        internal bool SaveSelectedIndicesViewState
        {
            get
            {
                if ((((base.Events[EventSelectedIndexChanged] != null) || (base.Events[EventTextChanged] != null)) || (!base.IsEnabled || !this.Visible)) || ((this.AutoPostBack && (this.Page != null)) && !this.Page.ClientSupportsJavaScript))
                {
                    return true;
                }
                foreach (ListItem item in this.Items)
                {
                    if (!item.Enabled)
                    {
                        return true;
                    }
                }
                Type type = base.GetType();
                return ((!(type == typeof(DropDownList)) && !(type == typeof(ListBox))) && (!(type == typeof(CheckBoxList)) && !(type == typeof(RadioButtonList))));
            }
        }

        [DefaultValue(0), Bindable(true), Browsable(false), WebSysDescription("WebControl_SelectedIndex"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Themeable(false), WebCategory("Behavior")]
        public virtual int SelectedIndex
        {
            get
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].Selected)
                    {
                        return i;
                    }
                }
                return -1;
            }
            set
            {
                if (value < -1)
                {
                    if (this.Items.Count != 0)
                    {
                        throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("ListControl_SelectionOutOfRange", new object[] { this.ID, "SelectedIndex" }));
                    }
                    value = -1;
                }
                if (((this.Items.Count != 0) && (value < this.Items.Count)) || (value == -1))
                {
                    this.ClearSelection();
                    if (value >= 0)
                    {
                        this.Items[value].Selected = true;
                    }
                }
                else if (this._stateLoaded)
                {
                    throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("ListControl_SelectionOutOfRange", new object[] { this.ID, "SelectedIndex" }));
                }
                this.cachedSelectedIndex = value;
            }
        }

        internal virtual ArrayList SelectedIndicesInternal
        {
            get
            {
                this.cachedSelectedIndices = new ArrayList(3);
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].Selected)
                    {
                        this.cachedSelectedIndices.Add(i);
                    }
                }
                return this.cachedSelectedIndices;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Behavior"), Browsable(false), DefaultValue((string) null), WebSysDescription("ListControl_SelectedItem")]
        public virtual ListItem SelectedItem
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    return this.Items[selectedIndex];
                }
                return null;
            }
        }

        [Bindable(true, BindingDirection.TwoWay), WebCategory("Behavior"), Browsable(false), DefaultValue(""), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Themeable(false), WebSysDescription("ListControl_SelectedValue")]
        public virtual string SelectedValue
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                if (selectedIndex >= 0)
                {
                    return this.Items[selectedIndex].Value;
                }
                return string.Empty;
            }
            set
            {
                if (this.Items.Count != 0)
                {
                    if ((value == null) || (base.DesignMode && (value.Length == 0)))
                    {
                        this.ClearSelection();
                        return;
                    }
                    ListItem item = this.Items.FindByValue(value);
                    if ((((this.Page != null) && this.Page.IsPostBack) && this._stateLoaded) && (item == null))
                    {
                        throw new ArgumentOutOfRangeException("value", System.Web.SR.GetString("ListControl_SelectionOutOfRange", new object[] { this.ID, "SelectedValue" }));
                    }
                    if (item != null)
                    {
                        this.ClearSelection();
                        item.Selected = true;
                    }
                }
                this.cachedSelectedValue = value;
            }
        }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Select;
            }
        }

        [DefaultValue(""), WebCategory("Behavior"), WebSysDescription("ListControl_Text"), Browsable(false), Themeable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string Text
        {
            get
            {
                return this.SelectedValue;
            }
            set
            {
                this.SelectedValue = value;
            }
        }

        [WebCategory("Behavior"), DefaultValue(""), WebSysDescription("PostBackControl_ValidationGroup"), Themeable(false)]
        public virtual string ValidationGroup
        {
            get
            {
                string str = (string) this.ViewState["ValidationGroup"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["ValidationGroup"] = value;
            }
        }
    }
}

