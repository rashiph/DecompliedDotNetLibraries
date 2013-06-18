namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.VisualStyles;

    public class CollectionEditor : UITypeEditor
    {
        private System.Type collectionItemType;
        private ITypeDescriptorContext currentContext;
        private bool ignoreChangedEvents;
        private bool ignoreChangingEvents;
        private System.Type[] newItemTypes;
        private System.Type type;

        public CollectionEditor(System.Type type)
        {
            this.type = type;
        }

        protected virtual void CancelChanges()
        {
        }

        protected virtual bool CanRemoveInstance(object value)
        {
            IComponent component = value as IComponent;
            if (component != null)
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.NotInherited))
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual bool CanSelectMultipleInstances()
        {
            return true;
        }

        protected virtual CollectionForm CreateCollectionForm()
        {
            return new CollectionEditorCollectionForm(this);
        }

        protected virtual System.Type CreateCollectionItemType()
        {
            PropertyInfo[] properties = TypeDescriptor.GetReflectionType(this.CollectionType).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].Name.Equals("Item") || properties[i].Name.Equals("Items"))
                {
                    return properties[i].PropertyType;
                }
            }
            return typeof(object);
        }

        protected virtual object CreateInstance(System.Type itemType)
        {
            return CreateInstance(itemType, (IDesignerHost) this.GetService(typeof(IDesignerHost)), null);
        }

        internal static object CreateInstance(System.Type itemType, IDesignerHost host, string name)
        {
            object obj2 = null;
            if (typeof(IComponent).IsAssignableFrom(itemType) && (host != null))
            {
                obj2 = host.CreateComponent(itemType, name);
                if (host != null)
                {
                    IComponentInitializer designer = host.GetDesigner((IComponent) obj2) as IComponentInitializer;
                    if (designer != null)
                    {
                        designer.InitializeNewComponent(null);
                    }
                }
            }
            if (obj2 == null)
            {
                obj2 = TypeDescriptor.CreateInstance(host, itemType, null, null);
            }
            return obj2;
        }

        protected virtual System.Type[] CreateNewItemTypes()
        {
            return new System.Type[] { this.CollectionItemType };
        }

        protected virtual void DestroyInstance(object instance)
        {
            IComponent component = instance as IComponent;
            if (component != null)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    service.DestroyComponent(component);
                }
                else
                {
                    component.Dispose();
                }
            }
            else
            {
                IDisposable disposable = instance as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                this.currentContext = context;
                CollectionForm form = this.CreateCollectionForm();
                ITypeDescriptorContext currentContext = this.currentContext;
                form.EditValue = value;
                this.ignoreChangingEvents = false;
                this.ignoreChangedEvents = false;
                DesignerTransaction transaction = null;
                bool flag = true;
                IComponentChangeService service2 = null;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                try
                {
                    try
                    {
                        if (service != null)
                        {
                            transaction = service.CreateTransaction(System.Design.SR.GetString("CollectionEditorUndoBatchDesc", new object[] { this.CollectionItemType.Name }));
                        }
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw exception;
                        }
                        return value;
                    }
                    service2 = (service != null) ? ((IComponentChangeService) service.GetService(typeof(IComponentChangeService))) : null;
                    if (service2 != null)
                    {
                        service2.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                        service2.ComponentChanging += new ComponentChangingEventHandler(this.OnComponentChanging);
                    }
                    if (form.ShowEditorDialog(edSvc) == DialogResult.OK)
                    {
                        value = form.EditValue;
                        return value;
                    }
                    flag = false;
                }
                finally
                {
                    form.EditValue = null;
                    this.currentContext = currentContext;
                    if (transaction != null)
                    {
                        if (flag)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Cancel();
                        }
                    }
                    if (service2 != null)
                    {
                        service2.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                        service2.ComponentChanging -= new ComponentChangingEventHandler(this.OnComponentChanging);
                    }
                    form.Dispose();
                }
            }
            return value;
        }

        protected virtual string GetDisplayText(object value)
        {
            string str;
            if (value == null)
            {
                return string.Empty;
            }
            PropertyDescriptor defaultProperty = TypeDescriptor.GetProperties(value)["Name"];
            if ((defaultProperty != null) && (defaultProperty.PropertyType == typeof(string)))
            {
                str = (string) defaultProperty.GetValue(value);
                if ((str != null) && (str.Length > 0))
                {
                    return str;
                }
            }
            defaultProperty = TypeDescriptor.GetDefaultProperty(this.CollectionType);
            if ((defaultProperty != null) && (defaultProperty.PropertyType == typeof(string)))
            {
                str = (string) defaultProperty.GetValue(value);
                if ((str != null) && (str.Length > 0))
                {
                    return str;
                }
            }
            str = TypeDescriptor.GetConverter(value).ConvertToString(value);
            if ((str != null) && (str.Length != 0))
            {
                return str;
            }
            return value.GetType().Name;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual object[] GetItems(object editValue)
        {
            if ((editValue == null) || !(editValue is ICollection))
            {
                return new object[0];
            }
            ArrayList list = new ArrayList();
            ICollection is2 = (ICollection) editValue;
            foreach (object obj2 in is2)
            {
                list.Add(obj2);
            }
            object[] array = new object[list.Count];
            list.CopyTo(array, 0);
            return array;
        }

        protected virtual IList GetObjectsFromInstance(object instance)
        {
            ArrayList list = new ArrayList();
            list.Add(instance);
            return list;
        }

        protected object GetService(System.Type serviceType)
        {
            if (this.Context != null)
            {
                return this.Context.GetService(serviceType);
            }
            return null;
        }

        private bool IsAnyObjectInheritedReadOnly(object[] items)
        {
            IInheritanceService service = null;
            bool flag = false;
            foreach (object obj2 in items)
            {
                IComponent component = obj2 as IComponent;
                if ((component != null) && (component.Site == null))
                {
                    if (!flag)
                    {
                        flag = true;
                        if (this.Context != null)
                        {
                            service = (IInheritanceService) this.Context.GetService(typeof(IInheritanceService));
                        }
                    }
                    if ((service != null) && service.GetInheritanceAttribute(component).Equals(InheritanceAttribute.InheritedReadOnly))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (!this.ignoreChangedEvents && (sender != this.Context.Instance))
            {
                this.ignoreChangedEvents = true;
                this.Context.OnComponentChanged();
            }
        }

        private void OnComponentChanging(object sender, ComponentChangingEventArgs e)
        {
            if (!this.ignoreChangingEvents && (sender != this.Context.Instance))
            {
                this.ignoreChangingEvents = true;
                this.Context.OnComponentChanging();
            }
        }

        internal virtual void OnItemRemoving(object item)
        {
        }

        protected virtual object SetItems(object editValue, object[] value)
        {
            if ((editValue != null) && (editValue is IList))
            {
                IList list = (IList) editValue;
                list.Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    list.Add(value[i]);
                }
            }
            return editValue;
        }

        protected virtual void ShowHelp()
        {
            IHelpService service = this.GetService(typeof(IHelpService)) as IHelpService;
            if (service != null)
            {
                service.ShowHelpFromKeyword(this.HelpTopic);
            }
        }

        protected System.Type CollectionItemType
        {
            get
            {
                if (this.collectionItemType == null)
                {
                    this.collectionItemType = this.CreateCollectionItemType();
                }
                return this.collectionItemType;
            }
        }

        protected System.Type CollectionType
        {
            get
            {
                return this.type;
            }
        }

        protected ITypeDescriptorContext Context
        {
            get
            {
                return this.currentContext;
            }
        }

        protected virtual string HelpTopic
        {
            get
            {
                return "net.ComponentModel.CollectionEditor";
            }
        }

        protected System.Type[] NewItemTypes
        {
            get
            {
                if (this.newItemTypes == null)
                {
                    this.newItemTypes = this.CreateNewItemTypes();
                }
                return this.newItemTypes;
            }
        }

        private class CollectionEditorCollectionForm : CollectionEditor.CollectionForm
        {
            private CollectionEditor.SplitButton addButton;
            private ContextMenuStrip addDownMenu;
            private TableLayoutPanel addRemoveTableLayoutPanel;
            private System.Windows.Forms.Button cancelButton;
            private ArrayList createdItems;
            private bool dirty;
            private System.Windows.Forms.Button downButton;
            private CollectionEditor editor;
            private CollectionEditor.FilterListBox listbox;
            private static readonly double LOG10 = Math.Log(10.0);
            private Label membersLabel;
            private System.Windows.Forms.Button okButton;
            private TableLayoutPanel okCancelTableLayoutPanel;
            private ArrayList originalItems;
            private TableLayoutPanel overArchingTableLayoutPanel;
            private const int PAINT_INDENT = 0x1a;
            private const int PAINT_WIDTH = 20;
            private Label propertiesLabel;
            private VsPropertyGrid propertyBrowser;
            private System.Windows.Forms.Button removeButton;
            private ArrayList removedItems;
            private int suspendEnabledCount;
            private const int TEXT_INDENT = 1;
            private System.Windows.Forms.Button upButton;

            public CollectionEditorCollectionForm(CollectionEditor editor) : base(editor)
            {
                this.editor = editor;
                this.InitializeComponent();
                this.Text = System.Design.SR.GetString("CollectionEditorCaption", new object[] { base.CollectionItemType.Name });
                this.HookEvents();
                System.Type[] newItemTypes = base.NewItemTypes;
                if (newItemTypes.Length > 1)
                {
                    EventHandler handler = new EventHandler(this.AddDownMenu_click);
                    this.addButton.ShowSplit = true;
                    this.addDownMenu = new ContextMenuStrip();
                    this.addButton.ContextMenuStrip = this.addDownMenu;
                    for (int i = 0; i < newItemTypes.Length; i++)
                    {
                        this.addDownMenu.Items.Add(new TypeMenuItem(newItemTypes[i], handler));
                    }
                }
                this.AdjustListBoxItemHeight();
            }

            private void AddButton_click(object sender, EventArgs e)
            {
                this.PerformAdd();
            }

            private void AddDownMenu_click(object sender, EventArgs e)
            {
                if (sender is TypeMenuItem)
                {
                    TypeMenuItem item = (TypeMenuItem) sender;
                    this.CreateAndAddInstance(item.ItemType);
                }
            }

            private void AddItems(IList instances)
            {
                if (this.createdItems == null)
                {
                    this.createdItems = new ArrayList();
                }
                this.listbox.BeginUpdate();
                try
                {
                    foreach (object obj2 in instances)
                    {
                        if (obj2 != null)
                        {
                            this.dirty = true;
                            this.createdItems.Add(obj2);
                            ListItem item = new ListItem(this.editor, obj2);
                            this.listbox.Items.Add(item);
                        }
                    }
                }
                finally
                {
                    this.listbox.EndUpdate();
                }
                if (instances.Count == 1)
                {
                    this.UpdateItemWidths(this.listbox.Items[this.listbox.Items.Count - 1] as ListItem);
                }
                else
                {
                    this.UpdateItemWidths(null);
                }
                this.SuspendEnabledUpdates();
                try
                {
                    this.listbox.ClearSelected();
                    this.listbox.SelectedIndex = this.listbox.Items.Count - 1;
                    object[] objArray = new object[this.listbox.Items.Count];
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        objArray[i] = ((ListItem) this.listbox.Items[i]).Value;
                    }
                    base.Items = objArray;
                    if ((this.listbox.Items.Count > 0) && (this.listbox.SelectedIndex != (this.listbox.Items.Count - 1)))
                    {
                        this.listbox.ClearSelected();
                        this.listbox.SelectedIndex = this.listbox.Items.Count - 1;
                    }
                }
                finally
                {
                    this.ResumeEnabledUpdates(true);
                }
            }

            private void AdjustListBoxItemHeight()
            {
                this.listbox.ItemHeight = this.Font.Height + (SystemInformation.BorderSize.Width * 2);
            }

            private bool AllowRemoveInstance(object value)
            {
                return (((this.createdItems != null) && this.createdItems.Contains(value)) || base.CanRemoveInstance(value));
            }

            private int CalcItemWidth(Graphics g, ListItem item)
            {
                int count = this.listbox.Items.Count;
                if (count < 2)
                {
                    count = 2;
                }
                SizeF ef = g.MeasureString(count.ToString(CultureInfo.CurrentCulture), this.listbox.Font);
                int num2 = ((int) (Math.Log((double) (count - 1)) / LOG10)) + 1;
                int num3 = 4 + (num2 * (this.Font.Height / 2));
                num3 = Math.Max(num3, (int) Math.Ceiling((double) ef.Width)) + (SystemInformation.BorderSize.Width * 4);
                SizeF ef2 = g.MeasureString(this.GetDisplayText(item), this.listbox.Font);
                int num4 = 0;
                if ((item.Editor != null) && item.Editor.GetPaintValueSupported())
                {
                    num4 = 0x15;
                }
                return (((((int) Math.Ceiling((double) ef2.Width)) + num3) + num4) + (SystemInformation.BorderSize.Width * 4));
            }

            private void CancelButton_click(object sender, EventArgs e)
            {
                try
                {
                    this.editor.CancelChanges();
                    if (this.CollectionEditable && this.dirty)
                    {
                        this.dirty = false;
                        this.listbox.Items.Clear();
                        if (this.createdItems != null)
                        {
                            object[] objArray = this.createdItems.ToArray();
                            if (((objArray.Length > 0) && (objArray[0] is IComponent)) && (((IComponent) objArray[0]).Site != null))
                            {
                                return;
                            }
                            for (int i = 0; i < objArray.Length; i++)
                            {
                                base.DestroyInstance(objArray[i]);
                            }
                            this.createdItems.Clear();
                        }
                        if (this.removedItems != null)
                        {
                            this.removedItems.Clear();
                        }
                        if ((this.originalItems != null) && (this.originalItems.Count > 0))
                        {
                            object[] objArray2 = new object[this.originalItems.Count];
                            for (int j = 0; j < this.originalItems.Count; j++)
                            {
                                objArray2[j] = this.originalItems[j];
                            }
                            base.Items = objArray2;
                            this.originalItems.Clear();
                        }
                        else
                        {
                            base.Items = new object[0];
                        }
                    }
                }
                catch (Exception exception)
                {
                    base.DialogResult = DialogResult.None;
                    this.DisplayError(exception);
                }
            }

            private void CollectionEditor_HelpButtonClicked(object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                this.editor.ShowHelp();
            }

            private void CreateAndAddInstance(System.Type type)
            {
                try
                {
                    object instance = base.CreateInstance(type);
                    IList objectsFromInstance = this.editor.GetObjectsFromInstance(instance);
                    if (objectsFromInstance != null)
                    {
                        this.AddItems(objectsFromInstance);
                    }
                }
                catch (Exception exception)
                {
                    this.DisplayError(exception);
                }
            }

            private void DownButton_click(object sender, EventArgs e)
            {
                try
                {
                    this.SuspendEnabledUpdates();
                    this.dirty = true;
                    int selectedIndex = this.listbox.SelectedIndex;
                    if (selectedIndex != (this.listbox.Items.Count - 1))
                    {
                        int topIndex = this.listbox.TopIndex;
                        object obj2 = this.listbox.Items[selectedIndex];
                        this.listbox.Items[selectedIndex] = this.listbox.Items[selectedIndex + 1];
                        this.listbox.Items[selectedIndex + 1] = obj2;
                        if (topIndex < (this.listbox.Items.Count - 1))
                        {
                            this.listbox.TopIndex = topIndex + 1;
                        }
                        this.listbox.ClearSelected();
                        this.listbox.SelectedIndex = selectedIndex + 1;
                        Control control = (Control) sender;
                        if (control.Enabled)
                        {
                            control.Focus();
                        }
                    }
                }
                finally
                {
                    this.ResumeEnabledUpdates(true);
                }
            }

            private void Form_HelpRequested(object sender, HelpEventArgs e)
            {
                this.editor.ShowHelp();
            }

            private void Form_Shown(object sender, EventArgs e)
            {
                this.OnEditValueChanged();
            }

            private string GetDisplayText(ListItem item)
            {
                if (item != null)
                {
                    return item.ToString();
                }
                return string.Empty;
            }

            private void HookEvents()
            {
                this.listbox.KeyDown += new KeyEventHandler(this.Listbox_keyDown);
                this.listbox.DrawItem += new DrawItemEventHandler(this.Listbox_drawItem);
                this.listbox.SelectedIndexChanged += new EventHandler(this.Listbox_selectedIndexChanged);
                this.listbox.HandleCreated += new EventHandler(this.Listbox_handleCreated);
                this.upButton.Click += new EventHandler(this.UpButton_click);
                this.downButton.Click += new EventHandler(this.DownButton_click);
                this.propertyBrowser.PropertyValueChanged += new PropertyValueChangedEventHandler(this.PropertyGrid_propertyValueChanged);
                this.addButton.Click += new EventHandler(this.AddButton_click);
                this.removeButton.Click += new EventHandler(this.RemoveButton_click);
                this.okButton.Click += new EventHandler(this.OKButton_click);
                this.cancelButton.Click += new EventHandler(this.CancelButton_click);
                base.HelpButtonClicked += new CancelEventHandler(this.CollectionEditor_HelpButtonClicked);
                base.HelpRequested += new HelpEventHandler(this.Form_HelpRequested);
                base.Shown += new EventHandler(this.Form_Shown);
            }

            private void InitializeComponent()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(CollectionEditor));
                this.membersLabel = new Label();
                this.listbox = new CollectionEditor.FilterListBox();
                this.upButton = new System.Windows.Forms.Button();
                this.downButton = new System.Windows.Forms.Button();
                this.propertiesLabel = new Label();
                this.propertyBrowser = new VsPropertyGrid(base.Context);
                this.addButton = new CollectionEditor.SplitButton();
                this.removeButton = new System.Windows.Forms.Button();
                this.okButton = new System.Windows.Forms.Button();
                this.cancelButton = new System.Windows.Forms.Button();
                this.okCancelTableLayoutPanel = new TableLayoutPanel();
                this.overArchingTableLayoutPanel = new TableLayoutPanel();
                this.addRemoveTableLayoutPanel = new TableLayoutPanel();
                this.okCancelTableLayoutPanel.SuspendLayout();
                this.overArchingTableLayoutPanel.SuspendLayout();
                this.addRemoveTableLayoutPanel.SuspendLayout();
                base.SuspendLayout();
                manager.ApplyResources(this.membersLabel, "membersLabel");
                this.membersLabel.Margin = new Padding(0, 0, 3, 3);
                this.membersLabel.Name = "membersLabel";
                manager.ApplyResources(this.listbox, "listbox");
                this.listbox.SelectionMode = this.CanSelectMultipleInstances() ? SelectionMode.MultiExtended : SelectionMode.One;
                this.listbox.DrawMode = DrawMode.OwnerDrawFixed;
                this.listbox.FormattingEnabled = true;
                this.listbox.Margin = new Padding(0, 3, 3, 3);
                this.listbox.Name = "listbox";
                this.overArchingTableLayoutPanel.SetRowSpan(this.listbox, 2);
                manager.ApplyResources(this.upButton, "upButton");
                this.upButton.Name = "upButton";
                manager.ApplyResources(this.downButton, "downButton");
                this.downButton.Name = "downButton";
                manager.ApplyResources(this.propertiesLabel, "propertiesLabel");
                this.propertiesLabel.AutoEllipsis = true;
                this.propertiesLabel.Margin = new Padding(0, 0, 3, 3);
                this.propertiesLabel.Name = "propertiesLabel";
                manager.ApplyResources(this.propertyBrowser, "propertyBrowser");
                this.propertyBrowser.CommandsVisibleIfAvailable = false;
                this.propertyBrowser.Margin = new Padding(3, 3, 0, 3);
                this.propertyBrowser.Name = "propertyBrowser";
                this.overArchingTableLayoutPanel.SetRowSpan(this.propertyBrowser, 3);
                manager.ApplyResources(this.addButton, "addButton");
                this.addButton.Margin = new Padding(0, 3, 3, 3);
                this.addButton.Name = "addButton";
                manager.ApplyResources(this.removeButton, "removeButton");
                this.removeButton.Margin = new Padding(3, 3, 0, 3);
                this.removeButton.Name = "removeButton";
                manager.ApplyResources(this.okButton, "okButton");
                this.okButton.DialogResult = DialogResult.OK;
                this.okButton.Margin = new Padding(0, 3, 3, 0);
                this.okButton.Name = "okButton";
                manager.ApplyResources(this.cancelButton, "cancelButton");
                this.cancelButton.DialogResult = DialogResult.Cancel;
                this.cancelButton.Margin = new Padding(3, 3, 0, 0);
                this.cancelButton.Name = "cancelButton";
                manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
                this.overArchingTableLayoutPanel.SetColumnSpan(this.okCancelTableLayoutPanel, 3);
                this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
                this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
                this.okCancelTableLayoutPanel.Margin = new Padding(3, 3, 0, 0);
                this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
                manager.ApplyResources(this.overArchingTableLayoutPanel, "overArchingTableLayoutPanel");
                this.overArchingTableLayoutPanel.Controls.Add(this.downButton, 1, 2);
                this.overArchingTableLayoutPanel.Controls.Add(this.addRemoveTableLayoutPanel, 0, 3);
                this.overArchingTableLayoutPanel.Controls.Add(this.propertiesLabel, 2, 0);
                this.overArchingTableLayoutPanel.Controls.Add(this.membersLabel, 0, 0);
                this.overArchingTableLayoutPanel.Controls.Add(this.listbox, 0, 1);
                this.overArchingTableLayoutPanel.Controls.Add(this.propertyBrowser, 2, 1);
                this.overArchingTableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 0, 4);
                this.overArchingTableLayoutPanel.Controls.Add(this.upButton, 1, 1);
                this.overArchingTableLayoutPanel.Name = "overArchingTableLayoutPanel";
                manager.ApplyResources(this.addRemoveTableLayoutPanel, "addRemoveTableLayoutPanel");
                this.addRemoveTableLayoutPanel.Controls.Add(this.addButton, 0, 0);
                this.addRemoveTableLayoutPanel.Controls.Add(this.removeButton, 2, 0);
                this.addRemoveTableLayoutPanel.Margin = new Padding(0, 3, 3, 3);
                this.addRemoveTableLayoutPanel.Name = "addRemoveTableLayoutPanel";
                base.AcceptButton = this.okButton;
                manager.ApplyResources(this, "$this");
                base.AutoScaleMode = AutoScaleMode.Font;
                base.CancelButton = this.cancelButton;
                base.Controls.Add(this.overArchingTableLayoutPanel);
                base.HelpButton = true;
                base.MaximizeBox = false;
                base.MinimizeBox = false;
                base.Name = "CollectionEditor";
                base.ShowIcon = false;
                base.ShowInTaskbar = false;
                this.okCancelTableLayoutPanel.ResumeLayout(false);
                this.okCancelTableLayoutPanel.PerformLayout();
                this.overArchingTableLayoutPanel.ResumeLayout(false);
                this.overArchingTableLayoutPanel.PerformLayout();
                this.addRemoveTableLayoutPanel.ResumeLayout(false);
                this.addRemoveTableLayoutPanel.PerformLayout();
                base.ResumeLayout(false);
            }

            private void Listbox_drawItem(object sender, DrawItemEventArgs e)
            {
                if (e.Index != -1)
                {
                    ListItem item = (ListItem) this.listbox.Items[e.Index];
                    Graphics graphics = e.Graphics;
                    int count = this.listbox.Items.Count;
                    int num2 = (count > 1) ? (count - 1) : count;
                    SizeF ef = graphics.MeasureString(num2.ToString(CultureInfo.CurrentCulture), this.listbox.Font);
                    int num3 = ((int) (Math.Log((double) num2) / LOG10)) + 1;
                    int num4 = 4 + (num3 * (this.Font.Height / 2));
                    num4 = Math.Max(num4, (int) Math.Ceiling((double) ef.Width)) + (SystemInformation.BorderSize.Width * 4);
                    Rectangle rectangle = new Rectangle(e.Bounds.X, e.Bounds.Y, num4, e.Bounds.Height);
                    ControlPaint.DrawButton(graphics, rectangle, ButtonState.Normal);
                    rectangle.Inflate(-SystemInformation.BorderSize.Width * 2, -SystemInformation.BorderSize.Height * 2);
                    int num5 = num4;
                    Color window = SystemColors.Window;
                    Color windowText = SystemColors.WindowText;
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    {
                        window = SystemColors.Highlight;
                        windowText = SystemColors.HighlightText;
                    }
                    Rectangle rect = new Rectangle(e.Bounds.X + num5, e.Bounds.Y, e.Bounds.Width - num5, e.Bounds.Height);
                    graphics.FillRectangle(new SolidBrush(window), rect);
                    if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                    {
                        ControlPaint.DrawFocusRectangle(graphics, rect);
                    }
                    num5 += 2;
                    if ((item.Editor != null) && item.Editor.GetPaintValueSupported())
                    {
                        Rectangle rectangle3 = new Rectangle(e.Bounds.X + num5, e.Bounds.Y + 1, 20, e.Bounds.Height - 3);
                        graphics.DrawRectangle(SystemPens.ControlText, rectangle3.X, rectangle3.Y, rectangle3.Width - 1, rectangle3.Height - 1);
                        rectangle3.Inflate(-1, -1);
                        item.Editor.PaintValue(item.Value, graphics, rectangle3);
                        num5 += 0x1b;
                    }
                    using (StringFormat format = new StringFormat())
                    {
                        format.Alignment = StringAlignment.Center;
                        graphics.DrawString(e.Index.ToString(CultureInfo.CurrentCulture), this.Font, SystemBrushes.ControlText, new Rectangle(e.Bounds.X, e.Bounds.Y, num4, e.Bounds.Height), format);
                    }
                    Brush brush = new SolidBrush(windowText);
                    string displayText = this.GetDisplayText(item);
                    try
                    {
                        graphics.DrawString(displayText, this.Font, brush, new Rectangle(e.Bounds.X + num5, e.Bounds.Y, e.Bounds.Width - num5, e.Bounds.Height));
                    }
                    finally
                    {
                        if (brush != null)
                        {
                            brush.Dispose();
                        }
                    }
                    int num6 = num5 + ((int) graphics.MeasureString(displayText, this.Font).Width);
                    if ((num6 > e.Bounds.Width) && (this.listbox.HorizontalExtent < num6))
                    {
                        this.listbox.HorizontalExtent = num6;
                    }
                }
            }

            private void Listbox_handleCreated(object sender, EventArgs e)
            {
                this.UpdateItemWidths(null);
            }

            private void Listbox_keyDown(object sender, KeyEventArgs kevent)
            {
                switch (kevent.KeyData)
                {
                    case Keys.Insert:
                        this.PerformAdd();
                        return;

                    case Keys.Delete:
                        this.PerformRemove();
                        return;
                }
            }

            private void Listbox_selectedIndexChanged(object sender, EventArgs e)
            {
                this.UpdateEnabled();
            }

            private void OKButton_click(object sender, EventArgs e)
            {
                try
                {
                    if (!this.dirty || !this.CollectionEditable)
                    {
                        this.dirty = false;
                        base.DialogResult = DialogResult.Cancel;
                    }
                    else
                    {
                        if (this.dirty)
                        {
                            object[] objArray = new object[this.listbox.Items.Count];
                            for (int i = 0; i < objArray.Length; i++)
                            {
                                objArray[i] = ((ListItem) this.listbox.Items[i]).Value;
                            }
                            base.Items = objArray;
                        }
                        if ((this.removedItems != null) && this.dirty)
                        {
                            object[] objArray2 = this.removedItems.ToArray();
                            for (int j = 0; j < objArray2.Length; j++)
                            {
                                base.DestroyInstance(objArray2[j]);
                            }
                            this.removedItems.Clear();
                        }
                        if (this.createdItems != null)
                        {
                            this.createdItems.Clear();
                        }
                        if (this.originalItems != null)
                        {
                            this.originalItems.Clear();
                        }
                        this.listbox.Items.Clear();
                        this.dirty = false;
                    }
                }
                catch (Exception exception)
                {
                    base.DialogResult = DialogResult.None;
                    this.DisplayError(exception);
                }
            }

            private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
            {
                if (!this.dirty)
                {
                    foreach (object obj2 in this.originalItems)
                    {
                        if (obj2 == e.Component)
                        {
                            this.dirty = true;
                            break;
                        }
                    }
                }
            }

            protected override void OnEditValueChanged()
            {
                if (base.Visible)
                {
                    if (this.originalItems == null)
                    {
                        this.originalItems = new ArrayList();
                    }
                    this.originalItems.Clear();
                    this.listbox.Items.Clear();
                    this.propertyBrowser.Site = new CollectionEditor.PropertyGridSite(base.Context, this.propertyBrowser);
                    if (base.EditValue != null)
                    {
                        this.SuspendEnabledUpdates();
                        try
                        {
                            object[] items = base.Items;
                            for (int i = 0; i < items.Length; i++)
                            {
                                this.listbox.Items.Add(new ListItem(this.editor, items[i]));
                                this.originalItems.Add(items[i]);
                            }
                            if (this.listbox.Items.Count > 0)
                            {
                                this.listbox.SelectedIndex = 0;
                            }
                        }
                        finally
                        {
                            this.ResumeEnabledUpdates(true);
                        }
                    }
                    else
                    {
                        this.UpdateEnabled();
                    }
                    this.AdjustListBoxItemHeight();
                    this.UpdateItemWidths(null);
                }
            }

            protected override void OnFontChanged(EventArgs e)
            {
                base.OnFontChanged(e);
                this.AdjustListBoxItemHeight();
            }

            private void PerformAdd()
            {
                this.CreateAndAddInstance(base.NewItemTypes[0]);
            }

            private void PerformRemove()
            {
                int selectedIndex = this.listbox.SelectedIndex;
                if (selectedIndex != -1)
                {
                    this.SuspendEnabledUpdates();
                    try
                    {
                        if (this.listbox.SelectedItems.Count > 1)
                        {
                            ArrayList list = new ArrayList(this.listbox.SelectedItems);
                            foreach (ListItem item in list)
                            {
                                this.RemoveInternal(item);
                            }
                        }
                        else
                        {
                            this.RemoveInternal((ListItem) this.listbox.SelectedItem);
                        }
                        if (selectedIndex < this.listbox.Items.Count)
                        {
                            this.listbox.SelectedIndex = selectedIndex;
                        }
                        else if (this.listbox.Items.Count > 0)
                        {
                            this.listbox.SelectedIndex = this.listbox.Items.Count - 1;
                        }
                    }
                    finally
                    {
                        this.ResumeEnabledUpdates(true);
                    }
                }
            }

            private void PropertyGrid_propertyValueChanged(object sender, PropertyValueChangedEventArgs e)
            {
                this.dirty = true;
                this.SuspendEnabledUpdates();
                try
                {
                    this.listbox.RefreshItem(this.listbox.SelectedIndex);
                }
                finally
                {
                    this.ResumeEnabledUpdates(false);
                }
                this.UpdateItemWidths(null);
                this.listbox.Invalidate();
                this.propertiesLabel.Text = System.Design.SR.GetString("CollectionEditorProperties", new object[] { this.GetDisplayText((ListItem) this.listbox.SelectedItem) });
            }

            private void RemoveButton_click(object sender, EventArgs e)
            {
                this.PerformRemove();
                Control control = (Control) sender;
                if (control.Enabled)
                {
                    control.Focus();
                }
            }

            private void RemoveInternal(ListItem item)
            {
                if (item != null)
                {
                    this.editor.OnItemRemoving(item.Value);
                    this.dirty = true;
                    if ((this.createdItems != null) && this.createdItems.Contains(item.Value))
                    {
                        base.DestroyInstance(item.Value);
                        this.createdItems.Remove(item.Value);
                        this.listbox.Items.Remove(item);
                    }
                    else
                    {
                        try
                        {
                            if (!base.CanRemoveInstance(item.Value))
                            {
                                throw new Exception(System.Design.SR.GetString("CollectionEditorCantRemoveItem", new object[] { this.GetDisplayText(item) }));
                            }
                            if (this.removedItems == null)
                            {
                                this.removedItems = new ArrayList();
                            }
                            this.removedItems.Add(item.Value);
                            this.listbox.Items.Remove(item);
                        }
                        catch (Exception exception)
                        {
                            this.DisplayError(exception);
                        }
                    }
                    this.UpdateItemWidths(null);
                }
            }

            private void ResumeEnabledUpdates(bool updateNow)
            {
                this.suspendEnabledCount--;
                if (updateNow)
                {
                    this.UpdateEnabled();
                }
                else
                {
                    base.BeginInvoke(new MethodInvoker(this.UpdateEnabled));
                }
            }

            protected internal override DialogResult ShowEditorDialog(IWindowsFormsEditorService edSvc)
            {
                IComponentChangeService service = null;
                DialogResult oK = DialogResult.OK;
                try
                {
                    service = (IComponentChangeService) this.editor.Context.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                    }
                    base.ActiveControl = this.listbox;
                    oK = base.ShowEditorDialog(edSvc);
                }
                finally
                {
                    if (service != null)
                    {
                        service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    }
                }
                return oK;
            }

            private void SuspendEnabledUpdates()
            {
                this.suspendEnabledCount++;
            }

            private void UpButton_click(object sender, EventArgs e)
            {
                int selectedIndex = this.listbox.SelectedIndex;
                if (selectedIndex != 0)
                {
                    this.dirty = true;
                    try
                    {
                        this.SuspendEnabledUpdates();
                        int topIndex = this.listbox.TopIndex;
                        object obj2 = this.listbox.Items[selectedIndex];
                        this.listbox.Items[selectedIndex] = this.listbox.Items[selectedIndex - 1];
                        this.listbox.Items[selectedIndex - 1] = obj2;
                        if (topIndex > 0)
                        {
                            this.listbox.TopIndex = topIndex - 1;
                        }
                        this.listbox.ClearSelected();
                        this.listbox.SelectedIndex = selectedIndex - 1;
                        Control control = (Control) sender;
                        if (control.Enabled)
                        {
                            control.Focus();
                        }
                    }
                    finally
                    {
                        this.ResumeEnabledUpdates(true);
                    }
                }
            }

            private void UpdateEnabled()
            {
                if (this.suspendEnabledCount <= 0)
                {
                    bool flag = (this.listbox.SelectedItem != null) && this.CollectionEditable;
                    this.removeButton.Enabled = flag && this.AllowRemoveInstance(((ListItem) this.listbox.SelectedItem).Value);
                    this.upButton.Enabled = flag && (this.listbox.Items.Count > 1);
                    this.downButton.Enabled = flag && (this.listbox.Items.Count > 1);
                    this.propertyBrowser.Enabled = flag;
                    this.addButton.Enabled = this.CollectionEditable;
                    if (this.listbox.SelectedItem == null)
                    {
                        this.propertiesLabel.Text = System.Design.SR.GetString("CollectionEditorPropertiesNone");
                        this.propertyBrowser.SelectedObject = null;
                    }
                    else
                    {
                        object[] objArray;
                        if (this.IsImmutable)
                        {
                            objArray = new object[] { new SelectionWrapper(base.CollectionType, base.CollectionItemType, this.listbox, this.listbox.SelectedItems) };
                        }
                        else
                        {
                            objArray = new object[this.listbox.SelectedItems.Count];
                            for (int i = 0; i < objArray.Length; i++)
                            {
                                objArray[i] = ((ListItem) this.listbox.SelectedItems[i]).Value;
                            }
                        }
                        switch (this.listbox.SelectedItems.Count)
                        {
                            case 1:
                            case -1:
                                this.propertiesLabel.Text = System.Design.SR.GetString("CollectionEditorProperties", new object[] { this.GetDisplayText((ListItem) this.listbox.SelectedItem) });
                                break;

                            default:
                                this.propertiesLabel.Text = System.Design.SR.GetString("CollectionEditorPropertiesMultiSelect");
                                break;
                        }
                        if (this.editor.IsAnyObjectInheritedReadOnly(objArray))
                        {
                            this.propertyBrowser.SelectedObjects = null;
                            this.propertyBrowser.Enabled = false;
                            this.removeButton.Enabled = false;
                            this.upButton.Enabled = false;
                            this.downButton.Enabled = false;
                            this.propertiesLabel.Text = System.Design.SR.GetString("CollectionEditorInheritedReadOnlySelection");
                        }
                        else
                        {
                            this.propertyBrowser.Enabled = true;
                            this.propertyBrowser.SelectedObjects = objArray;
                        }
                    }
                }
            }

            private void UpdateItemWidths(ListItem item)
            {
                if (this.listbox.IsHandleCreated)
                {
                    using (Graphics graphics = this.listbox.CreateGraphics())
                    {
                        int horizontalExtent = this.listbox.HorizontalExtent;
                        if (item != null)
                        {
                            int num2 = this.CalcItemWidth(graphics, item);
                            if (num2 > horizontalExtent)
                            {
                                this.listbox.HorizontalExtent = num2;
                            }
                        }
                        else
                        {
                            int num3 = 0;
                            foreach (ListItem item2 in this.listbox.Items)
                            {
                                int num4 = this.CalcItemWidth(graphics, item2);
                                if (num4 > num3)
                                {
                                    num3 = num4;
                                }
                            }
                            this.listbox.HorizontalExtent = num3;
                        }
                    }
                }
            }

            private bool IsImmutable
            {
                get
                {
                    if (!TypeDescriptor.GetConverter(base.CollectionItemType).GetCreateInstanceSupported())
                    {
                        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(base.CollectionItemType))
                        {
                            if (!descriptor.IsReadOnly)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }

            private class ListItem
            {
                private CollectionEditor parentCollectionEditor;
                private object uiTypeEditor;
                private object value;

                public ListItem(CollectionEditor parentCollectionEditor, object value)
                {
                    this.value = value;
                    this.parentCollectionEditor = parentCollectionEditor;
                }

                public override string ToString()
                {
                    return this.parentCollectionEditor.GetDisplayText(this.value);
                }

                public UITypeEditor Editor
                {
                    get
                    {
                        if (this.uiTypeEditor == null)
                        {
                            this.uiTypeEditor = TypeDescriptor.GetEditor(this.value, typeof(UITypeEditor));
                            if (this.uiTypeEditor == null)
                            {
                                this.uiTypeEditor = this;
                            }
                        }
                        if (this.uiTypeEditor != this)
                        {
                            return (UITypeEditor) this.uiTypeEditor;
                        }
                        return null;
                    }
                }

                public object Value
                {
                    get
                    {
                        return this.value;
                    }
                    set
                    {
                        this.uiTypeEditor = null;
                        this.value = value;
                    }
                }
            }

            private class SelectionWrapper : PropertyDescriptor, ICustomTypeDescriptor
            {
                private ICollection collection;
                private System.Type collectionItemType;
                private System.Type collectionType;
                private Control control;
                private PropertyDescriptorCollection properties;
                private object value;

                public SelectionWrapper(System.Type collectionType, System.Type collectionItemType, Control control, ICollection collection) : base("Value", new Attribute[] { new CategoryAttribute(collectionItemType.Name) })
                {
                    this.collectionType = collectionType;
                    this.collectionItemType = collectionItemType;
                    this.control = control;
                    this.collection = collection;
                    this.properties = new PropertyDescriptorCollection(new PropertyDescriptor[] { this });
                    this.value = this;
                    foreach (CollectionEditor.CollectionEditorCollectionForm.ListItem item in collection)
                    {
                        if (this.value == this)
                        {
                            this.value = item.Value;
                        }
                        else
                        {
                            object obj2 = item.Value;
                            if (this.value != null)
                            {
                                if (obj2 == null)
                                {
                                    this.value = null;
                                }
                                else
                                {
                                    if (this.value.Equals(obj2))
                                    {
                                        goto Label_00C9;
                                    }
                                    this.value = null;
                                }
                                break;
                            }
                            if (obj2 != null)
                            {
                                this.value = null;
                                break;
                            }
                        Label_00C9:;
                        }
                    }
                }

                public override bool CanResetValue(object component)
                {
                    return false;
                }

                public override object GetValue(object component)
                {
                    return this.value;
                }

                public override void ResetValue(object component)
                {
                }

                public override void SetValue(object component, object value)
                {
                    this.value = value;
                    foreach (CollectionEditor.CollectionEditorCollectionForm.ListItem item in this.collection)
                    {
                        item.Value = value;
                    }
                    this.control.Invalidate();
                    this.OnValueChanged(component, EventArgs.Empty);
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return false;
                }

                AttributeCollection ICustomTypeDescriptor.GetAttributes()
                {
                    return TypeDescriptor.GetAttributes(this.collectionItemType);
                }

                string ICustomTypeDescriptor.GetClassName()
                {
                    return this.collectionItemType.Name;
                }

                string ICustomTypeDescriptor.GetComponentName()
                {
                    return null;
                }

                TypeConverter ICustomTypeDescriptor.GetConverter()
                {
                    return null;
                }

                EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
                {
                    return null;
                }

                PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
                {
                    return this;
                }

                object ICustomTypeDescriptor.GetEditor(System.Type editorBaseType)
                {
                    return null;
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
                {
                    return EventDescriptorCollection.Empty;
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
                {
                    return EventDescriptorCollection.Empty;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
                {
                    return this.properties;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
                {
                    return this.properties;
                }

                object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
                {
                    return this;
                }

                public override System.Type ComponentType
                {
                    get
                    {
                        return this.collectionType;
                    }
                }

                public override bool IsReadOnly
                {
                    get
                    {
                        return false;
                    }
                }

                public override System.Type PropertyType
                {
                    get
                    {
                        return this.collectionItemType;
                    }
                }
            }

            private class TypeMenuItem : ToolStripMenuItem
            {
                private System.Type itemType;

                public TypeMenuItem(System.Type itemType, EventHandler handler) : base(itemType.Name, null, handler)
                {
                    this.itemType = itemType;
                }

                public System.Type ItemType
                {
                    get
                    {
                        return this.itemType;
                    }
                }
            }
        }

        protected abstract class CollectionForm : Form
        {
            private const short EditableDynamic = 0;
            private const short EditableNo = 2;
            private short editableState;
            private const short EditableYes = 1;
            private CollectionEditor editor;
            private object value;

            public CollectionForm(CollectionEditor editor)
            {
                this.editor = editor;
            }

            protected bool CanRemoveInstance(object value)
            {
                return this.editor.CanRemoveInstance(value);
            }

            protected virtual bool CanSelectMultipleInstances()
            {
                return this.editor.CanSelectMultipleInstances();
            }

            protected object CreateInstance(System.Type itemType)
            {
                return this.editor.CreateInstance(itemType);
            }

            protected void DestroyInstance(object instance)
            {
                this.editor.DestroyInstance(instance);
            }

            protected virtual void DisplayError(Exception e)
            {
                IUIService service = (IUIService) this.GetService(typeof(IUIService));
                if (service != null)
                {
                    service.ShowError(e);
                }
                else
                {
                    string message = e.Message;
                    if ((message == null) || (message.Length == 0))
                    {
                        message = e.ToString();
                    }
                    System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
                }
            }

            protected override object GetService(System.Type serviceType)
            {
                return this.editor.GetService(serviceType);
            }

            protected abstract void OnEditValueChanged();
            protected internal virtual DialogResult ShowEditorDialog(IWindowsFormsEditorService edSvc)
            {
                return edSvc.ShowDialog(this);
            }

            internal virtual bool CollectionEditable
            {
                get
                {
                    if (this.editableState != 0)
                    {
                        return (this.editableState == 1);
                    }
                    bool flag = typeof(IList).IsAssignableFrom(this.editor.CollectionType);
                    if (flag)
                    {
                        IList editValue = this.EditValue as IList;
                        if (editValue != null)
                        {
                            return !editValue.IsReadOnly;
                        }
                    }
                    return flag;
                }
                set
                {
                    if (value)
                    {
                        this.editableState = 1;
                    }
                    else
                    {
                        this.editableState = 2;
                    }
                }
            }

            protected System.Type CollectionItemType
            {
                get
                {
                    return this.editor.CollectionItemType;
                }
            }

            protected System.Type CollectionType
            {
                get
                {
                    return this.editor.CollectionType;
                }
            }

            protected ITypeDescriptorContext Context
            {
                get
                {
                    return this.editor.Context;
                }
            }

            public object EditValue
            {
                get
                {
                    return this.value;
                }
                set
                {
                    this.value = value;
                    this.OnEditValueChanged();
                }
            }

            protected object[] Items
            {
                get
                {
                    return this.editor.GetItems(this.EditValue);
                }
                set
                {
                    bool flag = false;
                    try
                    {
                        flag = this.Context.OnComponentChanging();
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                        this.DisplayError(exception);
                    }
                    if (flag)
                    {
                        object obj2 = this.editor.SetItems(this.EditValue, value);
                        if (obj2 != this.EditValue)
                        {
                            this.EditValue = obj2;
                        }
                        this.Context.OnComponentChanged();
                    }
                }
            }

            protected System.Type[] NewItemTypes
            {
                get
                {
                    return this.editor.NewItemTypes;
                }
            }
        }

        internal class FilterListBox : ListBox
        {
            private System.Windows.Forms.PropertyGrid grid;
            private Message lastKeyDown;

            public void RefreshItem(int index)
            {
                base.RefreshItem(index);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 0x100:
                        this.lastKeyDown = m;
                        if ((((int) ((long) m.WParam)) == 0xe5) && (this.PropertyGrid != null))
                        {
                            this.PropertyGrid.Focus();
                            System.Design.UnsafeNativeMethods.SetFocus(new HandleRef(this.PropertyGrid, this.PropertyGrid.Handle));
                            Application.DoEvents();
                            if (this.PropertyGrid.Focused || this.PropertyGrid.ContainsFocus)
                            {
                                System.Design.NativeMethods.SendMessage(System.Design.UnsafeNativeMethods.GetFocus(), 0x100, this.lastKeyDown.WParam, this.lastKeyDown.LParam);
                            }
                        }
                        break;

                    case 0x102:
                    {
                        if (((Control.ModifierKeys & (Keys.Alt | Keys.Control)) != Keys.None) || (this.PropertyGrid == null))
                        {
                            break;
                        }
                        this.PropertyGrid.Focus();
                        System.Design.UnsafeNativeMethods.SetFocus(new HandleRef(this.PropertyGrid, this.PropertyGrid.Handle));
                        Application.DoEvents();
                        if (!this.PropertyGrid.Focused && !this.PropertyGrid.ContainsFocus)
                        {
                            break;
                        }
                        IntPtr focus = System.Design.UnsafeNativeMethods.GetFocus();
                        System.Design.NativeMethods.SendMessage(focus, 0x100, this.lastKeyDown.WParam, this.lastKeyDown.LParam);
                        System.Design.NativeMethods.SendMessage(focus, 0x102, m.WParam, m.LParam);
                        return;
                    }
                }
                base.WndProc(ref m);
            }

            private System.Windows.Forms.PropertyGrid PropertyGrid
            {
                get
                {
                    if (this.grid == null)
                    {
                        foreach (Control control in base.Parent.Controls)
                        {
                            if (control is System.Windows.Forms.PropertyGrid)
                            {
                                this.grid = (System.Windows.Forms.PropertyGrid) control;
                                break;
                            }
                        }
                    }
                    return this.grid;
                }
            }
        }

        internal class PropertyGridSite : ISite, IServiceProvider
        {
            private IComponent comp;
            private bool inGetService;
            private IServiceProvider sp;

            public PropertyGridSite(IServiceProvider sp, IComponent comp)
            {
                this.sp = sp;
                this.comp = comp;
            }

            public object GetService(System.Type t)
            {
                if (!this.inGetService && (this.sp != null))
                {
                    try
                    {
                        this.inGetService = true;
                        return this.sp.GetService(t);
                    }
                    finally
                    {
                        this.inGetService = false;
                    }
                }
                return null;
            }

            public IComponent Component
            {
                get
                {
                    return this.comp;
                }
            }

            public IContainer Container
            {
                get
                {
                    return null;
                }
            }

            public bool DesignMode
            {
                get
                {
                    return false;
                }
            }

            public string Name
            {
                get
                {
                    return null;
                }
                set
                {
                }
            }
        }

        internal class SplitButton : System.Windows.Forms.Button
        {
            private PushButtonState _state;
            private Rectangle dropDownRectangle = new Rectangle();
            private const int pushButtonWidth = 14;
            private bool showSplit;

            private void ContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
            {
                ContextMenuStrip strip = sender as ContextMenuStrip;
                if (strip != null)
                {
                    strip.Closed -= new ToolStripDropDownClosedEventHandler(this.ContextMenuStrip_Closed);
                }
                this.SetButtonDrawState();
            }

            public override Size GetPreferredSize(Size proposedSize)
            {
                Size preferredSize = base.GetPreferredSize(proposedSize);
                if ((this.showSplit && !string.IsNullOrEmpty(this.Text)) && ((TextRenderer.MeasureText(this.Text, this.Font).Width + 14) > preferredSize.Width))
                {
                    return (preferredSize + new Size(14, 0));
                }
                return preferredSize;
            }

            protected override bool IsInputKey(Keys keyData)
            {
                return ((keyData.Equals(Keys.Down) && this.showSplit) || base.IsInputKey(keyData));
            }

            protected override void OnGotFocus(EventArgs e)
            {
                if (!this.showSplit)
                {
                    base.OnGotFocus(e);
                }
                else if (!this.State.Equals(PushButtonState.Pressed) && !this.State.Equals(PushButtonState.Disabled))
                {
                    this.State = PushButtonState.Default;
                }
            }

            protected override void OnKeyDown(KeyEventArgs kevent)
            {
                if (kevent.KeyCode.Equals(Keys.Down) && this.showSplit)
                {
                    this.ShowContextMenuStrip();
                }
                else
                {
                    base.OnKeyDown(kevent);
                }
            }

            protected override void OnLostFocus(EventArgs e)
            {
                if (!this.showSplit)
                {
                    base.OnLostFocus(e);
                }
                else if (!this.State.Equals(PushButtonState.Pressed) && !this.State.Equals(PushButtonState.Disabled))
                {
                    this.State = PushButtonState.Normal;
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (!this.showSplit)
                {
                    base.OnMouseDown(e);
                }
                else if (this.dropDownRectangle.Contains(e.Location))
                {
                    this.ShowContextMenuStrip();
                }
                else
                {
                    this.State = PushButtonState.Pressed;
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (!this.showSplit)
                {
                    base.OnMouseEnter(e);
                }
                else if (!this.State.Equals(PushButtonState.Pressed) && !this.State.Equals(PushButtonState.Disabled))
                {
                    this.State = PushButtonState.Hot;
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (!this.showSplit)
                {
                    base.OnMouseLeave(e);
                }
                else if (!this.State.Equals(PushButtonState.Pressed) && !this.State.Equals(PushButtonState.Disabled))
                {
                    if (this.Focused)
                    {
                        this.State = PushButtonState.Default;
                    }
                    else
                    {
                        this.State = PushButtonState.Normal;
                    }
                }
            }

            protected override void OnMouseUp(MouseEventArgs mevent)
            {
                if (!this.showSplit)
                {
                    base.OnMouseUp(mevent);
                }
                else if ((this.ContextMenuStrip == null) || !this.ContextMenuStrip.Visible)
                {
                    this.SetButtonDrawState();
                    if (base.Bounds.Contains(base.Parent.PointToClient(Cursor.Position)) && !this.dropDownRectangle.Contains(mevent.Location))
                    {
                        this.OnClick(new EventArgs());
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);
                if (this.showSplit)
                {
                    Graphics g = pevent.Graphics;
                    Rectangle bounds = new Rectangle(0, 0, base.Width, base.Height);
                    TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                    ButtonRenderer.DrawButton(g, bounds, this.State);
                    this.dropDownRectangle = new Rectangle((bounds.Right - 14) - 1, 4, 14, bounds.Height - 8);
                    if (this.RightToLeft == RightToLeft.Yes)
                    {
                        this.dropDownRectangle.X = bounds.Left + 1;
                        g.DrawLine(SystemPens.ButtonHighlight, bounds.Left + 14, 4, bounds.Left + 14, bounds.Bottom - 4);
                        g.DrawLine(SystemPens.ButtonHighlight, (bounds.Left + 14) + 1, 4, (bounds.Left + 14) + 1, bounds.Bottom - 4);
                        bounds.Offset(14, 0);
                        bounds.Width -= 14;
                    }
                    else
                    {
                        g.DrawLine(SystemPens.ButtonHighlight, bounds.Right - 14, 4, bounds.Right - 14, bounds.Bottom - 4);
                        g.DrawLine(SystemPens.ButtonHighlight, (bounds.Right - 14) - 1, 4, (bounds.Right - 14) - 1, bounds.Bottom - 4);
                        bounds.Width -= 14;
                    }
                    this.PaintArrow(g, this.dropDownRectangle);
                    if (!base.UseMnemonic)
                    {
                        flags |= TextFormatFlags.NoPrefix;
                    }
                    else if (!this.ShowKeyboardCues)
                    {
                        flags |= TextFormatFlags.HidePrefix;
                    }
                    if (!string.IsNullOrEmpty(this.Text))
                    {
                        TextRenderer.DrawText(g, this.Text, this.Font, bounds, SystemColors.ControlText, flags);
                    }
                    if (this.Focused)
                    {
                        bounds.Inflate(-4, -4);
                    }
                }
            }

            private void PaintArrow(Graphics g, Rectangle dropDownRect)
            {
                Point point;
                point = new Point(Convert.ToInt32((int) (dropDownRect.Left + (dropDownRect.Width / 2))), Convert.ToInt32((int) (dropDownRect.Top + (dropDownRect.Height / 2)))) {
                    X = point.X + (dropDownRect.Width % 2)
                };
                Point[] points = new Point[] { new Point(point.X - 2, point.Y - 1), new Point(point.X + 3, point.Y - 1), new Point(point.X, point.Y + 2) };
                g.FillPolygon(SystemBrushes.ControlText, points);
            }

            private void SetButtonDrawState()
            {
                if (base.Bounds.Contains(base.Parent.PointToClient(Cursor.Position)))
                {
                    this.State = PushButtonState.Hot;
                }
                else if (this.Focused)
                {
                    this.State = PushButtonState.Default;
                }
                else
                {
                    this.State = PushButtonState.Normal;
                }
            }

            private void ShowContextMenuStrip()
            {
                this.State = PushButtonState.Pressed;
                if (this.ContextMenuStrip != null)
                {
                    this.ContextMenuStrip.Closed += new ToolStripDropDownClosedEventHandler(this.ContextMenuStrip_Closed);
                    this.ContextMenuStrip.Show(this, 0, base.Height);
                }
            }

            public bool ShowSplit
            {
                set
                {
                    if (value != this.showSplit)
                    {
                        this.showSplit = value;
                        base.Invalidate();
                    }
                }
            }

            private PushButtonState State
            {
                get
                {
                    return this._state;
                }
                set
                {
                    if (!this._state.Equals(value))
                    {
                        this._state = value;
                        base.Invalidate();
                    }
                }
            }
        }
    }
}

