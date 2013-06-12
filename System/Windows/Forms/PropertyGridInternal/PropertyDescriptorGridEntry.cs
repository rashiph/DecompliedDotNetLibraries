namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class PropertyDescriptorGridEntry : GridEntry
    {
        private bool activeXHide;
        protected IEventBindingService eventBindings;
        private System.ComponentModel.TypeConverter exceptionConverter;
        private System.Drawing.Design.UITypeEditor exceptionEditor;
        private bool forceRenderReadOnly;
        private string helpKeyword;
        private const int IMAGE_SIZE = 8;
        private bool isSerializeContentsProp;
        private byte parensAroundName;
        private const byte ParensAroundNameNo = 0;
        private const byte ParensAroundNameUnknown = 0xff;
        private const byte ParensAroundNameYes = 1;
        internal System.ComponentModel.PropertyDescriptor propertyInfo;
        private IPropertyValueUIService pvSvc;
        private bool pvSvcChecked;
        private PropertyValueUIItem[] pvUIItems;
        private bool readOnlyVerified;
        private static IEventBindingService targetBindingService;
        private static IComponent targetComponent;
        private static EventDescriptor targetEventdesc;
        private string toolTipText;
        private Rectangle[] uiItemRects;

        internal PropertyDescriptorGridEntry(PropertyGrid ownerGrid, GridEntry peParent, bool hide) : base(ownerGrid, peParent)
        {
            this.parensAroundName = 0xff;
            this.activeXHide = hide;
        }

        internal PropertyDescriptorGridEntry(PropertyGrid ownerGrid, GridEntry peParent, System.ComponentModel.PropertyDescriptor propInfo, bool hide) : base(ownerGrid, peParent)
        {
            this.parensAroundName = 0xff;
            this.activeXHide = hide;
            this.Initialize(propInfo);
        }

        internal override void EditPropertyValue(PropertyGridView iva)
        {
            base.EditPropertyValue(iva);
            if (!this.IsValueEditable)
            {
                RefreshPropertiesAttribute attribute = (RefreshPropertiesAttribute) this.propertyInfo.Attributes[typeof(RefreshPropertiesAttribute)];
                if ((attribute != null) && !attribute.RefreshProperties.Equals(RefreshProperties.None))
                {
                    this.GridEntryHost.Refresh((attribute != null) && attribute.Equals(RefreshPropertiesAttribute.All));
                }
            }
        }

        internal override Point GetLabelToolTipLocation(int mouseX, int mouseY)
        {
            if (this.pvUIItems != null)
            {
                for (int i = 0; i < this.pvUIItems.Length; i++)
                {
                    if (this.uiItemRects[i].Contains(mouseX, this.GridEntryHost.GetGridEntryHeight() / 2))
                    {
                        this.toolTipText = this.pvUIItems[i].ToolTip;
                        return new Point(mouseX, mouseY);
                    }
                }
            }
            this.toolTipText = null;
            return base.GetLabelToolTipLocation(mouseX, mouseY);
        }

        protected object GetPropertyValueCore(object target)
        {
            object obj2;
            if (this.propertyInfo == null)
            {
                return null;
            }
            if (target is ICustomTypeDescriptor)
            {
                target = ((ICustomTypeDescriptor) target).GetPropertyOwner(this.propertyInfo);
            }
            try
            {
                obj2 = this.propertyInfo.GetValue(target);
            }
            catch
            {
                throw;
            }
            return obj2;
        }

        protected void Initialize(System.ComponentModel.PropertyDescriptor propInfo)
        {
            this.propertyInfo = propInfo;
            this.isSerializeContentsProp = this.propertyInfo.SerializationVisibility == DesignerSerializationVisibility.Content;
            if (!this.activeXHide && this.IsPropertyReadOnly)
            {
                this.SetFlag(1, false);
            }
            if (this.isSerializeContentsProp && this.TypeConverter.GetPropertiesSupported())
            {
                this.SetFlag(0x20000, true);
            }
        }

        protected virtual void NotifyParentChange(GridEntry ge)
        {
            while (((ge != null) && (ge is PropertyDescriptorGridEntry)) && ((PropertyDescriptorGridEntry) ge).propertyInfo.Attributes.Contains(NotifyParentPropertyAttribute.Yes))
            {
                object valueOwner = ge.GetValueOwner();
                bool isValueType = valueOwner.GetType().IsValueType;
                while ((!(ge is PropertyDescriptorGridEntry) || isValueType) ? valueOwner.Equals(ge.GetValueOwner()) : (valueOwner == ge.GetValueOwner()))
                {
                    ge = ge.ParentGridEntry;
                    if (ge == null)
                    {
                        break;
                    }
                }
                if (ge != null)
                {
                    valueOwner = ge.GetValueOwner();
                    IComponentChangeService componentChangeService = this.ComponentChangeService;
                    if (componentChangeService != null)
                    {
                        componentChangeService.OnComponentChanging(valueOwner, ((PropertyDescriptorGridEntry) ge).propertyInfo);
                        componentChangeService.OnComponentChanged(valueOwner, ((PropertyDescriptorGridEntry) ge).propertyInfo, null, null);
                    }
                    ge.ClearCachedValues(false);
                    PropertyGridView gridEntryHost = this.GridEntryHost;
                    if (gridEntryHost != null)
                    {
                        gridEntryHost.InvalidateGridEntryValue(ge);
                    }
                }
            }
        }

        internal override bool NotifyValueGivenParent(object obj, int type)
        {
            bool flag;
            if (obj is ICustomTypeDescriptor)
            {
                obj = ((ICustomTypeDescriptor) obj).GetPropertyOwner(this.propertyInfo);
            }
            switch (type)
            {
                case 1:
                    this.SetPropertyValue(obj, null, true, System.Windows.Forms.SR.GetString("PropertyGridResetValue", new object[] { this.PropertyName }));
                    if (this.pvUIItems != null)
                    {
                        for (int i = 0; i < this.pvUIItems.Length; i++)
                        {
                            this.pvUIItems[i].Reset();
                        }
                    }
                    break;

                case 2:
                    try
                    {
                        return (this.propertyInfo.CanResetValue(obj) || ((this.pvUIItems != null) && (this.pvUIItems.Length > 0)));
                    }
                    catch
                    {
                        if (this.exceptionConverter == null)
                        {
                            this.Flags = 0;
                            this.exceptionConverter = new ExceptionConverter();
                            this.exceptionEditor = new ExceptionEditor();
                        }
                        return false;
                    }
                    goto Label_00F1;

                case 3:
                case 5:
                    if (this.eventBindings == null)
                    {
                        this.eventBindings = (IEventBindingService) this.GetService(typeof(IEventBindingService));
                    }
                    if ((this.eventBindings != null) && (this.eventBindings.GetEvent(this.propertyInfo) != null))
                    {
                        return this.ViewEvent(obj, null, null, true);
                    }
                    goto Label_0175;

                case 4:
                    goto Label_00F1;

                default:
                    goto Label_0175;
            }
            this.pvUIItems = null;
            return false;
        Label_00F1:
            try
            {
                flag = this.propertyInfo.ShouldSerializeValue(obj);
            }
            catch
            {
                if (this.exceptionConverter == null)
                {
                    this.Flags = 0;
                    this.exceptionConverter = new ExceptionConverter();
                    this.exceptionEditor = new ExceptionEditor();
                }
                flag = false;
            }
            return flag;
        Label_0175:
            return false;
        }

        public override void OnComponentChanged()
        {
            base.OnComponentChanged();
            this.NotifyParentChange(this);
        }

        public override bool OnMouseClick(int x, int y, int count, MouseButtons button)
        {
            if (((this.pvUIItems != null) && (count == 2)) && ((button & MouseButtons.Left) == MouseButtons.Left))
            {
                for (int i = 0; i < this.pvUIItems.Length; i++)
                {
                    if (this.uiItemRects[i].Contains(x, this.GridEntryHost.GetGridEntryHeight() / 2))
                    {
                        this.pvUIItems[i].InvokeHandler(this, this.propertyInfo, this.pvUIItems[i]);
                        return true;
                    }
                }
            }
            return base.OnMouseClick(x, y, count, button);
        }

        public override void PaintLabel(Graphics g, Rectangle rect, Rectangle clipRect, bool selected, bool paintFullLabel)
        {
            base.PaintLabel(g, rect, clipRect, selected, paintFullLabel);
            IPropertyValueUIService propertyValueUIService = this.PropertyValueUIService;
            if (propertyValueUIService != null)
            {
                this.pvUIItems = propertyValueUIService.GetPropertyUIValueItems(this, this.propertyInfo);
                if (this.pvUIItems != null)
                {
                    if ((this.uiItemRects == null) || (this.uiItemRects.Length != this.pvUIItems.Length))
                    {
                        this.uiItemRects = new Rectangle[this.pvUIItems.Length];
                    }
                    for (int i = 0; i < this.pvUIItems.Length; i++)
                    {
                        this.uiItemRects[i] = new Rectangle(rect.Right - (9 * (i + 1)), (rect.Height - 8) / 2, 8, 8);
                        g.DrawImage(this.pvUIItems[i].Image, this.uiItemRects[i]);
                    }
                    this.GridEntryHost.LabelPaintMargin = 9 * this.pvUIItems.Length;
                }
            }
        }

        private void SetFlagsAndExceptionInfo(int flags, ExceptionConverter converter, ExceptionEditor editor)
        {
            this.Flags = flags;
            this.exceptionConverter = converter;
            this.exceptionEditor = editor;
        }

        private object SetPropertyValue(object obj, object objVal, bool reset, string undoText)
        {
            DesignerTransaction transaction = null;
            try
            {
                object propertyValueCore = this.GetPropertyValueCore(obj);
                if ((objVal != null) && objVal.Equals(propertyValueCore))
                {
                    return objVal;
                }
                base.ClearCachedValues();
                IDesignerHost designerHost = this.DesignerHost;
                if (designerHost != null)
                {
                    string description = (undoText == null) ? System.Windows.Forms.SR.GetString("PropertyGridSetValue", new object[] { this.propertyInfo.Name }) : undoText;
                    transaction = designerHost.CreateTransaction(description);
                }
                bool flag = !(obj is IComponent) || (((IComponent) obj).Site == null);
                if (flag)
                {
                    try
                    {
                        if (this.ComponentChangeService != null)
                        {
                            this.ComponentChangeService.OnComponentChanging(obj, this.propertyInfo);
                        }
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw exception;
                        }
                        return propertyValueCore;
                    }
                }
                bool internalExpanded = this.InternalExpanded;
                int oldCount = -1;
                if (internalExpanded)
                {
                    oldCount = base.ChildCount;
                }
                RefreshPropertiesAttribute attribute = (RefreshPropertiesAttribute) this.propertyInfo.Attributes[typeof(RefreshPropertiesAttribute)];
                bool flag3 = internalExpanded || ((attribute != null) && !attribute.RefreshProperties.Equals(RefreshProperties.None));
                if (flag3)
                {
                    this.DisposeChildren();
                }
                EventDescriptor eventdesc = null;
                if ((obj != null) && (objVal is string))
                {
                    if (this.eventBindings == null)
                    {
                        this.eventBindings = (IEventBindingService) this.GetService(typeof(IEventBindingService));
                    }
                    if (this.eventBindings != null)
                    {
                        eventdesc = this.eventBindings.GetEvent(this.propertyInfo);
                    }
                    if (eventdesc == null)
                    {
                        object component = obj;
                        if ((this.propertyInfo is MergePropertyDescriptor) && (obj is Array))
                        {
                            Array array = obj as Array;
                            if (array.Length > 0)
                            {
                                component = array.GetValue(0);
                            }
                        }
                        eventdesc = TypeDescriptor.GetEvents(component)[this.propertyInfo.Name];
                    }
                }
                bool flag4 = false;
                try
                {
                    if (reset)
                    {
                        this.propertyInfo.ResetValue(obj);
                    }
                    else if (eventdesc != null)
                    {
                        this.ViewEvent(obj, (string) objVal, eventdesc, false);
                    }
                    else
                    {
                        this.SetPropertyValueCore(obj, objVal, true);
                    }
                    flag4 = true;
                    if (flag && (this.ComponentChangeService != null))
                    {
                        this.ComponentChangeService.OnComponentChanged(obj, this.propertyInfo, null, objVal);
                    }
                    this.NotifyParentChange(this);
                }
                finally
                {
                    if (flag3 && (this.GridEntryHost != null))
                    {
                        base.RecreateChildren(oldCount);
                        if (flag4)
                        {
                            this.GridEntryHost.Refresh((attribute != null) && attribute.Equals(RefreshPropertiesAttribute.All));
                        }
                    }
                }
                return obj;
            }
            catch (CheckoutException exception2)
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                    transaction = null;
                }
                if (exception2 != CheckoutException.Canceled)
                {
                    throw;
                }
                return null;
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                    transaction = null;
                }
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            return obj;
        }

        protected void SetPropertyValueCore(object obj, object value, bool doUndo)
        {
            if (this.propertyInfo != null)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    object component = obj;
                    if (component is ICustomTypeDescriptor)
                    {
                        component = ((ICustomTypeDescriptor) component).GetPropertyOwner(this.propertyInfo);
                    }
                    bool flag = false;
                    if (this.ParentGridEntry != null)
                    {
                        System.Type propertyType = this.ParentGridEntry.PropertyType;
                        flag = propertyType.IsValueType || propertyType.IsArray;
                    }
                    if (component != null)
                    {
                        this.propertyInfo.SetValue(component, value);
                        if (flag)
                        {
                            GridEntry parentGridEntry = this.ParentGridEntry;
                            if ((parentGridEntry != null) && parentGridEntry.IsValueEditable)
                            {
                                parentGridEntry.PropertyValue = obj;
                            }
                        }
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        private static void ShowCodeIdle(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(PropertyDescriptorGridEntry.ShowCodeIdle);
            if (targetBindingService != null)
            {
                targetBindingService.ShowCode(targetComponent, targetEventdesc);
                targetBindingService = null;
                targetComponent = null;
                targetEventdesc = null;
            }
        }

        protected bool ViewEvent(object obj, string newHandler, EventDescriptor eventdesc, bool alwaysNavigate)
        {
            object propertyValueCore = this.GetPropertyValueCore(obj);
            string str = propertyValueCore as string;
            if (((str == null) && (propertyValueCore != null)) && ((this.TypeConverter != null) && this.TypeConverter.CanConvertTo(typeof(string))))
            {
                str = this.TypeConverter.ConvertToString(propertyValueCore);
            }
            if ((newHandler == null) && !string.IsNullOrEmpty(str))
            {
                newHandler = str;
            }
            else if ((str == newHandler) && !string.IsNullOrEmpty(newHandler))
            {
                return true;
            }
            IComponent component = obj as IComponent;
            if ((component == null) && (this.propertyInfo is MergePropertyDescriptor))
            {
                Array array = obj as Array;
                if ((array != null) && (array.Length > 0))
                {
                    component = array.GetValue(0) as IComponent;
                }
            }
            if (component == null)
            {
                return false;
            }
            if (this.propertyInfo.IsReadOnly)
            {
                return false;
            }
            if (eventdesc == null)
            {
                if (this.eventBindings == null)
                {
                    this.eventBindings = (IEventBindingService) this.GetService(typeof(IEventBindingService));
                }
                if (this.eventBindings != null)
                {
                    eventdesc = this.eventBindings.GetEvent(this.propertyInfo);
                }
            }
            IDesignerHost designerHost = this.DesignerHost;
            DesignerTransaction transaction = null;
            try
            {
                if (eventdesc.EventType == null)
                {
                    return false;
                }
                if (designerHost != null)
                {
                    string str2 = (component.Site != null) ? component.Site.Name : component.GetType().Name;
                    transaction = this.DesignerHost.CreateTransaction(System.Windows.Forms.SR.GetString("WindowsFormsSetEvent", new object[] { str2 + "." + this.PropertyName }));
                }
                if (this.eventBindings == null)
                {
                    ISite site = component.Site;
                    if (site != null)
                    {
                        this.eventBindings = (IEventBindingService) site.GetService(typeof(IEventBindingService));
                    }
                }
                if ((newHandler == null) && (this.eventBindings != null))
                {
                    newHandler = this.eventBindings.CreateUniqueMethodName(component, eventdesc);
                }
                if (newHandler != null)
                {
                    if (this.eventBindings != null)
                    {
                        bool flag = false;
                        foreach (string str3 in this.eventBindings.GetCompatibleMethods(eventdesc))
                        {
                            if (newHandler == str3)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            alwaysNavigate = true;
                        }
                    }
                    this.propertyInfo.SetValue(obj, newHandler);
                }
                if (alwaysNavigate && (this.eventBindings != null))
                {
                    targetBindingService = this.eventBindings;
                    targetComponent = component;
                    targetEventdesc = eventdesc;
                    Application.Idle += new EventHandler(PropertyDescriptorGridEntry.ShowCodeIdle);
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                    transaction = null;
                }
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            return true;
        }

        public override bool AllowMerge
        {
            get
            {
                MergablePropertyAttribute attribute = (MergablePropertyAttribute) this.propertyInfo.Attributes[typeof(MergablePropertyAttribute)];
                if (attribute != null)
                {
                    return attribute.IsDefaultAttribute();
                }
                return true;
            }
        }

        internal override AttributeCollection Attributes
        {
            get
            {
                return this.propertyInfo.Attributes;
            }
        }

        internal override bool Enumerable
        {
            get
            {
                return (base.Enumerable && !this.IsPropertyReadOnly);
            }
        }

        public override string HelpKeyword
        {
            get
            {
                if (this.helpKeyword == null)
                {
                    object valueOwner = this.GetValueOwner();
                    if (valueOwner == null)
                    {
                        return null;
                    }
                    HelpKeywordAttribute attribute = (HelpKeywordAttribute) this.propertyInfo.Attributes[typeof(HelpKeywordAttribute)];
                    if ((attribute != null) && !attribute.IsDefaultAttribute())
                    {
                        return attribute.HelpKeyword;
                    }
                    if (this is ImmutablePropertyDescriptorGridEntry)
                    {
                        this.helpKeyword = this.PropertyName;
                        GridEntry parentGridEntry = this;
                        while (parentGridEntry.ParentGridEntry != null)
                        {
                            parentGridEntry = parentGridEntry.ParentGridEntry;
                            if ((parentGridEntry.PropertyValue == valueOwner) || (valueOwner.GetType().IsValueType && (valueOwner.GetType() == parentGridEntry.PropertyValue.GetType())))
                            {
                                this.helpKeyword = parentGridEntry.PropertyName + "." + this.helpKeyword;
                                break;
                            }
                        }
                    }
                    else
                    {
                        string className = "";
                        System.Type componentType = this.propertyInfo.ComponentType;
                        if (componentType.IsCOMObject)
                        {
                            className = TypeDescriptor.GetClassName(valueOwner);
                        }
                        else
                        {
                            System.Type type = valueOwner.GetType();
                            if (!componentType.IsPublic || !componentType.IsAssignableFrom(type))
                            {
                                System.ComponentModel.PropertyDescriptor descriptor = TypeDescriptor.GetProperties(type)[this.PropertyName];
                                if (descriptor != null)
                                {
                                    componentType = descriptor.ComponentType;
                                }
                                else
                                {
                                    componentType = null;
                                }
                            }
                            if (componentType == null)
                            {
                                className = TypeDescriptor.GetClassName(valueOwner);
                            }
                            else
                            {
                                className = componentType.FullName;
                            }
                        }
                        this.helpKeyword = className + "." + this.propertyInfo.Name;
                    }
                }
                return this.helpKeyword;
            }
        }

        internal override string HelpKeywordInternal
        {
            get
            {
                return this.PropertyLabel;
            }
        }

        protected virtual bool IsPropertyReadOnly
        {
            get
            {
                return this.propertyInfo.IsReadOnly;
            }
        }

        public override bool IsValueEditable
        {
            get
            {
                return (((this.exceptionConverter == null) && !this.IsPropertyReadOnly) && base.IsValueEditable);
            }
        }

        internal override string LabelToolTipText
        {
            get
            {
                if (this.toolTipText == null)
                {
                    return base.LabelToolTipText;
                }
                return this.toolTipText;
            }
        }

        public override bool NeedsDropDownButton
        {
            get
            {
                return (base.NeedsDropDownButton && !this.IsPropertyReadOnly);
            }
        }

        internal bool ParensAroundName
        {
            get
            {
                if (0xff == this.parensAroundName)
                {
                    if (((ParenthesizePropertyNameAttribute) this.propertyInfo.Attributes[typeof(ParenthesizePropertyNameAttribute)]).NeedParenthesis)
                    {
                        this.parensAroundName = 1;
                    }
                    else
                    {
                        this.parensAroundName = 0;
                    }
                }
                return (this.parensAroundName == 1);
            }
        }

        public override string PropertyCategory
        {
            get
            {
                string category = this.propertyInfo.Category;
                if ((category != null) && (category.Length != 0))
                {
                    return category;
                }
                return base.PropertyCategory;
            }
        }

        public override string PropertyDescription
        {
            get
            {
                return this.propertyInfo.Description;
            }
        }

        public override System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return this.propertyInfo;
            }
        }

        public override string PropertyLabel
        {
            get
            {
                string displayName = this.propertyInfo.DisplayName;
                if (this.ParensAroundName)
                {
                    displayName = "(" + displayName + ")";
                }
                return displayName;
            }
        }

        public override string PropertyName
        {
            get
            {
                if (this.propertyInfo != null)
                {
                    return this.propertyInfo.Name;
                }
                return null;
            }
        }

        public override System.Type PropertyType
        {
            get
            {
                return this.propertyInfo.PropertyType;
            }
        }

        public override object PropertyValue
        {
            get
            {
                try
                {
                    object propertyValueCore = this.GetPropertyValueCore(this.GetValueOwner());
                    if (this.exceptionConverter != null)
                    {
                        this.SetFlagsAndExceptionInfo(0, null, null);
                    }
                    return propertyValueCore;
                }
                catch (Exception exception)
                {
                    if (this.exceptionConverter == null)
                    {
                        this.SetFlagsAndExceptionInfo(0, new ExceptionConverter(), new ExceptionEditor());
                    }
                    return exception;
                }
            }
            set
            {
                this.SetPropertyValue(this.GetValueOwner(), value, false, null);
            }
        }

        private IPropertyValueUIService PropertyValueUIService
        {
            get
            {
                if (!this.pvSvcChecked && (this.pvSvc == null))
                {
                    this.pvSvc = (IPropertyValueUIService) this.GetService(typeof(IPropertyValueUIService));
                    this.pvSvcChecked = true;
                }
                return this.pvSvc;
            }
        }

        public override bool ShouldRenderReadOnly
        {
            get
            {
                if (base.ForceReadOnly || this.forceRenderReadOnly)
                {
                    return true;
                }
                if ((this.propertyInfo.IsReadOnly && !this.readOnlyVerified) && this.GetFlagSet(0x80))
                {
                    System.Type propertyType = this.PropertyType;
                    if ((propertyType != null) && ((propertyType.IsArray || propertyType.IsValueType) || propertyType.IsPrimitive))
                    {
                        this.SetFlag(0x80, false);
                        this.SetFlag(0x100, true);
                        this.forceRenderReadOnly = true;
                    }
                }
                this.readOnlyVerified = true;
                return ((base.ShouldRenderReadOnly && !this.isSerializeContentsProp) && !base.NeedsCustomEditorButton);
            }
        }

        internal override System.ComponentModel.TypeConverter TypeConverter
        {
            get
            {
                if (this.exceptionConverter != null)
                {
                    return this.exceptionConverter;
                }
                if (base.converter == null)
                {
                    base.converter = this.propertyInfo.Converter;
                }
                return base.TypeConverter;
            }
        }

        internal override System.Drawing.Design.UITypeEditor UITypeEditor
        {
            get
            {
                if (this.exceptionEditor != null)
                {
                    return this.exceptionEditor;
                }
                base.editor = (System.Drawing.Design.UITypeEditor) this.propertyInfo.GetEditor(typeof(System.Drawing.Design.UITypeEditor));
                return base.UITypeEditor;
            }
        }

        private class ExceptionConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                if (!(destinationType == typeof(string)))
                {
                    throw base.GetConvertToException(value, destinationType);
                }
                if (!(value is Exception))
                {
                    return null;
                }
                Exception innerException = (Exception) value;
                if (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }
                return innerException.Message;
            }
        }

        private class ExceptionEditor : UITypeEditor
        {
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                Exception ex = value as Exception;
                if (ex != null)
                {
                    IUIService service = null;
                    if (context != null)
                    {
                        service = (IUIService) context.GetService(typeof(IUIService));
                    }
                    if (service != null)
                    {
                        service.ShowError(ex);
                        return value;
                    }
                    string message = ex.Message;
                    if ((message == null) || (message.Length == 0))
                    {
                        message = ex.ToString();
                    }
                    RTLAwareMessageBox.Show(null, message, System.Windows.Forms.SR.GetString("PropertyGridExceptionInfo"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                }
                return value;
            }

            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }
        }
    }
}

