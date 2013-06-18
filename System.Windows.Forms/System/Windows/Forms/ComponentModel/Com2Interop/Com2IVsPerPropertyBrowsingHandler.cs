namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security;
    using System.Windows.Forms;

    [SuppressUnmanagedCodeSecurity]
    internal class Com2IVsPerPropertyBrowsingHandler : Com2ExtendedBrowsingHandler
    {
        public static bool AllowChildProperties(Com2PropertyDescriptor propDesc)
        {
            if (!(propDesc.TargetObject is NativeMethods.IVsPerPropertyBrowsing))
            {
                return false;
            }
            bool pfDisplay = false;
            return ((((NativeMethods.IVsPerPropertyBrowsing) propDesc.TargetObject).DisplayChildProperties(propDesc.DISPID, ref pfDisplay) == 0) && pfDisplay);
        }

        private void OnCanResetPropertyValue(Com2PropertyDescriptor sender, GetBoolValueEvent boolEvent)
        {
            if (sender.TargetObject is NativeMethods.IVsPerPropertyBrowsing)
            {
                NativeMethods.IVsPerPropertyBrowsing targetObject = (NativeMethods.IVsPerPropertyBrowsing) sender.TargetObject;
                bool pfCanReset = boolEvent.Value;
                if (NativeMethods.Succeeded(targetObject.CanResetPropertyValue(sender.DISPID, ref pfCanReset)))
                {
                    boolEvent.Value = pfCanReset;
                }
            }
        }

        private void OnGetBaseAttributes(Com2PropertyDescriptor sender, GetAttributesEvent attrEvent)
        {
            NativeMethods.IVsPerPropertyBrowsing targetObject = sender.TargetObject as NativeMethods.IVsPerPropertyBrowsing;
            if (targetObject != null)
            {
                string[] pbstrLocalizeDescription = new string[1];
                if ((targetObject.GetLocalizedPropertyInfo(sender.DISPID, CultureInfo.CurrentCulture.LCID, null, pbstrLocalizeDescription) == 0) && (pbstrLocalizeDescription[0] != null))
                {
                    attrEvent.Add(new DescriptionAttribute(pbstrLocalizeDescription[0]));
                }
            }
        }

        private void OnGetDisplayName(Com2PropertyDescriptor sender, GetNameItemEvent nameItem)
        {
            if (sender.TargetObject is NativeMethods.IVsPerPropertyBrowsing)
            {
                NativeMethods.IVsPerPropertyBrowsing targetObject = (NativeMethods.IVsPerPropertyBrowsing) sender.TargetObject;
                string[] pbstrLocalizedName = new string[1];
                if ((targetObject.GetLocalizedPropertyInfo(sender.DISPID, CultureInfo.CurrentCulture.LCID, pbstrLocalizedName, null) == 0) && (pbstrLocalizedName[0] != null))
                {
                    nameItem.Name = pbstrLocalizedName[0];
                }
            }
        }

        private void OnGetDynamicAttributes(Com2PropertyDescriptor sender, GetAttributesEvent attrEvent)
        {
            if (sender.TargetObject is NativeMethods.IVsPerPropertyBrowsing)
            {
                NativeMethods.IVsPerPropertyBrowsing targetObject = (NativeMethods.IVsPerPropertyBrowsing) sender.TargetObject;
                if (sender.CanShow)
                {
                    bool pfHide = sender.Attributes[typeof(BrowsableAttribute)].Equals(BrowsableAttribute.No);
                    if (targetObject.HideProperty(sender.DISPID, ref pfHide) == 0)
                    {
                        attrEvent.Add(pfHide ? BrowsableAttribute.No : BrowsableAttribute.Yes);
                    }
                }
                if (typeof(UnsafeNativeMethods.IDispatch).IsAssignableFrom(sender.PropertyType) && sender.CanShow)
                {
                    bool pfDisplay = false;
                    if ((targetObject.DisplayChildProperties(sender.DISPID, ref pfDisplay) == 0) && pfDisplay)
                    {
                        attrEvent.Add(BrowsableAttribute.Yes);
                    }
                }
            }
        }

        private void OnGetIsReadOnly(Com2PropertyDescriptor sender, GetBoolValueEvent gbvevent)
        {
            if (sender.TargetObject is NativeMethods.IVsPerPropertyBrowsing)
            {
                NativeMethods.IVsPerPropertyBrowsing targetObject = (NativeMethods.IVsPerPropertyBrowsing) sender.TargetObject;
                if (targetObject.IsPropertyReadOnly(sender.DISPID, false) == 0)
                {
                    gbvevent.Value = fReadOnly;
                }
            }
        }

        private void OnGetTypeConverterAndTypeEditor(Com2PropertyDescriptor sender, GetTypeConverterAndTypeEditorEvent gveevent)
        {
            if (((sender.TargetObject is NativeMethods.IVsPerPropertyBrowsing) && sender.CanShow) && typeof(UnsafeNativeMethods.IDispatch).IsAssignableFrom(sender.PropertyType))
            {
                NativeMethods.IVsPerPropertyBrowsing targetObject = (NativeMethods.IVsPerPropertyBrowsing) sender.TargetObject;
                bool pfDisplay = false;
                int num = targetObject.DisplayChildProperties(sender.DISPID, ref pfDisplay);
                if (gveevent.TypeConverter is Com2IDispatchConverter)
                {
                    gveevent.TypeConverter = new Com2IDispatchConverter(sender, (num == 0) && pfDisplay);
                }
                else
                {
                    gveevent.TypeConverter = new Com2IDispatchConverter(sender, (num == 0) && pfDisplay, gveevent.TypeConverter);
                }
            }
        }

        private void OnResetPropertyValue(Com2PropertyDescriptor sender, EventArgs e)
        {
            if (sender.TargetObject is NativeMethods.IVsPerPropertyBrowsing)
            {
                NativeMethods.IVsPerPropertyBrowsing targetObject = (NativeMethods.IVsPerPropertyBrowsing) sender.TargetObject;
                int dISPID = sender.DISPID;
                if (NativeMethods.Succeeded(targetObject.CanResetPropertyValue(dISPID, false)))
                {
                    targetObject.ResetPropertyValue(dISPID);
                }
            }
        }

        private void OnShouldSerializeValue(Com2PropertyDescriptor sender, GetBoolValueEvent gbvevent)
        {
            if (sender.TargetObject is NativeMethods.IVsPerPropertyBrowsing)
            {
                NativeMethods.IVsPerPropertyBrowsing targetObject = (NativeMethods.IVsPerPropertyBrowsing) sender.TargetObject;
                bool fDefault = true;
                if ((targetObject.HasDefaultValue(sender.DISPID, ref fDefault) == 0) && !fDefault)
                {
                    gbvevent.Value = true;
                }
            }
        }

        public override void SetupPropertyHandlers(Com2PropertyDescriptor[] propDesc)
        {
            if (propDesc != null)
            {
                for (int i = 0; i < propDesc.Length; i++)
                {
                    propDesc[i].QueryGetDynamicAttributes += new GetAttributesEventHandler(this.OnGetDynamicAttributes);
                    propDesc[i].QueryGetBaseAttributes += new GetAttributesEventHandler(this.OnGetBaseAttributes);
                    propDesc[i].QueryGetDisplayName += new GetNameItemEventHandler(this.OnGetDisplayName);
                    propDesc[i].QueryGetIsReadOnly += new GetBoolValueEventHandler(this.OnGetIsReadOnly);
                    propDesc[i].QueryShouldSerializeValue += new GetBoolValueEventHandler(this.OnShouldSerializeValue);
                    propDesc[i].QueryCanResetValue += new GetBoolValueEventHandler(this.OnCanResetPropertyValue);
                    propDesc[i].QueryResetValue += new Com2EventHandler(this.OnResetPropertyValue);
                    propDesc[i].QueryGetTypeConverterAndTypeEditor += new GetTypeConverterAndTypeEditorEventHandler(this.OnGetTypeConverterAndTypeEditor);
                }
            }
        }

        public override System.Type Interface
        {
            get
            {
                return typeof(NativeMethods.IVsPerPropertyBrowsing);
            }
        }
    }
}

