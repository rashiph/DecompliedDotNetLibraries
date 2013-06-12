namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    internal class Com2PropertyDescriptor : PropertyDescriptor, ICloneable
    {
        private Attribute[] baseAttrs;
        private bool baseReadOnly;
        private bool canShow;
        private Com2Properties com2props;
        private TypeConverter converter;
        private int dispid;
        private string displayName;
        private object editor;
        private static readonly object EventCanResetValue = new object();
        private static readonly object EventGetBaseAttributes = new object();
        private static readonly object EventGetDisplayName = new object();
        private static readonly object EventGetDisplayValue = new object();
        private static readonly object EventGetDynamicAttributes = new object();
        private static readonly object EventGetIsReadOnly = new object();
        private static readonly object EventGetTypeConverterAndTypeEditor = new object();
        private static readonly object EventResetValue = new object();
        private EventHandlerList events;
        private static readonly object EventShouldRefresh = new object();
        private static readonly object EventShouldSerializeValue = new object();
        private static readonly Guid GUID_COLOR = new Guid("{66504301-BE0F-101A-8BBB-00AA00300CAB}");
        private bool hrHidden;
        private bool inAttrQuery;
        private object lastValue;
        private static IDictionary oleConverters = new SortedList();
        private System.Type propertyType;
        private bool queryRefresh;
        private bool readOnly;
        private int refreshState;
        private object typeData;
        private bool typeHide;
        private Com2DataTypeToManagedDataTypeConverter valueConverter;

        public event GetBoolValueEventHandler QueryCanResetValue
        {
            add
            {
                this.Events.AddHandler(EventCanResetValue, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventCanResetValue, value);
            }
        }

        public event GetAttributesEventHandler QueryGetBaseAttributes
        {
            add
            {
                this.Events.AddHandler(EventGetBaseAttributes, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventGetBaseAttributes, value);
            }
        }

        public event GetNameItemEventHandler QueryGetDisplayName
        {
            add
            {
                this.Events.AddHandler(EventGetDisplayName, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventGetDisplayName, value);
            }
        }

        public event GetNameItemEventHandler QueryGetDisplayValue
        {
            add
            {
                this.Events.AddHandler(EventGetDisplayValue, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventGetDisplayValue, value);
            }
        }

        public event GetAttributesEventHandler QueryGetDynamicAttributes
        {
            add
            {
                this.Events.AddHandler(EventGetDynamicAttributes, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventGetDynamicAttributes, value);
            }
        }

        public event GetBoolValueEventHandler QueryGetIsReadOnly
        {
            add
            {
                this.Events.AddHandler(EventGetIsReadOnly, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventGetIsReadOnly, value);
            }
        }

        public event GetTypeConverterAndTypeEditorEventHandler QueryGetTypeConverterAndTypeEditor
        {
            add
            {
                this.Events.AddHandler(EventGetTypeConverterAndTypeEditor, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventGetTypeConverterAndTypeEditor, value);
            }
        }

        public event Com2EventHandler QueryResetValue
        {
            add
            {
                this.Events.AddHandler(EventResetValue, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventResetValue, value);
            }
        }

        public event GetBoolValueEventHandler QueryShouldSerializeValue
        {
            add
            {
                this.Events.AddHandler(EventShouldSerializeValue, value);
            }
            remove
            {
                this.Events.RemoveHandler(EventShouldSerializeValue, value);
            }
        }

        static Com2PropertyDescriptor()
        {
            oleConverters[GUID_COLOR] = typeof(Com2ColorConverter);
            oleConverters[typeof(SafeNativeMethods.IFontDisp).GUID] = typeof(Com2FontConverter);
            oleConverters[typeof(UnsafeNativeMethods.IFont).GUID] = typeof(Com2FontConverter);
            oleConverters[typeof(UnsafeNativeMethods.IPictureDisp).GUID] = typeof(Com2PictureConverter);
            oleConverters[typeof(UnsafeNativeMethods.IPicture).GUID] = typeof(Com2PictureConverter);
        }

        public Com2PropertyDescriptor(int dispid, string name, Attribute[] attrs, bool readOnly, System.Type propType, object typeData, bool hrHidden) : base(name, attrs)
        {
            this.baseReadOnly = readOnly;
            this.readOnly = readOnly;
            this.baseAttrs = attrs;
            this.SetNeedsRefresh(0x8000, true);
            this.hrHidden = hrHidden;
            this.SetNeedsRefresh(4, readOnly);
            this.propertyType = propType;
            this.dispid = dispid;
            if (typeData != null)
            {
                this.typeData = typeData;
                if (typeData is Com2Enum)
                {
                    this.converter = new Com2EnumConverter((Com2Enum) typeData);
                }
                else if (typeData is Guid)
                {
                    this.valueConverter = this.CreateOleTypeConverter((System.Type) oleConverters[(Guid) typeData]);
                }
            }
            this.canShow = true;
            if (attrs != null)
            {
                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i].Equals(BrowsableAttribute.No) && !hrHidden)
                    {
                        this.canShow = false;
                        break;
                    }
                }
            }
            if (this.canShow && ((propType == typeof(object)) || ((this.valueConverter == null) && (propType == typeof(UnsafeNativeMethods.IDispatch)))))
            {
                this.typeHide = true;
            }
        }

        public override bool CanResetValue(object component)
        {
            if (component is ICustomTypeDescriptor)
            {
                component = ((ICustomTypeDescriptor) component).GetPropertyOwner(this);
            }
            if (component == this.TargetObject)
            {
                GetBoolValueEvent gvbe = new GetBoolValueEvent(false);
                this.OnCanResetValue(gvbe);
                return gvbe.Value;
            }
            return false;
        }

        public object Clone()
        {
            return new Com2PropertyDescriptor(this.dispid, this.Name, (Attribute[]) this.baseAttrs.Clone(), this.readOnly, this.propertyType, this.typeData, this.hrHidden);
        }

        protected override AttributeCollection CreateAttributeCollection()
        {
            return new AttributeCollection(this.AttributeArray);
        }

        private Com2DataTypeToManagedDataTypeConverter CreateOleTypeConverter(System.Type t)
        {
            if (t == null)
            {
                return null;
            }
            ConstructorInfo constructor = t.GetConstructor(new System.Type[] { typeof(Com2PropertyDescriptor) });
            if (constructor != null)
            {
                return (Com2DataTypeToManagedDataTypeConverter) constructor.Invoke(new object[] { this });
            }
            return (Com2DataTypeToManagedDataTypeConverter) Activator.CreateInstance(t);
        }

        private TypeConverter GetBaseTypeConverter()
        {
            if (this.PropertyType == null)
            {
                return new TypeConverter();
            }
            TypeConverter converter = null;
            TypeConverterAttribute attribute = (TypeConverterAttribute) this.Attributes[typeof(TypeConverterAttribute)];
            if (attribute != null)
            {
                string converterTypeName = attribute.ConverterTypeName;
                if ((converterTypeName != null) && (converterTypeName.Length > 0))
                {
                    System.Type c = System.Type.GetType(converterTypeName);
                    if ((c != null) && typeof(TypeConverter).IsAssignableFrom(c))
                    {
                        try
                        {
                            converter = (TypeConverter) Activator.CreateInstance(c);
                            if (converter != null)
                            {
                                this.refreshState |= 0x2000;
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            if (converter == null)
            {
                if (!typeof(UnsafeNativeMethods.IDispatch).IsAssignableFrom(this.PropertyType))
                {
                    converter = base.Converter;
                }
                else
                {
                    converter = new Com2IDispatchConverter(this, false);
                }
            }
            if (converter == null)
            {
                converter = new TypeConverter();
            }
            return converter;
        }

        private object GetBaseTypeEditor(System.Type editorBaseType)
        {
            if (this.PropertyType == null)
            {
                return null;
            }
            object editor = null;
            EditorAttribute attribute = (EditorAttribute) this.Attributes[typeof(EditorAttribute)];
            if (attribute != null)
            {
                string editorBaseTypeName = attribute.EditorBaseTypeName;
                if ((editorBaseTypeName != null) && (editorBaseTypeName.Length > 0))
                {
                    System.Type type = System.Type.GetType(editorBaseTypeName);
                    if ((type != null) && (type == editorBaseType))
                    {
                        System.Type type2 = System.Type.GetType(attribute.EditorTypeName);
                        if (type2 != null)
                        {
                            try
                            {
                                editor = Activator.CreateInstance(type2);
                                if (editor != null)
                                {
                                    this.refreshState |= 0x4000;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            if (editor == null)
            {
                editor = base.GetEditor(editorBaseType);
            }
            return editor;
        }

        public virtual string GetDisplayValue(string defaultValue)
        {
            GetNameItemEvent gnie = new GetNameItemEvent(defaultValue);
            this.OnGetDisplayValue(gnie);
            return ((gnie.Name == null) ? null : gnie.Name.ToString());
        }

        public override object GetEditor(System.Type editorBaseType)
        {
            if (!this.TypeEditorValid)
            {
                if (this.PropertyType == null)
                {
                    return null;
                }
                if (editorBaseType == typeof(UITypeEditor))
                {
                    TypeConverter typeConverter = null;
                    this.GetTypeConverterAndTypeEditor(ref typeConverter, editorBaseType, ref this.editor);
                    if (!this.TypeConverterValid)
                    {
                        this.converter = typeConverter;
                        this.SetNeedsRefresh(0x20, false);
                    }
                    this.SetNeedsRefresh(0x40, false);
                }
                else
                {
                    this.editor = base.GetEditor(editorBaseType);
                }
            }
            return this.editor;
        }

        public object GetNativeValue(object component)
        {
            if (component == null)
            {
                return null;
            }
            if (component is ICustomTypeDescriptor)
            {
                component = ((ICustomTypeDescriptor) component).GetPropertyOwner(this);
            }
            if (((component == null) || !Marshal.IsComObject(component)) || !(component is UnsafeNativeMethods.IDispatch))
            {
                return null;
            }
            UnsafeNativeMethods.IDispatch dispatch = (UnsafeNativeMethods.IDispatch) component;
            object[] pVarResult = new object[1];
            System.Windows.Forms.NativeMethods.tagEXCEPINFO pExcepInfo = new System.Windows.Forms.NativeMethods.tagEXCEPINFO();
            Guid empty = Guid.Empty;
            int errorCode = dispatch.Invoke(this.dispid, ref empty, SafeNativeMethods.GetThreadLCID(), 2, new System.Windows.Forms.NativeMethods.tagDISPPARAMS(), pVarResult, pExcepInfo, null);
            switch (errorCode)
            {
                case 0:
                case 1:
                    if ((pVarResult[0] != null) && !Convert.IsDBNull(pVarResult[0]))
                    {
                        this.lastValue = pVarResult[0];
                        break;
                    }
                    this.lastValue = null;
                    break;

                case -2147352567:
                    return null;

                default:
                    throw new ExternalException(System.Windows.Forms.SR.GetString("DispInvokeFailed", new object[] { "GetValue", errorCode }), errorCode);
            }
            return this.lastValue;
        }

        private bool GetNeedsRefresh(int mask)
        {
            return ((this.refreshState & mask) != 0);
        }

        public void GetTypeConverterAndTypeEditor(ref TypeConverter typeConverter, System.Type editorBaseType, ref object typeEditor)
        {
            TypeConverter currentConverter = typeConverter;
            object currentEditor = typeEditor;
            if (currentConverter == null)
            {
                currentConverter = this.GetBaseTypeConverter();
            }
            if (currentEditor == null)
            {
                currentEditor = this.GetBaseTypeEditor(editorBaseType);
            }
            if (((this.refreshState & 0x2000) == 0) && (this.PropertyType == typeof(Com2Variant)))
            {
                System.Type propertyType = this.PropertyType;
                object propertyValue = this.GetValue(this.TargetObject);
                if (propertyValue != null)
                {
                    propertyValue.GetType();
                }
                ComNativeDescriptor.ResolveVariantTypeConverterAndTypeEditor(propertyValue, ref currentConverter, editorBaseType, ref currentEditor);
            }
            if (currentConverter is Com2PropDescMainConverter)
            {
                currentConverter = ((Com2PropDescMainConverter) currentConverter).InnerConverter;
            }
            GetTypeConverterAndTypeEditorEvent e = new GetTypeConverterAndTypeEditorEvent(currentConverter, currentEditor);
            this.OnGetTypeConverterAndTypeEditor(e);
            currentConverter = e.TypeConverter;
            currentEditor = e.TypeEditor;
            if (currentConverter == null)
            {
                currentConverter = this.GetBaseTypeConverter();
            }
            if (currentEditor == null)
            {
                currentEditor = this.GetBaseTypeEditor(editorBaseType);
            }
            System.Type type = currentConverter.GetType();
            if ((type != typeof(TypeConverter)) && (type != typeof(Com2PropDescMainConverter)))
            {
                currentConverter = new Com2PropDescMainConverter(this, currentConverter);
            }
            typeConverter = currentConverter;
            typeEditor = currentEditor;
        }

        public override object GetValue(object component)
        {
            this.lastValue = this.GetNativeValue(component);
            if (this.ConvertingNativeType && (this.lastValue != null))
            {
                this.lastValue = this.valueConverter.ConvertNativeToManaged(this.lastValue, this);
            }
            else if (((this.lastValue != null) && (this.propertyType != null)) && (this.propertyType.IsEnum && this.lastValue.GetType().IsPrimitive))
            {
                try
                {
                    this.lastValue = Enum.ToObject(this.propertyType, this.lastValue);
                }
                catch
                {
                }
            }
            return this.lastValue;
        }

        public bool IsCurrentValue(object value)
        {
            return ((value == this.lastValue) || ((this.lastValue != null) && this.lastValue.Equals(value)));
        }

        protected void OnCanResetValue(GetBoolValueEvent gvbe)
        {
            this.RaiseGetBoolValueEvent(EventCanResetValue, gvbe);
        }

        protected void OnGetBaseAttributes(GetAttributesEvent e)
        {
            try
            {
                this.com2props.AlwaysValid = this.com2props.CheckValid();
                GetAttributesEventHandler handler = (GetAttributesEventHandler) this.Events[EventGetBaseAttributes];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            finally
            {
                this.com2props.AlwaysValid = false;
            }
        }

        protected void OnGetDisplayName(GetNameItemEvent gnie)
        {
            this.RaiseGetNameItemEvent(EventGetDisplayName, gnie);
        }

        protected void OnGetDisplayValue(GetNameItemEvent gnie)
        {
            this.RaiseGetNameItemEvent(EventGetDisplayValue, gnie);
        }

        protected void OnGetDynamicAttributes(GetAttributesEvent e)
        {
            try
            {
                this.com2props.AlwaysValid = this.com2props.CheckValid();
                GetAttributesEventHandler handler = (GetAttributesEventHandler) this.Events[EventGetDynamicAttributes];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            finally
            {
                this.com2props.AlwaysValid = false;
            }
        }

        protected void OnGetIsReadOnly(GetBoolValueEvent gvbe)
        {
            this.RaiseGetBoolValueEvent(EventGetIsReadOnly, gvbe);
        }

        protected void OnGetTypeConverterAndTypeEditor(GetTypeConverterAndTypeEditorEvent e)
        {
            try
            {
                this.com2props.AlwaysValid = this.com2props.CheckValid();
                GetTypeConverterAndTypeEditorEventHandler handler = (GetTypeConverterAndTypeEditorEventHandler) this.Events[EventGetTypeConverterAndTypeEditor];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            finally
            {
                this.com2props.AlwaysValid = false;
            }
        }

        protected void OnResetValue(EventArgs e)
        {
            this.RaiseCom2Event(EventResetValue, e);
        }

        protected void OnShouldRefresh(GetRefreshStateEvent gvbe)
        {
            this.RaiseGetBoolValueEvent(EventShouldRefresh, gvbe);
        }

        protected void OnShouldSerializeValue(GetBoolValueEvent gvbe)
        {
            this.RaiseGetBoolValueEvent(EventShouldSerializeValue, gvbe);
        }

        private void RaiseCom2Event(object key, EventArgs e)
        {
            try
            {
                this.com2props.AlwaysValid = this.com2props.CheckValid();
                Com2EventHandler handler = (Com2EventHandler) this.Events[key];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            finally
            {
                this.com2props.AlwaysValid = false;
            }
        }

        private void RaiseGetBoolValueEvent(object key, GetBoolValueEvent e)
        {
            try
            {
                this.com2props.AlwaysValid = this.com2props.CheckValid();
                GetBoolValueEventHandler handler = (GetBoolValueEventHandler) this.Events[key];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            finally
            {
                this.com2props.AlwaysValid = false;
            }
        }

        private void RaiseGetNameItemEvent(object key, GetNameItemEvent e)
        {
            try
            {
                this.com2props.AlwaysValid = this.com2props.CheckValid();
                GetNameItemEventHandler handler = (GetNameItemEventHandler) this.Events[key];
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            finally
            {
                this.com2props.AlwaysValid = false;
            }
        }

        public override void ResetValue(object component)
        {
            if (component is ICustomTypeDescriptor)
            {
                component = ((ICustomTypeDescriptor) component).GetPropertyOwner(this);
            }
            if (component == this.TargetObject)
            {
                this.OnResetValue(EventArgs.Empty);
            }
        }

        internal void SetNeedsRefresh(int mask, bool value)
        {
            if (value)
            {
                this.refreshState |= mask;
            }
            else
            {
                this.refreshState &= ~mask;
            }
        }

        public override void SetValue(object component, object value)
        {
            if (this.readOnly)
            {
                throw new NotSupportedException(System.Windows.Forms.SR.GetString("COM2ReadonlyProperty", new object[] { this.Name }));
            }
            if (component is ICustomTypeDescriptor)
            {
                component = ((ICustomTypeDescriptor) component).GetPropertyOwner(this);
            }
            if (((component != null) && Marshal.IsComObject(component)) && (component is UnsafeNativeMethods.IDispatch))
            {
                if (this.valueConverter != null)
                {
                    bool cancelSet = false;
                    value = this.valueConverter.ConvertManagedToNative(value, this, ref cancelSet);
                    if (cancelSet)
                    {
                        return;
                    }
                }
                UnsafeNativeMethods.IDispatch dispatch = (UnsafeNativeMethods.IDispatch) component;
                System.Windows.Forms.NativeMethods.tagDISPPARAMS pDispParams = new System.Windows.Forms.NativeMethods.tagDISPPARAMS();
                System.Windows.Forms.NativeMethods.tagEXCEPINFO pExcepInfo = new System.Windows.Forms.NativeMethods.tagEXCEPINFO();
                pDispParams.cArgs = 1;
                pDispParams.cNamedArgs = 1;
                int[] numArray = new int[] { -3 };
                GCHandle handle = GCHandle.Alloc(numArray, GCHandleType.Pinned);
                try
                {
                    pDispParams.rgdispidNamedArgs = Marshal.UnsafeAddrOfPinnedArrayElement(numArray, 0);
                    IntPtr ptr = Marshal.AllocCoTaskMem(0x10);
                    SafeNativeMethods.VariantInit(new HandleRef(null, ptr));
                    Marshal.GetNativeVariantForObject(value, ptr);
                    pDispParams.rgvarg = ptr;
                    try
                    {
                        Guid empty = Guid.Empty;
                        int dwMessageId = dispatch.Invoke(this.dispid, ref empty, SafeNativeMethods.GetThreadLCID(), 4, pDispParams, null, pExcepInfo, new IntPtr[1]);
                        string message = null;
                        if ((dwMessageId == -2147352567) && (pExcepInfo.scode != 0))
                        {
                            dwMessageId = pExcepInfo.scode;
                            message = pExcepInfo.bstrDescription;
                        }
                        switch (dwMessageId)
                        {
                            case -2147467260:
                            case -2147221492:
                                return;

                            case 0:
                            case 1:
                                this.OnValueChanged(component, EventArgs.Empty);
                                this.lastValue = value;
                                return;
                        }
                        if (dispatch is UnsafeNativeMethods.ISupportErrorInfo)
                        {
                            empty = typeof(UnsafeNativeMethods.IDispatch).GUID;
                            if (System.Windows.Forms.NativeMethods.Succeeded(((UnsafeNativeMethods.ISupportErrorInfo) dispatch).InterfaceSupportsErrorInfo(ref empty)))
                            {
                                UnsafeNativeMethods.IErrorInfo errorInfo = null;
                                UnsafeNativeMethods.GetErrorInfo(0, ref errorInfo);
                                if ((errorInfo != null) && System.Windows.Forms.NativeMethods.Succeeded(errorInfo.GetDescription(null)))
                                {
                                    message = pBstrDescription;
                                }
                            }
                        }
                        else if (message == null)
                        {
                            StringBuilder lpBuffer = new StringBuilder(0x100);
                            if (SafeNativeMethods.FormatMessage(0x1200, System.Windows.Forms.NativeMethods.NullHandleRef, dwMessageId, CultureInfo.CurrentCulture.LCID, lpBuffer, 0xff, System.Windows.Forms.NativeMethods.NullHandleRef) == 0)
                            {
                                message = string.Format(CultureInfo.CurrentCulture, System.Windows.Forms.SR.GetString("DispInvokeFailed", new object[] { "SetValue", dwMessageId }), new object[0]);
                            }
                            else
                            {
                                message = lpBuffer.ToString();
                                while (((message.Length > 0) && (message[message.Length - 1] == '\n')) || (message[message.Length - 1] == '\r'))
                                {
                                    message = message.Substring(0, message.Length - 1);
                                }
                            }
                        }
                        throw new ExternalException(message, dwMessageId);
                    }
                    finally
                    {
                        SafeNativeMethods.VariantClear(new HandleRef(null, ptr));
                        Marshal.FreeCoTaskMem(ptr);
                    }
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            GetBoolValueEvent gvbe = new GetBoolValueEvent(false);
            this.OnShouldSerializeValue(gvbe);
            return gvbe.Value;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                if (!this.AttributesValid && !this.InAttrQuery)
                {
                    this.AttributeArray = this.BaseAttributes;
                    ArrayList list = null;
                    if (this.typeHide && this.canShow)
                    {
                        if (list == null)
                        {
                            list = new ArrayList(this.AttributeArray);
                        }
                        list.Add(new BrowsableAttribute(false));
                    }
                    else if (this.hrHidden)
                    {
                        object targetObject = this.TargetObject;
                        if ((targetObject != null) && System.Windows.Forms.NativeMethods.Succeeded(new ComNativeDescriptor().GetPropertyValue(targetObject, this.dispid, new object[1])))
                        {
                            if (list == null)
                            {
                                list = new ArrayList(this.AttributeArray);
                            }
                            list.Add(new BrowsableAttribute(true));
                            this.hrHidden = false;
                        }
                    }
                    this.inAttrQuery = true;
                    try
                    {
                        ArrayList attrList = new ArrayList();
                        this.OnGetDynamicAttributes(new GetAttributesEvent(attrList));
                        if ((attrList.Count != 0) && (list == null))
                        {
                            list = new ArrayList(this.AttributeArray);
                        }
                        for (int i = 0; i < attrList.Count; i++)
                        {
                            Attribute attribute = (Attribute) attrList[i];
                            list.Add(attribute);
                        }
                    }
                    finally
                    {
                        this.inAttrQuery = false;
                    }
                    this.SetNeedsRefresh(1, false);
                    if (list != null)
                    {
                        Attribute[] array = new Attribute[list.Count];
                        list.CopyTo(array, 0);
                        this.AttributeArray = array;
                    }
                }
                return base.Attributes;
            }
        }

        protected bool AttributesValid
        {
            get
            {
                bool flag = !this.GetNeedsRefresh(1);
                if (this.queryRefresh)
                {
                    GetRefreshStateEvent gvbe = new GetRefreshStateEvent(Com2ShouldRefreshTypes.Attributes, !flag);
                    this.OnShouldRefresh(gvbe);
                    flag = !gvbe.Value;
                    this.SetNeedsRefresh(1, gvbe.Value);
                }
                return flag;
            }
        }

        protected Attribute[] BaseAttributes
        {
            get
            {
                if (this.GetNeedsRefresh(0x8000))
                {
                    this.SetNeedsRefresh(0x8000, false);
                    int num = (this.baseAttrs == null) ? 0 : this.baseAttrs.Length;
                    ArrayList attrList = new ArrayList();
                    if (num != 0)
                    {
                        attrList.AddRange(this.baseAttrs);
                    }
                    this.OnGetBaseAttributes(new GetAttributesEvent(attrList));
                    if (attrList.Count != num)
                    {
                        this.baseAttrs = new Attribute[attrList.Count];
                    }
                    if (this.baseAttrs != null)
                    {
                        attrList.CopyTo(this.baseAttrs, 0);
                    }
                    else
                    {
                        this.baseAttrs = new Attribute[0];
                    }
                }
                return this.baseAttrs;
            }
            set
            {
                this.baseAttrs = value;
            }
        }

        public bool CanShow
        {
            get
            {
                return this.canShow;
            }
        }

        public override System.Type ComponentType
        {
            get
            {
                return typeof(UnsafeNativeMethods.IDispatch);
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                if (!this.TypeConverterValid)
                {
                    object typeEditor = null;
                    this.GetTypeConverterAndTypeEditor(ref this.converter, typeof(UITypeEditor), ref typeEditor);
                    if (!this.TypeEditorValid)
                    {
                        this.editor = typeEditor;
                        this.SetNeedsRefresh(0x40, false);
                    }
                    this.SetNeedsRefresh(0x20, false);
                }
                return this.converter;
            }
        }

        public bool ConvertingNativeType
        {
            get
            {
                return (this.valueConverter != null);
            }
        }

        protected virtual object DefaultValue
        {
            get
            {
                return null;
            }
        }

        public int DISPID
        {
            get
            {
                return this.dispid;
            }
        }

        public override string DisplayName
        {
            get
            {
                if (!this.DisplayNameValid)
                {
                    GetNameItemEvent gnie = new GetNameItemEvent(base.DisplayName);
                    this.OnGetDisplayName(gnie);
                    this.displayName = gnie.NameString;
                    this.SetNeedsRefresh(2, false);
                }
                return this.displayName;
            }
        }

        protected bool DisplayNameValid
        {
            get
            {
                bool flag = (this.displayName != null) && !this.GetNeedsRefresh(2);
                if (this.queryRefresh)
                {
                    GetRefreshStateEvent gvbe = new GetRefreshStateEvent(Com2ShouldRefreshTypes.DisplayName, !flag);
                    this.OnShouldRefresh(gvbe);
                    this.SetNeedsRefresh(2, gvbe.Value);
                    flag = !gvbe.Value;
                }
                return flag;
            }
        }

        protected EventHandlerList Events
        {
            get
            {
                if (this.events == null)
                {
                    this.events = new EventHandlerList();
                }
                return this.events;
            }
        }

        protected bool InAttrQuery
        {
            get
            {
                return this.inAttrQuery;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if (!this.ReadOnlyValid)
                {
                    this.readOnly |= this.Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
                    GetBoolValueEvent gvbe = new GetBoolValueEvent(this.readOnly);
                    this.OnGetIsReadOnly(gvbe);
                    this.readOnly = gvbe.Value;
                    this.SetNeedsRefresh(4, false);
                }
                return this.readOnly;
            }
        }

        internal Com2Properties PropertyManager
        {
            get
            {
                return this.com2props;
            }
            set
            {
                this.com2props = value;
            }
        }

        public override System.Type PropertyType
        {
            get
            {
                if (this.valueConverter != null)
                {
                    return this.valueConverter.ManagedType;
                }
                return this.propertyType;
            }
        }

        protected bool ReadOnlyValid
        {
            get
            {
                if (this.baseReadOnly)
                {
                    return true;
                }
                bool flag = !this.GetNeedsRefresh(4);
                if (this.queryRefresh)
                {
                    GetRefreshStateEvent gvbe = new GetRefreshStateEvent(Com2ShouldRefreshTypes.ReadOnly, !flag);
                    this.OnShouldRefresh(gvbe);
                    this.SetNeedsRefresh(4, gvbe.Value);
                    flag = !gvbe.Value;
                }
                return flag;
            }
        }

        public virtual object TargetObject
        {
            get
            {
                if (this.com2props != null)
                {
                    return this.com2props.TargetObject;
                }
                return null;
            }
        }

        protected bool TypeConverterValid
        {
            get
            {
                bool flag = (this.converter != null) && !this.GetNeedsRefresh(0x20);
                if (this.queryRefresh)
                {
                    GetRefreshStateEvent gvbe = new GetRefreshStateEvent(Com2ShouldRefreshTypes.TypeConverter, !flag);
                    this.OnShouldRefresh(gvbe);
                    this.SetNeedsRefresh(0x20, gvbe.Value);
                    flag = !gvbe.Value;
                }
                return flag;
            }
        }

        protected bool TypeEditorValid
        {
            get
            {
                bool flag = (this.editor != null) && !this.GetNeedsRefresh(0x40);
                if (this.queryRefresh)
                {
                    GetRefreshStateEvent gvbe = new GetRefreshStateEvent(Com2ShouldRefreshTypes.TypeEditor, !flag);
                    this.OnShouldRefresh(gvbe);
                    this.SetNeedsRefresh(0x40, gvbe.Value);
                    flag = !gvbe.Value;
                }
                return flag;
            }
        }

        private class Com2PropDescMainConverter : Com2ExtendedTypeConverter
        {
            private const int AllowSubprops = 1;
            private const int CheckSubprops = 0;
            private Com2PropertyDescriptor pd;
            private int subprops;
            private const int SupressSubprops = 2;

            public Com2PropDescMainConverter(Com2PropertyDescriptor pd, TypeConverter baseConverter) : base(baseConverter)
            {
                this.pd = pd;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                object obj2 = base.ConvertTo(context, culture, value, destinationType);
                if ((!(destinationType == typeof(string)) || !this.pd.IsCurrentValue(value)) || this.pd.PropertyType.IsEnum)
                {
                    return obj2;
                }
                Com2EnumConverter wrappedConverter = (Com2EnumConverter) base.GetWrappedConverter(typeof(Com2EnumConverter));
                if (wrappedConverter == null)
                {
                    return this.pd.GetDisplayValue((string) obj2);
                }
                return wrappedConverter.ConvertTo(value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value, attributes);
                if ((properties != null) && (properties.Count > 0))
                {
                    properties = properties.Sort();
                    PropertyDescriptor[] array = new PropertyDescriptor[properties.Count];
                    properties.CopyTo(array, 0);
                    properties = new PropertyDescriptorCollection(array, true);
                }
                return properties;
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                if (this.subprops == 0)
                {
                    if (!base.GetPropertiesSupported(context))
                    {
                        this.subprops = 2;
                    }
                    else if (((this.pd.valueConverter != null) && this.pd.valueConverter.AllowExpand) || Com2IVsPerPropertyBrowsingHandler.AllowChildProperties(this.pd))
                    {
                        this.subprops = 1;
                    }
                }
                return (this.subprops == 1);
            }
        }
    }
}

