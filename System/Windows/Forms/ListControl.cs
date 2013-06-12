namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [LookupBindingProperties("DataSource", "DisplayMember", "ValueMember", "SelectedValue"), ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true)]
    public abstract class ListControl : Control
    {
        private CurrencyManager dataManager;
        private object dataSource;
        private BindingMemberInfo displayMember;
        private TypeConverter displayMemberConverter;
        private static readonly object EVENT_DATASOURCECHANGED = new object();
        private static readonly object EVENT_DISPLAYMEMBERCHANGED = new object();
        private static readonly object EVENT_FORMAT = new object();
        private static readonly object EVENT_FORMATINFOCHANGED = new object();
        private static readonly object EVENT_FORMATSTRINGCHANGED = new object();
        private static readonly object EVENT_FORMATTINGENABLEDCHANGED = new object();
        private static readonly object EVENT_SELECTEDVALUECHANGED = new object();
        private static readonly object EVENT_VALUEMEMBERCHANGED = new object();
        private IFormatProvider formatInfo;
        private string formatString = string.Empty;
        private bool formattingEnabled;
        private bool inSetDataConnection;
        private bool isDataSourceInitEventHooked;
        private bool isDataSourceInitialized;
        private static TypeConverter stringTypeConverter = null;
        private BindingMemberInfo valueMember;

        [System.Windows.Forms.SRDescription("ListControlOnDataSourceChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler DataSourceChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_DATASOURCECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DATASOURCECHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ListControlOnDisplayMemberChangedDescr")]
        public event EventHandler DisplayMemberChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_DISPLAYMEMBERCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_DISPLAYMEMBERCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ListControlFormatDescr")]
        public event ListControlConvertEventHandler Format
        {
            add
            {
                base.Events.AddHandler(EVENT_FORMAT, value);
                this.RefreshItems();
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FORMAT, value);
                this.RefreshItems();
            }
        }

        [Browsable(false), System.Windows.Forms.SRDescription("ListControlFormatInfoChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged"), EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler FormatInfoChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_FORMATINFOCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FORMATINFOCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ListControlFormatStringChangedDescr")]
        public event EventHandler FormatStringChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_FORMATSTRINGCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FORMATSTRINGCHANGED, value);
            }
        }

        [System.Windows.Forms.SRCategory("CatPropertyChanged"), System.Windows.Forms.SRDescription("ListControlFormattingEnabledChangedDescr")]
        public event EventHandler FormattingEnabledChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_FORMATTINGENABLEDCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_FORMATTINGENABLEDCHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListControlOnSelectedValueChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler SelectedValueChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_SELECTEDVALUECHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_SELECTEDVALUECHANGED, value);
            }
        }

        [System.Windows.Forms.SRDescription("ListControlOnValueMemberChangedDescr"), System.Windows.Forms.SRCategory("CatPropertyChanged")]
        public event EventHandler ValueMemberChanged
        {
            add
            {
                base.Events.AddHandler(EVENT_VALUEMEMBERCHANGED, value);
            }
            remove
            {
                base.Events.RemoveHandler(EVENT_VALUEMEMBERCHANGED, value);
            }
        }

        protected ListControl()
        {
        }

        private bool BindingMemberInfoInDataManager(BindingMemberInfo bindingMemberInfo)
        {
            if (this.dataManager != null)
            {
                PropertyDescriptorCollection itemProperties = this.dataManager.GetItemProperties();
                int count = itemProperties.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!typeof(IList).IsAssignableFrom(itemProperties[i].PropertyType) && itemProperties[i].Name.Equals(bindingMemberInfo.BindingField))
                    {
                        return true;
                    }
                }
                for (int j = 0; j < count; j++)
                {
                    if (!typeof(IList).IsAssignableFrom(itemProperties[j].PropertyType) && (string.Compare(itemProperties[j].Name, bindingMemberInfo.BindingField, true, CultureInfo.CurrentCulture) == 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DataManager_ItemChanged(object sender, ItemChangedEventArgs e)
        {
            if (this.dataManager != null)
            {
                if (e.Index == -1)
                {
                    this.SetItemsCore(this.dataManager.List);
                    if (this.AllowSelection)
                    {
                        this.SelectedIndex = this.dataManager.Position;
                    }
                }
                else
                {
                    this.SetItemCore(e.Index, this.dataManager[e.Index]);
                }
            }
        }

        private void DataManager_PositionChanged(object sender, EventArgs e)
        {
            if ((this.dataManager != null) && this.AllowSelection)
            {
                this.SelectedIndex = this.dataManager.Position;
            }
        }

        private void DataSourceDisposed(object sender, EventArgs e)
        {
            this.SetDataConnection(null, new BindingMemberInfo(""), true);
        }

        private void DataSourceInitialized(object sender, EventArgs e)
        {
            this.SetDataConnection(this.dataSource, this.displayMember, true);
        }

        protected object FilterItemOnProperty(object item)
        {
            return this.FilterItemOnProperty(item, this.displayMember.BindingField);
        }

        protected object FilterItemOnProperty(object item, string field)
        {
            if ((item != null) && (field.Length > 0))
            {
                try
                {
                    PropertyDescriptor descriptor;
                    if (this.dataManager != null)
                    {
                        descriptor = this.dataManager.GetItemProperties().Find(field, true);
                    }
                    else
                    {
                        descriptor = TypeDescriptor.GetProperties(item).Find(field, true);
                    }
                    if (descriptor != null)
                    {
                        item = descriptor.GetValue(item);
                    }
                }
                catch
                {
                }
            }
            return item;
        }

        internal int FindStringInternal(string str, IList items, int startIndex, bool exact)
        {
            return this.FindStringInternal(str, items, startIndex, exact, true);
        }

        internal int FindStringInternal(string str, IList items, int startIndex, bool exact, bool ignorecase)
        {
            if ((str != null) && (items != null))
            {
                if ((startIndex < -1) || (startIndex >= items.Count))
                {
                    return -1;
                }
                bool flag = false;
                int length = str.Length;
                int num2 = 0;
                for (int i = (startIndex + 1) % items.Count; num2 < items.Count; i = (i + 1) % items.Count)
                {
                    num2++;
                    if (exact)
                    {
                        flag = string.Compare(str, this.GetItemText(items[i]), ignorecase, CultureInfo.CurrentCulture) == 0;
                    }
                    else
                    {
                        flag = string.Compare(str, 0, this.GetItemText(items[i]), 0, length, ignorecase, CultureInfo.CurrentCulture) == 0;
                    }
                    if (flag)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public string GetItemText(object item)
        {
            if (!this.formattingEnabled)
            {
                if (item == null)
                {
                    return string.Empty;
                }
                item = this.FilterItemOnProperty(item, this.displayMember.BindingField);
                if (item == null)
                {
                    return "";
                }
                return Convert.ToString(item, CultureInfo.CurrentCulture);
            }
            object obj2 = this.FilterItemOnProperty(item, this.displayMember.BindingField);
            ListControlConvertEventArgs e = new ListControlConvertEventArgs(obj2, typeof(string), item);
            this.OnFormat(e);
            if ((e.Value != item) && (e.Value is string))
            {
                return (string) e.Value;
            }
            if (stringTypeConverter == null)
            {
                stringTypeConverter = TypeDescriptor.GetConverter(typeof(string));
            }
            try
            {
                return (string) Formatter.FormatObject(obj2, typeof(string), this.DisplayMemberConverter, stringTypeConverter, this.formatString, this.formatInfo, null, DBNull.Value);
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsSecurityOrCriticalException(exception))
                {
                    throw;
                }
                return ((obj2 != null) ? Convert.ToString(item, CultureInfo.CurrentCulture) : "");
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if ((keyData & Keys.Alt) == Keys.Alt)
            {
                return false;
            }
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.PageUp:
                case Keys.Next:
                case Keys.End:
                case Keys.Home:
                    return true;
            }
            return base.IsInputKey(keyData);
        }

        protected override void OnBindingContextChanged(EventArgs e)
        {
            this.SetDataConnection(this.dataSource, this.displayMember, true);
            base.OnBindingContextChanged(e);
        }

        protected virtual void OnDataSourceChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_DATASOURCECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDisplayMemberChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_DISPLAYMEMBERCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFormat(ListControlConvertEventArgs e)
        {
            ListControlConvertEventHandler handler = base.Events[EVENT_FORMAT] as ListControlConvertEventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFormatInfoChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_FORMATINFOCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFormatStringChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_FORMATSTRINGCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFormattingEnabledChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_FORMATTINGENABLEDCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            this.OnSelectedValueChanged(EventArgs.Empty);
        }

        protected virtual void OnSelectedValueChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_SELECTEDVALUECHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnValueMemberChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EVENT_VALUEMEMBERCHANGED] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected abstract void RefreshItem(int index);
        protected virtual void RefreshItems()
        {
        }

        private void SetDataConnection(object newDataSource, BindingMemberInfo newDisplayMember, bool force)
        {
            bool flag = this.dataSource != newDataSource;
            bool flag2 = !this.displayMember.Equals(newDisplayMember);
            if (!this.inSetDataConnection)
            {
                try
                {
                    if ((force || flag) || flag2)
                    {
                        this.inSetDataConnection = true;
                        IList list = (this.DataManager != null) ? this.DataManager.List : null;
                        bool flag3 = this.DataManager == null;
                        this.UnwireDataSource();
                        this.dataSource = newDataSource;
                        this.displayMember = newDisplayMember;
                        this.WireDataSource();
                        if (this.isDataSourceInitialized)
                        {
                            CurrencyManager manager = null;
                            if (((newDataSource != null) && (this.BindingContext != null)) && (newDataSource != Convert.DBNull))
                            {
                                manager = (CurrencyManager) this.BindingContext[newDataSource, newDisplayMember.BindingPath];
                            }
                            if (this.dataManager != manager)
                            {
                                if (this.dataManager != null)
                                {
                                    this.dataManager.ItemChanged -= new ItemChangedEventHandler(this.DataManager_ItemChanged);
                                    this.dataManager.PositionChanged -= new EventHandler(this.DataManager_PositionChanged);
                                }
                                this.dataManager = manager;
                                if (this.dataManager != null)
                                {
                                    this.dataManager.ItemChanged += new ItemChangedEventHandler(this.DataManager_ItemChanged);
                                    this.dataManager.PositionChanged += new EventHandler(this.DataManager_PositionChanged);
                                }
                            }
                            if (((this.dataManager != null) && (flag2 || flag)) && (((this.displayMember.BindingMember != null) && (this.displayMember.BindingMember.Length != 0)) && !this.BindingMemberInfoInDataManager(this.displayMember)))
                            {
                                throw new ArgumentException(System.Windows.Forms.SR.GetString("ListControlWrongDisplayMember"), "newDisplayMember");
                            }
                            if (((this.dataManager != null) && ((flag || flag2) || force)) && (flag2 || (force && ((list != this.dataManager.List) || flag3))))
                            {
                                this.DataManager_ItemChanged(this.dataManager, new ItemChangedEventArgs(-1));
                            }
                        }
                        this.displayMemberConverter = null;
                    }
                    if (flag)
                    {
                        this.OnDataSourceChanged(EventArgs.Empty);
                    }
                    if (flag2)
                    {
                        this.OnDisplayMemberChanged(EventArgs.Empty);
                    }
                }
                finally
                {
                    this.inSetDataConnection = false;
                }
            }
        }

        protected virtual void SetItemCore(int index, object value)
        {
        }

        protected abstract void SetItemsCore(IList items);
        private void UnwireDataSource()
        {
            if (this.dataSource is IComponent)
            {
                ((IComponent) this.dataSource).Disposed -= new EventHandler(this.DataSourceDisposed);
            }
            ISupportInitializeNotification dataSource = this.dataSource as ISupportInitializeNotification;
            if ((dataSource != null) && this.isDataSourceInitEventHooked)
            {
                dataSource.Initialized -= new EventHandler(this.DataSourceInitialized);
                this.isDataSourceInitEventHooked = false;
            }
        }

        private void WireDataSource()
        {
            if (this.dataSource is IComponent)
            {
                ((IComponent) this.dataSource).Disposed += new EventHandler(this.DataSourceDisposed);
            }
            ISupportInitializeNotification dataSource = this.dataSource as ISupportInitializeNotification;
            if ((dataSource != null) && !dataSource.IsInitialized)
            {
                dataSource.Initialized += new EventHandler(this.DataSourceInitialized);
                this.isDataSourceInitEventHooked = true;
                this.isDataSourceInitialized = false;
            }
            else
            {
                this.isDataSourceInitialized = true;
            }
        }

        protected virtual bool AllowSelection
        {
            get
            {
                return true;
            }
        }

        internal bool BindingFieldEmpty
        {
            get
            {
                return (this.displayMember.BindingField.Length <= 0);
            }
        }

        protected CurrencyManager DataManager
        {
            get
            {
                return this.dataManager;
            }
        }

        [System.Windows.Forms.SRDescription("ListControlDataSourceDescr"), System.Windows.Forms.SRCategory("CatData"), DefaultValue((string) null), RefreshProperties(RefreshProperties.Repaint), AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get
            {
                return this.dataSource;
            }
            set
            {
                if (((value != null) && !(value is IList)) && !(value is IListSource))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("BadDataSourceForComplexBinding"));
                }
                if (this.dataSource != value)
                {
                    try
                    {
                        this.SetDataConnection(value, this.displayMember, false);
                    }
                    catch
                    {
                        this.DisplayMember = "";
                    }
                    if (value == null)
                    {
                        this.DisplayMember = "";
                    }
                }
            }
        }

        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("ListControlDisplayMemberDescr"), System.Windows.Forms.SRCategory("CatData"), DefaultValue(""), TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string DisplayMember
        {
            get
            {
                return this.displayMember.BindingMember;
            }
            set
            {
                BindingMemberInfo displayMember = this.displayMember;
                try
                {
                    this.SetDataConnection(this.dataSource, new BindingMemberInfo(value), false);
                }
                catch
                {
                    this.displayMember = displayMember;
                }
            }
        }

        private TypeConverter DisplayMemberConverter
        {
            get
            {
                if ((this.displayMemberConverter == null) && (this.DataManager != null))
                {
                    PropertyDescriptorCollection itemProperties = this.DataManager.GetItemProperties();
                    if (itemProperties != null)
                    {
                        PropertyDescriptor descriptor = itemProperties.Find(this.displayMember.BindingField, true);
                        if (descriptor != null)
                        {
                            this.displayMemberConverter = descriptor.Converter;
                        }
                    }
                }
                return this.displayMemberConverter;
            }
        }

        [Browsable(false), DefaultValue((string) null), EditorBrowsable(EditorBrowsableState.Advanced)]
        public IFormatProvider FormatInfo
        {
            get
            {
                return this.formatInfo;
            }
            set
            {
                if (value != this.formatInfo)
                {
                    this.formatInfo = value;
                    this.RefreshItems();
                    this.OnFormatInfoChanged(EventArgs.Empty);
                }
            }
        }

        [Editor("System.Windows.Forms.Design.FormatStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), MergableProperty(false), System.Windows.Forms.SRDescription("ListControlFormatStringDescr"), DefaultValue("")]
        public string FormatString
        {
            get
            {
                return this.formatString;
            }
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (!value.Equals(this.formatString))
                {
                    this.formatString = value;
                    this.RefreshItems();
                    this.OnFormatStringChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ListControlFormattingEnabledDescr")]
        public bool FormattingEnabled
        {
            get
            {
                return this.formattingEnabled;
            }
            set
            {
                if (value != this.formattingEnabled)
                {
                    this.formattingEnabled = value;
                    this.RefreshItems();
                    this.OnFormattingEnabledChanged(EventArgs.Empty);
                }
            }
        }

        public abstract int SelectedIndex { get; set; }

        [System.Windows.Forms.SRCategory("CatData"), Browsable(false), Bindable(true), DefaultValue((string) null), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), System.Windows.Forms.SRDescription("ListControlSelectedValueDescr")]
        public object SelectedValue
        {
            get
            {
                if ((this.SelectedIndex != -1) && (this.dataManager != null))
                {
                    object item = this.dataManager[this.SelectedIndex];
                    return this.FilterItemOnProperty(item, this.valueMember.BindingField);
                }
                return null;
            }
            set
            {
                if (this.dataManager != null)
                {
                    string bindingField = this.valueMember.BindingField;
                    if (string.IsNullOrEmpty(bindingField))
                    {
                        throw new InvalidOperationException(System.Windows.Forms.SR.GetString("ListControlEmptyValueMemberInSettingSelectedValue"));
                    }
                    PropertyDescriptor property = this.dataManager.GetItemProperties().Find(bindingField, true);
                    int num = this.dataManager.Find(property, value, true);
                    this.SelectedIndex = num;
                }
            }
        }

        [System.Windows.Forms.SRCategory("CatData"), DefaultValue(""), Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), System.Windows.Forms.SRDescription("ListControlValueMemberDescr")]
        public string ValueMember
        {
            get
            {
                return this.valueMember.BindingMember;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                BindingMemberInfo newDisplayMember = new BindingMemberInfo(value);
                if (!newDisplayMember.Equals(this.valueMember))
                {
                    if (this.DisplayMember.Length == 0)
                    {
                        this.SetDataConnection(this.DataSource, newDisplayMember, false);
                    }
                    if (((this.dataManager != null) && (value != null)) && ((value.Length != 0) && !this.BindingMemberInfoInDataManager(newDisplayMember)))
                    {
                        throw new ArgumentException(System.Windows.Forms.SR.GetString("ListControlWrongValueMember"), "value");
                    }
                    this.valueMember = newDisplayMember;
                    this.OnValueMemberChanged(EventArgs.Empty);
                    this.OnSelectedValueChanged(EventArgs.Empty);
                }
            }
        }
    }
}

