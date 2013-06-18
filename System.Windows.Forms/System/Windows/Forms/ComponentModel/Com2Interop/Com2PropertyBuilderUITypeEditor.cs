namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class Com2PropertyBuilderUITypeEditor : Com2ExtendedUITypeEditor
    {
        private int bldrType;
        private string guidString;
        private Com2PropertyDescriptor propDesc;

        public Com2PropertyBuilderUITypeEditor(Com2PropertyDescriptor pd, string guidString, int type, UITypeEditor baseEditor) : base(baseEditor)
        {
            this.propDesc = pd;
            this.guidString = guidString;
            this.bldrType = type;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IntPtr focus = UnsafeNativeMethods.GetFocus();
            IUIService service = (IUIService) provider.GetService(typeof(IUIService));
            if (service != null)
            {
                IWin32Window dialogOwnerWindow = service.GetDialogOwnerWindow();
                if (dialogOwnerWindow != null)
                {
                    focus = dialogOwnerWindow.Handle;
                }
            }
            bool actionCommitted = false;
            object pvarValue = value;
            try
            {
                object targetObject = this.propDesc.TargetObject;
                if (targetObject is ICustomTypeDescriptor)
                {
                    targetObject = ((ICustomTypeDescriptor) targetObject).GetPropertyOwner(this.propDesc);
                }
                System.Windows.Forms.NativeMethods.IProvidePropertyBuilder builder = (System.Windows.Forms.NativeMethods.IProvidePropertyBuilder) targetObject;
                if (System.Windows.Forms.NativeMethods.Failed(builder.ExecuteBuilder(this.propDesc.DISPID, this.guidString, null, new HandleRef(null, focus), ref pvarValue, ref actionCommitted)))
                {
                    actionCommitted = false;
                }
            }
            catch (ExternalException)
            {
            }
            if (actionCommitted && ((this.bldrType & 4) == 0))
            {
                return pvarValue;
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

