namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class Com2PropertyPageUITypeEditor : Com2ExtendedUITypeEditor, ICom2PropertyPageDisplayService
    {
        private Guid guid;
        private Com2PropertyDescriptor propDesc;

        public Com2PropertyPageUITypeEditor(Com2PropertyDescriptor pd, Guid guid, UITypeEditor baseEditor) : base(baseEditor)
        {
            this.propDesc = pd;
            this.guid = guid;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IntPtr focus = UnsafeNativeMethods.GetFocus();
            try
            {
                ICom2PropertyPageDisplayService service = (ICom2PropertyPageDisplayService) provider.GetService(typeof(ICom2PropertyPageDisplayService));
                if (service == null)
                {
                    service = this;
                }
                object instance = context.Instance;
                if (!instance.GetType().IsArray)
                {
                    instance = this.propDesc.TargetObject;
                    if (instance is ICustomTypeDescriptor)
                    {
                        instance = ((ICustomTypeDescriptor) instance).GetPropertyOwner(this.propDesc);
                    }
                }
                service.ShowPropertyPage(this.propDesc.Name, instance, this.propDesc.DISPID, this.guid, focus);
            }
            catch (Exception exception)
            {
                if (provider != null)
                {
                    IUIService service2 = (IUIService) provider.GetService(typeof(IUIService));
                    if (service2 != null)
                    {
                        service2.ShowError(exception, System.Windows.Forms.SR.GetString("ErrorTypeConverterFailed"));
                    }
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public unsafe void ShowPropertyPage(string title, object component, int dispid, Guid pageGuid, IntPtr parentHandle)
        {
            Guid[] arr = new Guid[] { pageGuid };
            IntPtr handle = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0);
            object[] objArray = component.GetType().IsArray ? ((object[]) component) : new object[] { component };
            int length = objArray.Length;
            IntPtr[] ptrArray = new IntPtr[length];
            try
            {
                for (int i = 0; i < length; i++)
                {
                    ptrArray[i] = Marshal.GetIUnknownForObject(objArray[i]);
                }
                try
                {
                    fixed (IntPtr* ptrRef = ptrArray)
                    {
                        SafeNativeMethods.OleCreatePropertyFrame(new HandleRef(null, parentHandle), 0, 0, title, length, new HandleRef(null, (IntPtr) ((ulong) ptrRef)), 1, new HandleRef(null, handle), SafeNativeMethods.GetThreadLCID(), 0, IntPtr.Zero);
                    }
                }
                finally
                {
                    ptrRef = null;
                }
            }
            finally
            {
                for (int j = 0; j < length; j++)
                {
                    if (ptrArray[j] != IntPtr.Zero)
                    {
                        Marshal.Release(ptrArray[j]);
                    }
                }
            }
        }
    }
}

