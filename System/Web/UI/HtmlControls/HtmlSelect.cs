namespace System.Web.UI.HtmlControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [SupportsEventValidation, ValidationProperty("Value"), ControlBuilder(typeof(HtmlSelectBuilder)), DefaultEvent("ServerChange")]
    public class HtmlSelect : HtmlContainerControl, IPostBackDataHandler, IParserAccessor
    {
        private DataSourceView _currentView;
        private bool _currentViewIsFromDataSourceID;
        private bool _currentViewValid;
        private bool _inited;
        private bool _pagePreLoadFired;
        private bool _requiresDataBinding;
        private bool _throwOnDataPropertyChange;
        private int cachedSelectedIndex;
        internal const string DataBoundViewStateKey = "_!DataBound";
        private object dataSource;
        private static readonly object EventServerChange = new object();
        private ListItemCollection items;

        [WebCategory("Action"), WebSysDescription("HtmlSelect_OnServerChange")]
        public event EventHandler ServerChange
        {
            add
            {
                base.Events.AddHandler(EventServerChange, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventServerChange, value);
            }
        }

        public HtmlSelect() : base("select")
        {
            this.cachedSelectedIndex = -1;
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (!(obj is ListItem))
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_Have_Children_Of_Type", new object[] { "HtmlSelect", obj.GetType().Name }));
            }
            this.Items.Add((ListItem) obj);
        }

        protected virtual void ClearSelection()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].Selected = false;
            }
        }

        private DataSourceView ConnectToDataSourceView()
        {
            if (!this._currentViewValid || base.DesignMode)
            {
                if ((this._currentView != null) && this._currentViewIsFromDataSourceID)
                {
                    this._currentView.DataSourceViewChanged -= new EventHandler(this.OnDataSourceViewChanged);
                }
                IDataSource source = null;
                string dataSourceID = this.DataSourceID;
                if (dataSourceID.Length != 0)
                {
                    Control control = DataBoundControlHelper.FindControl(this, dataSourceID);
                    if (control == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("DataControl_DataSourceDoesntExist", new object[] { this.ID, dataSourceID }));
                    }
                    source = control as IDataSource;
                    if (source == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("DataControl_DataSourceIDMustBeDataControl", new object[] { this.ID, dataSourceID }));
                    }
                }
                if (source == null)
                {
                    source = new ReadOnlyDataSource(this.DataSource, this.DataMember);
                }
                else if (this.DataSource != null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataControl_MultipleDataSources", new object[] { this.ID }));
                }
                DataSourceView view = source.GetView(this.DataMember);
                if (view == null)
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("DataControl_ViewNotFound", new object[] { this.ID }));
                }
                this._currentViewIsFromDataSourceID = this.IsBoundUsingDataSourceID;
                this._currentView = view;
                if ((this._currentView != null) && this._currentViewIsFromDataSourceID)
                {
                    this._currentView.DataSourceViewChanged += new EventHandler(this.OnDataSourceViewChanged);
                }
                this._currentViewValid = true;
            }
            return this._currentView;
        }

        protected override ControlCollection CreateControlCollection()
        {
            return new EmptyControlCollection(this);
        }

        protected void EnsureDataBound()
        {
            try
            {
                this._throwOnDataPropertyChange = true;
                if (this.RequiresDataBinding && (this.DataSourceID.Length > 0))
                {
                    this.DataBind();
                }
            }
            finally
            {
                this._throwOnDataPropertyChange = false;
            }
        }

        protected virtual IEnumerable GetData()
        {
            DataSourceView view = this.ConnectToDataSourceView();
            if (view != null)
            {
                return view.ExecuteSelect(DataSourceSelectArguments.Empty);
            }
            return null;
        }

        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string[] values = postCollection.GetValues(postDataKey);
            bool flag = false;
            if (values == null)
            {
                if (this.SelectedIndex != -1)
                {
                    this.SelectedIndex = -1;
                    flag = true;
                }
            }
            else if (!this.Multiple)
            {
                int num = this.Items.FindByValueInternal(values[0], false);
                if (this.SelectedIndex != num)
                {
                    this.SelectedIndex = num;
                    flag = true;
                }
            }
            else
            {
                int length = values.Length;
                int[] selectedIndices = this.SelectedIndices;
                int[] numArray2 = new int[length];
                for (int i = 0; i < length; i++)
                {
                    numArray2[i] = this.Items.FindByValueInternal(values[i], false);
                }
                if (selectedIndices.Length == length)
                {
                    for (int j = 0; j < length; j++)
                    {
                        if (numArray2[j] != selectedIndices[j])
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    this.Select(numArray2);
                }
            }
            if (flag)
            {
                base.ValidateEvent(postDataKey);
            }
            return flag;
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                Triplet triplet = (Triplet) savedState;
                base.LoadViewState(triplet.First);
                ((IStateManager) this.Items).LoadViewState(triplet.Second);
                object third = triplet.Third;
                if (third != null)
                {
                    this.Select((int[]) third);
                }
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);
            IEnumerable data = this.GetData();
            if (data != null)
            {
                bool flag = false;
                string dataTextField = this.DataTextField;
                string dataValueField = this.DataValueField;
                this.Items.Clear();
                ICollection is2 = data as ICollection;
                if (is2 != null)
                {
                    this.Items.Capacity = is2.Count;
                }
                if ((dataTextField.Length != 0) || (dataValueField.Length != 0))
                {
                    flag = true;
                }
                foreach (object obj2 in data)
                {
                    ListItem item = new ListItem();
                    if (flag)
                    {
                        if (dataTextField.Length > 0)
                        {
                            item.Text = DataBinder.GetPropertyValue(obj2, dataTextField, null);
                        }
                        if (dataValueField.Length > 0)
                        {
                            item.Value = DataBinder.GetPropertyValue(obj2, dataValueField, null);
                        }
                    }
                    else
                    {
                        item.Text = item.Value = obj2.ToString();
                    }
                    this.Items.Add(item);
                }
            }
            if (this.cachedSelectedIndex != -1)
            {
                this.SelectedIndex = this.cachedSelectedIndex;
                this.cachedSelectedIndex = -1;
            }
            this.ViewState["_!DataBound"] = true;
            this.RequiresDataBinding = false;
        }

        protected virtual void OnDataPropertyChanged()
        {
            if (this._throwOnDataPropertyChange)
            {
                throw new HttpException(System.Web.SR.GetString("DataBoundControl_InvalidDataPropertyChange", new object[] { this.ID }));
            }
            if (this._inited)
            {
                this.RequiresDataBinding = true;
            }
            this._currentViewValid = false;
        }

        protected virtual void OnDataSourceViewChanged(object sender, EventArgs e)
        {
            this.RequiresDataBinding = true;
        }

        protected internal override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (this.Page != null)
            {
                this.Page.PreLoad += new EventHandler(this.OnPagePreLoad);
                if (!base.IsViewStateEnabled && this.Page.IsPostBack)
                {
                    this.RequiresDataBinding = true;
                }
            }
        }

        protected internal override void OnLoad(EventArgs e)
        {
            this._inited = true;
            this.ConnectToDataSourceView();
            if (((this.Page != null) && !this._pagePreLoadFired) && (this.ViewState["_!DataBound"] == null))
            {
                if (!this.Page.IsPostBack)
                {
                    this.RequiresDataBinding = true;
                }
                else if (base.IsViewStateEnabled)
                {
                    this.RequiresDataBinding = true;
                }
            }
            base.OnLoad(e);
        }

        private void OnPagePreLoad(object sender, EventArgs e)
        {
            this._inited = true;
            if (this.Page != null)
            {
                this.Page.PreLoad -= new EventHandler(this.OnPagePreLoad);
                if (!this.Page.IsPostBack)
                {
                    this.RequiresDataBinding = true;
                }
                if ((this.Page.IsPostBack && base.IsViewStateEnabled) && (this.ViewState["_!DataBound"] == null))
                {
                    this.RequiresDataBinding = true;
                }
            }
            this._pagePreLoadFired = true;
        }

        protected internal override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if ((this.Page != null) && !base.Disabled)
            {
                if (this.Size > 1)
                {
                    this.Page.RegisterRequiresPostBack(this);
                }
                this.Page.RegisterEnabledControl(this);
            }
            this.EnsureDataBound();
        }

        protected virtual void OnServerChange(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventServerChange];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void RaisePostDataChangedEvent()
        {
            this.OnServerChange(EventArgs.Empty);
        }

        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            if (this.Page != null)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.RenderedNameAttribute);
            }
            writer.WriteAttribute("name", this.RenderedNameAttribute);
            base.Attributes.Remove("name");
            base.Attributes.Remove("DataValueField");
            base.Attributes.Remove("DataTextField");
            base.Attributes.Remove("DataMember");
            base.Attributes.Remove("DataSourceID");
            base.RenderAttributes(writer);
        }

        protected internal override void RenderChildren(HtmlTextWriter writer)
        {
            bool flag = false;
            bool flag2 = !this.Multiple;
            writer.WriteLine();
            writer.Indent++;
            ListItemCollection items = this.Items;
            int count = items.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    ListItem item = items[i];
                    writer.WriteBeginTag("option");
                    if (item.Selected)
                    {
                        if (flag2)
                        {
                            if (flag)
                            {
                                throw new HttpException(System.Web.SR.GetString("HtmlSelect_Cant_Multiselect_In_Single_Mode"));
                            }
                            flag = true;
                        }
                        writer.WriteAttribute("selected", "selected");
                    }
                    writer.WriteAttribute("value", item.Value, true);
                    item.Attributes.Remove("text");
                    item.Attributes.Remove("value");
                    item.Attributes.Remove("selected");
                    item.Attributes.Render(writer);
                    writer.Write('>');
                    HttpUtility.HtmlEncode(item.Text, writer);
                    writer.WriteEndTag("option");
                    writer.WriteLine();
                }
            }
            writer.Indent--;
        }

        protected override object SaveViewState()
        {
            object x = base.SaveViewState();
            object y = ((IStateManager) this.Items).SaveViewState();
            object z = null;
            if (((base.Events[EventServerChange] != null) || base.Disabled) || !this.Visible)
            {
                z = this.SelectedIndices;
            }
            if (((z == null) && (y == null)) && (x == null))
            {
                return null;
            }
            return new Triplet(x, y, z);
        }

        protected virtual void Select(int[] selectedIndices)
        {
            this.ClearSelection();
            for (int i = 0; i < selectedIndices.Length; i++)
            {
                int num2 = selectedIndices[i];
                if ((num2 >= 0) && (num2 < this.Items.Count))
                {
                    this.Items[num2].Selected = true;
                }
            }
        }

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            return this.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            this.RaisePostDataChangedEvent();
        }

        protected override void TrackViewState()
        {
            base.TrackViewState();
            ((IStateManager) this.Items).TrackViewState();
        }

        [DefaultValue(""), WebSysDescription("HtmlSelect_DataMember"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Data")]
        public virtual string DataMember
        {
            get
            {
                object obj2 = this.ViewState["DataMember"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                base.Attributes["DataMember"] = HtmlControl.MapStringAttributeToString(value);
                this.OnDataPropertyChanged();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Data"), DefaultValue((string) null), WebSysDescription("BaseDataBoundControl_DataSource")]
        public virtual object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if (((value != null) && !(value is IListSource)) && !(value is IEnumerable))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Invalid_DataSource_Type", new object[] { this.ID }));
                }
                this.dataSource = value;
                this.OnDataPropertyChanged();
            }
        }

        [DefaultValue(""), WebSysDescription("BaseDataBoundControl_DataSourceID"), WebCategory("Data")]
        public virtual string DataSourceID
        {
            get
            {
                object obj2 = this.ViewState["DataSourceID"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["DataSourceID"] = value;
                this.OnDataPropertyChanged();
            }
        }

        [DefaultValue(""), WebCategory("Data"), WebSysDescription("HtmlSelect_DataTextField")]
        public virtual string DataTextField
        {
            get
            {
                string str = base.Attributes["DataTextField"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                base.Attributes["DataTextField"] = HtmlControl.MapStringAttributeToString(value);
                if (this._inited)
                {
                    this.RequiresDataBinding = true;
                }
            }
        }

        [WebCategory("Data"), DefaultValue(""), WebSysDescription("HtmlSelect_DataValueField")]
        public virtual string DataValueField
        {
            get
            {
                string str = base.Attributes["DataValueField"];
                if (str != null)
                {
                    return str;
                }
                return string.Empty;
            }
            set
            {
                base.Attributes["DataValueField"] = HtmlControl.MapStringAttributeToString(value);
                if (this._inited)
                {
                    this.RequiresDataBinding = true;
                }
            }
        }

        public override string InnerHtml
        {
            get
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerHtml_not_supported", new object[] { base.GetType().Name }));
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerHtml_not_supported", new object[] { base.GetType().Name }));
            }
        }

        public override string InnerText
        {
            get
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerText_not_supported", new object[] { base.GetType().Name }));
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("InnerText_not_supported", new object[] { base.GetType().Name }));
            }
        }

        protected bool IsBoundUsingDataSourceID
        {
            get
            {
                return (this.DataSourceID.Length > 0);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public ListItemCollection Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new ListItemCollection();
                    if (base.IsTrackingViewState)
                    {
                        ((IStateManager) this.items).TrackViewState();
                    }
                }
                return this.items;
            }
        }

        [WebCategory("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue("")]
        public bool Multiple
        {
            get
            {
                string str = base.Attributes["multiple"];
                if (str == null)
                {
                    return false;
                }
                return str.Equals("multiple");
            }
            set
            {
                if (value)
                {
                    base.Attributes["multiple"] = "multiple";
                }
                else
                {
                    base.Attributes["multiple"] = null;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebCategory("Behavior"), DefaultValue("")]
        public string Name
        {
            get
            {
                return this.UniqueID;
            }
            set
            {
            }
        }

        internal string RenderedNameAttribute
        {
            get
            {
                return this.Name;
            }
        }

        protected bool RequiresDataBinding
        {
            get
            {
                return this._requiresDataBinding;
            }
            set
            {
                this._requiresDataBinding = value;
            }
        }

        [Browsable(false), HtmlControlPersistable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
                if ((this.Size > 1) || this.Multiple)
                {
                    return -1;
                }
                if (this.Items.Count > 0)
                {
                    this.Items[0].Selected = true;
                }
                return 0;
            }
            set
            {
                if (this.Items.Count == 0)
                {
                    this.cachedSelectedIndex = value;
                }
                else
                {
                    if ((value < -1) || (value >= this.Items.Count))
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    this.ClearSelection();
                    if (value >= 0)
                    {
                        this.Items[value].Selected = true;
                    }
                }
            }
        }

        protected virtual int[] SelectedIndices
        {
            get
            {
                int length = 0;
                int[] sourceArray = new int[3];
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].Selected)
                    {
                        if (length == sourceArray.Length)
                        {
                            int[] array = new int[length + length];
                            sourceArray.CopyTo(array, 0);
                            sourceArray = array;
                        }
                        sourceArray[length++] = i;
                    }
                }
                int[] destinationArray = new int[length];
                Array.Copy(sourceArray, 0, destinationArray, 0, length);
                return destinationArray;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Size
        {
            get
            {
                string s = base.Attributes["size"];
                if (s == null)
                {
                    return -1;
                }
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            set
            {
                base.Attributes["size"] = HtmlControl.MapIntegerAttributeToString(value);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                if ((selectedIndex >= 0) && (selectedIndex < this.Items.Count))
                {
                    return this.Items[selectedIndex].Value;
                }
                return string.Empty;
            }
            set
            {
                int num = this.Items.FindByValueInternal(value, true);
                if (num >= 0)
                {
                    this.SelectedIndex = num;
                }
            }
        }
    }
}

