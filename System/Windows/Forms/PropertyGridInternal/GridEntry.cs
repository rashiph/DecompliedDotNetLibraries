namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Windows.Forms.Internal;
    using System.Windows.Forms.VisualStyles;

    internal abstract class GridEntry : GridItem, ITypeDescriptorContext, IServiceProvider
    {
        private GridEntryAccessibleObject accessibleObject;
        internal static System.Windows.Forms.PropertyGridInternal.AttributeTypeSorter AttributeTypeSorter = new System.Windows.Forms.PropertyGridInternal.AttributeTypeSorter();
        private CacheItems cacheItems;
        private GridEntryCollection childCollection;
        protected System.ComponentModel.TypeConverter converter;
        protected static IComparer DisplayNameComparer = new DisplayNameSortComparer();
        protected System.Drawing.Design.UITypeEditor editor;
        private static object EVENT_LABEL_CLICK = new object();
        private static object EVENT_LABEL_DBLCLICK = new object();
        private static object EVENT_OUTLINE_CLICK = new object();
        private static object EVENT_OUTLINE_DBLCLICK = new object();
        private static object EVENT_RECREATE_CHILDREN = new object();
        private static object EVENT_VALUE_CLICK = new object();
        private static object EVENT_VALUE_DBLCLICK = new object();
        private EventEntry eventList;
        internal const int FL_CATEGORIES = 0x200000;
        internal const int FL_CHECKED = -2147483648;
        internal const int FL_EXPAND = 0x10000;
        internal const int FL_EXPANDABLE = 0x20000;
        internal const int FL_EXPANDABLE_FAILED = 0x80000;
        internal const int FL_NO_CUSTOM_PAINT = 0x100000;
        internal const int FLAG_CUSTOM_EDITABLE = 0x10;
        internal const int FLAG_CUSTOM_PAINT = 4;
        internal const int FLAG_DISPOSED = 0x2000;
        internal const int FLAG_DROPDOWN_EDITABLE = 0x20;
        internal const int FLAG_ENUMERABLE = 2;
        internal const int FLAG_FORCE_READONLY = 0x400;
        internal const int FLAG_IMMEDIATELY_EDITABLE = 8;
        internal const int FLAG_IMMUTABLE = 0x200;
        internal const int FLAG_LABEL_BOLD = 0x40;
        internal const int FLAG_READONLY_EDITABLE = 0x80;
        internal const int FLAG_RENDER_PASSWORD = 0x1000;
        internal const int FLAG_RENDER_READONLY = 0x100;
        internal const int FLAG_TEXT_EDITABLE = 1;
        internal int flags;
        protected bool hasFocus;
        protected static readonly Point InvalidPoint = new Point(-2147483648, -2147483648);
        protected Point labelTipPoint = InvalidPoint;
        private bool lastPaintWithExplorerStyle;
        private const int maximumLengthOfPropertyString = 0x3e8;
        protected const int NOTIFY_CAN_RESET = 2;
        protected const int NOTIFY_DBL_CLICK = 3;
        protected const int NOTIFY_RESET = 1;
        protected const int NOTIFY_RETURN = 5;
        protected const int NOTIFY_SHOULD_PERSIST = 4;
        protected const int OUTLINE_ICON_PADDING = 5;
        private Rectangle outlineRect = Rectangle.Empty;
        protected PropertyGrid ownerGrid;
        internal GridEntry parentPE;
        private static char passwordReplaceChar;
        private static BooleanSwitch PbrsAssertPropsSwitch = new BooleanSwitch("PbrsAssertProps", "PropertyBrowser : Assert on broken properties");
        private int propertyDepth;
        protected System.Windows.Forms.PropertySort PropertySort;
        protected Point valueTipPoint = InvalidPoint;

        protected GridEntry(PropertyGrid owner, GridEntry peParent)
        {
            this.parentPE = peParent;
            this.ownerGrid = owner;
            if (peParent != null)
            {
                this.propertyDepth = peParent.PropertyDepth + 1;
                this.PropertySort = peParent.PropertySort;
                if (peParent.ForceReadOnly)
                {
                    this.flags |= 0x400;
                }
            }
            else
            {
                this.propertyDepth = -1;
            }
        }

        protected virtual void AddEventHandler(object key, Delegate handler)
        {
            lock (this)
            {
                if (handler != null)
                {
                    for (EventEntry entry = this.eventList; entry != null; entry = entry.next)
                    {
                        if (entry.key == key)
                        {
                            entry.handler = Delegate.Combine(entry.handler, handler);
                            goto Label_0060;
                        }
                    }
                    this.eventList = new EventEntry(this.eventList, key, handler);
                }
            Label_0060:;
            }
        }

        public virtual void AddOnLabelClick(EventHandler h)
        {
            this.AddEventHandler(EVENT_LABEL_CLICK, h);
        }

        public virtual void AddOnLabelDoubleClick(EventHandler h)
        {
            this.AddEventHandler(EVENT_LABEL_DBLCLICK, h);
        }

        public virtual void AddOnOutlineClick(EventHandler h)
        {
            this.AddEventHandler(EVENT_OUTLINE_CLICK, h);
        }

        public virtual void AddOnOutlineDoubleClick(EventHandler h)
        {
            this.AddEventHandler(EVENT_OUTLINE_DBLCLICK, h);
        }

        public virtual void AddOnRecreateChildren(GridEntryRecreateChildrenEventHandler h)
        {
            this.AddEventHandler(EVENT_RECREATE_CHILDREN, h);
        }

        public virtual void AddOnValueClick(EventHandler h)
        {
            this.AddEventHandler(EVENT_VALUE_CLICK, h);
        }

        public virtual void AddOnValueDoubleClick(EventHandler h)
        {
            this.AddEventHandler(EVENT_VALUE_DBLCLICK, h);
        }

        public virtual bool CanResetPropertyValue()
        {
            return this.NotifyValue(2);
        }

        internal void ClearCachedValues()
        {
            this.ClearCachedValues(true);
        }

        internal void ClearCachedValues(bool clearChildren)
        {
            if (this.cacheItems != null)
            {
                this.cacheItems.useValueString = false;
                this.cacheItems.lastValue = null;
                this.cacheItems.useShouldSerialize = false;
            }
            if (clearChildren)
            {
                for (int i = 0; i < this.ChildCollection.Count; i++)
                {
                    this.ChildCollection.GetEntry(i).ClearCachedValues();
                }
            }
        }

        public object ConvertTextToValue(string text)
        {
            if (this.TypeConverter.CanConvertFrom(this, typeof(string)))
            {
                return this.TypeConverter.ConvertFromString(this, text);
            }
            return text;
        }

        internal static IRootGridEntry Create(PropertyGridView view, object[] rgobjs, IServiceProvider baseProvider, IDesignerHost currentHost, PropertyTab tab, System.Windows.Forms.PropertySort initialSortType)
        {
            IRootGridEntry entry = null;
            if ((rgobjs == null) || (rgobjs.Length == 0))
            {
                return null;
            }
            try
            {
                if (rgobjs.Length == 1)
                {
                    return new SingleSelectRootGridEntry(view, rgobjs[0], baseProvider, currentHost, tab, initialSortType);
                }
                entry = new MultiSelectRootGridEntry(view, rgobjs, baseProvider, currentHost, tab, initialSortType);
            }
            catch (Exception)
            {
                throw;
            }
            return entry;
        }

        protected virtual bool CreateChildren()
        {
            return this.CreateChildren(false);
        }

        protected virtual bool CreateChildren(bool diffOldChildren)
        {
            if (!this.GetFlagSet(0x20000))
            {
                if (this.childCollection != null)
                {
                    this.childCollection.Clear();
                }
                else
                {
                    this.childCollection = new GridEntryCollection(this, new GridEntry[0]);
                }
                return false;
            }
            if ((!diffOldChildren && (this.childCollection != null)) && (this.childCollection.Count > 0))
            {
                return true;
            }
            GridEntry[] entryArray = this.GetPropEntries(this, this.PropertyValue, this.PropertyType);
            bool flag = (entryArray != null) && (entryArray.Length > 0);
            if ((diffOldChildren && (this.childCollection != null)) && (this.childCollection.Count > 0))
            {
                bool flag2 = true;
                if (entryArray.Length == this.childCollection.Count)
                {
                    for (int i = 0; i < entryArray.Length; i++)
                    {
                        if (!entryArray[i].NonParentEquals(this.childCollection[i]))
                        {
                            flag2 = false;
                            break;
                        }
                    }
                }
                else
                {
                    flag2 = false;
                }
                if (flag2)
                {
                    return true;
                }
            }
            if (!flag)
            {
                this.SetFlag(0x80000, true);
                if (this.childCollection != null)
                {
                    this.childCollection.Clear();
                }
                else
                {
                    this.childCollection = new GridEntryCollection(this, new GridEntry[0]);
                }
                if (this.InternalExpanded)
                {
                    this.InternalExpanded = false;
                }
                return flag;
            }
            if (this.childCollection != null)
            {
                this.childCollection.Clear();
                this.childCollection.AddRange(entryArray);
                return flag;
            }
            this.childCollection = new GridEntryCollection(this, entryArray);
            return flag;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.flags |= -2147483648;
            this.SetFlag(0x2000, true);
            this.cacheItems = null;
            this.converter = null;
            this.editor = null;
            this.accessibleObject = null;
            if (disposing)
            {
                this.DisposeChildren();
            }
        }

        public virtual void DisposeChildren()
        {
            if (this.childCollection != null)
            {
                this.childCollection.Dispose();
                this.childCollection = null;
            }
        }

        public virtual bool DoubleClickPropertyValue()
        {
            return this.NotifyValue(3);
        }

        internal virtual void EditPropertyValue(PropertyGridView iva)
        {
            if (this.UITypeEditor != null)
            {
                try
                {
                    object propertyValue = this.PropertyValue;
                    object obj3 = this.UITypeEditor.EditValue(this, this, propertyValue);
                    if (!this.Disposed)
                    {
                        if ((obj3 != propertyValue) && this.IsValueEditable)
                        {
                            iva.CommitValue(this, obj3);
                        }
                        if (this.InternalExpanded)
                        {
                            PropertyGridView.GridPositionData data = this.GridEntryHost.CaptureGridPositionData();
                            this.InternalExpanded = false;
                            this.RecreateChildren();
                            data.Restore(this.GridEntryHost);
                        }
                        else
                        {
                            this.RecreateChildren();
                        }
                    }
                }
                catch (Exception exception)
                {
                    IUIService service = (IUIService) this.GetService(typeof(IUIService));
                    if (service != null)
                    {
                        service.ShowError(exception);
                    }
                    else
                    {
                        RTLAwareMessageBox.Show(this.GridEntryHost, exception.Message, System.Windows.Forms.SR.GetString("PBRSErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                    }
                }
            }
        }

        public override bool Equals(object obj)
        {
            return (this.NonParentEquals(obj) && (((GridEntry) obj).ParentGridEntry == this.ParentGridEntry));
        }

        ~GridEntry()
        {
            this.Dispose(false);
        }

        public virtual object FindPropertyValue(string propertyName, System.Type propertyType)
        {
            object valueOwner = this.GetValueOwner();
            System.ComponentModel.PropertyDescriptor descriptor = TypeDescriptor.GetProperties(valueOwner)[propertyName];
            if ((descriptor != null) && (descriptor.PropertyType == propertyType))
            {
                return descriptor.GetValue(valueOwner);
            }
            if (this.parentPE != null)
            {
                return this.parentPE.FindPropertyValue(propertyName, propertyType);
            }
            return null;
        }

        protected virtual Brush GetBackgroundBrush(Graphics g)
        {
            return this.GridEntryHost.GetBackgroundBrush(g);
        }

        internal virtual int GetChildIndex(GridEntry pe)
        {
            return this.Children.GetEntry(pe);
        }

        public virtual object GetChildValueOwner(GridEntry childEntry)
        {
            return this.PropertyValue;
        }

        public virtual IComponent[] GetComponents()
        {
            IComponent component = this.Component;
            if (component != null)
            {
                return new IComponent[] { component };
            }
            return null;
        }

        protected virtual Delegate GetEventHandler(object key)
        {
            lock (this)
            {
                for (EventEntry entry = this.eventList; entry != null; entry = entry.next)
                {
                    if (entry.key == key)
                    {
                        return entry.handler;
                    }
                }
                return null;
            }
        }

        protected virtual bool GetFlagSet(int flag)
        {
            return ((flag & this.Flags) != 0);
        }

        protected Font GetFont(bool boldFont)
        {
            if (boldFont)
            {
                return this.GridEntryHost.GetBoldFont();
            }
            return this.GridEntryHost.GetBaseFont();
        }

        public override int GetHashCode()
        {
            object propertyLabel = this.PropertyLabel;
            object propertyType = this.PropertyType;
            uint num = (propertyLabel == null) ? 0 : ((uint) propertyLabel.GetHashCode());
            uint num2 = (propertyType == null) ? 0 : ((uint) propertyType.GetHashCode());
            uint hashCode = (uint) base.GetType().GetHashCode();
            return (int) ((num ^ ((num2 << 13) | (num2 >> 0x13))) ^ ((hashCode << 0x1a) | (hashCode >> 6)));
        }

        protected IntPtr GetHfont(bool boldFont)
        {
            if (boldFont)
            {
                return this.GridEntryHost.GetBoldHfont();
            }
            return this.GridEntryHost.GetBaseHfont();
        }

        protected int GetLabelTextWidth(string labelText, Graphics g, Font f)
        {
            if (this.cacheItems == null)
            {
                this.cacheItems = new CacheItems();
            }
            else if (((this.cacheItems.useCompatTextRendering == this.ownerGrid.UseCompatibleTextRendering) && (this.cacheItems.lastLabel == labelText)) && f.Equals(this.cacheItems.lastLabelFont))
            {
                return this.cacheItems.lastLabelWidth;
            }
            SizeF ef = PropertyGrid.MeasureTextHelper.MeasureText(this.ownerGrid, g, labelText, f);
            this.cacheItems.lastLabelWidth = (int) ef.Width;
            this.cacheItems.lastLabel = labelText;
            this.cacheItems.lastLabelFont = f;
            this.cacheItems.useCompatTextRendering = this.ownerGrid.UseCompatibleTextRendering;
            return this.cacheItems.lastLabelWidth;
        }

        internal virtual Point GetLabelToolTipLocation(int mouseX, int mouseY)
        {
            return this.labelTipPoint;
        }

        internal bool GetMultipleLines(string valueString)
        {
            if ((valueString.IndexOf('\n') <= 0) && (valueString.IndexOf('\r') <= 0))
            {
                return false;
            }
            return true;
        }

        protected virtual GridEntry[] GetPropEntries(GridEntry peParent, object obj, System.Type objType)
        {
            if (obj == null)
            {
                return null;
            }
            GridEntry[] entryArray = null;
            Attribute[] attributeArray = new Attribute[this.BrowsableAttributes.Count];
            this.BrowsableAttributes.CopyTo(attributeArray, 0);
            PropertyTab currentTab = this.CurrentTab;
            try
            {
                bool forceReadOnly = this.ForceReadOnly;
                if (!forceReadOnly)
                {
                    ReadOnlyAttribute attribute = (ReadOnlyAttribute) TypeDescriptor.GetAttributes(obj)[typeof(ReadOnlyAttribute)];
                    forceReadOnly = (attribute != null) && !attribute.IsDefaultAttribute();
                }
                if (!this.TypeConverter.GetPropertiesSupported(this) && !this.AlwaysAllowExpand)
                {
                    return entryArray;
                }
                PropertyDescriptorCollection descriptors = null;
                System.ComponentModel.PropertyDescriptor defaultProperty = null;
                if (currentTab != null)
                {
                    descriptors = currentTab.GetProperties(this, obj, attributeArray);
                    defaultProperty = currentTab.GetDefaultProperty(obj);
                }
                else
                {
                    descriptors = this.TypeConverter.GetProperties(this, obj, attributeArray);
                    defaultProperty = TypeDescriptor.GetDefaultProperty(obj);
                }
                if (descriptors == null)
                {
                    return null;
                }
                if ((this.PropertySort & System.Windows.Forms.PropertySort.Alphabetical) != System.Windows.Forms.PropertySort.NoSort)
                {
                    if ((objType == null) || !objType.IsArray)
                    {
                        descriptors = descriptors.Sort(DisplayNameComparer);
                    }
                    System.ComponentModel.PropertyDescriptor[] descriptorArray = new System.ComponentModel.PropertyDescriptor[descriptors.Count];
                    descriptors.CopyTo(descriptorArray, 0);
                    descriptors = new PropertyDescriptorCollection(this.SortParenProperties(descriptorArray));
                }
                if ((defaultProperty == null) && (descriptors.Count > 0))
                {
                    defaultProperty = descriptors[0];
                }
                if (((descriptors == null) || (descriptors.Count == 0)) && (((objType != null) && objType.IsArray) && (obj != null)))
                {
                    Array array = (Array) obj;
                    entryArray = new GridEntry[array.Length];
                    for (int i = 0; i < entryArray.Length; i++)
                    {
                        entryArray[i] = new ArrayElementGridEntry(this.ownerGrid, peParent, i);
                    }
                    return entryArray;
                }
                bool createInstanceSupported = this.TypeConverter.GetCreateInstanceSupported(this);
                entryArray = new GridEntry[descriptors.Count];
                int num2 = 0;
                foreach (System.ComponentModel.PropertyDescriptor descriptor2 in descriptors)
                {
                    GridEntry entry;
                    bool hide = false;
                    try
                    {
                        object component = obj;
                        if (obj is ICustomTypeDescriptor)
                        {
                            component = ((ICustomTypeDescriptor) obj).GetPropertyOwner(descriptor2);
                        }
                        descriptor2.GetValue(component);
                    }
                    catch (Exception)
                    {
                        bool enabled = PbrsAssertPropsSwitch.Enabled;
                        hide = true;
                    }
                    if (createInstanceSupported)
                    {
                        entry = new ImmutablePropertyDescriptorGridEntry(this.ownerGrid, peParent, descriptor2, hide);
                    }
                    else
                    {
                        entry = new PropertyDescriptorGridEntry(this.ownerGrid, peParent, descriptor2, hide);
                    }
                    if (forceReadOnly)
                    {
                        entry.flags |= 0x400;
                    }
                    if (descriptor2.Equals(defaultProperty))
                    {
                        this.DefaultChild = entry;
                    }
                    entryArray[num2++] = entry;
                }
            }
            catch (Exception)
            {
            }
            return entryArray;
        }

        public virtual string GetPropertyTextValue()
        {
            return this.GetPropertyTextValue(this.PropertyValue);
        }

        public virtual string GetPropertyTextValue(object value)
        {
            string str = null;
            System.ComponentModel.TypeConverter typeConverter = this.TypeConverter;
            try
            {
                str = typeConverter.ConvertToString(this, value);
            }
            catch (Exception)
            {
            }
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        public virtual object[] GetPropertyValueList()
        {
            ICollection standardValues = this.TypeConverter.GetStandardValues(this);
            if (standardValues != null)
            {
                object[] array = new object[standardValues.Count];
                standardValues.CopyTo(array, 0);
                return array;
            }
            return new object[0];
        }

        public virtual object GetService(System.Type serviceType)
        {
            if (serviceType == typeof(GridItem))
            {
                return this;
            }
            if (this.parentPE != null)
            {
                return this.parentPE.GetService(serviceType);
            }
            return null;
        }

        public virtual string GetTestingInfo()
        {
            string str = "object = (";
            string propertyTextValue = this.GetPropertyTextValue();
            if (propertyTextValue == null)
            {
                propertyTextValue = "(null)";
            }
            else
            {
                propertyTextValue = propertyTextValue.Replace('\0', ' ');
            }
            System.Type propertyType = this.PropertyType;
            if (propertyType == null)
            {
                propertyType = typeof(object);
            }
            object obj2 = str + this.FullLabel;
            return string.Concat(new object[] { obj2, "), property = (", this.PropertyLabel, ",", propertyType.AssemblyQualifiedName, "), value = [", propertyTextValue, "], expandable = ", this.Expandable.ToString(), ", readOnly = ", this.ShouldRenderReadOnly });
        }

        public virtual object GetValueOwner()
        {
            if (this.parentPE == null)
            {
                return this.PropertyValue;
            }
            return this.parentPE.GetChildValueOwner(this);
        }

        public virtual object[] GetValueOwners()
        {
            object valueOwner = this.GetValueOwner();
            if (valueOwner != null)
            {
                return new object[] { valueOwner };
            }
            return null;
        }

        internal int GetValueTextWidth(string valueString, Graphics g, Font f)
        {
            if (this.cacheItems == null)
            {
                this.cacheItems = new CacheItems();
            }
            else if (((this.cacheItems.lastValueTextWidth != -1) && (this.cacheItems.lastValueString == valueString)) && f.Equals(this.cacheItems.lastValueFont))
            {
                return this.cacheItems.lastValueTextWidth;
            }
            this.cacheItems.lastValueTextWidth = (int) g.MeasureString(valueString, f).Width;
            this.cacheItems.lastValueString = valueString;
            this.cacheItems.lastValueFont = f;
            return this.cacheItems.lastValueTextWidth;
        }

        public virtual System.Type GetValueType()
        {
            return this.PropertyType;
        }

        internal virtual bool NonParentEquals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (!(obj is GridEntry))
            {
                return false;
            }
            GridEntry entry = (GridEntry) obj;
            return ((entry.PropertyLabel.Equals(this.PropertyLabel) && entry.PropertyType.Equals(this.PropertyType)) && (entry.PropertyDepth == this.PropertyDepth));
        }

        internal virtual bool NotifyChildValue(GridEntry pe, int type)
        {
            return pe.NotifyValueGivenParent(pe.GetValueOwner(), type);
        }

        internal virtual bool NotifyValue(int type)
        {
            return ((this.parentPE == null) || this.parentPE.NotifyChildValue(this, type));
        }

        internal virtual bool NotifyValueGivenParent(object obj, int type)
        {
            return false;
        }

        public virtual void OnComponentChanged()
        {
            if (this.ComponentChangeService != null)
            {
                this.ComponentChangeService.OnComponentChanged(this.GetValueOwner(), this.PropertyDescriptor, null, null);
            }
        }

        public virtual bool OnComponentChanging()
        {
            if (this.ComponentChangeService != null)
            {
                try
                {
                    this.ComponentChangeService.OnComponentChanging(this.GetValueOwner(), this.PropertyDescriptor);
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw exception;
                    }
                    return false;
                }
            }
            return true;
        }

        protected virtual void OnLabelClick(EventArgs e)
        {
            this.RaiseEvent(EVENT_LABEL_CLICK, e);
        }

        protected virtual void OnLabelDoubleClick(EventArgs e)
        {
            this.RaiseEvent(EVENT_LABEL_DBLCLICK, e);
        }

        public virtual bool OnMouseClick(int x, int y, int count, MouseButtons button)
        {
            PropertyGridView gridEntryHost = this.GridEntryHost;
            if ((button & MouseButtons.Left) != MouseButtons.Left)
            {
                return false;
            }
            int labelWidth = gridEntryHost.GetLabelWidth();
            if ((x >= 0) && (x <= labelWidth))
            {
                if (this.Expandable && this.OutlineRect.Contains(x, y))
                {
                    if ((count % 2) == 0)
                    {
                        this.OnOutlineDoubleClick(EventArgs.Empty);
                    }
                    else
                    {
                        this.OnOutlineClick(EventArgs.Empty);
                    }
                    return true;
                }
                if ((count % 2) == 0)
                {
                    this.OnLabelDoubleClick(EventArgs.Empty);
                }
                else
                {
                    this.OnLabelClick(EventArgs.Empty);
                }
                return true;
            }
            labelWidth += gridEntryHost.GetSplitterWidth();
            if ((x < labelWidth) || (x > (labelWidth + gridEntryHost.GetValueWidth())))
            {
                return false;
            }
            if ((count % 2) == 0)
            {
                this.OnValueDoubleClick(EventArgs.Empty);
            }
            else
            {
                this.OnValueClick(EventArgs.Empty);
            }
            return true;
        }

        protected virtual void OnOutlineClick(EventArgs e)
        {
            this.RaiseEvent(EVENT_OUTLINE_CLICK, e);
        }

        protected virtual void OnOutlineDoubleClick(EventArgs e)
        {
            this.RaiseEvent(EVENT_OUTLINE_DBLCLICK, e);
        }

        protected virtual void OnRecreateChildren(GridEntryRecreateChildrenEventArgs e)
        {
            Delegate eventHandler = this.GetEventHandler(EVENT_RECREATE_CHILDREN);
            if (eventHandler != null)
            {
                ((GridEntryRecreateChildrenEventHandler) eventHandler)(this, e);
            }
        }

        protected virtual void OnValueClick(EventArgs e)
        {
            this.RaiseEvent(EVENT_VALUE_CLICK, e);
        }

        protected virtual void OnValueDoubleClick(EventArgs e)
        {
            this.RaiseEvent(EVENT_VALUE_DBLCLICK, e);
        }

        internal bool OnValueReturnKey()
        {
            return this.NotifyValue(5);
        }

        public virtual void PaintLabel(Graphics g, Rectangle rect, Rectangle clipRect, bool selected, bool paintFullLabel)
        {
            PropertyGridView gridEntryHost = this.GridEntryHost;
            string propertyLabel = this.PropertyLabel;
            int width = gridEntryHost.GetOutlineIconSize() + 5;
            Brush brush = selected ? SystemBrushes.Highlight : this.GetBackgroundBrush(g);
            if (selected && !this.hasFocus)
            {
                brush = gridEntryHost.GetLineBrush(g);
            }
            bool boldFont = (this.Flags & 0x40) != 0;
            Font f = this.GetFont(boldFont);
            int num2 = this.GetLabelTextWidth(propertyLabel, g, f);
            int num3 = paintFullLabel ? num2 : 0;
            int x = rect.X + this.PropertyLabelIndent;
            Brush brush2 = brush;
            if (paintFullLabel && (num3 >= (rect.Width - (x + 2))))
            {
                int num5 = (x + num3) + 2;
                g.FillRectangle(brush2, width - 1, rect.Y, (num5 - width) + 3, rect.Height);
                Pen pen = new Pen(gridEntryHost.GetLineColor());
                g.DrawLine(pen, num5, rect.Y, num5, rect.Height);
                pen.Dispose();
                rect.Width = num5 - rect.X;
            }
            else
            {
                g.FillRectangle(brush2, rect.X, rect.Y, rect.Width, rect.Height);
            }
            Brush lineBrush = gridEntryHost.GetLineBrush(g);
            g.FillRectangle(lineBrush, rect.X, rect.Y, width, rect.Height);
            if (selected && this.hasFocus)
            {
                g.FillRectangle(SystemBrushes.Highlight, x, rect.Y, (rect.Width - x) - 1, rect.Height);
            }
            int num6 = Math.Min((int) ((rect.Width - x) - 1), (int) (num2 + 2));
            Rectangle a = new Rectangle(x, rect.Y + 1, num6, rect.Height - 1);
            if (Rectangle.Intersect(a, clipRect).IsEmpty)
            {
                goto Label_0292;
            }
            Region clip = g.Clip;
            g.SetClip(a);
            Color color = (selected && this.hasFocus) ? SystemColors.HighlightText : g.GetNearestColor(this.LabelTextColor);
            if (this.ownerGrid.UseCompatibleTextRendering)
            {
                using (Brush brush4 = new SolidBrush(color))
                {
                    StringFormat format = new StringFormat(StringFormatFlags.NoWrap) {
                        Trimming = StringTrimming.None
                    };
                    g.DrawString(propertyLabel, f, brush4, a, format);
                    goto Label_0257;
                }
            }
            TextRenderer.DrawText(g, propertyLabel, f, a, color, PropertyGrid.MeasureTextHelper.GetTextRendererFlags());
        Label_0257:
            g.SetClip(clip, CombineMode.Replace);
            clip.Dispose();
            if (num6 <= num2)
            {
                this.labelTipPoint = new Point(x + 2, rect.Y + 1);
            }
            else
            {
                this.labelTipPoint = InvalidPoint;
            }
        Label_0292:
            rect.Y--;
            rect.Height += 2;
            this.PaintOutline(g, rect);
        }

        public virtual void PaintOutline(Graphics g, Rectangle r)
        {
            if (this.GridEntryHost.IsExplorerTreeSupported)
            {
                if (!this.lastPaintWithExplorerStyle)
                {
                    this.outlineRect = Rectangle.Empty;
                    this.lastPaintWithExplorerStyle = true;
                }
                this.PaintOutlineWithExplorerTreeStyle(g, r);
            }
            else
            {
                if (this.lastPaintWithExplorerStyle)
                {
                    this.outlineRect = Rectangle.Empty;
                    this.lastPaintWithExplorerStyle = false;
                }
                this.PaintOutlineWithClassicStyle(g, r);
            }
        }

        private void PaintOutlineWithClassicStyle(Graphics g, Rectangle r)
        {
            if (this.Expandable)
            {
                bool internalExpanded = this.InternalExpanded;
                Rectangle outlineRect = this.OutlineRect;
                outlineRect = Rectangle.Intersect(r, outlineRect);
                if (!outlineRect.IsEmpty)
                {
                    Pen pen;
                    Brush backgroundBrush = this.GetBackgroundBrush(g);
                    Color textColor = this.GridEntryHost.GetTextColor();
                    if (textColor.IsSystemColor)
                    {
                        pen = SystemPens.FromSystemColor(textColor);
                    }
                    else
                    {
                        pen = new Pen(textColor);
                    }
                    g.FillRectangle(backgroundBrush, outlineRect);
                    g.DrawRectangle(pen, outlineRect.X, outlineRect.Y, outlineRect.Width - 1, outlineRect.Height - 1);
                    int num = 2;
                    g.DrawLine(pen, (int) (outlineRect.X + num), (int) (outlineRect.Y + (outlineRect.Height / 2)), (int) (((outlineRect.X + outlineRect.Width) - num) - 1), (int) (outlineRect.Y + (outlineRect.Height / 2)));
                    if (!internalExpanded)
                    {
                        g.DrawLine(pen, (int) (outlineRect.X + (outlineRect.Width / 2)), (int) (outlineRect.Y + num), (int) (outlineRect.X + (outlineRect.Width / 2)), (int) (((outlineRect.Y + outlineRect.Height) - num) - 1));
                    }
                    if (!textColor.IsSystemColor)
                    {
                        pen.Dispose();
                    }
                }
            }
        }

        private void PaintOutlineWithExplorerTreeStyle(Graphics g, Rectangle r)
        {
            if (this.Expandable)
            {
                bool internalExpanded = this.InternalExpanded;
                Rectangle outlineRect = this.OutlineRect;
                outlineRect = Rectangle.Intersect(r, outlineRect);
                if (!outlineRect.IsEmpty)
                {
                    VisualStyleElement opened = null;
                    if (internalExpanded)
                    {
                        opened = VisualStyleElement.ExplorerTreeView.Glyph.Opened;
                    }
                    else
                    {
                        opened = VisualStyleElement.ExplorerTreeView.Glyph.Closed;
                    }
                    new VisualStyleRenderer(opened).DrawBackground(g, outlineRect);
                }
            }
        }

        public virtual void PaintValue(object val, Graphics g, Rectangle rect, Rectangle clipRect, PaintValueFlags paintFlags)
        {
            string lastValueString;
            PropertyGridView gridEntryHost = this.GridEntryHost;
            int valuePaintIndent = 0;
            Color textColor = gridEntryHost.GetTextColor();
            if (this.ShouldRenderReadOnly)
            {
                textColor = this.GridEntryHost.GrayTextColor;
            }
            if ((paintFlags & PaintValueFlags.FetchValue) != PaintValueFlags.None)
            {
                if ((this.cacheItems != null) && this.cacheItems.useValueString)
                {
                    lastValueString = this.cacheItems.lastValueString;
                    val = this.cacheItems.lastValue;
                }
                else
                {
                    val = this.PropertyValue;
                    lastValueString = this.GetPropertyTextValue(val);
                    if (this.cacheItems == null)
                    {
                        this.cacheItems = new CacheItems();
                    }
                    this.cacheItems.lastValueString = lastValueString;
                    this.cacheItems.useValueString = true;
                    this.cacheItems.lastValueTextWidth = -1;
                    this.cacheItems.lastValueFont = null;
                    this.cacheItems.lastValue = val;
                }
            }
            else
            {
                lastValueString = this.GetPropertyTextValue(val);
            }
            Brush backgroundBrush = this.GetBackgroundBrush(g);
            if ((paintFlags & PaintValueFlags.DrawSelected) != PaintValueFlags.None)
            {
                backgroundBrush = SystemBrushes.Highlight;
                textColor = SystemColors.HighlightText;
            }
            Brush brush = backgroundBrush;
            g.FillRectangle(brush, clipRect);
            if (this.IsCustomPaint)
            {
                valuePaintIndent = gridEntryHost.GetValuePaintIndent();
                Rectangle a = new Rectangle(rect.X + 1, rect.Y + 1, gridEntryHost.GetValuePaintWidth(), gridEntryHost.GetGridEntryHeight() - 2);
                if (!Rectangle.Intersect(a, clipRect).IsEmpty)
                {
                    System.Drawing.Design.UITypeEditor uITypeEditor = this.UITypeEditor;
                    if (uITypeEditor != null)
                    {
                        uITypeEditor.PaintValue(new PaintValueEventArgs(this, val, g, a));
                    }
                    a.Width--;
                    a.Height--;
                    g.DrawRectangle(SystemPens.WindowText, a);
                }
            }
            rect.X += valuePaintIndent + gridEntryHost.GetValueStringIndent();
            rect.Width -= valuePaintIndent + (2 * gridEntryHost.GetValueStringIndent());
            bool boldFont = ((paintFlags & PaintValueFlags.CheckShouldSerialize) != PaintValueFlags.None) && this.ShouldSerializePropertyValue();
            if ((lastValueString != null) && (lastValueString.Length > 0))
            {
                Font f = this.GetFont(boldFont);
                if (lastValueString.Length > 0x3e8)
                {
                    lastValueString = lastValueString.Substring(0, 0x3e8);
                }
                int num2 = this.GetValueTextWidth(lastValueString, g, f);
                bool flag2 = false;
                if ((num2 >= rect.Width) || this.GetMultipleLines(lastValueString))
                {
                    flag2 = true;
                }
                if (!Rectangle.Intersect(rect, clipRect).IsEmpty)
                {
                    if ((paintFlags & PaintValueFlags.PaintInPlace) != PaintValueFlags.None)
                    {
                        rect.Offset(1, 2);
                    }
                    else
                    {
                        rect.Offset(1, 1);
                    }
                    Matrix transform = g.Transform;
                    IntPtr hdc = g.GetHdc();
                    IntNativeMethods.RECT lpRect = IntNativeMethods.RECT.FromXYWH((rect.X + ((int) transform.OffsetX)) + 2, (rect.Y + ((int) transform.OffsetY)) - 1, rect.Width - 4, rect.Height);
                    IntPtr hfont = this.GetHfont(boldFont);
                    int crColor = 0;
                    int clr = 0;
                    Color color2 = ((paintFlags & PaintValueFlags.DrawSelected) != PaintValueFlags.None) ? SystemColors.Highlight : this.GridEntryHost.BackColor;
                    try
                    {
                        crColor = System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(g, hdc), System.Windows.Forms.SafeNativeMethods.RGBToCOLORREF(textColor.ToArgb()));
                        clr = System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(g, hdc), System.Windows.Forms.SafeNativeMethods.RGBToCOLORREF(color2.ToArgb()));
                        hfont = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(g, hdc), new HandleRef(null, hfont));
                        int nFormat = 0x2960;
                        if (gridEntryHost.DrawValuesRightToLeft)
                        {
                            nFormat |= 0x20002;
                        }
                        if (this.ShouldRenderPassword)
                        {
                            if (passwordReplaceChar == '\0')
                            {
                                if (Environment.OSVersion.Version.Major > 4)
                                {
                                    passwordReplaceChar = '●';
                                }
                                else
                                {
                                    passwordReplaceChar = '*';
                                }
                            }
                            lastValueString = new string(passwordReplaceChar, lastValueString.Length);
                        }
                        IntUnsafeNativeMethods.DrawText(new HandleRef(g, hdc), lastValueString, ref lpRect, nFormat);
                    }
                    finally
                    {
                        System.Windows.Forms.SafeNativeMethods.SetTextColor(new HandleRef(g, hdc), crColor);
                        System.Windows.Forms.SafeNativeMethods.SetBkColor(new HandleRef(g, hdc), clr);
                        hfont = System.Windows.Forms.SafeNativeMethods.SelectObject(new HandleRef(g, hdc), new HandleRef(null, hfont));
                        g.ReleaseHdcInternal(hdc);
                    }
                    if (flag2)
                    {
                        this.ValueToolTipLocation = new Point(rect.X + 2, rect.Y - 1);
                    }
                    else
                    {
                        this.ValueToolTipLocation = InvalidPoint;
                    }
                }
            }
        }

        protected virtual void RaiseEvent(object key, EventArgs e)
        {
            Delegate eventHandler = this.GetEventHandler(key);
            if (eventHandler != null)
            {
                ((EventHandler) eventHandler)(this, e);
            }
        }

        internal void RecreateChildren()
        {
            this.RecreateChildren(-1);
        }

        internal void RecreateChildren(int oldCount)
        {
            bool flag = this.InternalExpanded || (oldCount > 0);
            if (oldCount == -1)
            {
                oldCount = this.VisibleChildCount;
            }
            this.ResetState();
            if (oldCount != 0)
            {
                foreach (GridEntry entry in this.ChildCollection)
                {
                    entry.RecreateChildren();
                }
                this.DisposeChildren();
                this.InternalExpanded = flag;
                this.OnRecreateChildren(new GridEntryRecreateChildrenEventArgs(oldCount, this.VisibleChildCount));
            }
        }

        public virtual void Refresh()
        {
            System.Type propertyType = this.PropertyType;
            if ((propertyType != null) && propertyType.IsArray)
            {
                this.CreateChildren(true);
            }
            if (this.childCollection != null)
            {
                if ((this.InternalExpanded && (this.cacheItems != null)) && ((this.cacheItems.lastValue != null) && (this.cacheItems.lastValue != this.PropertyValue)))
                {
                    this.ClearCachedValues();
                    this.RecreateChildren();
                    return;
                }
                if (this.InternalExpanded)
                {
                    IEnumerator enumerator = this.childCollection.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        object current = enumerator.Current;
                        ((GridEntry) current).Refresh();
                    }
                }
                else
                {
                    this.DisposeChildren();
                }
            }
            this.ClearCachedValues();
        }

        protected virtual void RemoveEventHandler(object key, Delegate handler)
        {
            lock (this)
            {
                if (handler != null)
                {
                    EventEntry eventList = this.eventList;
                    EventEntry entry2 = null;
                    while (eventList != null)
                    {
                        if (eventList.key == key)
                        {
                            eventList.handler = Delegate.Remove(eventList.handler, handler);
                            if (eventList.handler == null)
                            {
                                if (entry2 == null)
                                {
                                    this.eventList = eventList.next;
                                }
                                else
                                {
                                    entry2.next = eventList.next;
                                }
                            }
                            break;
                        }
                        entry2 = eventList;
                        eventList = eventList.next;
                    }
                }
            }
        }

        protected virtual void RemoveEventHandlers()
        {
            this.eventList = null;
        }

        public virtual void RemoveOnLabelClick(EventHandler h)
        {
            this.RemoveEventHandler(EVENT_LABEL_CLICK, h);
        }

        public virtual void RemoveOnLabelDoubleClick(EventHandler h)
        {
            this.RemoveEventHandler(EVENT_LABEL_DBLCLICK, h);
        }

        public virtual void RemoveOnOutlineClick(EventHandler h)
        {
            this.RemoveEventHandler(EVENT_OUTLINE_CLICK, h);
        }

        public virtual void RemoveOnOutlineDoubleClick(EventHandler h)
        {
            this.RemoveEventHandler(EVENT_OUTLINE_DBLCLICK, h);
        }

        public virtual void RemoveOnRecreateChildren(GridEntryRecreateChildrenEventHandler h)
        {
            this.RemoveEventHandler(EVENT_RECREATE_CHILDREN, h);
        }

        public virtual void RemoveOnValueClick(EventHandler h)
        {
            this.RemoveEventHandler(EVENT_VALUE_CLICK, h);
        }

        public virtual void RemoveOnValueDoubleClick(EventHandler h)
        {
            this.RemoveEventHandler(EVENT_VALUE_DBLCLICK, h);
        }

        public virtual void ResetPropertyValue()
        {
            this.NotifyValue(1);
            this.Refresh();
        }

        protected void ResetState()
        {
            this.Flags = 0;
            this.ClearCachedValues();
        }

        public override bool Select()
        {
            if (!this.Disposed)
            {
                try
                {
                    this.GridEntryHost.SelectedGridEntry = this;
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        protected virtual void SetFlag(int flag, bool fVal)
        {
            this.SetFlag(flag, fVal ? flag : 0);
        }

        protected virtual void SetFlag(int flag, int val)
        {
            this.Flags = (this.Flags & ~flag) | val;
        }

        protected virtual void SetFlag(int flag_valid, int flag, bool fVal)
        {
            this.SetFlag(flag_valid | flag, (int) (flag_valid | (fVal ? flag : 0)));
        }

        public virtual bool SetPropertyTextValue(string str)
        {
            bool flag = (this.childCollection != null) && (this.childCollection.Count > 0);
            this.PropertyValue = this.ConvertTextToValue(str);
            this.CreateChildren();
            bool flag2 = (this.childCollection != null) && (this.childCollection.Count > 0);
            return (flag != flag2);
        }

        internal virtual bool ShouldSerializePropertyValue()
        {
            if (this.cacheItems != null)
            {
                if (this.cacheItems.useShouldSerialize)
                {
                    return this.cacheItems.lastShouldSerialize;
                }
                this.cacheItems.lastShouldSerialize = this.NotifyValue(4);
                this.cacheItems.useShouldSerialize = true;
            }
            else
            {
                this.cacheItems = new CacheItems();
                this.cacheItems.lastShouldSerialize = this.NotifyValue(4);
                this.cacheItems.useShouldSerialize = true;
            }
            return this.cacheItems.lastShouldSerialize;
        }

        private System.ComponentModel.PropertyDescriptor[] SortParenProperties(System.ComponentModel.PropertyDescriptor[] props)
        {
            System.ComponentModel.PropertyDescriptor[] descriptorArray = null;
            int num = 0;
            for (int i = 0; i < props.Length; i++)
            {
                if (((ParenthesizePropertyNameAttribute) props[i].Attributes[typeof(ParenthesizePropertyNameAttribute)]).NeedParenthesis)
                {
                    if (descriptorArray == null)
                    {
                        descriptorArray = new System.ComponentModel.PropertyDescriptor[props.Length];
                    }
                    descriptorArray[num++] = props[i];
                    props[i] = null;
                }
            }
            if (num > 0)
            {
                for (int j = 0; j < props.Length; j++)
                {
                    if (props[j] != null)
                    {
                        descriptorArray[num++] = props[j];
                    }
                }
                props = descriptorArray;
            }
            return props;
        }

        public override string ToString()
        {
            return (base.GetType().FullName + " " + this.PropertyLabel);
        }

        public AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibleObject == null)
                {
                    this.accessibleObject = new GridEntryAccessibleObject(this);
                }
                return this.accessibleObject;
            }
        }

        public virtual bool AllowMerge
        {
            get
            {
                return true;
            }
        }

        internal virtual bool AlwaysAllowExpand
        {
            get
            {
                return false;
            }
        }

        internal virtual AttributeCollection Attributes
        {
            get
            {
                return TypeDescriptor.GetAttributes(this.PropertyType);
            }
        }

        public virtual AttributeCollection BrowsableAttributes
        {
            get
            {
                if (this.parentPE != null)
                {
                    return this.parentPE.BrowsableAttributes;
                }
                return null;
            }
            set
            {
                this.parentPE.BrowsableAttributes = value;
            }
        }

        protected GridEntryCollection ChildCollection
        {
            get
            {
                if (this.childCollection == null)
                {
                    this.childCollection = new GridEntryCollection(this, null);
                }
                return this.childCollection;
            }
            set
            {
                if (this.childCollection != value)
                {
                    if (this.childCollection != null)
                    {
                        this.childCollection.Dispose();
                        this.childCollection = null;
                    }
                    this.childCollection = value;
                }
            }
        }

        public int ChildCount
        {
            get
            {
                if (this.Children != null)
                {
                    return this.Children.Count;
                }
                return 0;
            }
        }

        public virtual GridEntryCollection Children
        {
            get
            {
                if ((this.childCollection == null) && !this.Disposed)
                {
                    this.CreateChildren();
                }
                return this.childCollection;
            }
        }

        public virtual IComponent Component
        {
            get
            {
                object valueOwner = this.GetValueOwner();
                if (valueOwner is IComponent)
                {
                    return (IComponent) valueOwner;
                }
                if (this.parentPE != null)
                {
                    return this.parentPE.Component;
                }
                return null;
            }
        }

        protected virtual IComponentChangeService ComponentChangeService
        {
            get
            {
                return this.parentPE.ComponentChangeService;
            }
        }

        public virtual IContainer Container
        {
            get
            {
                IComponent component = this.Component;
                if (component != null)
                {
                    ISite site = component.Site;
                    if (site != null)
                    {
                        return site.Container;
                    }
                }
                return null;
            }
        }

        public virtual PropertyTab CurrentTab
        {
            get
            {
                if (this.parentPE != null)
                {
                    return this.parentPE.CurrentTab;
                }
                return null;
            }
            set
            {
                if (this.parentPE != null)
                {
                    this.parentPE.CurrentTab = value;
                }
            }
        }

        internal virtual GridEntry DefaultChild
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        internal virtual IDesignerHost DesignerHost
        {
            get
            {
                if (this.parentPE != null)
                {
                    return this.parentPE.DesignerHost;
                }
                return null;
            }
            set
            {
                if (this.parentPE != null)
                {
                    this.parentPE.DesignerHost = value;
                }
            }
        }

        internal bool Disposed
        {
            get
            {
                return this.GetFlagSet(0x2000);
            }
        }

        internal virtual bool Enumerable
        {
            get
            {
                return ((this.Flags & 2) != 0);
            }
        }

        public override bool Expandable
        {
            get
            {
                bool flagSet = this.GetFlagSet(0x20000);
                if ((flagSet && (this.childCollection != null)) && (this.childCollection.Count > 0))
                {
                    return true;
                }
                if (this.GetFlagSet(0x80000))
                {
                    return false;
                }
                if ((flagSet && ((this.cacheItems == null) || (this.cacheItems.lastValue == null))) && (this.PropertyValue == null))
                {
                    flagSet = false;
                }
                return flagSet;
            }
        }

        public override bool Expanded
        {
            get
            {
                return this.InternalExpanded;
            }
            set
            {
                this.GridEntryHost.SetExpand(this, value);
            }
        }

        internal virtual int Flags
        {
            get
            {
                if ((this.flags & -2147483648) == 0)
                {
                    this.flags |= -2147483648;
                    System.ComponentModel.TypeConverter typeConverter = this.TypeConverter;
                    System.Drawing.Design.UITypeEditor uITypeEditor = this.UITypeEditor;
                    object instance = this.Instance;
                    bool forceReadOnly = this.ForceReadOnly;
                    if (instance != null)
                    {
                        forceReadOnly |= TypeDescriptor.GetAttributes(instance).Contains(InheritanceAttribute.InheritedReadOnly);
                    }
                    if (typeConverter.GetStandardValuesSupported(this))
                    {
                        this.flags |= 2;
                    }
                    if ((!forceReadOnly && typeConverter.CanConvertFrom(this, typeof(string))) && !typeConverter.GetStandardValuesExclusive(this))
                    {
                        this.flags |= 1;
                    }
                    bool flag2 = TypeDescriptor.GetAttributes(this.PropertyType)[typeof(ImmutableObjectAttribute)].Equals(ImmutableObjectAttribute.Yes);
                    bool flag3 = flag2 || typeConverter.GetCreateInstanceSupported(this);
                    if (flag3)
                    {
                        this.flags |= 0x200;
                    }
                    if (typeConverter.GetPropertiesSupported(this))
                    {
                        this.flags |= 0x20000;
                        if ((!forceReadOnly && ((this.flags & 1) == 0)) && !flag2)
                        {
                            this.flags |= 0x80;
                        }
                    }
                    if (this.Attributes.Contains(PasswordPropertyTextAttribute.Yes))
                    {
                        this.flags |= 0x1000;
                    }
                    if (uITypeEditor != null)
                    {
                        if (uITypeEditor.GetPaintValueSupported(this))
                        {
                            this.flags |= 4;
                        }
                        if (!forceReadOnly)
                        {
                            switch (uITypeEditor.GetEditStyle(this))
                            {
                                case UITypeEditorEditStyle.Modal:
                                    this.flags |= 0x10;
                                    if (!flag3 && !this.PropertyType.IsValueType)
                                    {
                                        this.flags |= 0x80;
                                    }
                                    break;

                                case UITypeEditorEditStyle.DropDown:
                                    this.flags |= 0x20;
                                    break;
                            }
                        }
                    }
                }
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        public bool Focus
        {
            get
            {
                return this.hasFocus;
            }
            set
            {
                if (!this.Disposed)
                {
                    if (this.cacheItems != null)
                    {
                        this.cacheItems.lastValueString = null;
                        this.cacheItems.useValueString = false;
                        this.cacheItems.useShouldSerialize = false;
                    }
                    if (this.hasFocus != value)
                    {
                        this.hasFocus = value;
                        if (value)
                        {
                            int childID = this.GridEntryHost.AccessibilityGetGridEntryChildID(this);
                            if (childID >= 0)
                            {
                                PropertyGridView.PropertyGridViewAccessibleObject accessibilityObject = (PropertyGridView.PropertyGridViewAccessibleObject) this.GridEntryHost.AccessibilityObject;
                                accessibilityObject.NotifyClients(AccessibleEvents.Focus, childID);
                                accessibilityObject.NotifyClients(AccessibleEvents.Selection, childID);
                            }
                        }
                    }
                }
            }
        }

        internal virtual bool ForceReadOnly
        {
            get
            {
                return ((this.flags & 0x400) != 0);
            }
        }

        public string FullLabel
        {
            get
            {
                string fullLabel = null;
                if (this.parentPE != null)
                {
                    fullLabel = this.parentPE.FullLabel;
                }
                if (fullLabel != null)
                {
                    fullLabel = fullLabel + ".";
                }
                else
                {
                    fullLabel = "";
                }
                return (fullLabel + this.PropertyLabel);
            }
        }

        internal virtual PropertyGridView GridEntryHost
        {
            get
            {
                if (this.parentPE != null)
                {
                    return this.parentPE.GridEntryHost;
                }
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override GridItemCollection GridItems
        {
            get
            {
                if (this.Disposed)
                {
                    throw new ObjectDisposedException(System.Windows.Forms.SR.GetString("GridItemDisposed"));
                }
                if ((this.IsExpandable && (this.childCollection != null)) && (this.childCollection.Count == 0))
                {
                    this.CreateChildren();
                }
                return this.Children;
            }
        }

        public override System.Windows.Forms.GridItemType GridItemType
        {
            get
            {
                return System.Windows.Forms.GridItemType.Property;
            }
        }

        internal virtual bool HasValue
        {
            get
            {
                return true;
            }
        }

        public virtual string HelpKeyword
        {
            get
            {
                string helpKeyword = null;
                if (this.parentPE != null)
                {
                    helpKeyword = this.parentPE.HelpKeyword;
                }
                if (helpKeyword == null)
                {
                    helpKeyword = string.Empty;
                }
                return helpKeyword;
            }
        }

        internal virtual string HelpKeywordInternal
        {
            get
            {
                return this.HelpKeyword;
            }
        }

        public virtual object Instance
        {
            get
            {
                object valueOwner = this.GetValueOwner();
                if ((this.parentPE != null) && (valueOwner == null))
                {
                    return this.parentPE.Instance;
                }
                return valueOwner;
            }
        }

        internal virtual bool InternalExpanded
        {
            get
            {
                return (((this.childCollection != null) && (this.childCollection.Count != 0)) && this.GetFlagSet(0x10000));
            }
            set
            {
                if (this.Expandable && (value != this.InternalExpanded))
                {
                    if ((this.childCollection != null) && (this.childCollection.Count > 0))
                    {
                        this.SetFlag(0x10000, value);
                    }
                    else
                    {
                        this.SetFlag(0x10000, false);
                        if (value)
                        {
                            bool fVal = this.CreateChildren();
                            this.SetFlag(0x10000, fVal);
                        }
                    }
                }
            }
        }

        public virtual bool IsCustomPaint
        {
            get
            {
                if ((this.flags & -2147483648) == 0)
                {
                    System.Drawing.Design.UITypeEditor uITypeEditor = this.UITypeEditor;
                    if (uITypeEditor != null)
                    {
                        if (((this.flags & 4) != 0) || ((this.flags & 0x100000) != 0))
                        {
                            return ((this.flags & 4) != 0);
                        }
                        if (uITypeEditor.GetPaintValueSupported(this))
                        {
                            this.flags |= 4;
                            return true;
                        }
                        this.flags |= 0x100000;
                        return false;
                    }
                }
                return ((this.Flags & 4) != 0);
            }
        }

        public virtual bool IsExpandable
        {
            get
            {
                return this.Expandable;
            }
            set
            {
                if (value != this.GetFlagSet(0x20000))
                {
                    this.SetFlag(0x80000, false);
                    this.SetFlag(0x20000, value);
                }
            }
        }

        public virtual bool IsTextEditable
        {
            get
            {
                return (this.IsValueEditable && ((this.Flags & 1) != 0));
            }
        }

        public virtual bool IsValueEditable
        {
            get
            {
                return (!this.ForceReadOnly && (0 != (this.Flags & 0x33)));
            }
        }

        public override string Label
        {
            get
            {
                return this.PropertyLabel;
            }
        }

        protected virtual Color LabelTextColor
        {
            get
            {
                if (this.ShouldRenderReadOnly)
                {
                    return this.GridEntryHost.GrayTextColor;
                }
                return this.GridEntryHost.GetTextColor();
            }
        }

        internal virtual string LabelToolTipText
        {
            get
            {
                return this.PropertyLabel;
            }
        }

        public virtual bool NeedsCustomEditorButton
        {
            get
            {
                if ((this.Flags & 0x10) == 0)
                {
                    return false;
                }
                if (!this.IsValueEditable)
                {
                    return ((this.Flags & 0x80) != 0);
                }
                return true;
            }
        }

        public virtual bool NeedsDropDownButton
        {
            get
            {
                return ((this.Flags & 0x20) != 0);
            }
        }

        public Rectangle OutlineRect
        {
            get
            {
                if (this.outlineRect.IsEmpty)
                {
                    PropertyGridView gridEntryHost = this.GridEntryHost;
                    int outlineIconSize = gridEntryHost.GetOutlineIconSize();
                    int num2 = outlineIconSize + 5;
                    int x = (this.propertyDepth * num2) + 2;
                    int y = (gridEntryHost.GetGridEntryHeight() - outlineIconSize) / 2;
                    this.outlineRect = new Rectangle(x, y, outlineIconSize, outlineIconSize);
                }
                return this.outlineRect;
            }
        }

        public PropertyGrid OwnerGrid
        {
            get
            {
                return this.ownerGrid;
            }
        }

        public override GridItem Parent
        {
            get
            {
                if (this.Disposed)
                {
                    throw new ObjectDisposedException(System.Windows.Forms.SR.GetString("GridItemDisposed"));
                }
                return this.ParentGridEntry;
            }
        }

        public virtual GridEntry ParentGridEntry
        {
            get
            {
                return this.parentPE;
            }
            set
            {
                this.parentPE = value;
                if (value != null)
                {
                    this.propertyDepth = value.PropertyDepth + 1;
                    if (this.childCollection != null)
                    {
                        for (int i = 0; i < this.childCollection.Count; i++)
                        {
                            this.childCollection.GetEntry(i).ParentGridEntry = this;
                        }
                    }
                }
            }
        }

        public virtual string PropertyCategory
        {
            get
            {
                return CategoryAttribute.Default.Category;
            }
        }

        public virtual int PropertyDepth
        {
            get
            {
                return this.propertyDepth;
            }
        }

        public virtual string PropertyDescription
        {
            get
            {
                return null;
            }
        }

        public override System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return null;
            }
        }

        public virtual string PropertyLabel
        {
            get
            {
                return null;
            }
        }

        internal virtual int PropertyLabelIndent
        {
            get
            {
                int num = this.GridEntryHost.GetOutlineIconSize() + 5;
                return (((this.propertyDepth + 1) * num) + 1);
            }
        }

        public virtual string PropertyName
        {
            get
            {
                return this.PropertyLabel;
            }
        }

        public virtual System.Type PropertyType
        {
            get
            {
                object propertyValue = this.PropertyValue;
                if (propertyValue != null)
                {
                    return propertyValue.GetType();
                }
                return null;
            }
        }

        public virtual object PropertyValue
        {
            get
            {
                if (this.cacheItems != null)
                {
                    return this.cacheItems.lastValue;
                }
                return null;
            }
            set
            {
            }
        }

        public virtual bool ShouldRenderPassword
        {
            get
            {
                return ((this.Flags & 0x1000) != 0);
            }
        }

        public virtual bool ShouldRenderReadOnly
        {
            get
            {
                return (this.ForceReadOnly || (((this.Flags & 0x100) != 0) || (!this.IsValueEditable && (0 == (this.Flags & 0x80)))));
            }
        }

        internal virtual System.ComponentModel.TypeConverter TypeConverter
        {
            get
            {
                if (this.converter == null)
                {
                    object propertyValue = this.PropertyValue;
                    if (propertyValue == null)
                    {
                        this.converter = TypeDescriptor.GetConverter(this.PropertyType);
                    }
                    else
                    {
                        this.converter = TypeDescriptor.GetConverter(propertyValue);
                    }
                }
                return this.converter;
            }
        }

        internal virtual System.Drawing.Design.UITypeEditor UITypeEditor
        {
            get
            {
                if ((this.editor == null) && (this.PropertyType != null))
                {
                    this.editor = (System.Drawing.Design.UITypeEditor) TypeDescriptor.GetEditor(this.PropertyType, typeof(System.Drawing.Design.UITypeEditor));
                }
                return this.editor;
            }
        }

        public override object Value
        {
            get
            {
                return this.PropertyValue;
            }
        }

        internal Point ValueToolTipLocation
        {
            get
            {
                if (!this.ShouldRenderPassword)
                {
                    return this.valueTipPoint;
                }
                return InvalidPoint;
            }
            set
            {
                this.valueTipPoint = value;
            }
        }

        internal int VisibleChildCount
        {
            get
            {
                if (!this.Expanded)
                {
                    return 0;
                }
                int childCount = this.ChildCount;
                int num2 = childCount;
                for (int i = 0; i < childCount; i++)
                {
                    num2 += this.ChildCollection.GetEntry(i).VisibleChildCount;
                }
                return num2;
            }
        }

        private class CacheItems
        {
            public string lastLabel;
            public Font lastLabelFont;
            public int lastLabelWidth;
            public bool lastShouldSerialize;
            public object lastValue;
            public Font lastValueFont;
            public string lastValueString;
            public int lastValueTextWidth;
            public bool useCompatTextRendering;
            public bool useShouldSerialize;
            public bool useValueString;
        }

        public class DisplayNameSortComparer : IComparer
        {
            public int Compare(object left, object right)
            {
                return string.Compare(((PropertyDescriptor) left).DisplayName, ((PropertyDescriptor) right).DisplayName, true, CultureInfo.CurrentCulture);
            }
        }

        private sealed class EventEntry
        {
            internal Delegate handler;
            internal object key;
            internal GridEntry.EventEntry next;

            internal EventEntry(GridEntry.EventEntry next, object key, Delegate handler)
            {
                this.next = next;
                this.key = key;
                this.handler = handler;
            }
        }

        [ComVisible(true)]
        public class GridEntryAccessibleObject : AccessibleObject
        {
            private GridEntry owner;

            public GridEntryAccessibleObject(GridEntry owner)
            {
                this.owner = owner;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void DoDefaultAction()
            {
                this.owner.OnOutlineClick(EventArgs.Empty);
            }

            public override AccessibleObject GetFocused()
            {
                if (this.owner.Focus)
                {
                    return this;
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                System.Windows.Forms.PropertyGridInternal.PropertyGridView.PropertyGridViewAccessibleObject parent = (System.Windows.Forms.PropertyGridInternal.PropertyGridView.PropertyGridViewAccessibleObject) this.Parent;
                switch (navdir)
                {
                    case AccessibleNavigation.Up:
                    case AccessibleNavigation.Left:
                    case AccessibleNavigation.Previous:
                        return parent.Previous(this.owner);

                    case AccessibleNavigation.Down:
                    case AccessibleNavigation.Right:
                    case AccessibleNavigation.Next:
                        return parent.Next(this.owner);
                }
                return null;
            }

            [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            public override void Select(AccessibleSelection flags)
            {
                if (this.PropertyGridView.InvokeRequired)
                {
                    this.PropertyGridView.Invoke(new SelectDelegate(this.Select), new object[] { flags });
                }
                else
                {
                    if ((flags & AccessibleSelection.TakeFocus) == AccessibleSelection.TakeFocus)
                    {
                        this.PropertyGridView.FocusInternal();
                    }
                    if ((flags & AccessibleSelection.TakeSelection) == AccessibleSelection.TakeSelection)
                    {
                        this.PropertyGridView.AccessibilitySelect(this.owner);
                    }
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    return this.PropertyGridView.AccessibilityGetGridEntryBounds(this.owner);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    if (!this.owner.Expandable)
                    {
                        return base.DefaultAction;
                    }
                    if (this.owner.Expanded)
                    {
                        return System.Windows.Forms.SR.GetString("AccessibleActionCollapse");
                    }
                    return System.Windows.Forms.SR.GetString("AccessibleActionExpand");
                }
            }

            public override string Description
            {
                get
                {
                    return this.owner.PropertyDescription;
                }
            }

            public override string Name
            {
                get
                {
                    return this.owner.PropertyLabel;
                }
            }

            public override AccessibleObject Parent
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.owner.GridEntryHost.AccessibilityObject;
                }
            }

            private System.Windows.Forms.PropertyGridInternal.PropertyGridView PropertyGridView
            {
                get
                {
                    return (System.Windows.Forms.PropertyGridInternal.PropertyGridView) ((System.Windows.Forms.PropertyGridInternal.PropertyGridView.PropertyGridViewAccessibleObject) this.Parent).Owner;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Row;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates states = AccessibleStates.Selectable | AccessibleStates.Focusable;
                    if (this.owner.Focus)
                    {
                        states |= AccessibleStates.Focused;
                    }
                    System.Windows.Forms.PropertyGridInternal.PropertyGridView.PropertyGridViewAccessibleObject parent = (System.Windows.Forms.PropertyGridInternal.PropertyGridView.PropertyGridViewAccessibleObject) this.Parent;
                    if (parent.GetSelected() == this)
                    {
                        states |= AccessibleStates.Selected;
                    }
                    if (this.owner.Expandable)
                    {
                        if (this.owner.Expanded)
                        {
                            states |= AccessibleStates.Expanded;
                        }
                        else
                        {
                            states |= AccessibleStates.Collapsed;
                        }
                    }
                    if (this.owner.ShouldRenderReadOnly)
                    {
                        states |= AccessibleStates.ReadOnly;
                    }
                    if (this.owner.ShouldRenderPassword)
                    {
                        states |= AccessibleStates.Protected;
                    }
                    return states;
                }
            }

            public override string Value
            {
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                get
                {
                    return this.owner.GetPropertyTextValue();
                }
                [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
                set
                {
                    this.owner.SetPropertyTextValue(value);
                }
            }

            private delegate void SelectDelegate(AccessibleSelection flags);
        }

        [Flags]
        internal enum PaintValueFlags
        {
            CheckShouldSerialize = 4,
            DrawSelected = 1,
            FetchValue = 2,
            None = 0,
            PaintInPlace = 8
        }
    }
}

