namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class DesignerContextDescriptor : IWindowsFormsEditorService, ITypeDescriptorContext, IServiceProvider
    {
        private Component _component;
        private IDesignerHost _host;
        private PropertyDescriptor _propertyDescriptor;

        public DesignerContextDescriptor(Component component, PropertyDescriptor imageProperty, IDesignerHost host)
        {
            this._component = component;
            this._propertyDescriptor = imageProperty;
            this._host = host;
        }

        public Image OpenImageCollection()
        {
            object obj2 = this._propertyDescriptor.GetValue(this._component);
            if (this._propertyDescriptor != null)
            {
                Image image = null;
                UITypeEditor editor = this._propertyDescriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
                if (editor != null)
                {
                    image = (Image) editor.EditValue(this, this, obj2);
                }
                if (image != null)
                {
                    return image;
                }
            }
            return (Image) obj2;
        }

        void ITypeDescriptorContext.OnComponentChanged()
        {
        }

        bool ITypeDescriptorContext.OnComponentChanging()
        {
            return false;
        }

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if (serviceType == typeof(IWindowsFormsEditorService))
            {
                return this;
            }
            return this._host.GetService(serviceType);
        }

        void IWindowsFormsEditorService.CloseDropDown()
        {
        }

        void IWindowsFormsEditorService.DropDownControl(Control control)
        {
        }

        DialogResult IWindowsFormsEditorService.ShowDialog(Form dialog)
        {
            DialogResult result;
            IntPtr focus = System.Design.UnsafeNativeMethods.GetFocus();
            IUIService service = (IUIService) ((IServiceProvider) this).GetService(typeof(IUIService));
            if (service != null)
            {
                result = service.ShowDialog(dialog);
            }
            else
            {
                result = dialog.ShowDialog(this._component as IWin32Window);
            }
            if (focus != IntPtr.Zero)
            {
                System.Design.UnsafeNativeMethods.SetFocus(new HandleRef(null, focus));
            }
            return result;
        }

        IContainer ITypeDescriptorContext.Container
        {
            get
            {
                return null;
            }
        }

        object ITypeDescriptorContext.Instance
        {
            get
            {
                return this._component;
            }
        }

        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get
            {
                return this._propertyDescriptor;
            }
        }
    }
}

