namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class EditorServiceContext : IWindowsFormsEditorService, ITypeDescriptorContext, IServiceProvider
    {
        private IComponentChangeService _componentChangeSvc;
        private ComponentDesigner _designer;
        private PropertyDescriptor _targetProperty;

        internal EditorServiceContext(ComponentDesigner designer)
        {
            this._designer = designer;
        }

        internal EditorServiceContext(ComponentDesigner designer, PropertyDescriptor prop)
        {
            this._designer = designer;
            this._targetProperty = prop;
            if (prop == null)
            {
                prop = TypeDescriptor.GetDefaultProperty(designer.Component);
                if ((prop != null) && typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                {
                    this._targetProperty = prop;
                }
            }
        }

        internal EditorServiceContext(ComponentDesigner designer, PropertyDescriptor prop, string newVerbText) : this(designer, prop)
        {
            this._designer.Verbs.Add(new DesignerVerb(newVerbText, new EventHandler(this.OnEditItems)));
        }

        public static object EditValue(ComponentDesigner designer, object objectToChange, string propName)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(objectToChange)[propName];
            EditorServiceContext context = new EditorServiceContext(designer, prop);
            UITypeEditor editor = prop.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
            object obj2 = prop.GetValue(objectToChange);
            object obj3 = editor.EditValue(context, context, obj2);
            if (obj3 != obj2)
            {
                try
                {
                    prop.SetValue(objectToChange, obj3);
                }
                catch (CheckoutException)
                {
                }
            }
            return obj3;
        }

        private void OnEditItems(object sender, EventArgs e)
        {
            object component = this._targetProperty.GetValue(this._designer.Component);
            if (component != null)
            {
                CollectionEditor editor = TypeDescriptor.GetEditor(component, typeof(UITypeEditor)) as CollectionEditor;
                if (editor != null)
                {
                    editor.EditValue(this, this, component);
                }
            }
        }

        void ITypeDescriptorContext.OnComponentChanged()
        {
            this.ChangeService.OnComponentChanged(this._designer.Component, this._targetProperty, null, null);
        }

        bool ITypeDescriptorContext.OnComponentChanging()
        {
            try
            {
                this.ChangeService.OnComponentChanging(this._designer.Component, this._targetProperty);
            }
            catch (CheckoutException exception)
            {
                if (exception != CheckoutException.Canceled)
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        object IServiceProvider.GetService(System.Type serviceType)
        {
            if ((serviceType == typeof(ITypeDescriptorContext)) || (serviceType == typeof(IWindowsFormsEditorService)))
            {
                return this;
            }
            if (this._designer.Component.Site != null)
            {
                return this._designer.Component.Site.GetService(serviceType);
            }
            return null;
        }

        void IWindowsFormsEditorService.CloseDropDown()
        {
        }

        void IWindowsFormsEditorService.DropDownControl(Control control)
        {
        }

        DialogResult IWindowsFormsEditorService.ShowDialog(Form dialog)
        {
            IUIService service = (IUIService) ((IServiceProvider) this).GetService(typeof(IUIService));
            if (service != null)
            {
                return service.ShowDialog(dialog);
            }
            return dialog.ShowDialog(this._designer.Component as IWin32Window);
        }

        private IComponentChangeService ChangeService
        {
            get
            {
                if (this._componentChangeSvc == null)
                {
                    this._componentChangeSvc = (IComponentChangeService) ((IServiceProvider) this).GetService(typeof(IComponentChangeService));
                }
                return this._componentChangeSvc;
            }
        }

        IContainer ITypeDescriptorContext.Container
        {
            get
            {
                if (this._designer.Component.Site != null)
                {
                    return this._designer.Component.Site.Container;
                }
                return null;
            }
        }

        object ITypeDescriptorContext.Instance
        {
            get
            {
                return this._designer.Component;
            }
        }

        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get
            {
                return this._targetProperty;
            }
        }
    }
}

