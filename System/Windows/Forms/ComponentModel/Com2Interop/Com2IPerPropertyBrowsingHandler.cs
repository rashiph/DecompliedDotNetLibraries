namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Windows.Forms;

    [SuppressUnmanagedCodeSecurity]
    internal class Com2IPerPropertyBrowsingHandler : Com2ExtendedBrowsingHandler
    {
        internal static string GetDisplayString(System.Windows.Forms.NativeMethods.IPerPropertyBrowsing ppb, int dispid, ref bool success)
        {
            string[] pBstr = new string[1];
            if (ppb.GetDisplayString(dispid, pBstr) == 0)
            {
                success = pBstr[0] != null;
                return pBstr[0];
            }
            success = false;
            return null;
        }

        private Guid GetPropertyPageGuid(System.Windows.Forms.NativeMethods.IPerPropertyBrowsing target, int dispid)
        {
            Guid guid;
            if (target.MapPropertyToPage(dispid, out guid) == 0)
            {
                return guid;
            }
            return Guid.Empty;
        }

        private void OnGetBaseAttributes(Com2PropertyDescriptor sender, GetAttributesEvent attrEvent)
        {
            System.Windows.Forms.NativeMethods.IPerPropertyBrowsing targetObject = sender.TargetObject as System.Windows.Forms.NativeMethods.IPerPropertyBrowsing;
            if (targetObject != null)
            {
                bool flag = !Guid.Empty.Equals(this.GetPropertyPageGuid(targetObject, sender.DISPID));
                if ((sender.CanShow && flag) && typeof(UnsafeNativeMethods.IDispatch).IsAssignableFrom(sender.PropertyType))
                {
                    attrEvent.Add(BrowsableAttribute.Yes);
                }
            }
        }

        private void OnGetDisplayValue(Com2PropertyDescriptor sender, GetNameItemEvent gnievent)
        {
            try
            {
                if ((sender.TargetObject is System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) && (!(sender.Converter is Com2IPerPropertyEnumConverter) && !sender.ConvertingNativeType))
                {
                    bool success = true;
                    string str = GetDisplayString((System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) sender.TargetObject, sender.DISPID, ref success);
                    if (success)
                    {
                        gnievent.Name = str;
                    }
                }
            }
            catch
            {
            }
        }

        private void OnGetTypeConverterAndTypeEditor(Com2PropertyDescriptor sender, GetTypeConverterAndTypeEditorEvent gveevent)
        {
            if (sender.TargetObject is System.Windows.Forms.NativeMethods.IPerPropertyBrowsing)
            {
                System.Windows.Forms.NativeMethods.IPerPropertyBrowsing targetObject = (System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) sender.TargetObject;
                bool flag = false;
                System.Windows.Forms.NativeMethods.CA_STRUCT pCaStringsOut = new System.Windows.Forms.NativeMethods.CA_STRUCT();
                System.Windows.Forms.NativeMethods.CA_STRUCT pCaCookiesOut = new System.Windows.Forms.NativeMethods.CA_STRUCT();
                int errorCode = 0;
                try
                {
                    errorCode = targetObject.GetPredefinedStrings(sender.DISPID, pCaStringsOut, pCaCookiesOut);
                }
                catch (ExternalException exception)
                {
                    errorCode = exception.ErrorCode;
                }
                if (gveevent.TypeConverter is Com2IPerPropertyEnumConverter)
                {
                    gveevent.TypeConverter = null;
                }
                if (errorCode != 0)
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    OleStrCAMarshaler names = new OleStrCAMarshaler(pCaStringsOut);
                    Int32CAMarshaler values = new Int32CAMarshaler(pCaCookiesOut);
                    if ((names.Count > 0) && (values.Count > 0))
                    {
                        gveevent.TypeConverter = new Com2IPerPropertyEnumConverter(new Com2IPerPropertyBrowsingEnum(sender, this, names, values, true));
                    }
                }
                if (!flag && !sender.ConvertingNativeType)
                {
                    Guid propertyPageGuid = this.GetPropertyPageGuid(targetObject, sender.DISPID);
                    if (!Guid.Empty.Equals(propertyPageGuid))
                    {
                        gveevent.TypeEditor = new Com2PropertyPageUITypeEditor(sender, propertyPageGuid, (UITypeEditor) gveevent.TypeEditor);
                    }
                }
            }
        }

        public override void SetupPropertyHandlers(Com2PropertyDescriptor[] propDesc)
        {
            if (propDesc != null)
            {
                for (int i = 0; i < propDesc.Length; i++)
                {
                    propDesc[i].QueryGetBaseAttributes += new GetAttributesEventHandler(this.OnGetBaseAttributes);
                    propDesc[i].QueryGetDisplayValue += new GetNameItemEventHandler(this.OnGetDisplayValue);
                    propDesc[i].QueryGetTypeConverterAndTypeEditor += new GetTypeConverterAndTypeEditorEventHandler(this.OnGetTypeConverterAndTypeEditor);
                }
            }
        }

        public override System.Type Interface
        {
            get
            {
                return typeof(System.Windows.Forms.NativeMethods.IPerPropertyBrowsing);
            }
        }

        private class Com2IPerPropertyBrowsingEnum : Com2Enum
        {
            internal bool arraysFetched;
            private Com2IPerPropertyBrowsingHandler handler;
            private OleStrCAMarshaler nameMarshaller;
            internal Com2PropertyDescriptor target;
            private Int32CAMarshaler valueMarshaller;

            public Com2IPerPropertyBrowsingEnum(Com2PropertyDescriptor targetObject, Com2IPerPropertyBrowsingHandler handler, OleStrCAMarshaler names, Int32CAMarshaler values, bool allowUnknowns) : base(new string[0], new object[0], allowUnknowns)
            {
                this.target = targetObject;
                this.nameMarshaller = names;
                this.valueMarshaller = values;
                this.handler = handler;
                this.arraysFetched = false;
            }

            private void EnsureArrays()
            {
                if (!this.arraysFetched)
                {
                    this.arraysFetched = true;
                    try
                    {
                        object[] items = this.nameMarshaller.Items;
                        object[] objArray2 = this.valueMarshaller.Items;
                        System.Windows.Forms.NativeMethods.IPerPropertyBrowsing targetObject = (System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) this.target.TargetObject;
                        int length = 0;
                        if (items.Length > 0)
                        {
                            object[] sourceArray = new object[objArray2.Length];
                            System.Windows.Forms.NativeMethods.VARIANT pVarOut = new System.Windows.Forms.NativeMethods.VARIANT();
                            System.Type propertyType = this.target.PropertyType;
                            for (int i = items.Length - 1; i >= 0; i--)
                            {
                                int dwCookie = (int) objArray2[i];
                                if ((items[i] != null) && (items[i] is string))
                                {
                                    pVarOut.vt = 0;
                                    int num4 = targetObject.GetPredefinedValue(this.target.DISPID, dwCookie, pVarOut);
                                    if ((num4 == 0) && (pVarOut.vt != 0))
                                    {
                                        sourceArray[i] = pVarOut.ToObject();
                                        if (sourceArray[i].GetType() != propertyType)
                                        {
                                            if (propertyType.IsEnum)
                                            {
                                                sourceArray[i] = Enum.ToObject(propertyType, sourceArray[i]);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    sourceArray[i] = Convert.ChangeType(sourceArray[i], propertyType, CultureInfo.InvariantCulture);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }
                                    pVarOut.Clear();
                                    if (num4 == 0)
                                    {
                                        length++;
                                    }
                                    else if (length > 0)
                                    {
                                        Array.Copy(items, i, items, i + 1, length);
                                        Array.Copy(sourceArray, i, sourceArray, i + 1, length);
                                    }
                                }
                            }
                            string[] destinationArray = new string[length];
                            Array.Copy(items, 0, destinationArray, 0, length);
                            base.PopulateArrays(destinationArray, sourceArray);
                        }
                    }
                    catch (Exception)
                    {
                        base.PopulateArrays(new string[0], new object[0]);
                    }
                }
            }

            public override object FromString(string s)
            {
                this.EnsureArrays();
                return base.FromString(s);
            }

            protected override void PopulateArrays(string[] names, object[] values)
            {
            }

            public override string ToString(object v)
            {
                if (this.target.IsCurrentValue(v))
                {
                    bool success = false;
                    string str = Com2IPerPropertyBrowsingHandler.GetDisplayString((System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) this.target.TargetObject, this.target.DISPID, ref success);
                    if (success)
                    {
                        return str;
                    }
                }
                this.EnsureArrays();
                return base.ToString(v);
            }

            public override string[] Names
            {
                get
                {
                    this.EnsureArrays();
                    return base.Names;
                }
            }

            public override object[] Values
            {
                get
                {
                    this.EnsureArrays();
                    return base.Values;
                }
            }
        }

        private class Com2IPerPropertyEnumConverter : Com2EnumConverter
        {
            private Com2IPerPropertyBrowsingHandler.Com2IPerPropertyBrowsingEnum itemsEnum;

            public Com2IPerPropertyEnumConverter(Com2IPerPropertyBrowsingHandler.Com2IPerPropertyBrowsingEnum items) : base(items)
            {
                this.itemsEnum = items;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destType)
            {
                if ((destType == typeof(string)) && !this.itemsEnum.arraysFetched)
                {
                    object obj2 = this.itemsEnum.target.GetValue(this.itemsEnum.target.TargetObject);
                    if ((obj2 == value) || ((obj2 != null) && obj2.Equals(value)))
                    {
                        bool success = false;
                        string str = Com2IPerPropertyBrowsingHandler.GetDisplayString((System.Windows.Forms.NativeMethods.IPerPropertyBrowsing) this.itemsEnum.target.TargetObject, this.itemsEnum.target.DISPID, ref success);
                        if (success)
                        {
                            return str;
                        }
                    }
                }
                return base.ConvertTo(context, culture, value, destType);
            }
        }
    }
}

