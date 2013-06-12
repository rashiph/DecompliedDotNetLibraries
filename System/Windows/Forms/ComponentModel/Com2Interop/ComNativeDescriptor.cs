namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class ComNativeDescriptor : TypeDescriptionProvider
    {
        private const int CLEAR_INTERVAL = 0x19;
        private int clearCount;
        private Hashtable extendedBrowsingHandlers = new Hashtable();
        private static ComNativeDescriptor handler;
        private System.ComponentModel.WeakHashtable nativeProps = new System.ComponentModel.WeakHashtable();
        private AttributeCollection staticAttrs = new AttributeCollection(new Attribute[] { BrowsableAttribute.Yes, DesignTimeVisibleAttribute.No });

        private void CheckClear(object component)
        {
            if ((++this.clearCount % 0x19) == 0)
            {
                lock (this.nativeProps)
                {
                    Com2Properties properties;
                    this.clearCount = 0;
                    List<object> list = null;
                    foreach (DictionaryEntry entry in this.nativeProps)
                    {
                        properties = entry.Value as Com2Properties;
                        if ((properties != null) && properties.TooOld)
                        {
                            if (list == null)
                            {
                                list = new List<object>(3);
                            }
                            list.Add(entry.Key);
                        }
                    }
                    if (list != null)
                    {
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            object key = list[i];
                            properties = this.nativeProps[key] as Com2Properties;
                            if (properties != null)
                            {
                                properties.Disposed -= new EventHandler(this.OnPropsInfoDisposed);
                                properties.Dispose();
                                this.nativeProps.Remove(key);
                            }
                        }
                    }
                }
            }
        }

        internal AttributeCollection GetAttributes(object component)
        {
            ArrayList list = new ArrayList();
            if (component is System.Windows.Forms.NativeMethods.IManagedPerPropertyBrowsing)
            {
                object[] componentAttributes = Com2IManagedPerPropertyBrowsingHandler.GetComponentAttributes((System.Windows.Forms.NativeMethods.IManagedPerPropertyBrowsing) component, -1);
                for (int i = 0; i < componentAttributes.Length; i++)
                {
                    list.Add(componentAttributes[i]);
                }
            }
            if (Com2ComponentEditor.NeedsComponentEditor(component))
            {
                EditorAttribute attribute = new EditorAttribute(typeof(Com2ComponentEditor), typeof(ComponentEditor));
                list.Add(attribute);
            }
            if ((list == null) || (list.Count == 0))
            {
                return this.staticAttrs;
            }
            Attribute[] array = new Attribute[list.Count];
            list.CopyTo(array, 0);
            return new AttributeCollection(array);
        }

        internal string GetClassName(object component)
        {
            string pbstrClassName = null;
            if (((component is System.Windows.Forms.NativeMethods.IVsPerPropertyBrowsing) && System.Windows.Forms.NativeMethods.Succeeded(((System.Windows.Forms.NativeMethods.IVsPerPropertyBrowsing) component).GetClassName(ref pbstrClassName))) && (pbstrClassName != null))
            {
                return pbstrClassName;
            }
            UnsafeNativeMethods.ITypeInfo info = Com2TypeInfoProcessor.FindTypeInfo(component, true);
            if ((info != null) && (info != null))
            {
                try
                {
                    info.GetDocumentation(-1, ref pbstrClassName, null, null, null);
                    while (((pbstrClassName != null) && (pbstrClassName.Length > 0)) && (pbstrClassName[0] == '_'))
                    {
                        pbstrClassName = pbstrClassName.Substring(1);
                    }
                    return pbstrClassName;
                }
                catch
                {
                }
            }
            return "";
        }

        internal TypeConverter GetConverter(object component)
        {
            return TypeDescriptor.GetConverter(typeof(IComponent));
        }

        internal EventDescriptor GetDefaultEvent(object component)
        {
            return null;
        }

        internal PropertyDescriptor GetDefaultProperty(object component)
        {
            this.CheckClear(component);
            Com2Properties propsInfo = this.GetPropsInfo(component);
            if (propsInfo != null)
            {
                return propsInfo.DefaultProperty;
            }
            return null;
        }

        internal object GetEditor(object component, System.Type baseEditorType)
        {
            return TypeDescriptor.GetEditor(component.GetType(), baseEditorType);
        }

        internal EventDescriptorCollection GetEvents(object component)
        {
            return new EventDescriptorCollection(null);
        }

        internal EventDescriptorCollection GetEvents(object component, Attribute[] attributes)
        {
            return new EventDescriptorCollection(null);
        }

        internal string GetName(object component)
        {
            if (component is UnsafeNativeMethods.IDispatch)
            {
                int nameDispId = Com2TypeInfoProcessor.GetNameDispId((UnsafeNativeMethods.IDispatch) component);
                if (nameDispId != -1)
                {
                    bool succeeded = false;
                    object obj2 = this.GetPropertyValue(component, nameDispId, ref succeeded);
                    if (succeeded && (obj2 != null))
                    {
                        return obj2.ToString();
                    }
                }
            }
            return "";
        }

        public static object GetNativePropertyValue(object component, string propertyName, ref bool succeeded)
        {
            return Instance.GetPropertyValue(component, propertyName, ref succeeded);
        }

        internal PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            PropertyDescriptorCollection descriptors;
            Com2Properties propsInfo = this.GetPropsInfo(component);
            if (propsInfo == null)
            {
                return PropertyDescriptorCollection.Empty;
            }
            try
            {
                propsInfo.AlwaysValid = true;
                descriptors = new PropertyDescriptorCollection(propsInfo.Properties);
            }
            finally
            {
                propsInfo.AlwaysValid = false;
            }
            return descriptors;
        }

        internal int GetPropertyValue(object component, int dispid, object[] retval)
        {
            if (!(component is UnsafeNativeMethods.IDispatch))
            {
                return -2147467262;
            }
            UnsafeNativeMethods.IDispatch dispatch = (UnsafeNativeMethods.IDispatch) component;
            try
            {
                int scode;
                Guid empty = Guid.Empty;
                System.Windows.Forms.NativeMethods.tagEXCEPINFO pExcepInfo = new System.Windows.Forms.NativeMethods.tagEXCEPINFO();
                try
                {
                    scode = dispatch.Invoke(dispid, ref empty, SafeNativeMethods.GetThreadLCID(), 2, new System.Windows.Forms.NativeMethods.tagDISPPARAMS(), retval, pExcepInfo, null);
                    if (scode == -2147352567)
                    {
                        scode = pExcepInfo.scode;
                    }
                }
                catch (ExternalException exception)
                {
                    scode = exception.ErrorCode;
                }
                return scode;
            }
            catch
            {
            }
            return -2147467259;
        }

        internal object GetPropertyValue(object component, int dispid, ref bool succeeded)
        {
            if (component is UnsafeNativeMethods.IDispatch)
            {
                object[] retval = new object[1];
                if (this.GetPropertyValue(component, dispid, retval) == 0)
                {
                    succeeded = true;
                    return retval[0];
                }
                succeeded = false;
            }
            return null;
        }

        internal object GetPropertyValue(object component, string propertyName, ref bool succeeded)
        {
            if (!(component is UnsafeNativeMethods.IDispatch))
            {
                return null;
            }
            UnsafeNativeMethods.IDispatch dispatch = (UnsafeNativeMethods.IDispatch) component;
            string[] rgszNames = new string[] { propertyName };
            int[] rgDispId = new int[] { -1 };
            Guid empty = Guid.Empty;
            try
            {
                int hr = dispatch.GetIDsOfNames(ref empty, rgszNames, 1, SafeNativeMethods.GetThreadLCID(), rgDispId);
                if ((rgDispId[0] == -1) || System.Windows.Forms.NativeMethods.Failed(hr))
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
            return this.GetPropertyValue(component, rgDispId[0], ref succeeded);
        }

        private Com2Properties GetPropsInfo(object component)
        {
            this.CheckClear(component);
            Com2Properties properties = (Com2Properties) this.nativeProps[component];
            if ((properties == null) || !properties.CheckValid())
            {
                properties = Com2TypeInfoProcessor.GetProperties(component);
                if (properties != null)
                {
                    properties.Disposed += new EventHandler(this.OnPropsInfoDisposed);
                    this.nativeProps.SetWeak(component, properties);
                    properties.AddExtendedBrowsingHandlers(this.extendedBrowsingHandlers);
                }
            }
            return properties;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(System.Type objectType, object instance)
        {
            return new ComTypeDescriptor(this, instance);
        }

        internal bool IsNameDispId(object obj, int dispid)
        {
            return (((obj != null) && obj.GetType().IsCOMObject) && (dispid == Com2TypeInfoProcessor.GetNameDispId((UnsafeNativeMethods.IDispatch) obj)));
        }

        private void OnPropsInfoDisposed(object sender, EventArgs e)
        {
            Com2Properties properties = sender as Com2Properties;
            if (properties != null)
            {
                properties.Disposed -= new EventHandler(this.OnPropsInfoDisposed);
                lock (this.nativeProps)
                {
                    object targetObject = properties.TargetObject;
                    if ((targetObject == null) && this.nativeProps.ContainsValue(properties))
                    {
                        foreach (DictionaryEntry entry in this.nativeProps)
                        {
                            if (entry.Value == properties)
                            {
                                targetObject = entry.Key;
                                break;
                            }
                        }
                        if (targetObject == null)
                        {
                            return;
                        }
                    }
                    this.nativeProps.Remove(targetObject);
                }
            }
        }

        internal static void ResolveVariantTypeConverterAndTypeEditor(object propertyValue, ref TypeConverter currentConverter, System.Type editorType, ref object currentEditor)
        {
            object obj2 = propertyValue;
            if (((obj2 != null) && (obj2 != null)) && !Convert.IsDBNull(obj2))
            {
                System.Type type = obj2.GetType();
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if ((converter != null) && (converter.GetType() != typeof(TypeConverter)))
                {
                    currentConverter = converter;
                }
                object editor = TypeDescriptor.GetEditor(type, editorType);
                if (editor != null)
                {
                    currentEditor = editor;
                }
            }
        }

        internal static ComNativeDescriptor Instance
        {
            get
            {
                if (handler == null)
                {
                    handler = new ComNativeDescriptor();
                }
                return handler;
            }
        }

        private sealed class ComTypeDescriptor : ICustomTypeDescriptor
        {
            private ComNativeDescriptor _handler;
            private object _instance;

            internal ComTypeDescriptor(ComNativeDescriptor handler, object instance)
            {
                this._handler = handler;
                this._instance = instance;
            }

            AttributeCollection ICustomTypeDescriptor.GetAttributes()
            {
                return this._handler.GetAttributes(this._instance);
            }

            string ICustomTypeDescriptor.GetClassName()
            {
                return this._handler.GetClassName(this._instance);
            }

            string ICustomTypeDescriptor.GetComponentName()
            {
                return this._handler.GetName(this._instance);
            }

            TypeConverter ICustomTypeDescriptor.GetConverter()
            {
                return this._handler.GetConverter(this._instance);
            }

            EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
            {
                return this._handler.GetDefaultEvent(this._instance);
            }

            PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
            {
                return this._handler.GetDefaultProperty(this._instance);
            }

            object ICustomTypeDescriptor.GetEditor(System.Type editorBaseType)
            {
                return this._handler.GetEditor(this._instance, editorBaseType);
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
            {
                return this._handler.GetEvents(this._instance);
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
            {
                return this._handler.GetEvents(this._instance, attributes);
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
            {
                return this._handler.GetProperties(this._instance, null);
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
            {
                return this._handler.GetProperties(this._instance, attributes);
            }

            object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
            {
                return this._instance;
            }
        }
    }
}

